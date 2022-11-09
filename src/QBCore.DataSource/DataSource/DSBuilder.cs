using System.Collections;

namespace QBCore.DataSource;

internal sealed class DSBuilder : IDSBuilder
{
	public Type ConcreteType { get; }

	public string? Name
	{
		get => _name;
		set
		{
			if (_name != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(Name)}' is already set.");
			_name = value;
		}
	}
	public DataSourceOptions Options
	{
		get => _options;
		set {
			if (_options != DataSourceOptions.None && !value.HasFlag(_options))
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(Options)}' is already set.");
		
			var operationsBefore = _options & DSInfo.AllDSOperations;
			var operationsAfter = value & DSInfo.AllDSOperations;
			if (operationsBefore != DataSourceOptions.None && operationsAfter != DataSourceOptions.None && operationsBefore != operationsAfter)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(Options)}' is already set. DataSource operations can only be specified once.");

			_options = value;
		}
	}
	public IList<Type> Listeners
	{
		get => _listener;
	}

	public Type? ServiceInterface
	{
		get => _serviceInterface;
		set
		{
			if (_serviceInterface != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(ServiceInterface)}' is already set.");
			_serviceInterface = value;
		}
	}
	public bool? IsServiceSingleton
	{
		get => _isServiceSingleton;
		set
		{
			if (_isServiceSingleton != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(IsServiceSingleton)}' is already set.");
			_isServiceSingleton = value;
		}
	}

	public bool? BuildAutoController
	{
		get => _isAutoController;
		set
		{
			if (_isAutoController != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(BuildAutoController)}' is already set.");
			_isAutoController = value;
		}
	}
	public string? ControllerName
	{
		get => _webName;
		set
		{
			if (_webName != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(ControllerName)}' is already set.");
			_webName = value;
		}
	}

	public string? DataContextName
	{
		get => _dataContextName;
		set
		{
			if (_dataContextName != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(DataContextName)}' is already set.");
			_dataContextName = value;
		}
	}

	public Type? DataLayer
	{
		get => _dataLayer;
		set
		{
			if (_dataLayer != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(DataLayer)}' is already set.");
			_dataLayer = value;
		}
	}

	public Delegate? InsertBuilder
	{
		get => _insertBuilder;
		set
		{
			if (_insertBuilder != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(InsertBuilder)}' is already set.");
			_insertBuilder = value;
		}
	}

	public Delegate? SelectBuilder
	{
		get => _selectBuilder;
		set
		{
			if (_selectBuilder != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(SelectBuilder)}' is already set.");
			_selectBuilder = value;
		}
	}

	public Delegate? UpdateBuilder
	{
		get => _updateBuilder;
		set
		{
			if (_updateBuilder != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(UpdateBuilder)}' is already set.");
			_updateBuilder = value;
		}
	}

	public Delegate? DeleteBuilder
	{
		get => _deleteBuilder;
		set
		{
			if (_deleteBuilder != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(DeleteBuilder)}' is already set.");
			_deleteBuilder = value;
		}
	}

	public Delegate? SoftDelBuilder
	{
		get => _softDelBuilder;
		set
		{
			if (_softDelBuilder != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(SoftDelBuilder)}' is already set.");
			_softDelBuilder = value;
		}
	}

	public Delegate? RestoreBuilder
	{
		get => _restoreBuilder;
		set
		{
			if (_restoreBuilder != null)
				throw new InvalidOperationException($"DataSource '{ConcreteType.ToPretty()}' builder option '{nameof(RestoreBuilder)}' is already set.");
			_restoreBuilder = value;
		}
	}

	private string? _name;
	private DataSourceOptions _options;
	private ListenerTypeList _listener;

	private Type? _serviceInterface;
	private bool? _isServiceSingleton;

	private bool? _isAutoController;
	private string? _webName;

	private string? _dataContextName;

	private Type? _dataLayer;
	private Delegate? _insertBuilder;
	private Delegate? _selectBuilder;
	private Delegate? _updateBuilder;
	private Delegate? _deleteBuilder;
	private Delegate? _softDelBuilder;
	private Delegate? _restoreBuilder;

	public DSBuilder(Type dataSourceConcreteType)
	{
		ConcreteType = dataSourceConcreteType;
		_listener = new ListenerTypeList();
	}

	private sealed class ListenerTypeList : IList<Type>
	{
		public int Count => _list?.Count ?? 0;
		bool ICollection<Type>.IsReadOnly => false;

		private List<Type>? _list;

		public Type this[int index]
		{
			get => _list?[index] ?? throw new IndexOutOfRangeException();
			set => throw new InvalidOperationException($"Builder option '{nameof(IDSBuilder.Listeners)}' is already set.");
		}

		public IEnumerator<Type> GetEnumerator() => ((IEnumerable<Type>?)_list)?.GetEnumerator() ?? Enumerable.Empty<Type>().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(Type listenerType)
		{
			if (listenerType == null) throw new ArgumentNullException(nameof(listenerType));
			if (_list?.Contains(listenerType) == true) return;
			if (listenerType.GetInterfaceOf(typeof(IDataSourceListener)) == null) throw new ArgumentException($"'{listenerType.ToPretty()}' is not '{typeof(IDataSourceListener).ToPretty()}'.", nameof(listenerType));

			if (_list == null)
			{
				_list = new List<Type>(4);
			}
			
			_list.Add(listenerType);
		}

		public int IndexOf(Type item) => _list?.IndexOf(item) ?? -1;

		public void Insert(int index, Type item)
		{
			if (index != Count) throw new InvalidOperationException($"Builder option '{nameof(IDSBuilder.Listeners)}' is already set.");

			Add(item);
		}

		public void RemoveAt(int index) => throw new InvalidOperationException($"Builder option '{nameof(IDSBuilder.Listeners)}' is already set.");

		public void Clear() => throw new InvalidOperationException($"Builder option '{nameof(IDSBuilder.Listeners)}' is already set.");

		public bool Contains(Type item) => _list?.Contains(item) ?? false;

		public void CopyTo(Type[] array, int arrayIndex) => ((ICollection<Type>?)_list)?.CopyTo(array, arrayIndex);

		public bool Remove(Type item) => throw new InvalidOperationException($"Builder option '{nameof(IDSBuilder.Listeners)}' is already set.");
	}
}