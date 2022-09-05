using System.Linq.Expressions;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource;

public record DSCondition<TDocument>
{
	public readonly bool IsByOr;
	public readonly int Parentheses;
	public readonly DEPathDefinition<TDocument> Field;
	public readonly FO Operation;
	public readonly object? Value;

	public DSCondition(Expression<Func<TDocument, object?>> field, FO operation, object? value)
	{
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}

		Field = field.GetPropertyOrFieldPath(true);
		if (Field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the condition.", nameof(field));
		}

		Operation = operation;
		Value = value;
	}

	public DSCondition(DEPathDefinition<TDocument> field, FO operation, object? value)
	{
		if (field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the condition.", nameof(field));
		}

		Field = field;
		Operation = operation;
		Value = value;
	}

	public DSCondition(bool isByOr, Expression<Func<TDocument, object?>> field, FO operation, object? value)
	{
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}

		IsByOr = isByOr;

		Field = field.GetPropertyOrFieldPath(true);
		if (Field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the condition.", nameof(field));
		}

		Operation = operation;
		Value = value;
	}

	public DSCondition(bool isByOr, DEPathDefinition<TDocument> field, FO operation, object? value)
	{
		if (field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the condition.", nameof(field));
		}

		IsByOr = isByOr;
		Field = field;
		Operation = operation;
		Value = value;
	}

	public DSCondition(int parentheses, Expression<Func<TDocument, object?>> field, FO operation, object? value)
	{
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}

		Parentheses = parentheses;

		Field = field.GetPropertyOrFieldPath(true);
		if (Field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the condition.", nameof(field));
		}

		Operation = operation;
		Value = value;
	}

	public DSCondition(int parentheses, DEPathDefinition<TDocument> field, FO operation, object? value)
	{
		if (field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the condition.", nameof(field));
		}

		Parentheses = parentheses;
		Field = field;
		Operation = operation;
		Value = value;
	}

	public DSCondition(bool isByOr, int parentheses, Expression<Func<TDocument, object?>> field, FO operation, object? value)
	{
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}

		IsByOr = isByOr;
		Parentheses = parentheses;
		
		Field = field.GetPropertyOrFieldPath(true);
		if (Field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the condition.", nameof(field));
		}

		Operation = operation;
		Value = value;
	}

	public DSCondition(bool isByOr, int parentheses, DEPathDefinition<TDocument> field, FO operation, object? value)
	{
		if (field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the condition.", nameof(field));
		}

		IsByOr = isByOr;
		Parentheses = parentheses;
		Field = field;
		Operation = operation;
		Value = value;
	}
}