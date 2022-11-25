using QBCore.ObjectFactory;

namespace QBCore.DataSource;

/// <summary>
/// Implements a datasource naming model of QBCore application.
/// </summary>
/// <remarks>
/// Examples of full datasource names:<para />
/// "Order@DS"<para />
/// "FLT:Order:BrandId:Brand@DS"<para />
/// "CLL:Order:BrandId:Brand@DS"<para />
/// "Sales:positions@DS"<para />
/// "FLT:Sales:positions:ProductId:Product@DS"<para />
/// "CLL:Sales:positions:ProductId:Product@DS"<para />
/// </remarks>
public sealed class DSKeyName : OKeyName/* , IEquatable<DSKeyName>, IEquatable<string> */
{
	public override ReadOnlySpan<char> Tech => "DS";
	public override string Key => _key;

	public bool IsForFilter => ForField != null && _isForFilterOrForCard;
	public bool IsForCard => ForField != null && !_isForFilterOrForCard;
	private readonly bool _isForFilterOrForCard;
	public readonly string? CDSName;
	public readonly string? ForDSOrNodeName;
	public readonly string? ForField;
	public readonly string DSOrNodeName;
	private readonly string _key;

	static DSKeyName()
	{
		RegisterFactoryMethod("DS", okeyName => okeyName != null ? new DSKeyName(okeyName, false) : null);
	}

	public DSKeyName(string dsName)
	{
		if (dsName == null) throw new ArgumentNullException(nameof(dsName));
		if (dsName.Length == 0) throw new ArgumentException(nameof(dsName));

		DSOrNodeName = dsName;
		_key = MakeKey(this);
	}
	public DSKeyName(string cdsName, string nodeName)
	{
		if (cdsName == null) throw new ArgumentNullException(nameof(cdsName));
		if (cdsName.Length == 0) throw new ArgumentException(nameof(cdsName));
		if (nodeName == null) throw new ArgumentNullException(nameof(nodeName));
		if (nodeName.Length == 0) throw new ArgumentException(nameof(nodeName));

		CDSName = cdsName;
		DSOrNodeName = nodeName;
		_key = MakeKey(this);
	}
	public DSKeyName(bool isForFilterOrForCard, string forDSName, string forField, string dsName)
	{
		if (dsName == null) throw new ArgumentNullException(nameof(dsName));
		if (dsName.Length == 0) throw new ArgumentException(nameof(dsName));
		if (forDSName == null) throw new ArgumentNullException(nameof(forDSName));
		if (forDSName.Length == 0) throw new ArgumentException(nameof(forDSName));
		if (forField == null) throw new ArgumentNullException(nameof(forField));
		if (forField.Length == 0) throw new ArgumentException(nameof(forField));

		_isForFilterOrForCard = isForFilterOrForCard;
		ForDSOrNodeName = forDSName;
		ForField = forField;
		DSOrNodeName = dsName;
		_key = MakeKey(this);
	}
	public DSKeyName(bool isForFilterOrForCard, string cdsName, string forDSName, string forField, string dsName)
	{
		if (cdsName == null) throw new ArgumentNullException(nameof(cdsName));
		if (cdsName.Length == 0) throw new ArgumentException(nameof(cdsName));
		if (forDSName == null) throw new ArgumentNullException(nameof(forDSName));
		if (forDSName.Length == 0) throw new ArgumentException(nameof(forDSName));
		if (forField == null) throw new ArgumentNullException(nameof(forField));
		if (forField.Length == 0) throw new ArgumentException(nameof(forField));
		if (dsName == null) throw new ArgumentNullException(nameof(dsName));
		if (dsName.Length == 0) throw new ArgumentException(nameof(dsName));

		_isForFilterOrForCard = isForFilterOrForCard;
		CDSName = cdsName;
		ForDSOrNodeName = forDSName;
		ForField = forField;
		DSOrNodeName = dsName;
		_key = MakeKey(this);
	}
	private DSKeyName(string okeyName, bool _)
	{
		if (okeyName == null) throw new ArgumentNullException(nameof(okeyName));

		if (!TryParseKeyName(okeyName, out _isForFilterOrForCard, out CDSName, out ForDSOrNodeName, out ForField, out DSOrNodeName))
		{
			throw new ArgumentException(nameof(okeyName));
		}

		_key = okeyName;
	}

/* 	public override bool Equals(object? obj)
	{
		if (obj is DSKeyName modelName)
		{
			return Equals(modelName);
		}
		else if (obj is string str)
		{
			return Equals(str);
		}
		else if (obj is OKeyName okeyName)
		{
			return Equals(okeyName);
		}

		return false;
	}

	public bool Equals(DSKeyName? other)
	{
		if (other == null) return false;

		return _isForFilterOrForCard == other._isForFilterOrForCard
			&& CDSName == other.CDSName
			&& ForDSOrNodeName == other.ForDSOrNodeName
			&& ForField == other.ForField
			&& DSOrNodeName == other.DSOrNodeName;
	} */

/* 	public bool Equals(string? other)
	{
		if (other == null) return false;

		int i, j, separatorCount = other.Count(ch => ch == ':');
		switch (separatorCount)
		{
			case 0:
				return _isForFilterOrForCard == false
					&& CDSName == null
					&& ForField == null
					&& DSOrNodeName == other;

			case 1:
				if (_isForFilterOrForCard != false || CDSName == null || ForField != null) return false;

				i = other.IndexOf(':');
				if (!MemoryExtensions.Equals(CDSName.AsSpan(), other.AsSpan(0, i), StringComparison.Ordinal))
				{
					return false;
				}
				if (i + 1 >= other.Length || !MemoryExtensions.Equals(DSOrNodeName.AsSpan(), other.AsSpan(i + 1), StringComparison.Ordinal))
				{
					return false;
				}

				return true;

			case 3:
				if (CDSName != null || ForField == null) return false;

				i = other.IndexOf(':');
				if (MemoryExtensions.Equals("FLT".AsSpan(), other.AsSpan(0, i), StringComparison.Ordinal))
				{
					if (!_isForFilterOrForCard) return false;
				}
				else if (MemoryExtensions.Equals("CLL".AsSpan(), other.AsSpan(0, i), StringComparison.Ordinal))
				{
					if (_isForFilterOrForCard) return false;
				}
				else
				{
					return false;
				}

				j = other.IndexOf(':', ++i);
				if (!MemoryExtensions.Equals(ForDSOrNodeName.AsSpan(), other.AsSpan(i, j - i), StringComparison.Ordinal))
				{
					return false;
				}

				j = other.IndexOf(':', i = j + 1);
				if (!MemoryExtensions.Equals(ForField.AsSpan(), other.AsSpan(i, j - i), StringComparison.Ordinal))
				{
					return false;
				}

				if (j + 1 >= other.Length || !MemoryExtensions.Equals(DSOrNodeName.AsSpan(), other.AsSpan(j + 1), StringComparison.Ordinal))
				{
					return false;
				}

				return true;

			case 4:
				if (CDSName == null || ForField == null) return false;

				i = other.IndexOf(':');
				if (MemoryExtensions.Equals("FLT".AsSpan(), other.AsSpan(0, i), StringComparison.Ordinal))
				{
					if (!_isForFilterOrForCard) return false;
				}
				else if (MemoryExtensions.Equals("CLL".AsSpan(), other.AsSpan(0, i), StringComparison.Ordinal))
				{
					if (_isForFilterOrForCard) return false;
				}
				else
				{
					return false;
				}

				j = other.IndexOf(':', ++i);
				if (!MemoryExtensions.Equals(CDSName.AsSpan(), other.AsSpan(i, j - i), StringComparison.Ordinal))
				{
					return false;
				}

				j = other.IndexOf(':', i = j + 1);
				if (!MemoryExtensions.Equals(ForDSOrNodeName.AsSpan(), other.AsSpan(i, j - i), StringComparison.Ordinal))
				{
					return false;
				}

				j = other.IndexOf(':', i = j + 1);
				if (!MemoryExtensions.Equals(ForField.AsSpan(), other.AsSpan(i, j - i), StringComparison.Ordinal))
				{
					return false;
				}

				if (j + 1 >= other.Length || !MemoryExtensions.Equals(DSOrNodeName.AsSpan(), other.AsSpan(j + 1), StringComparison.Ordinal))
				{
					return false;
				}

				return true;
			default:
				return false;
		}
	} */

/* 	public static implicit operator string?(DSKeyName? okeyName)
	{
		return okeyName?.Key;
	}
	public static implicit operator DSKeyName?(string? okeyName)
	{
		return okeyName != null ? new DSKeyName(okeyName, false) : null;
	}

	public static bool operator ==(DSKeyName? a, DSKeyName? b)
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
	public static bool operator !=(DSKeyName? a, DSKeyName? b)
	{
		return !(a == b);
	}
 */
	private static string MakeKey(DSKeyName p)
	{
		if (p.ForField != null)
		{
			if (p.CDSName == null)
			{
				return string.Concat(p._isForFilterOrForCard ? "FLT:" : "CLL:", p.ForDSOrNodeName, ":", p.ForField, ":", p.DSOrNodeName);
			}
			else
			{
				return string.Concat(p._isForFilterOrForCard ? "FLT:" : "CLL:", p.CDSName, ":", p.ForDSOrNodeName, ":", p.ForField, ":", p.DSOrNodeName);
			}
		}
		else
		{
			if (p.CDSName == null)
			{
				return p.DSOrNodeName;
			}
			else
			{
				return string.Concat(p.CDSName, ":", p.DSOrNodeName);
			}
		}
	}

