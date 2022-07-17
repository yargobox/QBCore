using System.Diagnostics;

namespace QBCore.DataSource.QueryBuilder.Mongo;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal record BuilderField
(
	FieldPath Field,
	string? RefAlias,
	FieldPath? RefField,
	bool OptionalExclusion
)
{
	public bool IncludeOrExclude => RefAlias != null;

	private string DebuggerDisplay
	{
		get
		{
			return RefAlias != null
				? string.Format("Include {0} as {1}:{2}", Field.FullName, RefAlias, RefField?.FullName)
				: string.Format("{0} {1}", OptionalExclusion ? "Optional" : "Exclude", Field.FullName);
		}
	}
}