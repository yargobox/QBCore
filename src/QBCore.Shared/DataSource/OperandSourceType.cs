namespace QBCore.DataSource;

public enum OperandSourceType
{
	None = 0,
	Document = 1,
	ConstValue = 2,
	AppContext = 3,
	ModuleContext = 4,
	UserContext = 5,
	RequestContext = 6
}