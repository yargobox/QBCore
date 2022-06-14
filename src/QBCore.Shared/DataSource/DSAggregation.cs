using System.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDSAggregation
{
	Type ProjectionType { get; }
	string FieldName { get; }
	DSAggregationOperations Operation { get; }
	Origin ValueSource { get; }
}

public class DSAggregation<TProjection> : IDSAggregation
{
	public Type ProjectionType => typeof(TProjection);
	public string FieldName { get; }
	public DSAggregationOperations Operation { get; }
	public Origin ValueSource { get; }

	public DSAggregation(Expression<Func<TProjection, object?>> field, DSAggregationOperations operation, Origin valueSource)
	{
		FieldName = null!;//!!!GetMemberName(field);
		Operation = operation;
		ValueSource = valueSource;
	}
}