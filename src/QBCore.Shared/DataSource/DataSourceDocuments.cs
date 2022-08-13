using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public static class DataSourceDocuments
{
	private static Func<Type, bool> _documentExclusionSelector = _ => false;

	public static Func<Type, bool> DocumentExclusionSelector
	{
		get => _documentExclusionSelector;
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Interlocked.Exchange(ref _documentExclusionSelector, value);
		}
	}

	public static LazyObject<DSDocumentInfo> GetOrRegister(Type concreteType, Func<Type, LazyObject<DSDocumentInfo>> factoryMethod)
		=> GetOrRegister(StaticFactory.Documents, concreteType, factoryMethod);

	public static LazyObject<DSDocumentInfo> GetOrRegister(this IFactoryObjectDictionary<Type, LazyObject<DSDocumentInfo>> @this, Type concreteType, Func<Type, LazyObject<DSDocumentInfo>> factoryMethod)
		=> ((IFactoryObjectRegistry<Type, LazyObject<DSDocumentInfo>>)@this).TryGetOrRegisterObject(concreteType, factoryMethod);
}