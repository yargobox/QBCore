using System.Linq.Expressions;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.EfCore;

internal sealed class SoftDelQBBuilder<TDoc, TDelete> : CommonQBBuilder<TDoc, TDelete>, IEfCoreSoftDelQBBuilder<TDoc, TDelete> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public SoftDelQBBuilder()
	{
		if (DocInfo.IdField == null)
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have an id data entry.");
		var deDeleted = DocInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{typeof(TDoc).ToPretty()}' has a readonly date deletion field!");
	}
	public SoftDelQBBuilder(SoftDelQBBuilder<TDoc, TDelete> other) : base(other) { }
	public SoftDelQBBuilder(IQBBuilder other) : this()
	{
		if (other is null) throw new ArgumentNullException(nameof(other));

		if (other.DocType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make soft delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TDelete).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		other.Prepare();

		var top = other.Containers.FirstOrDefault();
		if (top?.DocumentType != typeof(TDoc) || top.ContainerType != ContainerTypes.Table)
		{
			throw new InvalidOperationException($"Could not make soft delete query builder '{typeof(TDoc).ToPretty()}, {typeof(TDelete).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		AutoBuild(top.DBSideName);
	}

	protected override void OnPrepare()
	{
		var top = Containers.FirstOrDefault();
		if (top?.ContainerOperation != ContainerOperations.Update)
		{
			throw new InvalidOperationException($"Incompatible configuration of soft delete query builder '{typeof(TDelete).ToPretty()}'.");
		}

		if (Conditions.Count == 0)
		{
			throw EX.QueryBuilder.Make.QueryBuilderMustHaveAtLeastOneCondition(DataLayer.Name, QueryBuilderType.ToString());
		}
	}

	public override QBBuilder<TDoc, TDelete> AutoBuild(string? tableName = null)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Soft delete query builder '{typeof(TDelete).ToPretty()}' has already been initialized.");
		}

		Update(tableName)
			.Condition(DocInfo.IdField!, FO.Equal, "@id")
			.Condition(DocInfo.DateDeletedField!, null, FO.IsNull);

		return this;
	}
	IEfCoreSoftDelQBBuilder<TDoc, TDelete> IEfCoreSoftDelQBBuilder<TDoc, TDelete>.AutoBuild(string? tableName)
	{
		AutoBuild(tableName);
		return this;
	}

	public override QBBuilder<TDoc, TDelete> Update(string? tableName = null)
	{
		return AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
	}
	IEfCoreSoftDelQBBuilder<TDoc, TDelete> IEfCoreSoftDelQBBuilder<TDoc, TDelete>.Update(string? tableName)
	{
		AddContainer(tableName, ContainerTypes.Table, ContainerOperations.Update);
		return this;
	}

	IEfCoreSoftDelQBBuilder<TDoc, TDelete> IEfCoreSoftDelQBBuilder<TDoc, TDelete>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
	{
		AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
		return this;
	}
	IEfCoreSoftDelQBBuilder<TDoc, TDelete> IEfCoreSoftDelQBBuilder<TDoc, TDelete>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
	{
		AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
		return this;
	}

	IEfCoreSoftDelQBBuilder<TDoc, TDelete> IEfCoreSoftDelQBBuilder<TDoc, TDelete>.Begin()
	{
		Begin();
		return this;
	}
	IEfCoreSoftDelQBBuilder<TDoc, TDelete> IEfCoreSoftDelQBBuilder<TDoc, TDelete>.End()
	{
		End();
		return this;
	}

	IEfCoreSoftDelQBBuilder<TDoc, TDelete> IEfCoreSoftDelQBBuilder<TDoc, TDelete>.And()
	{
		And();
		return this;
	}
	IEfCoreSoftDelQBBuilder<TDoc, TDelete> IEfCoreSoftDelQBBuilder<TDoc, TDelete>.Or()
	{
		Or();
		return this;
	}
}