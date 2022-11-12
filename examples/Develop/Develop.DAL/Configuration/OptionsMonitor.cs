using Microsoft.Extensions.Options;

namespace Develop.DAL.Configuration;

public sealed class OptionsMonitor<T> : IDisposable where T : class
{
	public T Value => _value ?? throw new InvalidOperationException($"Options '{typeof(T).Name}' hasn't been set yet.");

	private T? _value;
	private IDisposable? _changeHandler;

	public OptionsMonitor(IOptionsMonitor<T> optionsMonitor)
	{
		_value = optionsMonitor.CurrentValue;
		_changeHandler = optionsMonitor.OnChange(OnOptionsChanged);
	}

	private void OnOptionsChanged(T value)
	{
		_value = value;
	}

	public void Dispose()
	{
		if (_changeHandler != null)
		{
			GC.SuppressFinalize(this);

			var temp = _changeHandler;
			_changeHandler = null;
			_value = null;
			temp.Dispose();
		}
	}

	~OptionsMonitor()
	{
		Dispose();
	}
}