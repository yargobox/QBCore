namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeIgnoreAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeDataEntryAttribute : Attribute
{
	public string? DBSideName { get; set; }

	public DeDataEntryAttribute() { }
	public DeDataEntryAttribute(string dbSideName) => DBSideName = dbSideName;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeIdAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeReadOnlyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeDependsOnAttribute : Attribute
{
	public string[] DataEntries { get; init; } = Array.Empty<string>();

	public DeDependsOnAttribute(params string[] dataEntries)
	{
		DataEntries = dataEntries;
	}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeForeignIdAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeHiddenAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeNameAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeCreatedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeUpdatedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeModifiedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class DeDeletedAttribute : Attribute
{
}