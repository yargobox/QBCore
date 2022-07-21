using System.Diagnostics;

namespace QBCore.DataSource.QueryBuilder;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record QBField
{
	public readonly FieldPath Field;
	public readonly string? RefAlias;
	public readonly FieldPath? RefField;
	public readonly bool OptionalExclusion;

	public bool IncludeOrExclude => RefAlias != null;

	public QBField(FieldPath Field, string? RefAlias, FieldPath? RefField, bool OptionalExclusion)
	{
		this.Field = Field;
		this.RefAlias = RefAlias;
		this.RefField = RefField;
		this.OptionalExclusion = OptionalExclusion;
	}

	protected string DebuggerDisplay
	{
		get
		{
			return RefAlias != null
				? string.Format("Include {0} as {1}:{2}", Field.FullName, RefAlias, RefField?.FullName)
				: string.Format("{0} {1}", OptionalExclusion ? "Optional" : "Exclude", Field.FullName);
		}
	}
}