using System.Linq.Expressions;

namespace QBCore.DataSource;

public record DSCondition<TProjection>
{
	public readonly bool IsByOr;
	public readonly int Parentheses;
	public readonly Expression<Func<TProjection, object?>> Field;
	public readonly FO Operation;
	public readonly object? Value;

	public DSCondition(Expression<Func<TProjection, object?>> field, FO operation, object? value)
	{
		Field = field;
		Operation = operation;
		Value = value;
	}
	public DSCondition(bool isByOr, Expression<Func<TProjection, object?>> field, FO operation, object? value)
	{
		IsByOr = isByOr;
		Field = field;
		Operation = operation;
		Value = value;
	}
	public DSCondition(int parentheses, Expression<Func<TProjection, object?>> field, FO operation, object? value)
	{
		Parentheses = parentheses;
		Field = field;
		Operation = operation;
		Value = value;
	}
	public DSCondition(bool isByOr, int parentheses, Expression<Func<TProjection, object?>> field, FO operation, object? value)
	{
		IsByOr = isByOr;
		Parentheses = parentheses;
		Field = field;
		Operation = operation;
		Value = value;
	}
}