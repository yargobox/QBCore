namespace QBCore.DataSource.Builders;

public interface ICDSBuilder
{
	string Name { get; set; }
	ICDSNodeBuilder NodeBuilder { get; }

	ICDSBuilder SetName(string name);
}