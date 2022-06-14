namespace QBCore.DataSource.QueryBuilder;

public interface IQBMapper<TDocument, TProjection>
{
	void AutoMap();
	void SetIdMember<TField>(Func<TDocument, TField> memberSelector);
	void AddProperty<TField>(Func<TDocument, TField> memberSelector);
	void AddField<TField>(Func<TDocument, TField> memberSelector);
	void KnownColumn(string columnName, string dbtype, bool isNullable);
}