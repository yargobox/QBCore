namespace QBCore.ObjectFactory;

public interface IFactoryObjectRegistry<TKey, TInterface> : IReadOnlyDictionary<TKey, TInterface> where TKey : notnull
{
	void RegisterObject(TKey key, TInterface value);
	bool TryRegisterObject(TKey key, TInterface value);
	TInterface TryGetOrRegisterObject(TKey key, Func<TKey, TInterface> factoryMethod);
}