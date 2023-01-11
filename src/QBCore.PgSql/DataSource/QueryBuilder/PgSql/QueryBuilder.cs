using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using Npgsql;
using NpgsqlTypes;
using QBCore.Configuration;
using QBCore.Extensions;
using QBCore.Extensions.Internals;
using QBCore.Extensions.Linq;
using QBCore.Extensions.Text;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal abstract class QueryBuilder<TDoc, TDto> : IQueryBuilder<TDoc, TDto> where TDoc : class
{
	public abstract QueryBuilderTypes QueryBuilderType { get; }
	public Type DocType => typeof(TDoc);
	public DSDocumentInfo DocInfo => Builder.DocInfo;
	public Type DtoType => typeof(TDto);
	public DSDocumentInfo? DtoInfo => Builder.DtoInfo;
	public Type DataContextInterfaceType => typeof(IPgSqlDataContext);
	public IDataContext DataContext { get; }
	public QBBuilder<TDoc, TDto> Builder { get; }

	#region Const Properties
	private const FO _supportedOperations =
		  FO.Equal
		| FO.NotEqual
		| FO.Greater
		| FO.GreaterOrEqual
		| FO.Less
		| FO.LessOrEqual
		| FO.IsNull
		| FO.IsNotNull
		| FO.In
		| FO.NotIn
		| FO.Between
		| FO.NotBetween
		| FO.Like
		| FO.NotLike
		| FO.BitsAnd
		| FO.BitsOr;
	#endregion

	public QueryBuilder(QBBuilder<TDoc, TDto> builder, IDataContext dataContext)
	{
		if (builder is null) throw new ArgumentNullException(nameof(builder));
		if (dataContext is null) throw new ArgumentNullException(nameof(dataContext));

		Builder = builder;
		DataContext = dataContext;
	}

	#region BuildConditionTree

	protected static void BuildConditionTree(StringBuilder filter, IEnumerable<QBCondition> conditions, Func<string, DEPath, string> getDBSideName, IReadOnlyList<QBParameter> parameters, NpgsqlParameterCollection commandParams)
	{
		if (filter is null) throw new ArgumentNullException(nameof(filter));
		if (commandParams is null) throw new ArgumentNullException(nameof(commandParams));

		int startIndex = filter.Length;
		bool moveNext = true;

		using (var e = conditions.GetEnumerator())
		{
			while (moveNext && (moveNext = e.MoveNext()))
			{
				if (startIndex == filter.Length)
				{
					if (e.Current.Parentheses > 0)
					{
						BuildConditionTree(filter, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters, commandParams);
					}
					else
					{
						BuildCondition(filter, e.Current, getDBSideName, parameters, commandParams);
					}
				}
				else if (e.Current.IsByOr)
				{
					filter.Append(" OR ");

					if (e.Current.Parentheses > 0)
					{
						filter.Append('(');
						BuildConditionTree(filter, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters, commandParams);
						filter.Append(')');
					}
					else
					{
						BuildCondition(filter, e.Current, getDBSideName, parameters, commandParams);
					}
				}
				else
				{
					filter.Append(" AND ");

					if (e.Current.Parentheses > 0)
					{
						filter.Append('(');
						BuildConditionTree(filter, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters, commandParams);
						filter.Append(')');
					}
					else
					{
						BuildCondition(filter, e.Current, getDBSideName, parameters, commandParams);
					}
				}
			}
		}
	}

	private static int BuildConditionTree(StringBuilder filter, IEnumerator<QBCondition> e, ref bool moveNext, int parentheses, Func<string, DEPath, string> getDBSideName, IReadOnlyList<QBParameter> parameters, NpgsqlParameterCollection commandParams)
	{
		if (parentheses > 0)
		{
			var level = BuildConditionTree(filter, e, ref moveNext, parentheses - 1, getDBSideName, parameters, commandParams);
			if (level < 0)
			{
				return level + 1;
			}
		}
		else
		{
			BuildCondition(filter, e.Current, getDBSideName, parameters, commandParams);
		}

		while (moveNext && (moveNext = e.MoveNext()))
		{
			if (e.Current.IsByOr)
			{
				filter.Append(" OR ");

				if (e.Current.Parentheses > 0)
				{
					filter.Append('(');
					var level = BuildConditionTree(filter, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters, commandParams);
					filter.Append(')');

					if (level < 0)
					{
						return level + 1;
					}
				}
				else
				{
					BuildCondition(filter, e.Current, getDBSideName, parameters, commandParams);
					if (e.Current.Parentheses < 0)
					{
						return e.Current.Parentheses + 1;
					}
				}
			}
			else
			{
				filter.Append(" AND ");

				if (e.Current.Parentheses > 0)
				{
					filter.Append('(');
					var level = BuildConditionTree(filter, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters, commandParams);
					filter.Append(')');
					if (level < 0)
					{
						return level + 1;
					}
				}
				else
				{
					BuildCondition(filter, e.Current, getDBSideName, parameters, commandParams);
					if (e.Current.Parentheses < 0)
					{
						return e.Current.Parentheses + 1;
					}
				}
			}
		}

		return 0;
	}
	
	private static void BuildCondition(StringBuilder filter, QBCondition cond, Func<string, DEPath, string> getDBSideName, IReadOnlyList<QBParameter> parameters, NpgsqlParameterCollection commandParams)
	{
		if (cond.IsOnField)
		{
			MakeConditionOnField(filter, cond, getDBSideName);
		}
		else if (cond.IsOnParam)
		{
			var paramName = (string)cond.Value!;
			var parameter = parameters.FirstOrDefault(x => x.ParameterName == paramName);
			if (parameter == null)
			{
				throw new InvalidOperationException($"Query builder parameter '{paramName}' is not found.");
			}
			if (!parameter.HasValue)
			{
				throw new InvalidOperationException($"Query builder parameter '{paramName}' is not set.");
			}

			parameter.IsValueUsed = true;
			MakeConditionOnConst(filter, cond, getDBSideName, parameter, commandParams);
		}
		else if (cond.IsOnConst)
		{
			MakeConditionOnConst(filter, cond, getDBSideName, null, commandParams);
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	protected static void MakeConditionOnConst(StringBuilder filter, QBCondition cond, Func<string, DEPath, string> getDBSideName, QBParameter? parameter, NpgsqlParameterCollection commandParams)
	{
		if (!cond.IsOnParam && !cond.IsOnConst) throw new InvalidOperationException(nameof(MakeConditionOnConst) + " can only make conditions between a field and a constant value.");
		if (cond.IsOnParam == (parameter == null)) throw new ArgumentException(nameof(parameter));

		var constValue = cond.IsOnParam ? parameter!.Value : cond.Value;
		Type? compliment;
		var isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref constValue, cond.FieldType, cond.FieldUnderlyingType, out compliment);
		//var type = constValue?.GetType();
		//var itemType = constValue.GetEnumerationItemType();
		var leftField = getDBSideName(cond.Alias, cond.Field);

		switch (cond.Operation & _supportedOperations)
		{
			case FO.Equal:
			case FO.In:
				{
					if (!isCollection)
					{
						RenderCondition(filter, cond, parameter, constValue, true, "=", leftField, commandParams);
					}
					else
					{
						RenderConditionInNotIn(filter, cond, parameter, constValue, true, "IN", leftField, commandParams);
					}
					return;
				}
			case FO.NotEqual:
			case FO.NotIn:
				{
					if (!isCollection)
					{
						RenderCondition(filter, cond, parameter, constValue, true, "<>", leftField, commandParams);
					}
					else
					{
						RenderConditionInNotIn(filter, cond, parameter, constValue, true, "NOT IN", leftField, commandParams);
					}
					return;
				}
			case FO.Greater:
				{
					if (!isCollection)
					{
						RenderCondition(filter, cond, parameter, constValue, false, ">", leftField, commandParams);
					}
					else
					{
						RenderConditionGroup(filter, cond, parameter, constValue, false, ">", leftField, commandParams);
					}
					return;
				}
			case FO.GreaterOrEqual:
				{
					if (!isCollection)
					{
						RenderCondition(filter, cond, parameter, constValue, false, ">=", leftField, commandParams);
					}
					else
					{
						RenderConditionGroup(filter, cond, parameter, constValue, false, ">=", leftField, commandParams);
					}
					return;
				}
			case FO.Less:
				{
					if (!isCollection)
					{
						RenderCondition(filter, cond, parameter, constValue, false, "<", leftField, commandParams);
					}
					else
					{
						RenderConditionGroup(filter, cond, parameter, constValue, false, "<", leftField, commandParams);
					}
					return;
				}
			case FO.LessOrEqual:
				{
					if (!isCollection)
					{
						RenderCondition(filter, cond, parameter, constValue, false, "<=", leftField, commandParams);
					}
					else
					{
						RenderConditionGroup(filter, cond, parameter, constValue, false, "<=", leftField, commandParams);
					}
					return;
				}
			case FO.IsNull:
				{
					RenderCondition(filter, cond, parameter, null, false, "IS", leftField, commandParams);
					return;
				}
			case FO.IsNotNull:
				{
					RenderCondition(filter, cond, parameter, null, false, "IS NOT", leftField, commandParams);
					return;
				}
			case FO.Like:
				{
					if (!isCollection)
					{
						RenderCondition(filter, cond, parameter, constValue, true, "LIKE", leftField, commandParams);
					}
					else
					{
						RenderConditionGroup(filter, cond, parameter, constValue, true, "LIKE", leftField, commandParams);
					}
					return;
				}
			case FO.NotLike:
				{
					if (!isCollection)
					{
						RenderCondition(filter, cond, parameter, constValue, true, "NOT LIKE", leftField, commandParams);
					}
					else
					{
						RenderConditionGroup(filter, cond, parameter, constValue, true, "NOT LIKE", leftField, commandParams);
					}
					return;
				}
			case FO.BitsAnd:
				{
					if (!isCollection)
					{
						RenderConditionBitsAndOr(filter, cond, parameter, constValue, "&", leftField, commandParams);
					}
					else
					{
						RenderConditionGroupBitsAnd(filter, cond, parameter, constValue, leftField, commandParams);
					}
					return;
				}
			case FO.BitsOr:
				{
					if (!isCollection)
					{
						RenderConditionBitsAndOr(filter, cond, parameter, constValue, "|", leftField, commandParams);
					}
					else
					{
						var col = ArgumentHelper.ConvertUncheckedToCollection<long>(constValue, compliment ?? cond.FieldType);
						var lmask = col!.Aggregate(0L, (acc, x) => acc | x);

						object omask;
						if (cond.FieldUnderlyingType == typeof(long))
						{
							omask = lmask;
						}
						else if (cond.FieldUnderlyingType.IsEnum)
						{
							omask = Enum.ToObject(cond.FieldUnderlyingType, lmask);
						}
						else
						{
							omask = Convert.ChangeType(lmask, cond.FieldUnderlyingType);
						}

						RenderConditionBitsAndOr(filter, cond, parameter, omask, "|", leftField, commandParams);
					}
					return;
				}
			case FO.Between:
			case FO.NotBetween:
				{
					if (!isCollection)
					{
						throw new ArgumentException($"An even number of arguments greater than zero is expected for the 'Between' or 'NotBetween' operation on field {cond.Alias}.{cond.FieldPath}.");
					}

					RenderBetweenCondition(filter, cond, parameter, constValue, cond.Operation.HasFlag(FO.Between), leftField, commandParams);
					return;
				}
			default:
				throw new NotSupportedException();
		}
	}
	protected static void MakeConditionOnField(StringBuilder filter, QBCondition cond, Func<string, DEPath, string> getDBSideName)
	{
		if (!cond.IsOnField)
		{
			throw new InvalidOperationException(nameof(MakeConditionOnField) + " can only make conditions between fields.");
		}
		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			throw new InvalidOperationException("Case-sensitive or case-insensitive operations are not supported for conditions between fields.");
		}

		var leftField = getDBSideName(cond.Alias, cond.Field);
		var rightField = getDBSideName(cond.RefAlias!, cond.RefField!);

		switch (cond.Operation & _supportedOperations)
		{
			case FO.Equal: filter.Append(leftField).Append(" = ").Append(rightField); return;
			case FO.NotEqual: filter.Append(leftField).Append(" <> ").Append(rightField); return;
			case FO.Greater: filter.Append(leftField).Append(" > ").Append(rightField); return;
			case FO.GreaterOrEqual: filter.Append(leftField).Append(" >= ").Append(rightField); return;
			case FO.Less: filter.Append(leftField).Append(" < ").Append(rightField); return;
			case FO.LessOrEqual: filter.Append(leftField).Append(" <= ").Append(rightField); return;
			case FO.In: filter.Append(leftField).Append(" = ANY(").Append(rightField).Append(')'); return;
			case FO.NotIn: filter.Append("NOT (").Append(leftField).Append(" = ANY(").Append(rightField).Append("))"); return;
			case FO.BitsAnd: filter.Append('(').Append(leftField).Append(" & ").Append(rightField).Append(") = ").Append(rightField); return;
			case FO.BitsOr: filter.Append('(').Append(leftField).Append(" | ").Append(rightField).Append(") <> 0"); return;
			case FO.Like: filter.Append(leftField).Append(" LIKE ").Append(rightField); return;
			case FO.NotLike: filter.Append(leftField).Append(" NOT LIKE ").Append(rightField); return;
			case FO.IsNull:
			case FO.IsNotNull:
			case FO.Between:
			case FO.NotBetween:
			default:
				throw new InvalidOperationException($"An operation such as '{cond.Operation.ToString()}' is not supported for conditions between fields.");
		}
	}

	protected static string GetQuotedDBSideName(string? alias, DEPath fieldPath)
	{
		if (string.IsNullOrEmpty(alias))
		{
			return string.Concat("\"", fieldPath.GetDBSideName(), "\"");
		}
		else
		{
			return string.Concat(alias, ".\"", fieldPath.GetDBSideName(), "\"");
		}
	}
	protected static string GetQuotedDBSideNameWithoutAlias(string? _, DEPath fieldPath)
	{
		return string.Concat("\"", fieldPath.GetDBSideName(), "\"");
	}

	private static void RenderCondition(StringBuilder filter, QBCondition cond, QBParameter? parameter, object? value, bool allowCaseInsensitive, string command, string leftField, NpgsqlParameterCollection commandParams)
	{
		if (value is null)
		{
			if (command == "<>" || command == "IS NOT" || command == "NOT LIKE")
			{
				filter.Append(leftField).Append(" IS NOT NULL");
				return;
			}

			if (!cond.IsFieldNullable)
			{
				throw new ArgumentNullException(nameof(value), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
			}

			if (command == "=" || command == "LIKE")
			{
				command = "IS";
			}

			filter.Append(leftField).Append(' ').Append(command).Append(" NULL");
			return;
		}

		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			if (!allowCaseInsensitive)
			{
				throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
			}
			if (value.GetType() != typeof(string))
			{
				throw new InvalidOperationException($"Such operations can only be performed on strings. Field {cond.Alias}.{cond.FieldPath} is not a string type.");
			}

			commandParams.Add(MakeUnnamedParameter(parameter, ((string)value).ToLower()));//!!! Localization
			filter.Append("LOWER(").Append(leftField).Append(") ").Append(command).Append(" $").Append(commandParams.Count);
		}
		else
		{
			commandParams.Add(MakeUnnamedParameter(parameter, value));
			filter.Append(leftField).Append(' ').Append(command).Append(" $").Append(commandParams.Count);
		}
	}

	private static void RenderConditionInNotIn(StringBuilder filter, QBCondition cond, QBParameter? parameter, object? values, bool allowCaseInsensitive, string command, string leftField, NpgsqlParameterCollection commandParams)
	{
		command = command switch
		{
			"IN" => "=",
			"NOT IN" => "<>",
			_ => throw new ArgumentException(nameof(command))
		};

		values = PrepareCollection(cond, values, allowCaseInsensitive) ?? throw new ArgumentNullException(nameof(values));

		var commandParam = MakeUnnamedParameter(parameter, values);
		commandParam.NpgsqlDbType |= NpgsqlDbType.Array;
		commandParams.Add(commandParam);

		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			filter.Append("LOWER(").Append(leftField).Append(") ").Append(command).Append(" ANY(ARRAY[$").Append(commandParams.Count).Append("])");
		}
		else
		{
			filter.Append(leftField).Append(" ").Append(command).Append(" ANY(ARRAY[$").Append(commandParams.Count).Append("])");
		}
	}

	private static void RenderConditionGroup(StringBuilder filter, QBCondition cond, QBParameter? parameter, object? values, bool allowCaseInsensitive, string command, string leftField, NpgsqlParameterCollection commandParams)
	{
		var col = PrepareCollection(cond, values, allowCaseInsensitive) as IEnumerable ?? throw new ArgumentNullException(nameof(values));

		var startIndex = filter.Length;
		var itemCounter = 0;
		foreach (var value in col)
		{
			if (itemCounter++ > 0) filter.Append(" OR ");

			if (value is null)
			{
				if (!cond.IsFieldNullable)
				{
					throw new ArgumentNullException(nameof(value), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
				}

				filter.Append(leftField).Append(' ').Append(command).Append(" NULL");
			}
			else
			{
				commandParams.Add(MakeUnnamedParameter(parameter, value));

				filter.Append(leftField).Append(' ').Append(command).Append(" $").Append(commandParams.Count);
			}
		}

		if (itemCounter == 0)
		{
			filter.Append("false");
		}
		else if (itemCounter > 1)
		{
			filter.Insert(startIndex, '(');
			filter.Append(')');
		}
	}

	private static void RenderConditionBitsAndOr(StringBuilder filter, QBCondition cond, QBParameter? parameter, object? value, string command, string leftField, NpgsqlParameterCollection commandParams)
	{
		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
		}

		if (value is null)
		{
			if (!cond.IsFieldNullable)
			{
				throw new ArgumentNullException(nameof(value), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
			}

			if (command == "&")
				filter.Append('(').Append(leftField).Append(" & NULL) = NULL");
			else if (command == "|")
				filter.Append('(').Append(leftField).Append(" & NULL) <> 0");
			else
				throw new ArgumentException(nameof(command));

			return;
		}

		commandParams.Add(MakeUnnamedParameter(parameter, value));
		if (command == "&")
			filter.Append('(').Append(leftField).Append(" & $").Append(commandParams.Count).Append(") = $").Append(commandParams.Count);
		else if (command == "|")
			filter.Append('(').Append(leftField).Append(" & $").Append(commandParams.Count).Append(") <> 0");
		else
			throw new ArgumentException(nameof(command));
	}

	private static void RenderConditionGroupBitsAnd(StringBuilder filter, QBCondition cond, QBParameter? parameter, object? values, string leftField, NpgsqlParameterCollection commandParams)
	{
		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
		}

		var col = PrepareCollection(cond, values, false) as IEnumerable ?? throw new ArgumentNullException(nameof(values));

		var startIndex = filter.Length;
		var itemCounter = 0;
		foreach (var value in col)
		{
			if (itemCounter++ > 0) filter.Append(" OR ");

			if (value is null)
			{
				if (!cond.IsFieldNullable)
				{
					throw new ArgumentNullException(nameof(value), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
				}

				filter.Append('(').Append(leftField).Append(" & NULL) = NULL");
			}
			else
			{
				commandParams.Add(MakeUnnamedParameter(parameter, value));

				filter.Append('(').Append(leftField).Append(" & $").Append(commandParams.Count).Append(") = $").Append(commandParams.Count);
			}
		}

		if (itemCounter == 0)
		{
			filter.Append("false");
		}
		else if (itemCounter > 1)
		{
			filter.Insert(startIndex, '(');
			filter.Append(')');
		}
	}

	private static void RenderBetweenCondition(StringBuilder filter, QBCondition cond, QBParameter? parameter, object? values, bool betweenOrNotBetween, string leftField, NpgsqlParameterCollection commandParams)
	{
		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
		}

		var col = PrepareCollection(cond, values, false) as IEnumerable ?? throw new ArgumentNullException(nameof(values));

		int startIndex = filter.Length;
		int itemCounter = 0;
		foreach (var value in col)
		{
			if (itemCounter > 0 && (itemCounter & 1) == 0) filter.Append(" OR ");
			itemCounter++;

			if (value is null)
			{
				if (!cond.IsFieldNullable)
				{
					throw new ArgumentNullException(nameof(value), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
				}

				if ((itemCounter & 1) == 0)
				{
					filter.Append(leftField).Append(" BETWEEN NULL AND ");
				}
				else
				{
					filter.Append("NULL");
				}
			}
			else
			{
				commandParams.Add(MakeUnnamedParameter(parameter, value));

				if ((itemCounter & 1) == 0)
				{
					filter.Append(leftField).Append(" BETWEEN NULL AND $").Append(commandParams.Count);
				}
				else
				{
					filter.Append("$").Append(commandParams.Count);
				}
			}
		}

		if (itemCounter == 0)
		{
			throw new ArgumentException(nameof(values));
		}
		else if ((itemCounter & 1) == 1)
		{
			throw new ArgumentException($"An even number of arguments greater than zero is expected for the 'Between' or 'NotBetween' operation on field {cond.Alias}.{cond.FieldPath}.", nameof(values));
		}
		else if (itemCounter > 2)
		{
			filter.Insert(startIndex, '(');
			filter.Append(')');
		}
	}

	private static object? PrepareCollection(QBCondition cond, object? values, bool allowCaseInsensitive)
	{
		if (values is not null && cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			if (!allowCaseInsensitive)
			{
				throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
			}
			if (cond.FieldUnderlyingType != typeof(string) || values is not ICollection<string?> stringValues)
			{
				throw new InvalidOperationException($"Such operations can only be performed on strings. Field {cond.Alias}.{cond.FieldPath} is not a string type.");
			}

			var list = new List<string>(stringValues.Count);
			foreach (var s in stringValues)
			{
				if (s is null)
				{
					if (!cond.IsFieldNullable) throw new ArgumentNullException(nameof(values), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
				}
				else
				{
					list.Add(s.ToLower());//!!! Localization
				}
			}

			values = list;
		}

		return values;
	}

	internal static NpgsqlParameter MakeUnnamedParameter(QBParameter? parameter, object? value)
	{
		var commandParam = new NpgsqlParameter();

		if (parameter == null)
		{
			commandParam.NpgsqlValue = value is null ? DBNull.Value : value;
		}
		else
		{
			commandParam.Direction = parameter.Direction;

			if (parameter.DbType is null)
			{
				commandParam.NpgsqlValue = value is null ? DBNull.Value : value;
			}
			else if (parameter.DbType is NpgsqlDbType npgsqlDbType)
			{
				commandParam.NpgsqlDbType = npgsqlDbType;
				commandParam.NpgsqlValue = value is null ? DBNull.Value : value;
			}
			else if (parameter.DbType is System.Data.DbType dbType)
			{
				commandParam.DbType = dbType;
				commandParam.Value = value is null ? DBNull.Value : value;
			}
			else
			{
				throw new NotSupportedException($"Unsupported database type '{parameter.DbType.GetType().ToPretty()}'.");
			}

			if (parameter.DbTypeName is not null) commandParam.DataTypeName = parameter.DbTypeName;
			commandParam.IsNullable = parameter.IsNullable;
			if (parameter.SourceColumn is not null) commandParam.SourceColumn = parameter.SourceColumn;
			if (parameter.Size != 0) commandParam.Size = parameter.Size;
			if (parameter.Precision != 0) commandParam.Precision = parameter.Precision;
			if (parameter.Scale != 0) commandParam.Scale = parameter.Scale;
		}

		return commandParam;
	}

	#endregion

	/// <summary>
	/// Slice the conditions to the smalest possible parts separated by AND
	/// </summary>
	/// <param name="conditions">Regular conditions</param>
	/// <remarks>
	/// AND has a higher precedence than OR. To split correctly,
	/// the AND operands must be wrapped in parentheses.<para />
	///<para />
	/// a                                => a<para />
	/// a AND b                          => a, b<para />
	/// a OR b                           => a OR b<para />
	/// a AND b OR c = (a AND b) OR c    => (a AND b) OR c<para />
	/// a AND (b OR c)                   => a, b OR c<para />
	/// a OR b AND c = a OR (b AND c)    => a OR (b AND c)<para />
	/// (a OR b) AND c                   => a OR b, c<para />
	/// ((a OR b) AND c) AND e           => a OR b, c, e<para />
	/// ((a OR b) AND c) OR e            => ((a OR b) AND c) OR e
	/// </remarks>
	protected static List<List<QBCondition>> SliceConditions(IEnumerable<QBCondition> conditions)
	{
		var list = new List<List<QBCondition>>() { { conditions.ToList() } };
		List<QBCondition> conds;
		int first, splitIndex, sum;

		for (int i = 0, j; i < list.Count;)
		{
			conds = list[i];
			if (conds.Count <= 1)
			{
				i++;
				continue;
			}

			SqlSelectQBBuilder<TDoc, TDto>.TrimParentheses(conds);

			first = conds[0].Parentheses;
			splitIndex = -1;
			sum = 0;
			for (j = 1; j < conds.Count; j++)
			{
				if (sum + first <= 0)
				{
					if (conds[j].IsByOr)
					{
						splitIndex = -1;
						break;
					}
					else if (splitIndex < 0)
					{
						splitIndex = j;
					}
				}

				sum += conds[j].Parentheses;
			}

			if (splitIndex < 0 || splitIndex >= conds.Count - 1)
			{
				i++;
			}
			else
			{
				list.Insert(i + 1, conds.Skip(splitIndex).ToList());
				conds.RemoveRange(splitIndex, conds.Count - splitIndex);
			}
		}

		return list;
	}
}