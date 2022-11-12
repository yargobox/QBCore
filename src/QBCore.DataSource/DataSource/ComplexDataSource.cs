using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using QBCore.Extensions.Collections.Generic;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public abstract class ComplexDataSource<TComplexDataSource> : IComplexDataSource where TComplexDataSource : notnull, IComplexDataSource
{
	public ICDSInfo CDSInfo { get; }
	public IReadOnlyDictionary<string, IDataSource> Nodes { get; }
	public IDataSource Root => Nodes.Values.First();

	protected readonly IServiceProvider _serviceProvider;

	public ComplexDataSource(IServiceProvider serviceProvider)
	{
		if (serviceProvider == null)
		{
			throw new ArgumentNullException(nameof(serviceProvider));
		}

		_serviceProvider = serviceProvider;

		CDSInfo = StaticFactory.ComplexDataSources[typeof(TComplexDataSource)];

		var orderedDictionary = new OrderedDictionary<string, Lazy<IDataSource>>(CDSInfo.Nodes.Count, StringComparer.OrdinalIgnoreCase);
		Nodes = new NodeDictionary(orderedDictionary);

		foreach (var nodeInfo in CDSInfo.Nodes)
		{
			var dataSOurceType = typeof(ITransient<>).MakeGenericType(nodeInfo.Value.DataSourceType);
			var keyName = new DSKeyName(CDSInfo.Name, nodeInfo.Value.Name);

			orderedDictionary.Add(nodeInfo.Key, new Lazy<IDataSource>(() => CreateNode(dataSOurceType, keyName), LazyThreadSafetyMode.PublicationOnly));
		}
	}

	private IDataSource CreateNode(Type dataSourceType, DSKeyName keyName)
	{
		var pDS = (IDataSource) _serviceProvider.GetRequiredService(dataSourceType);
		pDS.Init(keyName, false);
		return pDS;
	}

	internal class NodeDictionary : IReadOnlyDictionary<string, IDataSource>
	{
		public readonly OrderedDictionary<string, Lazy<IDataSource>> Nodes;

		public NodeDictionary(OrderedDictionary<string, Lazy<IDataSource>> nodes) => Nodes = nodes;

		public IDataSource this[string key] => Nodes[key].Value;
		public IEnumerable<string> Keys => Nodes.Keys;
		public IEnumerable<IDataSource> Values => Nodes.Values.Select(x => x.Value);
		public int Count => Nodes.Count();

		public bool ContainsKey(string key) => Nodes.ContainsKey(key);

		public IEnumerator<KeyValuePair<string, IDataSource>> GetEnumerator()
			=> Nodes.Select(x => KeyValuePair.Create(x.Key, x.Value.Value)).GetEnumerator();

		public bool TryGetValue(string key, [MaybeNullWhen(false)] out IDataSource value)
		{
			value = Nodes.GetValueOrDefault(key)?.Value;
			return value != null;
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}