	private static bool TryParseKeyName(string modelName, out bool isForFilterOrForCard, out string? cdsName, out string? forDSOrNodeName, out string? forField, out string dsOrNodeName)
	{
		var arr = modelName.Split(':');
		var last = arr.Length - 1;
		if (last >= 0 && arr[last].EndsWith("@DS"))
		{
			arr[last] = arr[last].Substring(arr[last].Length - 3);
			switch (arr.Length)
			{
				case 1:
					isForFilterOrForCard = false;
					cdsName = null;
					forDSOrNodeName = null;
					forField = null;
					dsOrNodeName = arr[0];
					return true;
				case 2:
					isForFilterOrForCard = false;
					cdsName = arr[0];
					forDSOrNodeName = null;
					forField = null;
					dsOrNodeName = arr[1];
					return true;
				case 4:
					if (arr[0] == "FLT")
					{
						isForFilterOrForCard = true;
					}
					else if (arr[0] == "CLL")
					{
						isForFilterOrForCard = false;
					}
					else
					{
						throw new ArgumentException(nameof(modelName));
					}
					cdsName = null;
					forDSOrNodeName = arr[1];
					forField = arr[2];
					dsOrNodeName = arr[3];
					return true;
				case 5:
					if (arr[0] == "FLT")
					{
						isForFilterOrForCard = true;
					}
					else if (arr[0] == "CLL")
					{
						isForFilterOrForCard = false;
					}
					else
					{
						throw new ArgumentException(nameof(modelName));
					}
					cdsName = arr[1];
					forDSOrNodeName = arr[2];
					forField = arr[3];
					dsOrNodeName = arr[4];
					return true;
			}
		}

		isForFilterOrForCard = false;
		cdsName = null;
		forDSOrNodeName = null;
		forField = null;
		dsOrNodeName = string.Empty;
		return false;
	}
}