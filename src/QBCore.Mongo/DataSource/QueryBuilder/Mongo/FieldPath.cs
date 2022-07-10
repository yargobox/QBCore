using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

[DebuggerDisplay("{FullName}")]
internal sealed class FieldPath
{
	public sealed class Element
	{
		public readonly string Name;
		public readonly Type ElementType;
		public readonly bool IsNullable;
		public readonly Type DeclaringType;

		public string DBSideName => BsonClassMap.LookupClassMap(DeclaringType).GetMemberMap(Name).ElementName;

		public Element(string name, Type elementType, bool isNullable, Type declaringType)
		{
			Name = name;
			ElementType = elementType;
			IsNullable = isNullable;
			DeclaringType = declaringType;
		}
	}

	private readonly Element[]? _path;
	private readonly Element _last;
	private readonly string _fullName;
	private string? _dbSideName;

	public string Name => _last.Name;
	public string FullName => _fullName;
	public Type FieldType => _last.ElementType;
	public bool IsNullable => _last.IsNullable;
	public Type DeclaringType => _last.DeclaringType;

	public string DBSideName
	{
		get
		{
			if (_dbSideName == null)
			{
				if (_path == null)
				{
					_dbSideName = _last.DBSideName;
				}
				else
				{
					_dbSideName = string.Join(".", _path.Select(x => x.DBSideName));
				}
			}
			return _dbSideName;
		}
	}

	public int ElementCount => _path?.Length ?? 1;

	public IEnumerable<Element> Elements
	{
		get
		{
			if (_path == null)
			{
				yield return _last;
			}
			else
			{
				foreach (var element in _path) yield return element;
			}
		}
	}

	public FieldPath(LambdaExpression propertyOrFieldSelector, bool allowPointToSelf)
	{
		var memberInfos = propertyOrFieldSelector.GetPropertyOrFieldPath(x => x.Member, allowPointToSelf);

		if (memberInfos.Length == 0)
		{
			if (propertyOrFieldSelector.Body is ParameterExpression param)
			{
				_path = Array.Empty<Element>();
				_last = new Element(string.Empty, param.Type, false, param.Type);
				_fullName = string.Empty;
				_dbSideName = string.Empty;
			}
			else
			{
				throw new ArgumentException(nameof(propertyOrFieldSelector));
			}
		}
		else if (memberInfos.Length == 1)
		{
			if (memberInfos[0] is PropertyInfo pi)
			{
				_last = new Element(pi.Name, pi.PropertyType!, pi.IsNullable(), pi.DeclaringType!);
			}
			else if (memberInfos[0] is FieldInfo fi)
			{
				_last = new Element(fi.Name, fi.FieldType!, fi.IsNullable(), fi.DeclaringType!);
			}
			else
			{
				throw new ArgumentException(nameof(propertyOrFieldSelector));
			}

			_fullName = _last.Name;
		}
		else
		{
			_path = new Element[memberInfos.Length];
			
			for (int i = 0; i < memberInfos.Length; i++)
			{
				if (memberInfos[i] is PropertyInfo pi)
				{
					_path[i] = new Element(pi.Name, pi.PropertyType!, pi.IsNullable(), pi.DeclaringType!);
				}
				else if (memberInfos[i] is FieldInfo fi)
				{
					_path[i] = new Element(fi.Name, fi.FieldType!, fi.IsNullable(), fi.DeclaringType!);
				}
			}

			_last = _path[_path.Length - 1];
			_fullName = string.Join(".", _path.Select(x => x.Name));
		}
	}
}