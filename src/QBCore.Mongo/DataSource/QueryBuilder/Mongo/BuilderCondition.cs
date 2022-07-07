using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

[Flags]
internal enum BuilderConditionFlags
{
	None = 0,
	OnField = 1,
	OnConst = 2,
	OnParam = 4,
	IsByOr = 0x10,
	IsConnect = 0x80
}

internal record BuilderCondition
{
	public readonly BuilderConditionFlags Flags;
	public int Parentheses { get; init; }

	public readonly string Name;
	public readonly FieldPath Field;
	public readonly Type FieldUnderlyingType;

	public readonly string? RefName;
	public readonly FieldPath? RefField;
	public readonly Type? RefFieldUnderlyingType;

	public readonly object? Value;
	public readonly ConditionOperations Operation;

	public BuilderCondition(
		BuilderConditionFlags flags,
		int parentheses,
		string name,
		LambdaExpression field,
		string? refName,
		LambdaExpression? refField,
		object? value,
		ConditionOperations operation)
	{
		Flags = flags & (BuilderConditionFlags.OnField | BuilderConditionFlags.OnConst | BuilderConditionFlags.OnParam | BuilderConditionFlags.IsByOr | BuilderConditionFlags.IsConnect);
		Parentheses = parentheses;
		Name = name;
		Field = new FieldPath(field, false);
		RefName = refName;
		RefField = refField != null ? new FieldPath(refField, true) : null;
		Value = value;
		Operation = operation;

		FieldUnderlyingType = Field.FieldType.GetUnderlyingSystemType();
		RefFieldUnderlyingType = RefField?.FieldType.GetUnderlyingSystemType();
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

	public bool IsFieldNullable => Field.IsNullable;
	public bool? IsRefFieldNullable => RefField?.IsNullable;

	public string FieldPath => Field.Name;
	public string? RefFieldPath => RefField?.Name;

	public Type FieldType => Field.FieldType;
	public Type? RefFieldType => RefField?.FieldType;

	public Type FieldDeclaringType => Field.DeclaringType;
	public Type? RefFieldDeclaringType => RefField?.DeclaringType;
}