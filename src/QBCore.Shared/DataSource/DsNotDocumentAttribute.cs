namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class DsNotDocumentAttribute : Attribute { }