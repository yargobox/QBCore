namespace QBCore.DataSource.QueryBuilder.Mongo;

public class BsonCollectionAttribute : Attribute
{
	public string Name { get; init; }

	public BsonCollectionAttribute(string Name)
	{
		if (Name == null)
		{
			throw new ArgumentNullException(nameof(Name));
		}
		if (string.IsNullOrWhiteSpace(Name))
		{
			throw new ArgumentException(nameof(Name));
		}

		this.Name = Name;
	}
}