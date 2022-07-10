using System.Diagnostics;

namespace QBCore.DataSource.QueryBuilder.Mongo;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal record BuilderField
(
	FieldPath Field,
	string? RefName,
	FieldPath? RefField,
	bool OptionalExclusion
)
{
	public bool IncludeOrExclude => RefName != null;

	private string DebuggerDisplay
	{
		get
		{
			return RefName != null
				? string.Format("Include {0} as {1}:{2}", Field.FullName, RefName, RefField?.FullName)
				: string.Format("{0} {1}", OptionalExclusion ? "Optional" : "Exclude", Field.FullName);
		}
	}
}