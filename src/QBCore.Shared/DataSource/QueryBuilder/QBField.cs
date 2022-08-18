using System.Diagnostics;

namespace QBCore.DataSource.QueryBuilder;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record QBField
{
	public readonly DEPath Field;
	public readonly string? RefAlias;
	public readonly DEPath? RefField;
	public readonly bool OptionalExclusion;

	public bool IncludeOrExclude => RefAlias != null;

	public QBField(DEPath Field, string? RefAlias, DEPath? RefField, bool OptionalExclusion)
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
				? string.Format("Include {0} as {1}:{2}", Field.Path, RefAlias, RefField?.Path)
				: string.Format("{0} {1}", OptionalExclusion ? "Optional" : "Exclude", Field.Path);
		}
	}
}