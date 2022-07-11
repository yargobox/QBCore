using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.Extensions.Linq.Expressions;
using QBCore.Extensions.Text;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal abstract class QueryBuilder<TDocument, TProjection> : IQueryBuilder<TDocument, TProjection>
{
	public abstract QueryBuilderTypes QueryBuilderType { get; }
	public Type DocumentType => typeof(TDocument);
	public Type ProjectionType => typeof(TProjection);
	public Type DatabaseContextInterface => typeof(IMongoDbContext);
	public object DbContext { get => _dbContext; set => _dbContext = (IMongoDbContext)value; }
	public QBBuilder<TDocument, TProjection> Builder { get; }
	public abstract Origin Source { get; }

	protected IMongoDbContext _dbContext;


	#region Static ReadOnly & Const Properties

	private const ConditionOperations _supportedOperations =
		  ConditionOperations.Equal
		| ConditionOperations.NotEqual
		| ConditionOperations.Greater
		| ConditionOperations.GreaterOrEqual
		| ConditionOperations.Less
		| ConditionOperations.LessOrEqual
		| ConditionOperations.IsNull
		| ConditionOperations.IsNotNull
		| ConditionOperations.In
		| ConditionOperations.NotIn
		| ConditionOperations.Between
		| ConditionOperations.NotBetween
		| ConditionOperations.Like
		| ConditionOperations.NotLike
		| ConditionOperations.BitsAnd
		| ConditionOperations.BitsOr;

	private static readonly HashSet<Type> _integerTypes = new HashSet<Type>()
	{
		typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)
	};

	#endregion

	public QueryBuilder(QBBuilder<TDocument, TProjection> builder)
	{
		Builder = builder;
		_dbContext = null!;
	}

	protected static List<List<BuilderCondition>> SliceConditions(IEnumerable<BuilderCondition> conditions)
	{
		var conds = conditions.ToList();
		var list = new List<List<BuilderCondition>>();

		// Slice the condition to the smalest possible parts (that connected by AND)
		//
		// a                                => a
		// a AND b                          => a, b
		// a OR b                           => a OR b
		// (a OR b) AND c                   => a OR b, c
		// ((a OR b) AND c) AND e           => a OR b, c, e
		// ((a OR b) AND c) OR e            => ((a OR b) AND c) OR e
		//

		var count = conds.Count;
		while (count > 0)
		{
			TrimParentheses(conds);

			for (int i = 0, j = conds[0].Parentheses; i < conds.Count; i++)
			{
				// j == 0 means that this is the end of condition part, such as '(a OR b)' for '(a OR b) AND c'
				j += conds[i].Parentheses;
				if (j <= 0 && (i + 1 >= conds.Count || conds[i + 1].IsByOr == false))
				{
					list.Add(new List<BuilderCondition>(conds.Take(i + 1)));
					conds.RemoveRange(0, i + 1);
					break;
				}
			}

			if (count > conds.Count)
			{
				count = conds.Count;
				continue;
			}
			else
			{
				list.Add(new List<BuilderCondition>(conds));
				conds.Clear();
				break;
			}
		}

		for (int last = 0; last < list.Count; )
		{
			conds = list[last];
			count = conds.Count;

			if (count > 1)
			{
				TrimParentheses(conds);

				for (int i = 0, j = conds[0].Parentheses; i < conds.Count; i++)
				{
					// j == 0 means that this is the end of condition part, such as '(a OR b)' for '(a OR b) AND c'
					j += conds[i].Parentheses;
					if (j <= 0 && (i + 1 >= conds.Count || conds[i + 1].IsByOr == false))
					{
						if (i + 1 < conds.Count)
						{
							list.Insert(last, new List<BuilderCondition>(conds.Take(i + 1)));
							conds.RemoveRange(0, i + 1);
						}
						break;
					}
				}

				if (count == conds.Count)
				{
					last++;
				}
			}
			else
			{
				last++;
			}
		}

		return list;
	}
	private static void TrimParentheses(List<BuilderCondition> conds)
	{
		if (conds.Count == 0) return;
		if (conds.Count == 1)
		{
			System.Diagnostics.Debug.Assert(conds[0].Parentheses == 0);
			if (conds[0].Parentheses != 0)
			{
				conds[0] = conds[0] with { Parentheses = 0 };
			}
			return;
		}

		var first = conds[0].Parentheses;
		System.Diagnostics.Debug.Assert(first >= 0);
		if (first <= 0) return;

		var last = conds.Last().Parentheses;
		System.Diagnostics.Debug.Assert(last <= 0);
		if (last >= 0) return;

		int sum = 0, min = 0;
		for (int i = 1; i < conds.Count - 1; i++)
		{
			sum += conds[i].Parentheses;
			System.Diagnostics.Debug.Assert(sum + first >= 0);
			if (sum + first <= 0) return;
			if (sum < min) min = sum;
		}
		// (((  (a || b) && c                        )))		4 -1(-1) -3
		// (    (a || b) && (b || c)                 )			2  0(-1) -2
		//      (a || b) && (b || c)
		//      ((a || b) && (b || c)) || (d && e)
		// (    ((a || b) && (b || c)) || (d && e)   )			3 -1(-2) -2

		System.Diagnostics.Debug.Assert(first + sum + last == 0);

		conds[0] = conds[0] with { Parentheses =  first + min };
		conds[conds.Count - 1] = conds[conds.Count - 1] with { Parentheses = last + (first + min) };
	}

	#region BuildConditionTree

	protected static BuiltCondition? BuildConditionTree(bool useExprFormat, IEnumerable<BuilderCondition> conditions, Func<string, FieldPath, string> getDBSideName, IReadOnlyDictionary<string, object?>? arguments)
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
						filter = BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, arguments).filter;
					}
					else
					{
						filter = new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, arguments), useExprFormat);
					}
				}
				else if (e.Current.IsByOr)
				{
					if (e.Current.Parentheses > 0)
					{
						filter.AppendByOr(BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, arguments).filter);
					}
					else
					{
						filter.AppendByOr(new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, arguments), useExprFormat));
					}
				}
				else
				{
					if (e.Current.Parentheses > 0)
					{
						filter.AppendByAnd(BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, arguments).filter);
					}
					else
					{
						filter.AppendByAnd(new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, arguments), useExprFormat));
					}
				}
			}

			return filter;
		}
	}
	private static (BuiltCondition filter, int level) BuildConditionTree(bool useExprFormat, IEnumerator<BuilderCondition> e, ref bool moveNext, int parentheses, Func<string, FieldPath, string> getDBSideName, IReadOnlyDictionary<string, object?>? arguments)
	{
		BuiltCondition filter;

		if (parentheses > 0)
		{
			var result = BuildConditionTree(useExprFormat, e, ref moveNext, parentheses - 1, getDBSideName, arguments);
			filter = result.filter;
			if (result.level < 0)
			{
				return (filter, result.level + 1);
			}
		}
		else
		{
			filter = new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, arguments), useExprFormat);
		}

		while (moveNext && (moveNext = e.MoveNext()))
		{
			if (e.Current.IsByOr)
			{
				if (e.Current.Parentheses > 0)
				{
					var result = BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, arguments);
					filter.AppendByOr(result.filter);
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter.AppendByOr(new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, arguments), useExprFormat));
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
					var result = BuildConditionTree(useExprFormat, e, ref moveNext, e.Current.Parentheses - 1, getDBSideName, arguments);
					filter.AppendByAnd(result.filter);
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter.AppendByAnd(new BuiltCondition(BuildCondition(useExprFormat, e.Current, getDBSideName, arguments), useExprFormat));
					if (e.Current.Parentheses < 0)
					{
						return (filter, e.Current.Parentheses + 1);
					}
				}
			}
		}

		return (filter, 0);
	}

	private static BsonDocument BuildCondition(bool useExprFormat, BuilderCondition cond, Func<string, FieldPath, string> getDBSideName, IReadOnlyDictionary<string, object?>? arguments)
	{
		if (cond.IsOnField)
		{
			return MakeConditionOnField(useExprFormat && cond.IsConnect, cond, getDBSideName);
		}
		else if (cond.IsOnParam)
		{
			var paramName = (string)cond.Value!;
			object? value;
			if (arguments == null || !arguments.TryGetValue(paramName, out value))
			{
				throw new InvalidOperationException($"Query builder parameter {paramName} is not set.");
			}

			return MakeConditionOnConst(useExprFormat, cond, getDBSideName, value);
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
	protected static BsonDocument MakeConditionOnConst(bool useExprFormat, BuilderCondition cond, Func<string, FieldPath, string> getDBSideName, object? paramValue = null)
	{
		if (!cond.IsOnParam && !cond.IsOnConst)
		{
			throw new InvalidOperationException(nameof(MakeConditionOnConst) + " can only make conditions between a field and a constant value.");
		}

		var constValue = cond.IsOnParam ? paramValue : cond.Value;
		var leftField = getDBSideName(cond.Name, cond.Field);

		switch (cond.Operation & _supportedOperations)
		{
			case ConditionOperations.Equal:
				{
					var value = RenderToBsonValue(cond, constValue, true);
					return MakeEqBsonCondition(useExprFormat, leftField, value);
				}
			case ConditionOperations.NotEqual:
				{
					var value = RenderToBsonValue(cond, constValue, true);
					return MakeBsonCondition(useExprFormat, "$ne", leftField, value);
				}
			case ConditionOperations.Greater:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$gt", leftField, value);
				}
			case ConditionOperations.GreaterOrEqual:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$gte", leftField, value);
				}
			case ConditionOperations.Less:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$lt", leftField, value);
				}
			case ConditionOperations.LessOrEqual:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$lte", leftField, value);
				}
			case ConditionOperations.IsNull:
				{
					_ = RenderToBsonValue(cond, null, false);
					return MakeEqBsonCondition(useExprFormat, leftField, BsonNull.Value);
				}
			case ConditionOperations.IsNotNull:
				{
					//_ = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$ne", leftField, BsonNull.Value);
				}
			case ConditionOperations.In:
				{
					var value = RenderToBsonArray(cond, constValue, true);
					return MakeBsonCondition(useExprFormat, "$in", leftField, value);
				}
			case ConditionOperations.NotIn:
				{
					var value = RenderToBsonArray(cond, constValue, true);
					return MakeBsonCondition(useExprFormat, "$nin", leftField, value);
				}
			case ConditionOperations.Like:
				{
					var value = RenderToBsonContainsRegex(cond, constValue);
					return MakeEqBsonCondition(useExprFormat, leftField, value);
				}
			case ConditionOperations.NotLike:
				{
					var value = RenderToBsonContainsRegex(cond, constValue);
					return MakeBsonCondition(useExprFormat, "$ne", leftField, value);
				}
			case ConditionOperations.BitsAnd:
				{
					var value = RenderToBsonLong(cond, constValue);
					return MakeBsonCondition(useExprFormat, "$bitsAllSet", leftField, value);
				}
			case ConditionOperations.BitsOr:
				{
					var value = RenderToBsonLong(cond, constValue);
					return MakeBsonCondition(useExprFormat, "$bitsAnySet", leftField, value);
				}
			case ConditionOperations.Between:
			case ConditionOperations.NotBetween:
			default:
				throw new NotSupportedException();
		}
	}

	protected static BsonDocument MakeConditionOnField(bool rightIsVar, BuilderCondition cond, Func<string, FieldPath, string> getDBSideName)
	{
		if (!cond.IsOnField)
		{
			throw new InvalidOperationException(nameof(MakeConditionOnField) + " can only make conditions between fields.");
		}
		if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
		{
			throw new InvalidOperationException("Case-sensitive or case-insensitive operations are not supported for conditions between fields.");
		}

		var leftField = getDBSideName(cond.Name, cond.Field);
		var rightField = getDBSideName(cond.RefName!, cond.RefField!);

		switch (cond.Operation & _supportedOperations)
		{
			case ConditionOperations.Equal: return MakeBsonConditionOnFields(rightIsVar, "$eq", leftField, rightField);
			case ConditionOperations.NotEqual: return MakeBsonConditionOnFields(rightIsVar, "$ne", leftField, rightField);
			case ConditionOperations.Greater: return MakeBsonConditionOnFields(rightIsVar, "$gt", leftField, rightField);
			case ConditionOperations.GreaterOrEqual: return MakeBsonConditionOnFields(rightIsVar, "$gte", leftField, rightField);
			case ConditionOperations.Less: return MakeBsonConditionOnFields(rightIsVar, "$lt", leftField, rightField);
			case ConditionOperations.LessOrEqual: return MakeBsonConditionOnFields(rightIsVar, "$lte", leftField, rightField);
			case ConditionOperations.In: return MakeBsonConditionOnFields(rightIsVar, "$in", leftField, rightField);
			case ConditionOperations.NotIn: return MakeBsonConditionOnFields(rightIsVar, "$nin", leftField, rightField);
			case ConditionOperations.BitsAnd: return MakeBsonConditionOnFields(rightIsVar, "$bitsAllSet", leftField, rightField);
			case ConditionOperations.BitsOr: return MakeBsonConditionOnFields(rightIsVar, "$bitsAnySet", leftField, rightField);

			case ConditionOperations.IsNull:
			case ConditionOperations.IsNotNull:
			case ConditionOperations.Like:
			case ConditionOperations.NotLike:
			case ConditionOperations.Between:
			case ConditionOperations.NotBetween:
			default:
				throw new InvalidOperationException($"An operation such as '{cond.Operation.ToString()}' is not supported for conditions between fields.");
		}
	}

	private static BsonValue RenderToBsonValue(BuilderCondition cond, object? value, bool allowCaseInsensitive)
	{
		if (value == null)
		{
			if (!cond.IsFieldNullable)
			{
				throw new ArgumentNullException(nameof(value), $"Field {cond.Name}.{cond.FieldPath} does not support null values.");
			}

			return BsonNull.Value;
		}

		var type = value.GetType();
		if (type != cond.FieldType && type != cond.FieldUnderlyingType)
		{
			if (!TryConvertIntegerToOtherInteger(value, cond.FieldUnderlyingType, out value))
			{
				throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {value.GetType().ToPretty()}.", nameof(value));
			}
		}
		
		if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
		{
			if (!allowCaseInsensitive)
			{
				throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
			}
			if (value is string stringValue)
			{
				return new BsonRegularExpression(string.Concat("^", Regex.Escape(stringValue.ToLower()), "$"), "i");//!!! localization
			}
			throw new InvalidOperationException($"Such operations can only be performed on strings. Field {cond.Name}.{cond.FieldPath} is not a string type.");
		}

		return new BsonDocumentWrapper(value, BsonSerializer.SerializerRegistry.GetSerializer(cond.FieldType));
	}
	private static BsonValue RenderToBsonContainsRegex(BuilderCondition cond, object? value)
	{
		if (cond.FieldUnderlyingType != typeof(string))
		{
			throw new InvalidOperationException($"Such operations can only be performed on strings. Field {cond.Name}.{cond.FieldPath} is not a string type.");
		}
		if (value == null)
		{
			if (!cond.IsFieldNullable)
			{
				throw new ArgumentNullException(nameof(value), $"Field {cond.Name}.{cond.FieldPath} does not support null values.");
			}

			return BsonNull.Value;
		}
		if (value is string stringValue)
		{
			if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
			{
				return new BsonRegularExpression(Regex.Escape(stringValue.ToLower()), "i");//!!! localization
			}
			else
			{
				return new BsonRegularExpression(Regex.Escape(stringValue));
			}
		}
		throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {value.GetType().ToPretty()}.", nameof(value));
	}
	private static BsonValue RenderToBsonLong(BuilderCondition cond, object? value)
	{
		if (!_integerTypes.Contains(cond.FieldUnderlyingType))
		{
			throw new InvalidOperationException($"Such operations can only be performed on integers. Field {cond.Name}.{cond.FieldPath} is not an integer type.");
		}
		if (value == null)
		{
			if (!cond.IsFieldNullable)
			{
				throw new ArgumentNullException(nameof(value), $"Field {cond.Name}.{cond.FieldPath} does not support null values.");
			}

			return BsonNull.Value;
		}

		var type = value.GetType();
		if (type != cond.FieldType && type != cond.FieldUnderlyingType)
		{
			if (!_integerTypes.Contains(type) && !_integerTypes.Contains(type.GetUnderlyingSystemType()))
			{
				throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {type.ToPretty()}.", nameof(value));
			}
		}

		long longValue = unchecked((long)value);
		return new BsonInt64(longValue);
	}
	private static BsonValue RenderToBsonArray(BuilderCondition cond, object? value, bool allowCaseInsensitive)
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
					throw new ArgumentNullException(nameof(value), $"Field {cond.Name}.{cond.FieldPath} does not support null values.");
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
					throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {obj.GetType().ToPretty()}.", nameof(value));
				}

				if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
				{
					if (!allowCaseInsensitive)
					{
						throw new InvalidOperationException("Such an operation cannot be case sensitive or insensitive.");
					}
					if (obj is string stringValue)
					{
						array.Add(new BsonRegularExpression(string.Concat("^", Regex.Escape(stringValue.ToLower()), "$"), "i"));// !!! localization
					}
					throw new ArgumentException($"Such operations can only be performed on strings. Field {cond.Name}.{cond.FieldPath} is not a string type.", nameof(value));
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