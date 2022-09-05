using System.Linq.Expressions;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource;

public record DSSortOrder<TDocument>
{
	public readonly DEPathDefinition<TDocument> Field;
	public readonly SO SortOrder;

	public DSSortOrder(DEPathDefinition<TDocument> field, SO sortOrder = SO.None)
	{
		Field = field;
		SortOrder = sortOrder;
	}
	public DSSortOrder(Expression<Func<TDocument, object?>> field, SO sortOrder = SO.None)
	{
		Field = field.GetPropertyOrFieldPath();
		SortOrder = sortOrder;
	}
}