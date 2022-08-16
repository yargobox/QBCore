using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource;

[DebuggerDisplay("{ToString(true)}")]
public sealed class DataEntryPath : IEquatable<DataEntryPath>, IReadOnlyList<DataEntry>
{
	private readonly DataEntry[]? _path;
	private readonly DataEntry? _last;
	private readonly string _fullName;

	public string Name => _last?.Name ?? string.Empty;
	public string FullName => _fullName;
	public Type DataEntryType => _last?.DataEntryType ?? typeof(NotSupported);
	public bool IsNullable => _last?.IsNullable ?? false;
	public int Count => _path?.Length ?? 1;

	public DataEntryPath(LambdaExpression path, bool allowPointToSelf, IDataLayerInfo dataLayer)
	{
		var memberInfos = path.GetPropertyOrFieldPath(x => x.Member, allowPointToSelf);

		if (memberInfos.Length == 0)
		{
			_path = Array.Empty<DataEntry>();
			_fullName = string.Empty;
		}
		else if (memberInfos.Length == 1)
		{
			_last = DataEntry.GetDataEntry(memberInfos[0], dataLayer);
			_fullName = _last.Name;
		}
		else
		{
			_path = new DataEntry[memberInfos.Length];
			for (int i = 0; i < memberInfos.Length; i++)
			{
				_path[i] = DataEntry.GetDataEntry(memberInfos[i], dataLayer);
			}
			_last = _path[_path.Length - 1];
			_fullName = string.Join(".", _path.Select(x => x.Name));
		}
	}

	public DataEntryPath(Type documentType, string path, bool allowPointToSelf, IDataLayerInfo dataLayer)
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
			_path = Array.Empty<DataEntry>();
			_fullName = string.Empty;
		}
		else if (pathElements.Length == 1)
		{
			_last = DataEntry.GetDataEntry(documentType, pathElements[0], dataLayer);
			_fullName = _last.Name;
		}
		else
		{
			_path = new DataEntry[pathElements.Length];
			for (int i = 0; i < pathElements.Length; i++)
			{
				_path[i] = DataEntry.GetDataEntry(documentType, pathElements[i], dataLayer);
			}
			_last = _path[_path.Length - 1];
			_fullName = path;
		}
	}

	public override string ToString()
	{
		return _fullName;
	}

	public string ToString(bool shortDocumentName)
	{
		if (_path == null)
		{
			return _last!.ToString(shortDocumentName);
		}
		else if (_path.Length == 0)
		{
			return string.Empty;
		}
		else if (shortDocumentName)
		{
			return string.Concat(_path[0].Document.DocumentType.Name, ".", _fullName);
		}
		return string.Concat(_path[0].Document.DocumentType.FullName, ".", _fullName);
	}

	public override int GetHashCode()
	{
		if (_path == null)
		{
			return _last!.Document.DocumentType.GetHashCode() ^ _fullName.GetHashCode();
		}
		else if (_path.Length == 0)
		{
			return 0;
		}
		return _path[0].Document.DocumentType.GetHashCode() ^ _fullName.GetHashCode();
	}

	public override bool Equals(object? obj)
		=> obj == null ? false : Equals(obj as DataEntryPath);

	public bool Equals(DataEntryPath? other)
		=> other == null ? false : this.SequenceEqual(other);

	public DataEntry this[int index]
		=> _path != null
			? _path[index]
			: index == 0
				? _last!
				: throw new IndexOutOfRangeException(nameof(index));

	IEnumerator IEnumerable.GetEnumerator()
		=> this.GetEnumerator();

	public IEnumerator<DataEntry> GetEnumerator()
	{
		if (_path == null)
		{
			yield return _last!;
		}
		else
		{
			foreach (var elem in _path) yield return elem;
		}
	}
}


