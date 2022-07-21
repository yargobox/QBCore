namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DsIdAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DsRefAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DsNameAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DsCreatedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DsUpdatedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DsModifiedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class DsDeletedAttribute : Attribute
{
}