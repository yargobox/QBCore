namespace QBCore.ObjectFactory;

public abstract class OKeyName : IEquatable<OKeyName?>, IEquatable<string?>, IComparable<OKeyName?>, IComparable<string?>
{
	public virtual ReadOnlySpan<char> Tech => GetTech(Key);
	public abstract string Key { get; }

	private static Dictionary<string, Func<string?, OKeyName?>>? _okeyNameFactoryMethodByTech;

	protected OKeyName() { }

	public override string ToString() => Key;
	public override int GetHashCode() => Key.GetHashCode();

	public bool Equals(OKeyName? other) => Key == other?.Key;
	public bool Equals(string? other) => Key == other;
	public override bool Equals(object? obj)
	{
		if (obj is OKeyName modelName)
		{
			return Equals(modelName);
		}
		else if (obj is string str)
		{
			return Equals(str);
		}

		return false;
	}

	public static bool operator ==(OKeyName? a, OKeyName? b)
	{
		if (object.ReferenceEquals(a, null))
		{
			return object.ReferenceEquals(b, null);
		}
		else if (object.ReferenceEquals(b, null))
		{
			return false;
		}

		return a.Equals(b);
	}
	public static bool operator !=(OKeyName? a, OKeyName? b) => !(a == b);

	public int CompareTo(OKeyName? other) => Key.CompareTo(other?.Key);
	public int CompareTo(string? other) => Key.CompareTo(other);

	public static implicit operator string?(OKeyName? okeyName) => okeyName?.Key;
	public static implicit operator OKeyName?(string? okeyName) => FromString(okeyName);

	public static ReadOnlySpan<char> GetTech(string? okeyName)
	{
		if (okeyName == null) return ReadOnlySpan<char>.Empty;

		var i = okeyName.LastIndexOf('@');
		if (i < 0 || i + 1 >= okeyName.Length) return ReadOnlySpan<char>.Empty;

		return okeyName.AsSpan(i + 1);
	}

	public static OKeyName? FromString(string? okeyName)
	{
		if (okeyName == null) return null;

		var tech = GetTech(okeyName).ToString();
		return _okeyNameFactoryMethodByTech?[tech](okeyName);
	}

	protected static void RegisterFactoryMethod(string tech, Func<string?, OKeyName?> factoryMethod)
	{
		if (tech == null) throw new ArgumentNullException(nameof(tech));
		if (tech.Length == 0) throw new ArgumentException(nameof(tech));
		if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));

		Dictionary<string, Func<string?, OKeyName?>>? oldOne, newOne;

		do
		{
			oldOne = _okeyNameFactoryMethodByTech;
			newOne = oldOne == null
				? new Dictionary<string, Func<string?, OKeyName?>>(1)
				: new Dictionary<string, Func<string?, OKeyName?>>(oldOne);

			newOne[tech] = factoryMethod;
		}
		while (Interlocked.CompareExchange(ref _okeyNameFactoryMethodByTech, newOne, oldOne) != oldOne);
	}
}