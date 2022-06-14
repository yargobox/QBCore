namespace QBCore.ObjectFactory;

public sealed class BusinessObject : IEquatable<BusinessObject>, IComparable<BusinessObject>
{
	public string Key { get; }
	public string Tech { get; }
	public string Name { get; }
	public Type Type { get; }

	public BusinessObject(string tech, string name, Type type)
	{
		Tech = tech;
		Name = name;
		Type = type;

		Key = string.Concat(Name, "_", Tech);
	}

	public override string ToString() => Key;
	public override int GetHashCode() => Key.GetHashCode();
	public override bool Equals(object? obj) => Key == (obj as BusinessObject)?.Key;
	public bool Equals(BusinessObject? other) => Key == other?.Key;
	public int CompareTo(BusinessObject? other) => Key.CompareTo(other?.Key);

	public static bool operator== (BusinessObject? a, BusinessObject? b)
	{
		if (BusinessObject.ReferenceEquals(a, null)) return BusinessObject.ReferenceEquals(b, null);
		return a.Equals(b);
	}
	public static bool operator!= (BusinessObject? a, BusinessObject? b) => !(a == b);

	public static string MakeDSKey(string dataSourceName) => dataSourceName + "_DS";
	public static string MakeCDSKey(string complexDataSourceName) => complexDataSourceName + "_CDS";
	public static string MakeModuleKey(string moduleName) => moduleName + "_MODULE";
}