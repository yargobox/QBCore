using System.Diagnostics;
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

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal record BuilderCondition
{
	public readonly BuilderConditionFlags Flags;
	public int Parentheses;

	public readonly string Alias;
	public readonly FieldPath Field;
	public readonly Type FieldUnderlyingType;

	public readonly string? RefAlias;
	public readonly FieldPath? RefField;
	public readonly Type? RefFieldUnderlyingType;

	public readonly object? Value;
	public readonly FO Operation;

	public BuilderCondition(
		BuilderConditionFlags flags,
		int parentheses,
		string alias,
		LambdaExpression field,
		string? refAlias,
		LambdaExpression? refField,
		object? value,
		FO operation)
	{
		Flags = flags & (BuilderConditionFlags.OnField | BuilderConditionFlags.OnConst | BuilderConditionFlags.OnParam | BuilderConditionFlags.IsByOr | BuilderConditionFlags.IsConnect);
		Parentheses = parentheses;
		Alias = alias;
		Field = new FieldPath(field, false);
		RefAlias = refAlias;
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

	public string FieldPath => Field.FullName;
	public string? RefFieldPath => RefField?.FullName;

	public Type FieldType => Field.FieldType;
	public Type? RefFieldType => RefField?.FieldType;

	public Type FieldDeclaringType => Field.DeclaringType;
	public Type? RefFieldDeclaringType => RefField?.DeclaringType;

	private string DebuggerDisplay
	{
		get
		{
			var s = RefAlias != null
				? string.Format("{0}:{1} {2} {3}:{4}", Alias, Field.FullName, Operation.ToString(), RefAlias, RefField?.FullName)
				: string.Format("{0}:{1} {2} {3}", Alias, Field.FullName, Operation.ToString(), Value?.ToString());

			if (IsByOr)
			{
				if (Parentheses > 0)
					return string.Concat("OR ", new string('(', Parentheses), s);
				else if (Parentheses == 0)
					return string.Concat("OR ", s);
				else
					return string.Concat("OR ", s, new string(')', -Parentheses));
			}
			else
			{
				if (Parentheses > 0)
					return string.Concat("AND ", new string('(', Parentheses), s);
				else if (Parentheses == 0)
					return string.Concat("AND ", s);
				else
					return string.Concat("AND ", s, new string(')', -Parentheses));
			}
		}
	}
}