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

	public string? ControllerName
	{
		get => _webName;
		set
		{
			if (_webName != null)
				throw new InvalidOperationException($"Complex datasource '{ConcreteType.ToPretty()}' builder option '{nameof(ControllerName)}' is already set.");
			_webName = value;
		}
	}

	public ICDSNodeBuilder NodeBuilder { get; }

	private string? _name;
	private string? _webName;

	public CDSBuilder(Type concreteType)
	{
		ConcreteType = concreteType;
		NodeBuilder = new CDSNodeBuilder(new CDSNodeInfo());
	}
}