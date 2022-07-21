namespace QBCore.DataSource.QueryBuilder;

public record QBContainer
{
	public readonly Type DocumentType;
	public readonly string Alias;
	public readonly string DBSideName;
	public readonly ContainerTypes ContainerType;
	public readonly ContainerOperations ContainerOperation;

	public QBContainer(Type DocumentType, string Alias, string DBSideName, ContainerTypes ContainerType, ContainerOperations ContainerOperation)
	{
		this.DocumentType = DocumentType;
		this.Alias = Alias;
		this.DBSideName = DBSideName;
		this.ContainerType = ContainerType;
		this.ContainerOperation = ContainerOperation;
	}
}