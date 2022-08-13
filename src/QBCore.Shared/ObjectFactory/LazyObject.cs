namespace QBCore.ObjectFactory;

public sealed class LazyObject<T> where T : class?
{
	private readonly Func<T> _factoryMethod;
	private T? _instance;

	public T Value
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}

			lock (_factoryMethod)
			{
				if (_instance == null)
				{
					var newInstance = _factoryMethod();

					Interlocked.Exchange<T>(ref _instance!, newInstance);
				}

				return _instance;
			}
		}
	}

	public LazyObject(Func<T> factoryMethod)
	{
		if (factoryMethod == null)
		{
			throw new ArgumentNullException(nameof(factoryMethod));
		}

		_factoryMethod = factoryMethod;
	}

	public static explicit operator T(LazyObject<T> lazyObject) => lazyObject.Value;
}