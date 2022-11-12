using QBCore.DataSource.Core;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore, TDataSource>
{
	protected bool TryAddInternalObject(OKeyName okeyName, object? obj)
	{
		throw new NotImplementedException();
	}
	protected void AddInternalObject(OKeyName okeyName, object? obj)
	{
		throw new NotImplementedException();
	}
	protected object? SetInternalObject(OKeyName okeyName, object? obj)
	{
		throw new NotImplementedException();
	}
	protected bool RemoveInternalObject(OKeyName okeyName)
	{
		throw new NotImplementedException();
	}

	public void Init(DSKeyName? keyName = null, bool shared = true)
	{
		if (_okeyName == null)
		{
			keyName ??= new DSKeyName(DSInfo.Name);

			if (shared)
			{
				lock (SyncRoot)
				{
					if (_okeyName == null)
					{
						InitInternal(keyName);

						_okeyName = keyName;
					}
				}
			}
			else
			{
				InitInternal(keyName);

				_okeyName = keyName;
			}
		}
	}

	private void InitInternal(DSKeyName keyName)
	{
		if (keyName.ForField == null && keyName.CDSName != null)
		{
			var cdsInfo = StaticFactory.AppObjects[keyName.CDSName].AsCDSInfo();
			var node = cdsInfo.Nodes[keyName.DSOrNodeName];
			if (node.Parent != null && node.Parent.Conditions.Any())
			{
				var listener = new CDSChildNodeDSListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>(node.Parent.Conditions);
				AttachListener(listener, true);
			}
		}
	}
}