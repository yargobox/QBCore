using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource;

[DebuggerDisplay("{ToString(true)}")]
public sealed class DEPath : IEquatable<DEPath>, IReadOnlyList<DEInfo>
{
	private readonly DEInfo[]? _dataEntries;
	private readonly DEInfo? _last;
	private readonly string _path;

	public string Name => _last?.Name ?? string.Empty;
	public string Path => _path;
	public Type DataEntryType => _last?.DataEntryType ?? typeof(NotSupported);
	public Type UnderlyingType => _last?.UnderlyingType ?? typeof(NotSupported);
	public bool IsNullable => _last?.IsNullable ?? false;
	public int Count => _dataEntries?.Length ?? 1;
	public Type DocumentType => _dataEntries == null
		? _last!.Document.DocumentType
		: _dataEntries.Length == 0
			? typeof(void)
			: _dataEntries[0].Document.DocumentType;

	public DEPath(LambdaExpression path, bool allowPointToSelf, IDataLayerInfo dataLayer)
	{
		var memberInfos = path.GetPropertyOrFieldPath(x => x.Member, allowPointToSelf);

		if (memberInfos.Length == 0)
		{
			_dataEntries = Array.Empty<DEInfo>();
			_path = string.Empty;
		}
		else if (memberInfos.Length == 1)
		{
			_last = DEInfo.GetDataEntry(memberInfos[0], dataLayer);
			_path = _last.Name;
		}
		else
		{
			_dataEntries = new DEInfo[memberInfos.Length];
			for (int i = 0; i < memberInfos.Length; i++)
			{
				_dataEntries[i] = DEInfo.GetDataEntry(memberInfos[i], dataLayer);
			}
			_last = _dataEntries[_dataEntries.Length - 1];
			_path = string.Join(".", _dataEntries.Select(x => x.Name));
		}
	}

	public DEPath(Type documentType, string path, bool allowPointToSelf, IDataLayerInfo? dataLayer = null)
	{
		if (documentType == null)
		{
			throw new ArgumentNullException(nameof(documentType));
		}
		if (path == null)
		{
			throw new ArgumentNullException(nameof(path));
		}

		var pathElements = path.Split('.');

		if (pathElements.Length == 0)
		{
			_dataEntries = Array.Empty<DEInfo>();
			_path = string.Empty;
		}
		else if (pathElements.Length == 1)
		{
			_last = DEInfo.GetDataEntry(documentType, pathElements[0], dataLayer);
			_path = _last.Name;
		}
		else
		{
			_dataEntries = new DEInfo[pathElements.Length];
			for (int i = 0; i < pathElements.Length; i++)
			{
				_dataEntries[i] = DEInfo.GetDataEntry(documentType, pathElements[i], dataLayer);
			}
			_last = _dataEntries[_dataEntries.Length - 1];
			_path = path;
		}
	}

	public DEPath(Type documentType, string path, bool ignoreCase, bool allowPointToSelf, IDataLayerInfo? dataLayer = null)
	{
		if (documentType == null)
		{
			throw new ArgumentNullException(nameof(documentType));
		}
		if (path == null)
		{
			throw new ArgumentNullException(nameof(path));
		}

		var pathElements = path.Split('.');

		if (pathElements.Length == 0)
		{
			_dataEntries = Array.Empty<DEInfo>();
			_path = string.Empty;
		}
		else if (pathElements.Length == 1)
		{
			_last = DEInfo.GetDataEntry(documentType, pathElements[0], ignoreCase, dataLayer);
			_path = _last.Name;
		}
		else
		{
			_dataEntries = new DEInfo[pathElements.Length];
			for (int i = 0; i < pathElements.Length; i++)
			{
				_dataEntries[i] = DEInfo.GetDataEntry(documentType, pathElements[i], ignoreCase, dataLayer);
			}
			_last = _dataEntries[_dataEntries.Length - 1];
			_path = path;
		}
	}

	public DEPath(DEInfo dataEntryInfo)
	{
		if (dataEntryInfo == null)
		{
			throw new ArgumentNullException(nameof(dataEntryInfo));
		}

		_last = dataEntryInfo;
		_path = _last.Name;
	}

	public override string ToString()
	{
		return _path;
	}

	public string ToString(bool shortDocumentName)
	{
		if (_dataEntries == null)
		{
			return _last!.ToString(shortDocumentName);
		}
		else if (_dataEntries.Length == 0)
		{
			return string.Empty;
		}
		else if (shortDocumentName)
		{
			return string.Concat(_dataEntries[0].Document.DocumentType.Name, ".", _path);
		}
		return string.Concat(_dataEntries[0].Document.DocumentType.FullName, ".", _path);
	}

