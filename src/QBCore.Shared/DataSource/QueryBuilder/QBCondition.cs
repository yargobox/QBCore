using System.Diagnostics;

namespace QBCore.DataSource.QueryBuilder;

[Flags]
public enum QBConditionFlags
{
	None = 0,
	OnField = 1,
	OnConst = 2,
	OnParam = 4,
	IsByOr = 0x10,
	IsConnect = 0x80
}

public record QBConditionInfo
{
	public readonly QBConditionFlags Flags;

	public readonly string Alias;
	public readonly DEPath Field;
	public readonly Type FieldUnderlyingType;

	public readonly string? RefAlias;
	public readonly DEPath? RefField;
	public readonly Type? RefFieldUnderlyingType;

	public readonly object? Value;
	public readonly FO Operation;

	public QBConditionInfo(
		QBConditionFlags flags,
		string alias,
		DEPath field,
		string? refAlias,
		DEPath? refField,
		object? value,
		FO operation)
	{
		Flags = flags & (QBConditionFlags.OnField | QBConditionFlags.OnConst | QBConditionFlags.OnParam | QBConditionFlags.IsByOr | QBConditionFlags.IsConnect);
		Alias = alias;
		Field = field;
		RefAlias = refAlias;
		RefField = refField;
		Value = value;
		Operation = operation;

		FieldUnderlyingType = Field.DataEntryType.GetUnderlyingType();
		RefFieldUnderlyingType = RefField?.DataEntryType.GetUnderlyingType();
	}
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record QBCondition
{
	public readonly QBConditionInfo ConditionInfo;
	public int Parentheses;

	public string Alias => ConditionInfo.Alias;
	public DEPath Field => ConditionInfo.Field;
	public Type FieldUnderlyingType => ConditionInfo.FieldUnderlyingType;

	public string? RefAlias => ConditionInfo.RefAlias;
	public DEPath? RefField => ConditionInfo.RefField;
	public Type? RefFieldUnderlyingType => ConditionInfo.RefFieldUnderlyingType;

	public object? Value => ConditionInfo.Value;
	public FO Operation => ConditionInfo.Operation;

	public bool IsConnect => ConditionInfo.Flags.HasFlag(QBConditionFlags.IsConnect);
	public bool IsOnField => ConditionInfo.Flags.HasFlag(QBConditionFlags.OnField);
	public bool IsOnConst => ConditionInfo.Flags.HasFlag(QBConditionFlags.OnConst);
	public bool IsOnParam => ConditionInfo.Flags.HasFlag(QBConditionFlags.OnParam);
	public bool IsConnectOnField => ConditionInfo.Flags.HasFlag(QBConditionFlags.IsConnect | QBConditionFlags.OnField);
	public bool IsByOr => ConditionInfo.Flags.HasFlag(QBConditionFlags.IsByOr);

	public bool IsFieldNullable => Field.IsNullable;
	public bool? IsRefFieldNullable => RefField?.IsNullable;

	public string FieldPath => Field.Path;
	public string? RefFieldPath => RefField?.Path;

	public Type FieldType => Field.DataEntryType;
	public Type? RefFieldType => RefField?.DataEntryType;

	public QBCondition(QBConditionInfo conditionInfo)
		=> ConditionInfo = conditionInfo;
	public QBCondition(QBConditionFlags flags, string alias, DEPath field, string? refAlias, DEPath? refField, object? value, FO operation)
		=> ConditionInfo = new QBConditionInfo(flags, alias, field, refAlias, refField, value, operation);

	protected string DebuggerDisplay
	{
		get
		{
			var s = RefAlias != null
				? string.Format("{0}:{1} {2} {3}:{4}", Alias, Field.Path, Operation.ToString(), RefAlias, RefField?.Path)
				: string.Format("{0}:{1} {2} {3}", Alias, Field.Path, Operation.ToString(), Value?.ToString());

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