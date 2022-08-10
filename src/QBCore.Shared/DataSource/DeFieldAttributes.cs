namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeIgnoreAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeDataEntryAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeIdAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeForeignIdAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeViewNameAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeCreatedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeUpdatedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeModifiedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeDeletedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DeOrderAttribute : Attribute
{
	public int Order { get; init; }

	public DeOrderAttribute(int order) => Order = order;
}