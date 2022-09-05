using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.Extensions.Linq;
using QBCore.Extensions.Text;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal abstract class QueryBuilder<TDocument, TProjection> : IQueryBuilder<TDocument, TProjection>
{
	public abstract QueryBuilderTypes QueryBuilderType { get; }
	public Type DocumentType => typeof(TDocument);
	public DSDocumentInfo DocumentInfo => Builder.DocumentInfo;
	public Type ProjectionType => typeof(TProjection);
	public DSDocumentInfo? ProjectionInfo => Builder.ProjectionInfo;
	public Type DatabaseContextInterfaceType => typeof(IMongoDbContext);
	public IDataContext DataContext { get; }
	public QBBuilder<TDocument, TProjection> Builder { get; }

	protected IMongoDbContext _mongoDbContext;


	#region Static ReadOnly & Const Properties

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

	private static readonly HashSet<Type> _integerTypes = new HashSet<Type>()
	{
		typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)
	};

	protected static readonly BsonDocument _noneFilter = new BsonDocument { { "$expr", BsonBoolean.False } };

	#endregion

	public QueryBuilder(QBBuilder<TDocument, TProjection> builder, IDataContext dataContext)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}
		if (dataContext == null)
		{
			throw new ArgumentNullException(nameof(dataContext));
		}
		if (dataContext.Context is not IMongoDbContext)
		{
			throw new ArgumentException(nameof(dataContext));
		}

		Builder = builder;
		DataContext = dataContext;
		_mongoDbContext = (IMongoDbContext)dataContext.Context;
	}

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

			QBSelectBuilder<TDocument, TProjection>.TrimParentheses(conds);

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

	#region BuildConditionTree

	protected static BuiltCondition? BuildConditionTree(bool useExprFormat, IEnumerable<QBCondition> conditions, Func<string, DEPath, string> getDBSideName, IReadOnlyList<QBParameter> parameters)
	{
		BuiltCondition? filter = null;
		bool moveNext = true;

		using (var e = conditions.GetEnumerator())
		{
			while (moveNext && (moveNext = e.MoveNext()))
			{
				if (filter == null)
				{
					if (e.Current.Parentheses > 0)
					{
						filter = BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters).filter;
					}
					else
					{
						filter = new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, parameters), useExprFormat);
					}
				}
				else if (e.Current.IsByOr)
				{
					if (e.Current.Parentheses > 0)
					{
						filter.AppendByOr(BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters).filter);
					}
					else
					{
						filter.AppendByOr(new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, parameters), useExprFormat));
					}
				}
				else
				{
					if (e.Current.Parentheses > 0)
					{
						filter.AppendByAnd(BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters).filter);
					}
					else
					{
						filter.AppendByAnd(new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, parameters), useExprFormat));
					}
				}
			}

			return filter;
		}
	}
	private static (BuiltCondition filter, int level) BuildConditionTree(bool useExprFormat, IEnumerator<QBCondition> e, ref bool moveNext, int parentheses, Func<string, DEPath, string> getDBSideName, IReadOnlyList<QBParameter> parameters)
	{
		BuiltCondition filter;

		if (parentheses > 0)
		{
			var result = BuildConditionTree(useExprFormat, e, ref moveNext, parentheses - 1, getDBSideName, parameters);
			filter = result.filter;
			if (result.level < 0)
			{
				return (filter, result.level + 1);
			}
		}
		else
		{
			filter = new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, parameters), useExprFormat);
		}

		while (moveNext && (moveNext = e.MoveNext()))
		{
			if (e.Current.IsByOr)
			{
				if (e.Current.Parentheses > 0)
				{
					var result = BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters);
					filter.AppendByOr(result.filter);
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter.AppendByOr(new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, parameters), useExprFormat));
					if (e.Current.Parentheses < 0)
					{
						return (filter, e.Current.Parentheses + 1);
					}
				}
			}
			else
			{
				if (e.Current.Parentheses > 0)
				{
					var result = BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, parameters);
					filter.AppendByAnd(result.filter);
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter.AppendByAnd(new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, parameters), useExprFormat));
					if (e.Current.Parentheses < 0)
					{
						return (filter, e.Current.Parentheses + 1);
					}
				}
			}
		}

		return (filter, 0);
	}
	private static BsonDocument BuildCondition(bool useExprFormat, QBCondition cond, Func<string, DEPath, string> getDBSideName, IReadOnlyList<QBParameter> parameters)
	{
		if (cond.IsOnField)
		{
			return MakeConditionOnField(useExprFormat && cond.IsConnect, cond, getDBSideName);
		}
		else if (cond.IsOnParam)
		{
			var paramName = (string)cond.Value!;
			var parameter = parameters.FirstOrDefault(x => x.Name == paramName);
			if (parameter == null)
			{
				throw new InvalidOperationException($"Query builder parameter '{paramName}' is not found.");
			}
			if (!parameter.HasValue)
			{
				throw new InvalidOperationException($"Query builder parameter '{paramName}' is not set.");
			}

			parameter.IsValueUsed = true;
			return MakeConditionOnConst(useExprFormat, cond, getDBSideName, parameter.Value);
		}
		else if (cond.IsOnConst)
		{
			return MakeConditionOnConst(useExprFormat, cond, getDBSideName);
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	#endregion

	protected static BsonDocument MakeConditionOnConst(bool useExprFormat, QBCondition cond, Func<string, DEPath, string> getDBSideName, object? paramValue = null)
	{
		if (!cond.IsOnParam && !cond.IsOnConst)
		{
			throw new InvalidOperationException(nameof(MakeConditionOnConst) + " can only make conditions between a field and a constant value.");
		}

		var constValue = cond.IsOnParam ? paramValue : cond.Value;
		var itemType = GetEnumerationItemType(constValue);
		var leftField = getDBSideName(cond.Alias, cond.Field);

		switch (cond.Operation & _supportedOperations)
		{
			case FO.Equal:
			case FO.In:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonValue(cond, constValue, true);
						return MakeBsonCondition(useExprFormat, "$eq", leftField, value);
					}
					else
					{
						var values = RenderToBsonArray(cond, constValue);
						if (values.Count == 1)
							return MakeBsonCondition(useExprFormat, "$eq", leftField, values.First());
						else
							return MakeBsonCondition(useExprFormat, "$in", leftField, values);
					}
				}
			case FO.NotEqual:
			case FO.NotIn:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonValue(cond, constValue, true);
						return MakeBsonCondition(useExprFormat, "$ne", leftField, value);
					}
					else
					{
						var values = RenderToBsonArray(cond, constValue);
						if (values.Count == 1)
							return MakeBsonCondition(useExprFormat, "$ne", leftField, values.First());
						else
							return MakeBsonCondition(useExprFormat, "$nin", leftField, values);
					}
				}
			case FO.Greater:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonValue(cond, constValue, false);
						return MakeBsonCondition(useExprFormat, "$gt", leftField, value);
					}
					else
					{
						var values = RenderToBsonArray(cond, constValue);
						return MakeBsonArrayCondition(useExprFormat, "$gt", leftField, values);
					}
				}
			case FO.GreaterOrEqual:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonValue(cond, constValue, false);
						return MakeBsonCondition(useExprFormat, "$gte", leftField, value);
					}
					else
					{
						var values = RenderToBsonArray(cond, constValue);
						return MakeBsonArrayCondition(useExprFormat, "$gte", leftField, values);
					}
				}
			case FO.Less:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonValue(cond, constValue, false);
						return MakeBsonCondition(useExprFormat, "$lt", leftField, value);
					}
					else
					{
						var values = RenderToBsonArray(cond, constValue);
						return MakeBsonArrayCondition(useExprFormat, "$lt", leftField, values);
					}
				}
			case FO.LessOrEqual:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonValue(cond, constValue, false);
						return MakeBsonCondition(useExprFormat, "$lte", leftField, value);
					}
					else
					{
						var values = RenderToBsonArray(cond, constValue);
						return MakeBsonArrayCondition(useExprFormat, "$lte", leftField, values);
					}
				}
			case FO.IsNull:
				{
					RenderToBsonValue(cond, null, false);
					return MakeBsonCondition(useExprFormat, "$eq", leftField, BsonNull.Value);
				}
			case FO.IsNotNull:
				{
					return MakeBsonCondition(useExprFormat, "$ne", leftField, BsonNull.Value);
				}
			case FO.Like:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonRegexForLike(cond, constValue);
						return MakeBsonCondition(useExprFormat, "$eq", leftField, value);
					}
					else
					{
						var values = RenderToBsonRegexArrayForLike(cond, constValue);
						if (values.Count == 1)
							return MakeBsonCondition(useExprFormat, "$eq", leftField, values.First());
						else
							return MakeBsonCondition(useExprFormat, "$in", leftField, values);
					}
				}
			case FO.NotLike:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonRegexForLike(cond, constValue);
						return MakeBsonCondition(useExprFormat, "$ne", leftField, value);
					}
					else
					{
						var values = RenderToBsonRegexArrayForLike(cond, constValue);
						if (values.Count == 1)
							return MakeBsonCondition(useExprFormat, "$ne", leftField, values.First());
						else
							return MakeBsonCondition(useExprFormat, "$nin", leftField, values);
					}
				}
			case FO.BitsAnd:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonInt64(cond, constValue);
						return MakeBsonCondition(useExprFormat, "$bitsAllSet", leftField, value);
					}
					else
					{
						var values = RenderToLongArray(cond, constValue).Distinct();
						return MakeBsonArrayCondition(useExprFormat, "$bitsAllSet", leftField, new BsonArray(values));
					}
				}
			case FO.BitsOr:
				{
					if (constValue == null || itemType == null || (itemType != cond.FieldType && itemType != cond.FieldUnderlyingType))
					{
						var value = RenderToBsonInt64(cond, constValue);
						return MakeBsonCondition(useExprFormat, "$bitsAnySet", leftField, value);
					}
					else
					{
						var values = RenderToLongArray(cond, constValue);
						var mask = values.Aggregate(0L, (acc, x) => acc | x);
						if (mask == 0 && values.IsNullEmpty())
						{
							throw new ArgumentException("value");
						}

						return MakeBsonCondition(useExprFormat, "$bitsAnySet", leftField, mask);
					}
				}
			case FO.Between:
			case FO.NotBetween:
				{
					if (constValue == null || itemType == null)
					{
						throw new ArgumentException($"Expected IEnumerable of two arguments for the 'Between' or 'NotBetween' operation on field {cond.Alias}.{cond.FieldPath}.", nameof(paramValue));
					}

					var values = RenderToBsonArray(cond, constValue);
					if (values.Count != 2)
					{
						throw new ArgumentException($"Expected IEnumerable of two arguments for the 'Between' or 'NotBetween' operation on field {cond.Alias}.{cond.FieldPath}.", nameof(paramValue));
					}

					return MakeBsonBetweenCondition(useExprFormat, cond.Operation.HasFlag(FO.Between), leftField, values);
				}
			default:
				throw new NotSupportedException();
		}
	}
	protected static BsonDocument MakeConditionOnField(bool rightIsVar, QBCondition cond, Func<string, DEPath, string> getDBSideName)
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
			case FO.Equal: return MakeBsonConditionOnFields(rightIsVar, "$eq", leftField, rightField);
			case FO.NotEqual: return MakeBsonConditionOnFields(rightIsVar, "$ne", leftField, rightField);
			case FO.Greater: return MakeBsonConditionOnFields(rightIsVar, "$gt", leftField, rightField);
			case FO.GreaterOrEqual: return MakeBsonConditionOnFields(rightIsVar, "$gte", leftField, rightField);
			case FO.Less: return MakeBsonConditionOnFields(rightIsVar, "$lt", leftField, rightField);
			case FO.LessOrEqual: return MakeBsonConditionOnFields(rightIsVar, "$lte", leftField, rightField);
			case FO.In: return MakeBsonConditionOnFields(rightIsVar, "$in", leftField, rightField);
			case FO.NotIn: return MakeBsonConditionOnFields(rightIsVar, "$nin", leftField, rightField);
			case FO.BitsAnd: return MakeBsonConditionOnFields(rightIsVar, "$bitsAllSet", leftField, rightField);
			case FO.BitsOr: return MakeBsonConditionOnFields(rightIsVar, "$bitsAnySet", leftField, rightField);

			case FO.IsNull:
			case FO.IsNotNull:
			case FO.Like:
			case FO.NotLike:
			case FO.Between:
			case FO.NotBetween:
			default:
				throw new InvalidOperationException($"An operation such as '{cond.Operation.ToString()}' is not supported for conditions between fields.");
		}
	}

	protected static string GetDBSideName(string? alias, DEPath fieldPath)
	{
		return fieldPath.GetDBSideName();
	}

	private static BsonValue RenderToBsonValue(QBCondition cond, object? value, bool allowCaseInsensitive)
	{
		if (value == null)
		{
			if (!cond.IsFieldNullable)
			{
				throw new ArgumentNullException(nameof(value), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
			}

			return BsonNull.Value;
		}

		var type = value.GetType();
		if (type != cond.FieldType && type != cond.FieldUnderlyingType)
		{
			if (!TryConvertIntegerToOtherInteger(value, cond.FieldUnderlyingType, ref value))
			{
				throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {value!.GetType().ToPretty()}.", nameof(value));
			}
		}
		
		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			if (!allowCaseInsensitive)
			{
				throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
			}
			if (type != typeof(string))
			{
				throw new InvalidOperationException($"Such operations can only be performed on strings. Field {cond.Alias}.{cond.FieldPath} is not a string type.");
			}

			return new BsonRegularExpression(string.Concat("^", Regex.Escape(((string)value).ToLower()), "$"), "i");//!!! localization
		}

		return new BsonDocumentWrapper(value, BsonSerializer.SerializerRegistry.GetSerializer(cond.FieldType));
	}
	private static BsonArray RenderToBsonArray(QBCondition cond, object values)
	{
		if (values is not IEnumerable objectValues)
		{
			throw new ArgumentNullException(nameof(values), "IEnumerable<> is expected.");
		}

		var bsonArray = new BsonArray();
		IBsonSerializer? serializer = null;
		object? convertedValue = null;
		Type type;

		foreach (var value in objectValues)
		{
			if (value == null)
			{
				if (!cond.IsFieldNullable)
				{
					throw new ArgumentNullException(nameof(values), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
				}

				bsonArray.Add(BsonNull.Value);
				continue;
			}

			type = value.GetType();
			if (type != cond.FieldType && type != cond.FieldUnderlyingType)
			{
				if (!TryConvertIntegerToOtherInteger(value, cond.FieldUnderlyingType, ref convertedValue))
				{
					throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {type.ToPretty()}.", nameof(values));
				}

				bsonArray.Add(new BsonDocumentWrapper(convertedValue, serializer ?? (serializer = BsonSerializer.SerializerRegistry.GetSerializer(cond.FieldType))));
			}
			else if (type == typeof(string))
			{
				if (cond.Operation.HasFlag(FO.CaseInsensitive))
				{
					bsonArray.Add(new BsonRegularExpression(string.Concat("^", Regex.Escape(((string)value).ToLower()), "$"), "i"));
				}
				else
				{
					bsonArray.Add(new BsonString((string)value));
				}
			}
			else if (cond.Operation.HasFlag(FO.CaseInsensitive))
			{
				throw new InvalidOperationException($"Such operations can only be performed on strings. Field {cond.Alias}.{cond.FieldPath} is not a string type.");
			}
			else
			{
				bsonArray.Add(new BsonDocumentWrapper(value, serializer ?? (serializer = BsonSerializer.SerializerRegistry.GetSerializer(cond.FieldType))));
			}
		}

		return bsonArray;
	}

	private static BsonValue RenderToBsonRegexForLike(QBCondition cond, object? value)
	{
		if (cond.FieldUnderlyingType != typeof(string))
		{
			throw new InvalidOperationException($"Such operations can only be performed on strings. Field {cond.Alias}.{cond.FieldPath} is not a string type.");
		}
		if (value == null)
		{
			if (!cond.IsFieldNullable)
			{
				throw new ArgumentNullException(nameof(value), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
			}

			return BsonNull.Value;
		}
		if (value is not string stringValue)
		{
			throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {value.GetType().ToPretty()}.", nameof(value));
		}

		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			if (cond.Operation.HasFlag(FO.Like))
			{
				return new BsonRegularExpression(Regex.Escape(stringValue.ToLower()), "i");//!!! localization
			}
			else
			{
				return new BsonRegularExpression(string.Concat("(?s)^(?!.*", Regex.Escape(stringValue.ToLower()), ").*$"), "i");//!!! localization
			}
		}
		else
		{
			if (cond.Operation.HasFlag(FO.Like))
			{
				return new BsonRegularExpression(Regex.Escape(stringValue));//!!! localization
			}
			else
			{
				return new BsonRegularExpression(string.Concat("(?s)^(?!.*", Regex.Escape(stringValue), ").*$"));//!!! localization
			}
		}
	}
	private static BsonArray RenderToBsonRegexArrayForLike(QBCondition cond, object values)
	{
		if (cond.FieldUnderlyingType != typeof(string))
		{
			throw new InvalidOperationException($"Such operations can only be performed on strings. Field {cond.Alias}.{cond.FieldPath} is not a string type.");
		}

		var bsonArray = new BsonArray();

		if (values is IEnumerable<string?> stringValues)
		{
			foreach (var stringValue in stringValues)
			{
				if (stringValue == null)
				{
					if (!cond.IsFieldNullable)
					{
						throw new ArgumentNullException(nameof(values), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
					}

					bsonArray.Add(BsonNull.Value);
				}
				else if (cond.Operation.HasFlag(FO.CaseInsensitive))
				{
					if (cond.Operation.HasFlag(FO.Like))
					{
						bsonArray.Add(new BsonRegularExpression(Regex.Escape(stringValue.ToLower()), "i"));//!!! localization
					}
					else
					{
						bsonArray.Add(new BsonRegularExpression(string.Concat("(?s)^(?!.*", Regex.Escape(stringValue.ToLower()), ").*$"), "i"));//!!! localization
					}
				}
				else
				{
					if (cond.Operation.HasFlag(FO.Like))
					{
						bsonArray.Add(new BsonRegularExpression(Regex.Escape(stringValue)));//!!! localization
					}
					else
					{
						bsonArray.Add(new BsonRegularExpression(string.Concat("(?s)^(?!.*", Regex.Escape(stringValue), ").*$")));//!!! localization
					}
				}
			}
		}
		else if (values is IEnumerable objectValues)
		{
			foreach (var objectValue in objectValues)
			{
				if (objectValue == null)
				{
					if (!cond.IsFieldNullable)
					{
						throw new ArgumentNullException(nameof(values), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
					}

					bsonArray.Add(BsonNull.Value);
				}
				else if (objectValue is not string stringValue)
				{
					throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {values.GetType().ToPretty()}.", nameof(values));
				}
				else if (cond.Operation.HasFlag(FO.CaseInsensitive))
				{
					if (cond.Operation.HasFlag(FO.Like))
					{
						bsonArray.Add(new BsonRegularExpression(Regex.Escape(stringValue.ToLower()), "i"));//!!! localization
					}
					else
					{
						bsonArray.Add(new BsonRegularExpression(string.Concat("(?s)^(?!.*", Regex.Escape(stringValue.ToLower()), ").*$"), "i"));//!!! localization
					}
				}
				else
				{
					if (cond.Operation.HasFlag(FO.Like))
					{
						bsonArray.Add(new BsonRegularExpression(Regex.Escape(stringValue)));//!!! localization
					}
					else
					{
						bsonArray.Add(new BsonRegularExpression(string.Concat("(?s)^(?!.*", Regex.Escape(stringValue), ").*$")));//!!! localization
					}
				}
			}
		}
		else
		{
			throw new ArgumentNullException(nameof(values), "IEnumerable<string> is expected.");
		}

		return bsonArray;
	}

	private static BsonInt64 RenderToBsonInt64(QBCondition cond, object? value)
	{
		if (!_integerTypes.Contains(cond.FieldUnderlyingType))
		{
			throw new InvalidOperationException($"Such operations can only be performed on integers. Field {cond.Alias}.{cond.FieldPath} is not an integer type.");
		}
		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
		}
		if (value == null)
		{
			if (!cond.IsFieldNullable)
			{
				throw new ArgumentNullException(nameof(value), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
			}

			return 0L;
		}

		var type = value.GetType();
		if (type != cond.FieldType && type != cond.FieldUnderlyingType && !_integerTypes.Contains(type))
		{
			throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {type.ToPretty()}.", nameof(value));
		}

		return Convert.ToInt64(value);
	}
	private static IEnumerable<long> RenderToLongArray(QBCondition cond, object values)
	{
		if (!_integerTypes.Contains(cond.FieldUnderlyingType))
		{
			throw new InvalidOperationException($"Field {cond.Alias}.{cond.FieldPath} is not an integer type.");
		}
		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
		}
		
		if (values is IEnumerable<int> intValues)
		{
			return intValues.Cast<long>();
		}
		else if (values is IEnumerable<long> longValues)
		{
			return longValues;
		}
		else if (values == null)
		{
			throw new ArgumentNullException(nameof(values), "IEnumerable<> is expected.");
		}
		else if (values is IEnumerable objectValues)
		{
			return RenderToLongArray(cond, objectValues);
		}
		else
		{
			throw new ArgumentException("IEnumerable<> is expected.", nameof(values));
		}
	}
	private static IEnumerable<long> RenderToLongArray(QBCondition cond, IEnumerable objectValues)
	{
		Type type;
		foreach (var objectValue in objectValues)
		{
			if (objectValue == null)
			{
				if (!cond.IsFieldNullable)
				{
					throw new ArgumentNullException(nameof(objectValues), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
				}

				yield return 0L;
			}
			else
			{
				type = objectValue.GetType();
				if (type != cond.FieldType && type != cond.FieldUnderlyingType && !_integerTypes.Contains(type))
				{
					throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {type.ToPretty()}.", nameof(objectValues));
				}

				yield return Convert.ToInt64(objectValue);
			}
		}
	}

	private static BsonDocument MakeBsonConditionOnFields(bool rightIsVar, string command, string leftField, string rightField)
	{
		var bsonValue = new BsonArray();
		bsonValue.Add("$" + leftField);
		bsonValue.Add(rightIsVar ? "$$" + MakeVariableName(rightField) : "$" + rightField);

		return new BsonDocument { { command, bsonValue } };
	}
	private static BsonDocument MakeBsonCondition(bool useExprFormat, string command, string leftField, BsonValue value)
	{
		if (useExprFormat)
		{
			var bsonValue = new BsonArray();
			bsonValue.Add("$" + leftField);
			bsonValue.Add(value);

			return new BsonDocument { { command, bsonValue } };
		}
		else if (command == "$eq")
		{
			return new BsonDocument { { leftField, value } };
		}
		else
		{
			return new BsonDocument { { leftField, new BsonDocument { { command, value } } } };
		}
	}
	private static BsonDocument MakeBsonArrayCondition(bool useExprFormat, string command, string leftField, BsonArray values)
	{
		var countUpTo2 = values.CountUpTo(2);
		if (countUpTo2 <= 0)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			else
			{
				throw new ArgumentException(nameof(values));
			}
		}

		if (countUpTo2 == 1)
		{
			return MakeBsonCondition(useExprFormat, command, leftField, values.First());
		}

		if (useExprFormat)
		{
			BsonArray bsonValue, bsonArray = new BsonArray();
			foreach (var value in values)
			{
				bsonValue = new BsonArray();
				bsonValue.Add("$" + leftField);
				bsonValue.Add(value);
				bsonArray.Add(new BsonDocument { { command, bsonValue } });
			}
			return new BsonDocument { { "$or", bsonArray } };
		}
		else
		{
			var bsonArray = new BsonArray();
			foreach (var value in values)
			{
				bsonArray.Add(new BsonDocument { { leftField, new BsonDocument { { command, value } } } });
			}
			return new BsonDocument { { "$or", bsonArray } };
		}
	}
	private static BsonDocument MakeBsonBetweenCondition(bool useExprFormat, bool betweenOrNotBetween, string leftField, BsonArray values)
	{
		if (useExprFormat)
		{
			BsonArray bsonValue, bsonArray = new BsonArray();

			if (betweenOrNotBetween)
			{
				bsonValue = new BsonArray();
				bsonValue.Add("$" + leftField);
				bsonValue.Add(values[0]);
				bsonArray.Add(new BsonDocument { { "$gte", bsonValue } });

				bsonValue = new BsonArray();
				bsonValue.Add("$" + leftField);
				bsonValue.Add(values[1]);
				bsonArray.Add(new BsonDocument { { "$lte", bsonValue } });

				return new BsonDocument { { "$and", bsonArray } };
			}
			else
			{
				bsonValue = new BsonArray();
				bsonValue.Add("$" + leftField);
				bsonValue.Add(values[0]);
				bsonArray.Add(new BsonDocument { { "$lt", bsonValue } });

				bsonValue = new BsonArray();
				bsonValue.Add("$" + leftField);
				bsonValue.Add(values[1]);
				bsonArray.Add(new BsonDocument { { "$gt", bsonValue } });

				return new BsonDocument { { "$or", bsonArray } };
			}
		}
		else
		{
			var bsonArray = new BsonArray();
			if (betweenOrNotBetween)
			{
				bsonArray.Add(new BsonDocument { { leftField, new BsonDocument { { "$gte", values[0] } } } });
				bsonArray.Add(new BsonDocument { { leftField, new BsonDocument { { "$lte", values[1] } } } });
				return new BsonDocument { { "$and", bsonArray } };
			}
			else
			{
				bsonArray.Add(new BsonDocument { { leftField, new BsonDocument { { "$lt", values[0] } } } });
				bsonArray.Add(new BsonDocument { { leftField, new BsonDocument { { "$gt", values[1] } } } });
				return new BsonDocument { { "$or", bsonArray } };
			}
			
		}
	}

	internal static string MakeVariableName(string name) => name?.ToUnderScoresCase()!.Replace('.', '_')!;

	private static Type? GetEnumerationItemType(object? value)
	{
		if (value is not IEnumerable objectEnumeration)
		{
			return null;
		}
		
		foreach (var item in objectEnumeration)
		{
			if (item == null)
			{
				continue;
			}

			return item.GetType();
		}

		var types = value.GetType().GetInterfacesOf(typeof(IEnumerable<>)).Select(x => x.GetGenericArguments()[0]);
		
		return types.FirstOrDefault(x => x != typeof(object)) ?? types.FirstOrDefault();
	}

	private static bool TryConvertIntegerToOtherInteger(object fromValue, Type toType, [NotNullWhen(true)] ref object? toValue)
	{
		var trueType = fromValue.GetType().GetUnderlyingSystemType();
		if (_integerTypes.Contains(trueType) && _integerTypes.Contains(toType))
		{
			toValue = Convert.ChangeType(fromValue, toType);
			return true;
		}
		return false;
	}
}