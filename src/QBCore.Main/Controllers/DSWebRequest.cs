using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using QBCore.DataSource;
using QBCore.Extensions.Collections.Generic;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

public class DSWebRequest : DSRequest
{
	public override DSRequestCommands Command => _command;
	public override IDataSource DS => _ds;
	public override IComplexDataSource? CDS => _cds;
	public override IDataSource? ForDS => _forDS;
	public override DEPath? ForDE => _forDE;
	public override IReadOnlyDictionary<string, object?> CurrentRecordIDs => _currentRecordIDs;

	private readonly DSRequestCommands _command;
	private readonly IDataSource _ds;
	private readonly IComplexDataSource? _cds;
	private readonly IDataSource? _forDS;
	private readonly DEPath? _forDE;
	private readonly OrderedDictionary<string, object?> _currentRecordIDs;

	public DSWebRequest(IServiceProvider serviceProvider, ControllerContext controllerContext)
	{
		_currentRecordIDs = new OrderedDictionary<string, object?>(controllerContext.RouteData.Values.Count / 2 + 1);

		ICDSInfo? pCDSInfo = null;
		IDSInfo? pDSInfo = null;
		int i, controllerIndex = 0;
		string? stringValue;

		foreach (var routePart in controllerContext.RouteData.Values)
		{
			stringValue = routePart.Value as string;

			if (routePart.Key == "controller")
			{
				AddControllerToRoute(_currentRecordIDs, stringValue, ref pCDSInfo, out pDSInfo);

				if (pCDSInfo != null)
				{
					_cds = (IComplexDataSource)serviceProvider.GetRequiredService(pCDSInfo.ConcreteType);
					_ds = _cds.Nodes[stringValue];
				}
				else
				{
					_ds = (IDataSource)serviceProvider.GetRequiredService(pDSInfo.ConcreteType);
				}
			}
			else if (routePart.Key == "id")
			{
				if (_currentRecordIDs.Count == 0)
				{
					throw new ApplicationException("'id' has to come after a controller in the route.");
				}

				_currentRecordIDs.At(_currentRecordIDs.Count - 1, stringValue);
			}
			else if (routePart.Key == "action")
			{
				_command |= stringValue switch
				{
					"create" => DSRequestCommands.Create,
					"index" => DSRequestCommands.Index,
					"get" => DSRequestCommands.Get,
					"update" => DSRequestCommands.Update,
					"delete" => DSRequestCommands.Delete,
					"restore" => DSRequestCommands.Restore,
					_ => throw new ApplicationException($"Unknown datasource controller action '{stringValue}'")
				};
			}
			else if (routePart.Key == "filter_field")
			{
				if (pDSInfo == null)
				{
					throw new ApplicationException("'filter' has to come after its controller in the route.");
				}

				_command |= DSRequestCommands.FilterFlag;
				_forDS = (IDataSource)serviceProvider.GetRequiredService(pDSInfo.ConcreteType);
				_forDE = new DEPath(pDSInfo.DSTypeInfo.TSelect, stringValue!, true, false) ?? throw new KeyNotFoundException($"Unknown field name '{stringValue}'.");
			}
			else if (routePart.Key == "cell_field")
			{
				if (pDSInfo == null)
				{
					throw new ApplicationException("'filter' has to come after its controller in the route.");
				}

				_command |= DSRequestCommands.CellFlag;
				_forDS = (IDataSource)serviceProvider.GetRequiredService(pDSInfo.ConcreteType);
				_forDE = new DEPath(pDSInfo.DSTypeInfo.TSelect, stringValue!, true, false) ?? throw new KeyNotFoundException($"Unknown field name '{stringValue}'.");
			}
			else if (routePart.Key.Length > 1)
			{
				if (!int.TryParse(routePart.Key.AsSpan(1), out i) || i != controllerIndex)
				{
					throw new ApplicationException("Unsupported controller route.");
				}

				if (routePart.Key[0] == 'c')
				{
					AddControllerToRoute(_currentRecordIDs, stringValue, ref pCDSInfo, out pDSInfo);
				}
				else if (routePart.Key[0] == 'i')
				{
					if (_currentRecordIDs.Count == 0)
					{
						throw new ApplicationException("'id' has to come after a controller in the route.");
					}

					controllerIndex++;
					_currentRecordIDs.At(_currentRecordIDs.Count - 1, stringValue);
				}
				else
				{
					throw new ApplicationException("Unsupported controller route.");
				}
			}
			else
			{
				throw new ApplicationException("Unsupported controller route.");
			}
		}

		if (_currentRecordIDs.Count == 0 || _ds == null)
		{
			throw new ApplicationException("Unsupported controller route.");
		}

		var query = controllerContext.HttpContext.Request.Query;
		StringValues stringValues;
		if (query.TryGetValue("depends", out stringValues) && stringValues == "1")
		{
			_command |= DSRequestCommands.RefreshDependsFlag;
		}

		switch (_command)
		{
			case DSRequestCommands.Create:
			case DSRequestCommands.Index:
			case DSRequestCommands.Get:
			case DSRequestCommands.Update:
			case DSRequestCommands.Delete:
			case DSRequestCommands.Restore:
			case DSRequestCommands.RefreshOnCreate:
			case DSRequestCommands.RefreshOnUpdate:
			case DSRequestCommands.IndexForFilter:
			case DSRequestCommands.GetForFilter:
			case DSRequestCommands.IndexForCell:
			case DSRequestCommands.GetForCell: break;
			default: throw new KeyNotFoundException("Unsupported controller route request.");
		}
	}

	private static void AddControllerToRoute(OrderedDictionary<string, object?> currentRecordIDs, [NotNull] string? controllerName, ref ICDSInfo? pCDSInfo, out IDSInfo pDSInfo)
	{
		if (string.IsNullOrEmpty(controllerName)) throw new KeyNotFoundException("Unknown controller name!");

		if (pCDSInfo != null)
		{
			var node = pCDSInfo.Nodes.GetValueOrDefault(controllerName) ?? throw new KeyNotFoundException($"Unknown controller name '{controllerName}'.");
			pDSInfo = node.DSInfo;

			currentRecordIDs.Add(node.Name, null);
		}
		else
		{
			var pAppObjectInfo = StaticFactory.AppObjectByControllerNames.GetValueOrDefault(controllerName);

			if (currentRecordIDs.Count == 0 && pAppObjectInfo is ICDSInfo pCDSInfoInternal)
			{
				pCDSInfo = pCDSInfoInternal;

				var rootNode = pCDSInfo.Nodes.First().Value;
				pDSInfo = rootNode.DSInfo;

				currentRecordIDs.Add(rootNode.Name, null);
			}
			else if (pAppObjectInfo is IDSInfo pDSInfoInternal)
			{
				pDSInfo = pDSInfoInternal;

				currentRecordIDs.Add(pDSInfo.ControllerName ?? controllerName, null);
			}
			else
			{
				throw new KeyNotFoundException($"Unknown controller name '{controllerName}'.");
			}
		}
	}
}