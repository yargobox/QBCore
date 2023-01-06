using System.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDSAggregation
{
	Type DtoType { get; }
	string FieldName { get; }
	AggregationOperations Operation { get; }
}

public class DSAggregation<TDto> : IDSAggregation
{
	public Type DtoType => typeof(TDto);
	public string FieldName { get; }
	public AggregationOperations Operation { get; }

	public DSAggregation(Expression<Func<TDto, object?>> field, AggregationOperations operation)
	{
		FieldName = null!;//!!!GetMemberName(field);
		Operation = operation;
	}
}