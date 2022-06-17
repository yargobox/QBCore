namespace QBCore.DataSource.Builders;

internal sealed class CDSBuilder : ICDSBuilder
{
	public string Name { get; set; } = "[CDS]";
	public ICDSNodeBuilder NodeBuilder { get; }

	public CDSBuilder()
	{
		NodeBuilder = new CDSNodeBuilder(new CDSNode());
	}

	public ICDSBuilder SetName(string name)
	{
		Name = name;
		return this;
	}
}