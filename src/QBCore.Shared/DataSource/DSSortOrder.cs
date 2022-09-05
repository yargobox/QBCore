using System.Linq.Expressions;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource;

public record DSSortOrder<TDocument>
{
	public readonly DEPathDefinition<TDocument> Field;
	public readonly SO SortOrder;

	public DSSortOrder(DEPathDefinition<TDocument> field, SO sortOrder = SO.Ascending)
	{
		if (field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the sort order.", nameof(field));
		}

		Field = field;
		SortOrder = sortOrder;
	}

	public DSSortOrder(Expression<Func<TDocument, object?>> field, SO sortOrder = SO.Ascending)
	{
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}

		Field = field.GetPropertyOrFieldPath(true);
		if (Field.Count == 0)
		{
			throw new ArgumentException("There is no field specified to apply the sort order.", nameof(field));
		}

		SortOrder = sortOrder;
	}
}