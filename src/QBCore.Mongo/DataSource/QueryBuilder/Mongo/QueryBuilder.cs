using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.Extensions.Linq.Expressions;
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

	private static MethodInfo _makeConditionWithConstantValue = typeof(QueryBuilder<TDocument, TProjection>)
		.GetMethod(nameof(MakeConditionWithConstantValue), 1, new Type[] { typeof(BuilderCondition), typeof(object) })
			?? throw new NotImplementedException("Failed to find method " + nameof(MakeConditionWithConstantValue));

	#endregion

	public QueryBuilder(QBBuilder<TDocument, TProjection> builder)
	{
		Builder = builder;
		_dbContext = null!;
	}

	protected static string GetDBSideFieldName(Type documentType, string documentPropertyOrField)
	{
		var bsonClassMap = BsonClassMap.LookupClassMap(documentType);
		return bsonClassMap.GetMemberMap(documentPropertyOrField).ElementName;
	}
	protected static string GetDBSideFieldPath(Type documentType, LambdaExpression documentPropertyOrFieldSelector)
	{
		var path = documentPropertyOrFieldSelector.GetPropertyOrFieldPathAsArray();
		for (int i = 0; i < path.Length; i++)
		{
			path[i] = GetDBSideFieldName(documentType, path[i]);
		}
		return string.Join(".", path);
	}

	protected static BsonDocument MakeConditionWithConstantValue(bool useExprFormat, BuilderCondition cond, object? paramValue = null)
	{
		if (!cond.IsOnParam && !cond.IsOnConst)
		{
			throw new InvalidOperationException(nameof(MakeConditionWithConstantValue) + " can only make conditions with a constant value.");
		}

		var constValue = cond.IsOnParam ? paramValue : cond.Value;
		var name = GetDBSideFieldPath(cond.FieldDeclaringType, cond.Field);

		switch (cond.Operation & _supportedOperations)
		{
			case ConditionOperations.Equal:
				{
					var value = RenderToBsonValue(cond, constValue, true);
					return MakeEqBsonCondition(useExprFormat, name, value);
				}
			case ConditionOperations.NotEqual:
				{
					var value = RenderToBsonValue(cond, constValue, true);
					return MakeBsonCondition(useExprFormat, "$ne", name, value);
				}
			case ConditionOperations.Greater:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$gt", name, value);
				}
			case ConditionOperations.GreaterOrEqual:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$gte", name, value);
				}
			case ConditionOperations.Less:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$lt", name, value);
				}
			case ConditionOperations.LessOrEqual:
				{
					var value = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$lte", name, value);
				}
			case ConditionOperations.IsNull:
				{
					_ = RenderToBsonValue(cond, constValue, false);
					return MakeEqBsonCondition(useExprFormat, name, BsonNull.Value);
				}
			case ConditionOperations.IsNotNull:
				{
					_ = RenderToBsonValue(cond, constValue, false);
					return MakeBsonCondition(useExprFormat, "$ne", name, BsonNull.Value);
				}
			case ConditionOperations.In:
				{
					var value = RenderToBsonArray(cond, constValue, true);
					return MakeBsonCondition(useExprFormat, "$in", name, value);
				}
			case ConditionOperations.NotIn:
				{
					var value = RenderToBsonArray(cond, constValue, true);
					return MakeBsonCondition(useExprFormat, "$nin", name, value);
				}
			case ConditionOperations.Like:
				{
					var value = RenderToBsonContainsRegex(cond, constValue);
					return MakeEqBsonCondition(useExprFormat, name, value);
				}
			case ConditionOperations.NotLike:
				{
					var value = RenderToBsonContainsRegex(cond, constValue);
					return MakeBsonCondition(useExprFormat, "$ne", name, value);
				}
			case ConditionOperations.BitsAnd:
				{
					var value = RenderToBsonLong(cond, constValue);
					return MakeBsonCondition(useExprFormat, "$bitsAllSet", name, value);
				}
			case ConditionOperations.BitsOr:
				{
					var value = RenderToBsonLong(cond, constValue);
					return MakeBsonCondition(useExprFormat, "$bitsAnySet", name, value);
				}
			case ConditionOperations.Between:
			case ConditionOperations.NotBetween:
			default:
				throw new NotSupportedException();
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

	private static BsonDocument MakeBsonCondition(bool useExprFormat, string command, string name, BsonValue value)
	{
		if (useExprFormat)
		{
			var bsonValue = new BsonArray();
			bsonValue.Add("$" + name);
			bsonValue.Add(value);

			return new BsonDocument { { command, bsonValue } };
		}
		else
		{
			return new BsonDocument { { name, new BsonDocument { { command, value } } } };
		}
	}
	private static BsonDocument MakeEqBsonCondition(bool useExprFormat, string name, BsonValue value)
	{
		if (useExprFormat)
		{
			var bsonValue = new BsonArray();
			bsonValue.Add("$" + name);
			bsonValue.Add(value);

			return new BsonDocument { { "$eq", bsonValue } };
		}
		else
		{
			return new BsonDocument { { name, value } };
		}
	}

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

	//!!! don't forget to remove this
	protected static BsonDocument MakeConditionWithConstantValue<TLocal>(BuilderCondition cond, object? paramValue = null)
	{
		if (!cond.IsOnParam && !cond.IsOnConst)
		{
			throw new InvalidOperationException(nameof(MakeConditionWithConstantValue) + " can only make conditions with a constant value.");
		}

		FilterDefinition<TLocal> filterDefinition;
		object? constValue = cond.IsOnParam ? paramValue : cond.Value;

		switch (cond.Operation & _supportedOperations)
		{
			case ConditionOperations.Equal:
				{
					if (constValue == null)
					{
						if (!cond.IsFieldNullable)
							throw new ArgumentNullException($"Field {cond.Name}.{cond.FieldPath} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotImplementedException();
					}
					filterDefinition = Builders<TLocal>.Filter.Eq(cond.FieldPath, constValue);
					break;
				}
			case ConditionOperations.NotEqual:
				{
					if (constValue == null)
					{
						if (!cond.IsFieldNullable)
							throw new ArgumentNullException($"Field {cond.Name}.{cond.FieldPath} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotImplementedException();
					}
					filterDefinition = Builders<TLocal>.Filter.Ne(cond.FieldPath, constValue);
					break;
				}
			case ConditionOperations.Greater:
				{
					if (constValue == null)
					{
						if (!cond.IsFieldNullable)
							throw new ArgumentNullException($"Field {cond.Name}.{cond.FieldPath} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.Greater)} operator cannot be case insensitive.");
					}
					filterDefinition = Builders<TLocal>.Filter.Gt(cond.FieldPath, constValue);
					break;
				}
			case ConditionOperations.GreaterOrEqual:
				{
					if (constValue == null)
					{
						if (!cond.IsFieldNullable)
							throw new ArgumentNullException($"Field {cond.Name}.{cond.FieldPath} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.GreaterOrEqual)} operator cannot be case insensitive.");
					}
					filterDefinition = Builders<TLocal>.Filter.Gte(cond.FieldPath, constValue);
					break;
				}
			case ConditionOperations.Less:
				{
					if (constValue == null)
					{
						if (!cond.IsFieldNullable)
							throw new ArgumentNullException($"Field {cond.Name}.{cond.FieldPath} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.Less)} operator cannot be case insensitive.");
					}
					filterDefinition = Builders<TLocal>.Filter.Lt(cond.FieldPath, constValue);
					break;
				}
			case ConditionOperations.LessOrEqual:
				{
					if (constValue == null)
					{
						if (!cond.IsFieldNullable)
							throw new ArgumentNullException($"Field {cond.Name}.{cond.FieldPath} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.LessOrEqual)} operator cannot be case insensitive.");
					}
					filterDefinition = Builders<TLocal>.Filter.Lt(cond.FieldPath, constValue);
					break;
				}
			case ConditionOperations.IsNull:
				{
					if (constValue == null)
					{
						if (!cond.IsFieldNullable)
							throw new ArgumentNullException($"Field {cond.Name}.{cond.FieldPath} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.IsNull)} operator cannot be case insensitive.");
					}
					filterDefinition = Builders<TLocal>.Filter.Eq(cond.FieldPath, (object?)null);
					break;
				}
			case ConditionOperations.IsNotNull:
				{
					if (constValue == null)
					{
						if (!cond.IsFieldNullable)
							throw new ArgumentNullException($"Field {cond.Name}.{cond.FieldPath} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.IsNotNull)} operator cannot be case insensitive.");
					}
					filterDefinition = Builders<TLocal>.Filter.Lt(cond.FieldPath, constValue);
					break;
				}
			case ConditionOperations.In:
				{
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.In)} operator cannot be case insensitive.");
					}
					filterDefinition = Builders<TLocal>.Filter.In(cond.FieldPath, constValue as IEnumerable<object?>);
					break;
				}
			case ConditionOperations.NotIn:
				{
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.NotIn)} operator cannot be case insensitive.");
					}
					filterDefinition = Builders<TLocal>.Filter.Nin(cond.FieldPath, constValue as IEnumerable<object?>);
					break;
				}
			case ConditionOperations.Like:
				{
					var stringValue = constValue as string;

					if (stringValue == null)
					{
						filterDefinition = Builders<TLocal>.Filter.Eq(cond.FieldPath, (string?)null);
					}
					else 
					{
						if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
						{
							stringValue = stringValue.ToLower();
						}
						stringValue = Regex.Escape(stringValue);

						filterDefinition = Builders<TLocal>.Filter.Eq(cond.FieldPath, new BsonRegularExpression(stringValue, "i"));
					}
					break;
				}
			case ConditionOperations.NotLike:
				{
					var stringValue = constValue as string;

					if (stringValue == null)
					{
						filterDefinition = Builders<TLocal>.Filter.Ne(cond.FieldPath, (string?)null);
					}
					else 
					{
						if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
						{
							stringValue = stringValue.ToLower();
						}
						stringValue = Regex.Escape(stringValue);

						filterDefinition = Builders<TLocal>.Filter.Ne(cond.FieldPath, new BsonRegularExpression(stringValue, "i"));
					}
					break;
				}
			case ConditionOperations.BitsAnd:
				{
					if (constValue == null)
					{
						throw new ArgumentNullException($"{cond.Name}.{cond.FieldPath}", $"Operator {nameof(ConditionOperations.BitsAnd)} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.BitsAnd)} operator cannot be case insensitive.");
					}
					var longValue = unchecked((long)constValue);

					filterDefinition = Builders<TLocal>.Filter.BitsAllSet(cond.FieldPath, longValue);
					break;
				}
			case ConditionOperations.BitsOr:
				{
					if (constValue == null)
					{
						throw new ArgumentNullException($"{cond.Name}.{cond.FieldPath}", $"Operator {nameof(ConditionOperations.BitsOr)} does not support null values.");
					}
					else if (constValue.GetType() != cond.FieldType && constValue.GetType() != cond.FieldUnderlyingType)
					{
						if (!TryConvertIntegerToOtherInteger(constValue, cond.FieldUnderlyingType, out constValue))
							throw new ArgumentException($"Field {cond.Name}.{cond.FieldPath} has type {cond.FieldType.ToPretty()} not {constValue.GetType().ToPretty()}.");
					}
					if (cond.Operation.HasFlag(ConditionOperations.CaseInsensitive))
					{
						throw new NotSupportedException($"The {nameof(ConditionOperations.BitsOr)} operator cannot be case insensitive.");
					}
					var longValue = unchecked((long)constValue);

					filterDefinition = Builders<TLocal>.Filter.BitsAnySet(cond.FieldPath, longValue);
					break;
				}
			case ConditionOperations.Between:
			case ConditionOperations.NotBetween:
			default:
				throw new NotSupportedException();
		}

		return filterDefinition.Render(BsonSerializer.SerializerRegistry.GetSerializer<TLocal>(), BsonSerializer.SerializerRegistry);
	}
}