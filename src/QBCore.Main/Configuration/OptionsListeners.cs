using Microsoft.Extensions.Options;

namespace QBCore.Configuration;

public class OptionsListener<T> : IDisposable where T : class
{
	public T Value1 => _value1 ?? throw new InvalidOperationException($"Options '{typeof(T).Name}' hasn't been set yet.");

	private T? _value1;
	private IDisposable? _changeHandler1;

	public OptionsListener(IOptionsMonitor<T> optionsListener)
	{
		OnOptionsChanged1(optionsListener.CurrentValue);
		_changeHandler1 = optionsListener.OnChange(OnOptionsChanged1);
	}

	protected virtual void OnOptionsChanged1(T value)
	{
		if (value != null)
		{
			Interlocked.Exchange(ref _value1, value);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_changeHandler1 != null)
		{
			var temp1 = _changeHandler1;
			_changeHandler1 = null;
			_value1 = null;

			temp1.Dispose();
		}
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(true);
	}

	~OptionsListener()
	{
		Dispose(false);
	}
}

public class OptionsListener<T1, T2> : IDisposable where T1 : class where T2 : class
{
	public T1 Value1 => _value1 ?? throw new InvalidOperationException($"Options '{typeof(T1).Name}' hasn't been set yet.");
	public T2 Value2 => _value2 ?? throw new InvalidOperationException($"Options '{typeof(T2).Name}' hasn't been set yet.");

	private T1? _value1;
	private T2? _value2;
	private IDisposable? _changeHandler1;
	private IDisposable? _changeHandler2;

	public OptionsListener(IOptionsMonitor<T1> optionsMonitor1, IOptionsMonitor<T2> optionsMonitor2)
	{
		OnOptionsChanged1(optionsMonitor1.CurrentValue);
		OnOptionsChanged2(optionsMonitor2.CurrentValue);
		_changeHandler1 = optionsMonitor1.OnChange(OnOptionsChanged1);
		_changeHandler2 = optionsMonitor2.OnChange(OnOptionsChanged2);
	}

	protected virtual void OnOptionsChanged1(T1 value) => Interlocked.Exchange(ref _value1, value);
	protected virtual void OnOptionsChanged2(T2 value) => Interlocked.Exchange(ref _value2, value);

	protected virtual void Dispose(bool disposing)
	{
		if (_changeHandler1 != null || _changeHandler2 != null)
		{
			var temp1 = _changeHandler1;
			_changeHandler1 = null;
			_value1 = null;

			var temp2 = _changeHandler2;
			_changeHandler2 = null;
			_value2 = null;

			temp1?.Dispose();
			temp2?.Dispose();
		}
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(true);
	}

	~OptionsListener()
	{
		Dispose(false);
	}
}

public class OptionsListener<T1, T2, T3> : IDisposable where T1 : class where T2 : class where T3 : class
{
	public T1 Value1 => _value1 ?? throw new InvalidOperationException($"Options '{typeof(T1).Name}' hasn't been set yet.");
	public T2 Value2 => _value2 ?? throw new InvalidOperationException($"Options '{typeof(T2).Name}' hasn't been set yet.");
	public T3 Value3 => _value3 ?? throw new InvalidOperationException($"Options '{typeof(T3).Name}' hasn't been set yet.");

	private T1? _value1;
	private T2? _value2;
	private T3? _value3;
	private IDisposable? _changeHandler1;
	private IDisposable? _changeHandler2;
	private IDisposable? _changeHandler3;

	public OptionsListener(IOptionsMonitor<T1> optionsMonitor1, IOptionsMonitor<T2> optionsMonitor2, IOptionsMonitor<T3> optionsMonitor3)
	{
		OnOptionsChanged1(optionsMonitor1.CurrentValue);
		OnOptionsChanged2(optionsMonitor2.CurrentValue);
		OnOptionsChanged3(optionsMonitor3.CurrentValue);
		_changeHandler1 = optionsMonitor1.OnChange(OnOptionsChanged1);
		_changeHandler2 = optionsMonitor2.OnChange(OnOptionsChanged2);
		_changeHandler3 = optionsMonitor3.OnChange(OnOptionsChanged3);
	}

	protected virtual  void OnOptionsChanged1(T1 value) => Interlocked.Exchange(ref _value1, value);
	protected virtual  void OnOptionsChanged2(T2 value) => Interlocked.Exchange(ref _value2, value);
	protected virtual  void OnOptionsChanged3(T3 value) => Interlocked.Exchange(ref _value3, value);

	protected virtual void Dispose(bool disposing)
	{
		if (_changeHandler1 != null || _changeHandler2 != null || _changeHandler3 != null)
		{
			var temp1 = _changeHandler1;
			_changeHandler1 = null;
			_value1 = null;

			var temp2 = _changeHandler2;
			_changeHandler2 = null;
			_value2 = null;

			var temp3 = _changeHandler3;
			_changeHandler3 = null;
			_value3 = null;

			temp1?.Dispose();
			temp2?.Dispose();
			temp3?.Dispose();
		}
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(true);
	}

	~OptionsListener()
	{
		Dispose(false);
	}
}