	public override int GetHashCode()
	{
		if (_dataEntries == null)
		{
			return _last!.Document.DocumentType.GetHashCode() ^ _path.GetHashCode();
		}
		else if (_dataEntries.Length == 0)
		{
			return 0;
		}
		return _dataEntries[0].Document.DocumentType.GetHashCode() ^ _path.GetHashCode();
	}

	public override bool Equals(object? obj)
		=> obj == null ? false : Equals(obj as DEPath);

	public bool Equals(DEPath? other)
		=> other == null ? false : this.SequenceEqual(other);

	public DEInfo this[int index]
		=> _dataEntries != null
			? _dataEntries[index]
			: index == 0
				? _last!
				: throw new IndexOutOfRangeException(nameof(index));

	IEnumerator IEnumerable.GetEnumerator()
		=> this.GetEnumerator();

	public IEnumerator<DEInfo> GetEnumerator()
	{
		if (_dataEntries == null)
		{
			yield return _last!;
		}
		else
		{
			foreach (var elem in _dataEntries) yield return elem;
		}
	}
}


[DebuggerDisplay("{ToString()}")]
public readonly struct DEPathDefinition<TDoc> : IEquatable<DEPathDefinition<TDoc>>
{
	public readonly string? Path;
	public readonly DEPath? DataEntryPath;

	public int Count => DataEntryPath?.Count ?? Path!.Count(x => x == '.') + 1;

	public DEPathDefinition(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException(nameof(path));
		}

		Path = path;
		DataEntryPath = null;
	}

	public DEPathDefinition(DEPath dataEntryPath)
	{
		if (dataEntryPath == null)
		{
			throw new ArgumentNullException(nameof(dataEntryPath));
		}

		Path = null;
		DataEntryPath = dataEntryPath;
	}

	public DEPathDefinition(DEInfo dataEntryInfo)
	{
		if (dataEntryInfo == null)
		{
			throw new ArgumentNullException(nameof(dataEntryInfo));
		}
		if (dataEntryInfo.Document.DocumentType != typeof(TDoc))
		{
			throw new ArgumentException($"DataEntry '{dataEntryInfo.ToString(true)}' does not match the expected document type '{typeof(TDoc).ToPretty()}'.", nameof(dataEntryInfo));
		}

		Path = null;
		DataEntryPath = new DEPath(dataEntryInfo);
	}

	public readonly DEPath ToDataEntryPath(IDataLayerInfo dataLayer)
	{
		if (DataEntryPath != null)
		{
			return DataEntryPath;
		}
		
		return new DEPath(typeof(TDoc), Path!, true, dataLayer);
	}

	public readonly override string ToString()
	{
		return Path ?? DataEntryPath!.Path;
	}

	public readonly string ToString(bool shortDocumentName)
	{
		if (shortDocumentName)
		{
			return string.Concat(typeof(TDoc).Name, ".", (Path ?? DataEntryPath!.Path));
		}
		else
		{
			return string.Concat(typeof(TDoc).FullName, ".", (Path ?? DataEntryPath!.Path));
		}
	}

	public readonly override int GetHashCode()
	{
		return typeof(TDoc).GetHashCode() ^ (Path ?? DataEntryPath!.Path).GetHashCode();
	}

	public readonly override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is DEPathDefinition<TDoc> value2)
		{
			return Equals(value2);
		}
		return false;
	}

	public readonly bool Equals(DEPathDefinition<TDoc> other)
		=> (Path ?? DataEntryPath!.Path) == (other.Path ?? other.DataEntryPath!.Path);

	public static bool operator ==(DEPathDefinition<TDoc> a, DEPathDefinition<TDoc> b)
		=> a.Equals(b);

	public static bool operator !=(DEPathDefinition<TDoc> a, DEPathDefinition<TDoc> b)
		=> !a.Equals(b);

	public static implicit operator DEPathDefinition<TDoc>(string path)
		=> new DEPathDefinition<TDoc>(path);

	public static implicit operator DEPathDefinition<TDoc>(DEInfo dataEntryInfo)
		=> new DEPathDefinition<TDoc>(dataEntryInfo);

	public static implicit operator DEPathDefinition<TDoc>(DEPath path)
		=> new DEPathDefinition<TDoc>(path);
}


