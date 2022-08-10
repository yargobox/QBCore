using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder;

public interface IDSIdGenerator
{
	/// <summary>
	/// Generate a new Id
	/// </summary>
	object GenerateId(object container, object document, DataSourceIdGeneratorOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));

	/// <summary>
	/// Generate a new Id asynchronously
	/// </summary>
	Task<object> GenerateIdAsync(object container, object document, DataSourceIdGeneratorOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));

	/// <summary>
	/// Is id empty?
	/// </summary>
	bool IsEmpty(object? id);

	/// <summary>
	/// The maximum number of attempts to insert with a new regenerated Id if it fails with DuplicateKey state on Id.
	/// This property is used in an optimistic approach to Id generation, where the generator tries the last known Id
	/// plus a step, or in a pesemistic approach, where the generator obtains the last Id from the collection but does not
	/// block it.
	/// </summary>
	int MaxAttempts { get; }
}