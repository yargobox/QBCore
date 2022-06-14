namespace QBCore.DataSource.QueryBuilder;

internal sealed class QBMapper<TDocument, TProjection> : IQBMapper<TDocument, TProjection>, ICloneable
{
	public QBMapper() { }
	public QBMapper(QBMapper<TDocument, TProjection> other)
	{
		//!!!
	}
	public object Clone()
	{
		return new QBMapper<TDocument, TProjection>(this);
	}

	// context
	public List<KeyValuePair<string, string>> Fields = new List<KeyValuePair<string, string>>();

	public void AddField<TField>(Func<TDocument, TField> memberSelector)
	{
		//throw new NotImplementedException();
	}

	public void AddProperty<TField>(Func<TDocument, TField> memberSelector)
	{
		//throw new NotImplementedException();
	}

	public void AutoMap()
	{
		//throw new NotImplementedException();
	}

	public void KnownColumn(string columnName, string dbtype, bool isNullable)
	{
		//throw new NotImplementedException();
	}

	public void SetIdMember<TField>(Func<TDocument, TField> memberSelector)
	{
		//throw new NotImplementedException();
	}
}