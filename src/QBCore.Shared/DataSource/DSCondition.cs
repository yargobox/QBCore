using System.Linq.Expressions;
using QBCore.Extensions.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDSCondition
{
	Type ProjectionType { get; }
	string FieldName { get; }
	FO Operation { get; }
	object? Value { get; }
	Origin ValueSource { get; }
}

public class DSCondition<TProjection> : IDSCondition
{
	public Type ProjectionType => typeof(TProjection);
	public string FieldName { get; }
	public FO Operation { get; }
	public object? Value { get; }
	public Origin ValueSource  { get; }

	public DSCondition(Expression<Func<TProjection, object?>> field, FO operation, object? value, Origin valueSource)
	{
		FieldName = field.GetMemberName();
		Operation = operation;
		Value = value;
		ValueSource = valueSource;
	}
}