[DebuggerDisplay("{ToString()}")]
public readonly struct DEPathDefinition<TDoc, TField> : IEquatable<DEPathDefinition<TDoc, TField>>, IEquatable<DEPathDefinition<TDoc>>
{
	public readonly string? Path;
	public readonly DEPath? DataEntryPath;

	public int Count => DataEntryPath?.Count ?? Path!.Count(x => x == '.') + 1;

	public DEPathDefinition(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException(nameof(path));
		}

		Path = path;
		DataEntryPath = null;
	}

	public DEPathDefinition(DEPath dataEntryPath)
	{
		if (dataEntryPath == null)
		{
			throw new ArgumentNullException(nameof(dataEntryPath));
		}
		if (dataEntryPath.DataEntryType != typeof(TField))
		{
			throw new ArgumentException($"DataEntry '{dataEntryPath.ToString(true)}' is of type '{dataEntryPath.DataEntryType.ToPretty()}' which does not match the expected '{typeof(TField).ToPretty()}'.", nameof(dataEntryPath));
		}

		Path = null;
		DataEntryPath = dataEntryPath;
	}

	public DEPathDefinition(DEInfo dataEntryInfo)
	{
		if (dataEntryInfo == null)
		{
			throw new ArgumentNullException(nameof(dataEntryInfo));
		}
		if (dataEntryInfo.Document.DocumentType != typeof(TDoc))
		{
			throw new ArgumentException($"DataEntry '{dataEntryInfo.ToString(true)}' does not match the expected document type '{typeof(TDoc).ToPretty()}'.", nameof(dataEntryInfo));
		}
		if (dataEntryInfo.Document.DocumentType != typeof(TDoc) || dataEntryInfo.DataEntryType != typeof(TField))
		{
			throw new ArgumentException($"DataEntry '{dataEntryInfo.ToString(true)}' is of type '{dataEntryInfo.DataEntryType.ToPretty()}' which does not match the expected '{typeof(TField).ToPretty()}'.", nameof(dataEntryInfo));
		}

		Path = null;
		DataEntryPath = new DEPath(dataEntryInfo);
	}

	public readonly DEPath ToDataEntryPath(IDataLayerInfo dataLayer)
	{
		if (DataEntryPath != null)
		{
			return DataEntryPath;
		}
		
		var dataEntryPath = new DEPath(typeof(TDoc), Path!, true, dataLayer);
		if (dataEntryPath.DataEntryType != typeof(TField))
		{
			throw new ArgumentException($"DataEntry '{dataEntryPath.ToString(true)}' is of type '{dataEntryPath.DataEntryType.ToPretty()}' which does not match the expected '{typeof(TField).ToPretty()}'.", nameof(dataEntryPath));
		}
		return dataEntryPath;
	}

	public override string ToString()
	{
		if (Path != null)
		{
			return string.Concat(typeof(TDoc).ToPretty(), ".", Path);
		}
		else
		{
			return DataEntryPath!.ToString(true);
		}
	}

	public override int GetHashCode()
	{
		if (Path != null)
		{
			return typeof(TDoc).GetHashCode() ^ Path.GetHashCode();
		}
		else
		{
			return DataEntryPath!.GetHashCode();
		}
	}

	public override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is DEPathDefinition<TDoc, TField> value1)
		{
			return Equals(value1);
		}
		if (obj is DEPathDefinition<TDoc> value2)
		{
			return Equals(value2);
		}
		return false;
	}

	public bool Equals(DEPathDefinition<TDoc> other)
		=> (Path ?? DataEntryPath!.Path) == (other.Path ?? other.DataEntryPath!.Path);

	public bool Equals(DEPathDefinition<TDoc, TField> other)
		=> (Path ?? DataEntryPath!.Path) == (other.Path ?? other.DataEntryPath!.Path);

	public static bool operator ==(DEPathDefinition<TDoc, TField> a, DEPathDefinition<TDoc, TField> b)
		=> a.Equals(b);

	public static bool operator !=(DEPathDefinition<TDoc, TField> a, DEPathDefinition<TDoc, TField> b)
		=> !a.Equals(b);

	public static implicit operator DEPathDefinition<TDoc, TField>(DEPathDefinition<TDoc> def)
		=> def.Path != null
			? new DEPathDefinition<TDoc, TField>(def.Path)
			: new DEPathDefinition<TDoc, TField>(def.DataEntryPath!);

	public static implicit operator DEPathDefinition<TDoc, TField>(string path)
		=> new DEPathDefinition<TDoc, TField>(path);

	public static implicit operator DEPathDefinition<TDoc, TField>(DEInfo dataEntryInfo)
		=> new DEPathDefinition<TDoc, TField>(dataEntryInfo);
	public static implicit operator DEPathDefinition<TDoc, TField>(DEPath dataEntryPath)
		=> new DEPathDefinition<TDoc, TField>(dataEntryPath);
}