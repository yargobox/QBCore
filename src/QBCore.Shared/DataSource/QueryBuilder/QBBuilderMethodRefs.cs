using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

public sealed class QBBuilderMethodRefs
{
	public Delegate? InsertBuilder { get; init; }
	public Delegate? SelectBuilder { get; init; }
	public Delegate? UpdateBuilder { get; init; }
	public Delegate? DeleteBuilder { get; init; }
	public Delegate? SoftDelBuilder { get; init; }
	public Delegate? RestoreBuilder { get; init; }
}