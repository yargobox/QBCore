namespace QBCore.DataSource;

public interface ICDSCondition
{
	string NodeName { get; }
	Type DocumentType { get; }
	string FieldName { get; }
	FO Operation { get; }
	OperandSourceType OperandSourceType { get; }
	string? ParentNodeName { get; }
	Type? ParentDocumentType { get; }
	string? ParentFieldName { get; }
	object? ConstValue { get; }
	object? DefaultValue { get; }
}

public sealed record CDSCondition : ICDSCondition
{
	public string NodeName { get; set; }
	public Type DocumentType { get; set; }
	public string FieldName { get; set; }
	public FO Operation { get; set; }
	public OperandSourceType OperandSourceType { get; set; }
	public string? ParentNodeName { get; set; }
	public Type? ParentDocumentType { get; set; }
	public string? ParentFieldName { get; set; }
	public object? ConstValue { get; set; }
	public object? DefaultValue { get; set; }

	public CDSCondition(string nodeName, Type documentType, string fieldName)
	{
		NodeName = nodeName;
		DocumentType = documentType;
		FieldName = fieldName;
	}
}