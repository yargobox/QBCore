using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QBCore.Configuration;

namespace QBCore.DataSource;

internal sealed class EfDocumentInfo : DSDocumentInfo
{
	private static List<DbContext>? _dbContexts;

	public readonly IEntityType? EntityType;

	public EfDocumentInfo(Type documentType) : base(documentType)
	{
		InitDbContexts();

		EntityType = _dbContexts.Select(x => x.Model.FindEntityType(DocumentType)).Where(x => x != null).FirstOrDefault()!;
	}

	protected override DEInfo CreateDataEntryInfo(MemberInfo memberInfo, DataEntryFlags flags, ref object? methodSharedContext)
	{
		InitDbContexts();

		if (methodSharedContext is not IEntityType entityType)
		{
			methodSharedContext = entityType = EntityType ?? _dbContexts.Select(x => x.Model.FindEntityType(DocumentType)).Where(x => x != null).FirstOrDefault()!;
		}

		return new EfDEInfo(this, memberInfo, flags, entityType);
	}

	[MemberNotNull(nameof(_dbContexts))]
	private static void InitDbContexts()
	{
		if (_dbContexts == null)
		{
			List<DbContext>? dbContexts = new(2);
			try
			{
				foreach (var type in DataSourceDocuments.DataContextProviders.Where(x => x.GetInterfaceOf(typeof(IEfDataContextProvider)) != null))
				{
					using var provider = (IEfDataContextProvider?)Activator.CreateInstance(type)
						?? throw new InvalidOperationException($"Data context provider '{type.Name}' must have a public parameterless constructor.");

					dbContexts.AddRange(provider.Infos.Select(x => provider.CreateModelOrientedDbContext(x.Name)));
				}

				if (Interlocked.CompareExchange(ref _dbContexts, dbContexts, null) == null)
				{
					dbContexts = null;
				}
			}
			finally
			{
				dbContexts?.ForEach(x => (x as IDisposable)?.Dispose());
			}
		}
	}
}