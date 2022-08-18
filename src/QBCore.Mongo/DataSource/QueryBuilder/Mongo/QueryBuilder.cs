using System.Collections;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
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
		var leftField = getDBSideName(cond.Alias, cond.Field);

		switch (cond.Operation & _supportedOperations)
		{
			case FO.Equal:
				{
					var value = RenderToBsonValue(cond, constValue, true);
					return MakeEqBsonCondition(useExprFormat, leftField, value);
				}
			case FO.NotEqual:
				{
					var value = RenderToBsonValue(cond, constValue, true);
					return MakeBsonCondition(useExprFormat, "$ne", leftField, value);
				}
			case FO.Greater:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$gt", leftField, value);
				}
			case FO.GreaterOrEqual:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$gte", leftField, value);
				}
			case FO.Less:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$lt", leftField, value);
				}
			case FO.LessOrEqual:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$lte", leftField, value);
				}
			case FO.IsNull:
				{
					_ = RenderToBsonValue(cond, null, false);
					return MakeEqBsonCondition(useExprFormat, leftField, BsonNull.Value);
				}
			case FO.IsNotNull:
				{
					//_ = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$ne", leftField, BsonNull.Value);
				}
			case FO.In:
				{
					var value = RenderToBsonArray(cond, constValue, true);
					return MakeBsonCondition(useExprFormat, "$in", leftField, value);
				}
			case FO.NotIn:
				{
					var value = RenderToBsonArray(cond, constValue, true);
					return MakeBsonCondition(useExprFormat, "$nin", leftField, value);
				}
			case FO.Like:
				{
					var value = RenderToBsonContainsRegex(cond, constValue);
					return MakeEqBsonCondition(useExprFormat, leftField, value);
				}
			case FO.NotLike:
				{
					var value = RenderToBsonContainsRegex(cond, constValue);
					return MakeBsonCondition(useExprFormat, "$ne", leftField, value);
				}
			case FO.BitsAnd:
				{
					var value = RenderToBsonLong(cond, constValue);
					return MakeBsonCondition(useExprFormat, "$bitsAllSet", leftField, value);
				}
			case FO.BitsOr:
				{
					var value = RenderToBsonLong(cond, constValue);
					return MakeBsonCondition(useExprFormat, "$bitsAnySet", leftField, value);
				}
			case FO.Between:
			case FO.NotBetween:
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
			if (!TryConvertIntegerToOtherInteger(value, cond.FieldUnderlyingType, out value))
			{
				throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {value.GetType().ToPretty()}.", nameof(value));
			}
		}
		
		if (cond.Operation.HasFlag(FO.CaseInsensitive))
		{
			if (!allowCaseInsensitive)
			{
				throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
			}
			if (value is string stringValue)
			{
				return new BsonRegularExpression(string.Concat("^", Regex.Escape(stringValue.ToLower()), "$"), "i");//!!! localization
			}
			throw new InvalidOperationException($"Such operations can only be performed on strings. Field {cond.Alias}.{cond.FieldPath} is not a string type.");
		}

		return new BsonDocumentWrapper(value, BsonSerializer.SerializerRegistry.GetSerializer(cond.FieldType));
	}
	private static BsonValue RenderToBsonContainsRegex(QBCondition cond, object? value)
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
		if (value is string stringValue)
		{
			if (cond.Operation.HasFlag(FO.CaseInsensitive))
			{
				return new BsonRegularExpression(Regex.Escape(stringValue.ToLower()), "i");//!!! localization
			}
			else
			{
				return new BsonRegularExpression(Regex.Escape(stringValue));
			}
		}
		throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {value.GetType().ToPretty()}.", nameof(value));
	}
	private static BsonValue RenderToBsonLong(QBCondition cond, object? value)
	{
		if (!_integerTypes.Contains(cond.FieldUnderlyingType))
		{
			throw new InvalidOperationException($"Such operations can only be performed on integers. Field {cond.Alias}.{cond.FieldPath} is not an integer type.");
		}
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
			if (!_integerTypes.Contains(type) && !_integerTypes.Contains(type.GetUnderlyingSystemType()))
			{
				throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {type.ToPretty()}.", nameof(value));
			}
		}

		long longValue = unchecked((long)value);
		return new BsonInt64(longValue);
	}
	private static BsonValue RenderToBsonArray(QBCondition cond, object? value, bool allowCaseInsensitive)
	{
		if (value == null)
		{
			throw new ArgumentNullException(nameof(value), "IEnumerable<> is expected.");
		}

		var array = new BsonArray();
		IBsonSerializer? serializer = null;

		foreach (var obj in (IEnumerable)value)
		{
			if (obj == null)
			{
				if (!cond.IsFieldNullable)
				{
					throw new ArgumentNullException(nameof(value), $"Field {cond.Alias}.{cond.FieldPath} does not support null values.");
				}
				
				array.Add(BsonNull.Value);
			}
			else
			{
				if (obj.GetType() != cond.FieldType && obj.GetType() != cond.FieldUnderlyingType)
				{
					if (!allowCaseInsensitive)
					{
						object convertedValue;
						if (TryConvertIntegerToOtherInteger(obj, cond.FieldUnderlyingType, out convertedValue))
						{
							array.Add(new BsonDocumentWrapper(obj, serializer ?? (serializer = BsonSerializer.SerializerRegistry.GetSerializer(cond.FieldType))));
						}
					}
					throw new ArgumentException($"Field {cond.Alias}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {obj.GetType().ToPretty()}.", nameof(value));
				}

				if (cond.Operation.HasFlag(FO.CaseInsensitive))
				{
					if (!allowCaseInsensitive)
					{
						throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
					}
					if (obj is string stringValue)
					{
						array.Add(new BsonRegularExpression(string.Concat("^", Regex.Escape(stringValue.ToLower()), "$"), "i"));// !!! localization
					}
					throw new ArgumentException($"Such operations can only be performed on strings. Field {cond.Alias}.{cond.FieldPath} is not a string type.", nameof(value));
				}
				else
				{
					array.Add(new BsonDocumentWrapper(obj, serializer ?? (serializer = BsonSerializer.SerializerRegistry.GetSerializer(cond.FieldType))));
				}
			}
		}

		return array;
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
		else
		{
			return new BsonDocument { { leftField, new BsonDocument { { command, value } } } };
		}
	}
	private static BsonDocument MakeEqBsonCondition(bool useExprFormat, string leftField, BsonValue value)
	{
		if (useExprFormat)
		{
			var bsonValue = new BsonArray();
			bsonValue.Add("$" + leftField);
			bsonValue.Add(value);

			return new BsonDocument { { "$eq", bsonValue } };
		}
		else
		{
			return new BsonDocument { { leftField, value } };
		}
	}

	internal static string MakeVariableName(string name) => name?.ToUnderScoresCase()!.Replace('.', '_')!;

	private static bool TryConvertIntegerToOtherInteger(object fromValue, Type toType, out object toValue)
	{
		var trueType = fromValue.GetType().GetUnderlyingSystemType();
		if (_integerTypes.Contains(trueType) && _integerTypes.Contains(toType))
		{
			toValue = Convert.ChangeType(fromValue, toType);
			return true;
		}
		else
		{
			toValue = null!;
		}
		return false;
	}
}