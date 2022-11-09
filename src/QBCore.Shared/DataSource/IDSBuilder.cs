using System.Linq.Expressions;

namespace QBCore.DataSource;

public interface IDSBuilder
{
	public Type ConcreteType { get; }
	string? Name { get; set; }
	string? ControllerName { get; set; }

	DataSourceOptions Options { get; set; }
	IList<Type> Listeners { get; }

	Type? ServiceInterface { get; set; }
	bool? IsServiceSingleton { get; set; }

	bool? BuildAutoController { get; set; }
	
	string? DataContextName { get; set; }

	Type? DataLayer { get; set; }

	Delegate? InsertBuilder { get; set; }
	Delegate? SelectBuilder { get; set; }
	Delegate? UpdateBuilder { get; set; }
	Delegate? DeleteBuilder { get; set; }
	Delegate? SoftDelBuilder { get; set; }
	Delegate? RestoreBuilder { get; set; }
}