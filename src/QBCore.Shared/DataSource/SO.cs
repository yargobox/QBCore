namespace QBCore.DataSource;

/// <summary>
/// Sort Order
/// </summary>
[Flags]
public enum SO : uint
{
	/// <summary>
	/// No sort
	/// </summary>
	None = 0,

	/// <summary>
	/// Sort ascending
	/// </summary>
	Ascending = 1,

	/// <summary>
	/// Sort descending
	/// </summary>
	Descending = 2,

	/// <summary>
	/// Sort by searchable text phrase rank
	/// </summary>
	Rank = 4
}