using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

[DebuggerDisplay("{FullName}")]
public abstract class FieldPath
{
	public sealed record Element
	{
		public readonly string Name;
		public readonly Type ElementType;
		public readonly bool IsNullable;
		public readonly Type DeclaringType;

		public Element(string name, Type elementType, bool isNullable, Type declaringType)
		{
			Name = name;
			ElementType = elementType;
			IsNullable = isNullable;
			DeclaringType = declaringType;
		}
	}

	private readonly Element[]? _elements;
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
				if (_elements == null)
				{
					_dbSideName = GetDBSideName(_last);
				}
				else
				{
					_dbSideName = string.Join(".", _elements.Select(x => GetDBSideName(x)));
				}
			}
			return _dbSideName;
		}
	}

	public int ElementCount => _elements?.Length ?? 1;
	public IEnumerable<Element> Elements
	{
		get
		{
			if (_elements == null)
			{
				yield return _last;
			}
			else
			{
				foreach (var element in _elements) yield return element;
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
				_elements = Array.Empty<Element>();
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
			_elements = new Element[memberInfos.Length];
			
			for (int i = 0; i < memberInfos.Length; i++)
			{
				if (memberInfos[i] is PropertyInfo pi)
				{
					_elements[i] = new Element(pi.Name, pi.PropertyType!, pi.IsNullable(), pi.DeclaringType!);
				}
				else if (memberInfos[i] is FieldInfo fi)
				{
					_elements[i] = new Element(fi.Name, fi.FieldType!, fi.IsNullable(), fi.DeclaringType!);
				}
			}

			_last = _elements[_elements.Length - 1];
			_fullName = string.Join(".", _elements.Select(x => x.Name));
		}
	}

	protected abstract string GetDBSideName(Type declaringType, string propertyOrFieldName);
	public string GetDBSideName(Element elem) => GetDBSideName(elem.DeclaringType, elem.Name);
}