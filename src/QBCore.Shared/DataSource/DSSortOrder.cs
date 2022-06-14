using System.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDSSortOrder
{
	Type ProjectionType { get; }
	string FieldName { get; }
	public bool Descending { get; }
	Origin ValueSource { get; }
}

public class DSSortOrder<TProjection> : IDSSortOrder
{
	public Type ProjectionType => typeof(TProjection);
	public string FieldName { get; }
	public bool Descending { get; }
	public Origin ValueSource { get; }

	public DSSortOrder(Expression<Func<TProjection, object?>> field, bool descending, Origin valueSource)
	{
		FieldName = null!;//!!!GetMemberName(leftOperand);
		Descending = descending;
		ValueSource = valueSource;
	}
}