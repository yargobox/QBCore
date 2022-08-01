using System.Linq.Expressions;

namespace QBCore.DataSource;

public record DSSortOrder<TProjection>
{
	public readonly Expression<Func<TProjection, object?>> Field;
	public readonly bool Descending;

	public DSSortOrder(Expression<Func<TProjection, object?>> field, bool descending = false)
	{
		Field = field;
		Descending = descending;
	}
}