[DebuggerDisplay("{ToString()}")]
public readonly struct DEPathDefinition<TDocument> : IEquatable<DEPathDefinition<TDocument>>
{
	public readonly string? Path;
	public readonly DataEntryPath? DataEntryPath;

	public DEPathDefinition(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException(nameof(path));
		}

		Path = path;
		DataEntryPath = null;
	}

	public DEPathDefinition(DataEntryPath dataEntryPath)
	{
		if (dataEntryPath == null)
		{
			throw new ArgumentNullException(nameof(dataEntryPath));
		}

		Path = null;
		DataEntryPath = dataEntryPath;
	}

	public readonly DataEntryPath ToDataEntryPath(IDataLayerInfo dataLayer)
	{
		if (DataEntryPath != null)
		{
			return DataEntryPath;
		}
		
		return new DataEntryPath(typeof(TDocument), Path!, true, dataLayer);
	}

	public readonly override string ToString()
	{
		return Path ?? DataEntryPath!.FullName;
	}

	public readonly string ToString(bool shortDocumentName)
	{
		if (shortDocumentName)
		{
			return string.Concat(typeof(TDocument).Name, ".", (Path ?? DataEntryPath!.FullName));
		}
		else
		{
			return string.Concat(typeof(TDocument).FullName, ".", (Path ?? DataEntryPath!.FullName));
		}
	}

	public readonly override int GetHashCode()
	{
		return typeof(TDocument).GetHashCode() ^ (Path ?? DataEntryPath!.FullName).GetHashCode();
	}

	public readonly override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is DEPathDefinition<TDocument> value2)
		{
			return Equals(value2);
		}
		return false;
	}

	public readonly bool Equals(DEPathDefinition<TDocument> other)
		=> (Path ?? DataEntryPath!.FullName) == (other.Path ?? other.DataEntryPath!.FullName);

	public static implicit operator DEPathDefinition<TDocument>(string path)
		=> new DEPathDefinition<TDocument>(path);

	public static implicit operator DEPathDefinition<TDocument>(DataEntryPath path)
		=> new DEPathDefinition<TDocument>(path);

	public static bool operator ==(DEPathDefinition<TDocument> a, DEPathDefinition<TDocument> b)
		=> a.Equals(b);

	public static bool operator !=(DEPathDefinition<TDocument> a, DEPathDefinition<TDocument> b)
		=> !a.Equals(b);
}


[DebuggerDisplay("{ToString()}")]
public readonly struct DEPathDefinition<TDocument, TField> : IEquatable<DEPathDefinition<TDocument, TField>>, IEquatable<DEPathDefinition<TDocument>>
{
	public readonly string? Path;
	public readonly DataEntryPath? DataEntryPath;

	public DEPathDefinition(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException(nameof(path));
		}

		Path = path;
		DataEntryPath = null;
	}

	public DEPathDefinition(DataEntryPath dataEntryPath)
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

	public readonly DataEntryPath ToDataEntryPath(IDataLayerInfo dataLayer)
	{
		if (DataEntryPath != null)
		{
			return DataEntryPath;
		}
		
		var dataEntryPath = new DataEntryPath(typeof(TDocument), Path!, true, dataLayer);
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
			return string.Concat(typeof(TDocument).ToPretty(), ".", Path);
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
			return typeof(TDocument).GetHashCode() ^ Path.GetHashCode();
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
		if (obj is DEPathDefinition<TDocument, TField> value1)
		{
			return Equals(value1);
		}
		if (obj is DEPathDefinition<TDocument> value2)
		{
			return Equals(value2);
		}
		return false;
	}

	public bool Equals(DEPathDefinition<TDocument> other)
		=> (Path ?? DataEntryPath!.FullName) == (other.Path ?? other.DataEntryPath!.FullName);

	public bool Equals(DEPathDefinition<TDocument, TField> other)
		=> (Path ?? DataEntryPath!.FullName) == (other.Path ?? other.DataEntryPath!.FullName);

	public static bool operator ==(DEPathDefinition<TDocument, TField> a, DEPathDefinition<TDocument, TField> b)
		=> a.Equals(b);

	public static bool operator !=(DEPathDefinition<TDocument, TField> a, DEPathDefinition<TDocument, TField> b)
		=> !a.Equals(b);

	public static implicit operator DEPathDefinition<TDocument, TField>(DEPathDefinition<TDocument> def)
		=> def.Path != null
			? new DEPathDefinition<TDocument, TField>(def.Path)
			: new DEPathDefinition<TDocument, TField>(def.DataEntryPath!);

	public static implicit operator DEPathDefinition<TDocument, TField>(string path)
		=> new DEPathDefinition<TDocument, TField>(path);

	public static implicit operator DEPathDefinition<TDocument, TField>(DataEntryPath dataEntryPath)
		=> new DEPathDefinition<TDocument, TField>(dataEntryPath);
}