using System.Diagnostics;

namespace QBCore.DataSource.QueryBuilder;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record QBField
{
	protected const int OptionalFieldFlag = 1;
	protected const int ExcludedFieldFlag = 2;

	public readonly DEPath Field;
	public readonly string? RefAlias;
	public readonly DEPath? RefField;

	protected readonly int _flags;

	public bool IsOptional => (_flags & OptionalFieldFlag) != 0;
	public bool IsExcluded => (_flags & ExcludedFieldFlag) != 0;

	public QBField(DEPath Field, bool IsOptional = false, bool IsExcluded = false)
	{
		this.Field = Field;
		if (IsExcluded)
		{
			this._flags = ExcludedFieldFlag;
		}
		else if (IsOptional)
		{
			this._flags = OptionalFieldFlag;
		}
	}

	public QBField(DEPath Field, string? RefAlias, DEPath? RefField, bool IsOptional = false, bool IsExcluded = false)
	{
		this.Field = Field;
		this.RefAlias = RefAlias;
		this.RefField = RefField;
		if (IsExcluded)
		{
			this._flags = ExcludedFieldFlag;
		}
		else if (IsOptional)
		{
			this._flags = OptionalFieldFlag;
		}
	}

	protected string DebuggerDisplay
	{
		get
		{
			return RefAlias != null
				? $"{(IsExcluded ? "Excluded" : "Regular")} {(IsOptional ? "Optional" : "")} {Field.Path} as {RefAlias}:{RefField?.Path}"
				: $"{(IsExcluded ? "Excluded" : "Regular")} {Field.Path}";
		}
	}
}