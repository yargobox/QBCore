using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource;

[DebuggerDisplay("{FullName}")]
public abstract class DataEntryPath : IEquatable<DataEntryPath>, IReadOnlyList<DataEntry>
{
	private readonly DataEntry[]? _path;
	private readonly DataEntry? _last;
	private readonly string _fullName;

	public string Name => _last?.Name ?? string.Empty;
	public string FullName => _fullName;
	public Type DataEntryType => _last?.DataEntryType ?? typeof(NotSupported);
	public bool IsNullable => _last?.IsNullable ?? false;
	public int Count => _path?.Length ?? 1;

	public abstract IDataLayerInfo DataLayer { get; }

	public DataEntryPath(LambdaExpression path, bool allowPointToSelf)
	{
		var memberInfos = path.GetPropertyOrFieldPath(x => x.Member, allowPointToSelf);

		if (memberInfos.Length == 0)
		{
			_path = Array.Empty<DataEntry>();
			_fullName = string.Empty;
		}
		else if (memberInfos.Length == 1)
		{
			_last = DataEntry.GetDataEntry(memberInfos[0], DataLayer);
			_fullName = _last.Name;
		}
		else
		{
			_path = new DataEntry[memberInfos.Length];
			for (int i = 0; i < memberInfos.Length; i++)
			{
				_path[i] = DataEntry.GetDataEntry(memberInfos[i], DataLayer);
			}
			_last = _path[_path.Length - 1];
			_fullName = string.Join(".", _path.Select(x => x.Name));
		}
	}

	public DataEntryPath(Type documentType, string path, bool allowPointToSelf)
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
			_last = DataEntry.GetDataEntry(documentType, pathElements[0], DataLayer);
			_fullName = _last.Name;
		}
		else
		{
			_path = new DataEntry[pathElements.Length];
			for (int i = 0; i < pathElements.Length; i++)
			{
				_path[i] = DataEntry.GetDataEntry(documentType, pathElements[i], DataLayer);
			}
			_last = _path[_path.Length - 1];
			_fullName = string.Join(".", _path.Select(x => x.Name));
		}
	}

	public override string ToString()
	{
		if (_path == null)
		{
			return _last!.Name;
		}
		else if (_path.Length == 0)
		{
			return string.Empty;
		}
		else
		{
			return string.Join('.', _path.Select(x => x.Name));
		}
	}
	public virtual string ToString(bool shortDocumentName)
	{
		if (_path == null)
		{
			return _last!.ToString(shortDocumentName);
		}
		else if (_path.Length == 0)
		{
			return string.Empty;
		}
		else
		{
			return string.Concat(_path[0].ToString(shortDocumentName), ".", string.Join('.', _path.Skip(1).Select(x => x.Name)));
		}
	}

	public override int GetHashCode() => this.Aggregate(0, (acc, de) => acc ^ de.GetHashCode());

	public override bool Equals(object? obj) => obj == null ? false : Equals(obj as DataEntryPath);

	public bool Equals(DataEntryPath? other) => other == null ? false : this.SequenceEqual(other);

	public DataEntry this[int index] =>
		_path == null
			? (index == 0 ? _last! : throw new IndexOutOfRangeException(nameof(index)))
			: _path[index];

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

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}