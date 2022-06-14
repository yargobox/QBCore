namespace QBCore.ObjectFactory;

public interface IFactoryObjectDictionary<TKey, TInterface> : IReadOnlyDictionary<TKey, TInterface> where TKey : notnull
{
}