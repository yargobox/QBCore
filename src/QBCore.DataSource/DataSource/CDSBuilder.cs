namespace QBCore.DataSource;

internal sealed class CDSBuilder : ICDSBuilder
{
	public Type ConcreteType { get; }

	public string? Name
	{
		get => _name;
		set
		{
			if (_name != null)
				throw new InvalidOperationException($"Complex datasource '{ConcreteType.ToPretty()}' builder option '{nameof(Name)}' is already set.");
			_name = value;
		}
	}

	public ICDSNodeBuilder NodeBuilder { get; }

	private string? _name;

	public CDSBuilder(Type concreteType)
	{
		ConcreteType = concreteType;
		NodeBuilder = new CDSNodeBuilder(new CDSNode());
	}
}