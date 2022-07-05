using System.Linq.Expressions;
using System.Reflection;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

[Flags]
internal enum BuilderConditionFlags
{
	None = 0,
	OnField = 1,
	OnConst = 2,
	OnParam = 4,
	IsByOr = 0x10,
	IsConnect = 0x80,
	IsNullable = 0x0100,
	IsRefNullable = 0x0200
}

internal record BuilderCondition
{
	public readonly BuilderConditionFlags Flags;
	public int Parentheses { get; init; }

	public readonly string Name;
	public readonly LambdaExpression Field;
	public readonly Type FieldUnderlyingType;

	public readonly string? RefName;
	public readonly LambdaExpression? RefField;
	public readonly Type? RefFieldUnderlyingType;

	public readonly object? Value;
	public readonly ConditionOperations Operation;

	public BuilderCondition(
		BuilderConditionFlags Flags,
		int Parentheses,
		string Name,
		LambdaExpression Field,
		string? RefName,
		LambdaExpression? RefField,
		object? Value,
		ConditionOperations Operation)
	{
		this.Flags = Flags & (BuilderConditionFlags.OnField | BuilderConditionFlags.OnConst | BuilderConditionFlags.OnParam | BuilderConditionFlags.IsByOr | BuilderConditionFlags.IsConnect);
		this.Parentheses = Parentheses;
		this.Name = Name;
		this.Field = Field;
		this.RefName = RefName;
		this.RefField = RefField;
		this.Value = Value;
		this.Operation = Operation;

		var fieldMemberInfo = Field.GetPropertyOrFieldPath(x => GetPropertyOrFieldInfoFromMemberExpression(x)).Last();
		FieldUnderlyingType = fieldMemberInfo.propertyInfo?.PropertyType.GetUnderlyingSystemType()
			?? fieldMemberInfo.fieldInfo?.FieldType.GetUnderlyingSystemType()!;
		if ((fieldMemberInfo.propertyInfo?.IsNullable() ?? fieldMemberInfo.fieldInfo?.IsNullable()) == true)
		{
			this.Flags |= BuilderConditionFlags.IsNullable;
		}

		if (RefField != null)
		{
			var refFieldMemberInfo = RefField.GetPropertyOrFieldPath(x => GetPropertyOrFieldInfoFromMemberExpression(x)).Last();
			RefFieldUnderlyingType = refFieldMemberInfo.propertyInfo?.PropertyType.GetUnderlyingSystemType()
				?? refFieldMemberInfo.fieldInfo?.FieldType.GetUnderlyingSystemType();
			if ((refFieldMemberInfo.propertyInfo?.IsNullable() ?? refFieldMemberInfo.fieldInfo?.IsNullable()) == true)
			{
				this.Flags |= BuilderConditionFlags.IsRefNullable;
			}
		}
	}

	public bool IsConnect => Flags.HasFlag(BuilderConditionFlags.IsConnect);
	public bool IsOnField => Flags.HasFlag(BuilderConditionFlags.OnField);
	public bool IsOnConst => Flags.HasFlag(BuilderConditionFlags.OnConst);
	public bool IsOnParam => Flags.HasFlag(BuilderConditionFlags.OnParam);
	public bool IsConnectOnField => Flags.HasFlag(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnField);
	public BuilderConditionFlags OnWhat => Flags & (BuilderConditionFlags.OnField | BuilderConditionFlags.OnConst | BuilderConditionFlags.OnParam);
	public bool IsByOr
	{
		get => Flags.HasFlag(BuilderConditionFlags.IsByOr);
		init
		{
			if (value) Flags |= BuilderConditionFlags.IsByOr;
			else Flags &= ~BuilderConditionFlags.IsByOr;
		}
	}

	public bool IsFieldNullable => Flags.HasFlag(BuilderConditionFlags.IsNullable);
	public bool IsRefFieldNullable => Flags.HasFlag(BuilderConditionFlags.IsRefNullable);

	public string FieldPath => Field.GetPropertyOrFieldPath();
	public string? RefFieldPath => RefField?.GetPropertyOrFieldPath();

	public Type FieldType => Field.GetPropertyOrFieldInfo((propertyInfo, fieldInfo) => propertyInfo?.PropertyType ?? fieldInfo?.FieldType!);
	public Type? RefFieldType => RefField?.GetPropertyOrFieldInfo((propertyInfo, fieldInfo) => propertyInfo?.PropertyType ?? fieldInfo?.FieldType);

	public Type FieldDeclaringType => Field.GetPropertyOrFieldInfo((propertyInfo, fieldInfo) => propertyInfo?.DeclaringType ?? fieldInfo?.DeclaringType!);
	public Type? RefFieldDeclaringType => RefField?.GetPropertyOrFieldInfo((propertyInfo, fieldInfo) => propertyInfo?.DeclaringType ?? fieldInfo?.DeclaringType);

	static (PropertyInfo? propertyInfo, FieldInfo? fieldInfo) GetPropertyOrFieldInfoFromMemberExpression(MemberExpression memberExpression)
	{
		if (memberExpression.Member is PropertyInfo propertyInfo)
		{
			return (propertyInfo, null);
		}
		else if (memberExpression.Member is FieldInfo fieldInfo)
		{
			return (null, fieldInfo);
		}
		throw new ArgumentException($"Member expression {memberExpression.ToString()} is not a property or field type.");
	}
}