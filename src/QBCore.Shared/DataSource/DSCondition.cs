using System.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDSCondition
{
	Type ProjectionType { get; }
	string FieldName { get; }
	ConditionOperations Operation { get; }
	object? Value { get; }
	Origin ValueSource { get; }
}

public class DSCondition<TProjection> : IDSCondition
{
	public Type ProjectionType => typeof(TProjection);
	public string FieldName { get; }
	public ConditionOperations Operation { get; }
	public object? Value { get; }
	public Origin ValueSource  { get; }

	public DSCondition(Expression<Func<TProjection, object?>> field, ConditionOperations operation, object? value, Origin valueSource)
	{
		FieldName = null!;//!!!GetMemberName(field);
		Operation = operation;
		Value = value;
		ValueSource = valueSource;
	}
}