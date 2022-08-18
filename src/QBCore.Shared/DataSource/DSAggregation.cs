using System.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDSAggregation
{
	Type ProjectionType { get; }
	string FieldName { get; }
	AggregationOperations Operation { get; }
}

public class DSAggregation<TProjection> : IDSAggregation
{
	public Type ProjectionType => typeof(TProjection);
	public string FieldName { get; }
	public AggregationOperations Operation { get; }

	public DSAggregation(Expression<Func<TProjection, object?>> field, AggregationOperations operation)
	{
		FieldName = null!;//!!!GetMemberName(field);
		Operation = operation;
	}
}