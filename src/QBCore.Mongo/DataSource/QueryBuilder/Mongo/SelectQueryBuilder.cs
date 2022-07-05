using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.DataSource.Options;
using QBCore.Extensions.Collections.Generic;
using QBCore.Extensions.Linq;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class SelectQueryBuilder<TDocument, TSelect> : QueryBuilder<TDocument, TSelect>, ISelectQueryBuilder<TDocument, TSelect>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Select;

	public override Origin Source => new Origin(this.GetType());

	public SelectQueryBuilder(QBBuilder<TDocument, TSelect> building)
		: base(building)
	{
	}

	public IMongoCollection<TDocument> Collection
	{
		get
		{
			if (_collection == null)
			{
				if (_dbContext == null)
				{
					throw new InvalidOperationException($"Database context of select query builder '{typeof(TSelect).ToPretty()}' has not been set.");
				}

				var top = Builder.Containers.FirstOrDefault(x => x.ContainerOperation == BuilderContainerOperations.Select);
				if (top?.ContainerType == BuilderContainerTypes.Table || top?.ContainerType == BuilderContainerTypes.View)
				{
					_collection = _dbContext.DB.GetCollection<TDocument>(top!.DBSideName);
				}
				else
				{
					throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TSelect).ToPretty()}'.");
				}
			}
			return _collection;
		}
	}
	IMongoCollection<TDocument>? _collection;


	public IQueryable<TDocument> AsQueryable(DataSourceQueryableOptions? options = null)
	{
		if (options?.NativeOptions != null && options.NativeOptions is not AggregateOptions)
		{
			throw new ArgumentException(nameof(options.NativeOptions));
		}
		if (options?.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
		{
			throw new ArgumentException(nameof(options.NativeClientSession));
		}

		return Collection.AsQueryable((IClientSessionHandle?)options?.NativeClientSession, (AggregateOptions?)options?.NativeOptions);
	}

	public async Task<long> CountAsync(
		IReadOnlyCollection<QBCondition>? conditions = null,
		IReadOnlyCollection<QBParameter>? parameters = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		if (options?.NativeOptions != null && options.NativeOptions is not AggregateOptions)
		{
			throw new ArgumentException(nameof(options.NativeOptions));
		}
		if (options?.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
		{
			throw new ArgumentException(nameof(options.NativeClientSession));
		}
		var aggrOptions = (AggregateOptions?)options?.NativeOptions;
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;
		_ = Collection;
		var query = BuildSelectQuery();

		await Task.CompletedTask;
		throw new NotImplementedException();
		//return await Collection.CountDocumentsAsync(Builders<TDocument>.Filter.Empty, countOptions, cancellationToken);
	}

	public async IAsyncEnumerable<TSelect> SelectAsync(
		IReadOnlyCollection<QBCondition>? conditions = null,
		IReadOnlyCollection<QBParameter>? parameters = null,
		IReadOnlyCollection<QBSortOrder>? sortOrders = null,
		long? skip = null,
		int? take = null,
		DataSourceSelectOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		if (options?.NativeOptions != null && options.NativeOptions is not AggregateOptions)
		{
			throw new ArgumentException(nameof(options.NativeOptions));
		}
		if (options?.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
		{
			throw new ArgumentException(nameof(options.NativeClientSession));
		}
		var aggrOptions = (AggregateOptions?)options?.NativeOptions;
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;
		_ = Collection;
		var query = BuildSelectQuery();

		using (var cursor = clientSessionHandle == null
			? await Collection.AggregateAsync<TSelect>(query, aggrOptions, cancellationToken)
			: await Collection.AggregateAsync<TSelect>(clientSessionHandle, query, aggrOptions, cancellationToken))
		{
			while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
			{
				foreach (var doc in cursor.Current)
				{
					yield return doc;
				}
				cancellationToken.ThrowIfCancellationRequested();
			}
		}
	}

	private List<BsonDocument> BuildSelectQuery()
	{
		var containers = Builder.Containers;
		var conditions = Builder.Conditions;
		var result = new List<BsonDocument>();

		// Add pipeline stages for each container
		//
		var stages = new OrderedDictionary<string, List<List<BuilderCondition>>>((containers.Count + 1) << 1);
		foreach (var c in containers)
		{
			stages.Add(c.Name, new List<List<BuilderCondition>>());
		}

		// Fill the connect conditions
		//
		foreach (var name in conditions.Where(x => x.IsConnect).Select(x => x.Name).Distinct())
		{
			stages[name].Add(conditions.Where(x => x.IsConnect && x.IsOnField && x.Name == name).ToList());
			stages[name].Add(conditions.Where(x => x.IsConnect && !x.IsOnField && x.Name == name).ToList());
		}

		// Slice the conditions to the smallest possible parts
		//
		var slices = SliceConditions(conditions.Where(x => !x.IsConnect));

		// Propagate the slices to the pipeline stages using the dependencies of each condition in the slice.
		// The goal is to place the slice in the first possible stage.
		//
		int firstPossibleStage;
		foreach (var slice in slices)
		{
			firstPossibleStage = 0;

			foreach (var cond in slice)
			{
				firstPossibleStage = Math.Max(firstPossibleStage, stages.IndexOf(cond.Name));

				if (cond.IsOnField)
				{
					firstPossibleStage = Math.Max(firstPossibleStage, stages.IndexOf(cond.RefName!));
				}
			}

			stages.List[firstPossibleStage].Value.Add(slice);
		}

		// Initial $match
		//
 		if (stages.List[0].Value.Count > 0)
		{
			var pl = stages.First().Value;

			var fc = BuildConditionTree(pl[0]);
			foreach (var x in pl.Skip(1))
			{
				fc!.AppendByAnd(BuildConditionTree(x)!);
			}

			result.Add(new BsonDocument(true)
			{
				{
					"$match", fc!.BsonDocument
				},
				{
					"$match", BuildFilterDefinition(pl[0])!.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry)
				}
			});
		}

		for (int i = 0; i < stages.Count; i++)
		{

		}

		return result;
	}

	private List<List<BuilderCondition>> SliceConditions(IEnumerable<BuilderCondition> conditions)
	{
		var conds = conditions.ToList();
		var list = new List<List<BuilderCondition>>();

		// Slice the condition to the smalest possible parts (that connected by AND)
		//
		// a                                => a
		// a AND b                          => a, b
		// a OR b                           => a OR b
		// (a OR b) AND c                   => a OR b, c
		// ((a OR b) AND c) AND e           => a OR b, c, e
		// ((a OR b) AND c) OR e            => ((a OR b) AND c) OR e
		//

		var count = conds.Count;
		while (count > 0)
		{
			TrimParentheses(conds);

			for (int i = 1, j = conds[0].Parentheses; i < conds.Count; i++)
			{
				// j == 0 means that this is the end of condition part, such as '(a OR b)' for '(a OR b) AND c'
				j += conds[i].Parentheses;
				if (j <= 0 && (i + 1 >= conds.Count || conds[i + 1].IsByOr == false))
				{
					list.Add(new List<BuilderCondition>(conds.Take(i + 1)));
					conds.RemoveRange(0, i + 1);
					break;
				}
			}

			if (count > conds.Count)
			{
				count = conds.Count;
				continue;
			}
			else
			{
				list.Add(new List<BuilderCondition>(conds));
				conds.Clear();
				break;
			}
		}

		for (int last = 0; last < list.Count; )
		{
			conds = list[last];

			TrimParentheses(conds);

			count = conds.Count;

			for (int i = 1, j = conds[0].Parentheses; i < conds.Count; i++)
			{
				// j == 0 means that this is the end of condition part, such as '(a OR b)' for '(a OR b) AND c'
				j += conds[i].Parentheses;
				if (j <= 0 && (i + 1 >= conds.Count || conds[i + 1].IsByOr == false))
				{
					if (i + 1 < conds.Count)
					{
						list.Insert(last, new List<BuilderCondition>(conds.Take(i + 1)));
						conds.RemoveRange(0, i + 1);
					}
					break;
				}
			}

			if (count == conds.Count)
			{
				last++;
			}
		}

		return list;
	}
	private BuilderCondition EnsureByAndCondition(BuilderCondition cond)
	{
		return cond.IsByOr ? (cond with { IsByOr = false }) : cond;
	}
	private static void TrimParentheses(List<BuilderCondition> conds)
	{
		if (conds.Count == 0) return;
		if (conds.Count == 1)
		{
			System.Diagnostics.Debug.Assert(conds[0].Parentheses == 0);
			if (conds[0].Parentheses != 0)
			{
				conds[0] = conds[0] with { Parentheses = 0 };
			}
			return;
		}

		var first = conds[0].Parentheses;
		System.Diagnostics.Debug.Assert(first >= 0);
		if (first <= 0) return;

		var last = conds.Last().Parentheses;
		System.Diagnostics.Debug.Assert(last <= 0);
		if (last >= 0) return;

		int sum = 0, min = 0;
		for (int i = 1; i < conds.Count - 1; i++)
		{
			sum += conds[i].Parentheses;
			System.Diagnostics.Debug.Assert(sum + first >= 0);
			if (sum + first <= 0) return;
			if (sum < min) min = sum;
		}
		// (((  (a || b) && c                        )))		4 -1(-1) -3
		// (    (a || b) && (b || c)                 )			2  0(-1) -2
		//      (a || b) && (b || c)
		//      ((a || b) && (b || c)) || (d && e)
		// (    ((a || b) && (b || c)) || (d && e)   )			3 -1(-2) -2

		System.Diagnostics.Debug.Assert(first + sum + last == 0);

		conds[0] = conds[0] with { Parentheses =  first + min };
		conds[conds.Count - 1] = conds[conds.Count - 1] with { Parentheses = last + (first + min) };
	}

	#region Build filter conditions

	private static MethodInfo _buildCondition = typeof(SelectQueryBuilder<TDocument, TSelect>).GetMethod(nameof(BuildCondition), BindingFlags.Static | BindingFlags.NonPublic)
		?? throw new NotImplementedException("Failed to find method " + nameof(BuildCondition));

	private BuiltCondition? BuildConditionTree(IEnumerable<BuilderCondition> conditions)
	{
		BuiltCondition? filter = null;
		bool moveNext = true;

		using (var e = conditions.GetEnumerator())
		{
			while (moveNext && (moveNext = e.MoveNext()))
			{
				if (filter == null)
				{
					if (e.Current.Parentheses > 0)
					{
						filter = BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1).filter;
					}
					else
					{
						filter = new BuiltCondition(BuildCondition(e.Current, Builder));
					}
				}
				else if (e.Current.IsByOr)
				{
					if (e.Current.Parentheses > 0)
					{
						filter.AppendByOr(BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1).filter);
					}
					else
					{
						filter.AppendByOr(new BuiltCondition(BuildCondition(e.Current, Builder)));
					}
				}
				else
				{
					if (e.Current.Parentheses > 0)
					{
						filter.AppendByAnd(BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1).filter);
					}
					else
					{
						filter.AppendByAnd(new BuiltCondition(BuildCondition(e.Current, Builder)));
					}
				}
			}

			return filter;
		}
	}
	private (BuiltCondition filter, int level) BuildConditionTree(IEnumerator<BuilderCondition> e, ref bool moveNext, int parentheses)
	{
		BuiltCondition filter;

		if (parentheses > 0)
		{
			var result = BuildConditionTree(e, ref moveNext, parentheses - 1);
			filter = result.filter;
			if (result.level < 0)
			{
				return (filter, result.level + 1);
			}
		}
		else
		{
			filter = new BuiltCondition(BuildCondition(e.Current, Builder));
		}

		while (moveNext && (moveNext = e.MoveNext()))
		{
			if (e.Current.IsByOr)
			{
				if (e.Current.Parentheses > 0)
				{
					var result = BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1);
					filter.AppendByOr(result.filter);
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter.AppendByOr(new BuiltCondition(BuildCondition(e.Current, Builder)));
					if (e.Current.Parentheses < 0)
					{
						return (filter, e.Current.Parentheses + 1);
					}
				}
			}
			else
			{
				if (e.Current.Parentheses > 0)
				{
					var result = BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1);
					filter.AppendByAnd(result.filter);
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter.AppendByAnd(new BuiltCondition(BuildCondition(e.Current, Builder)));
					if (e.Current.Parentheses < 0)
					{
						return (filter, e.Current.Parentheses + 1);
					}
				}
			}
		}

		return (filter, 0);
	}

	public static BsonDocument BuildCondition<TContainer, TProjection>(BuilderCondition cond, QBBuilder<TContainer, TProjection> builder)
	{
		var pfn = _buildCondition.MakeGenericMethod(cond.FieldDeclaringType, cond.RefFieldDeclaringType ?? typeof(NotSupported), typeof(TContainer), typeof(TProjection));
		return (BsonDocument) pfn.Invoke(null, new object?[] { false, cond, builder, null })!;//!!!
	}
	private static BsonDocument BuildCondition<TLocal, TRef, TContainer, TProjection>(bool useExprFormat, BuilderCondition cond, QBBuilder<TContainer, TProjection> builder, IReadOnlyDictionary<string, object?> arguments)
	{
		if (cond.IsOnField)
		{
			throw new NotImplementedException();
		}
		else if (cond.IsOnParam)
		{
			var paramName = (string)cond.Value!;
			var param = builder.Parameters.First(x => x.Name == paramName);
			//!!!
			var value = arguments[paramName];

			return MakeConditionWithConstantValue(useExprFormat, cond, value);
		}
		else if (cond.IsOnConst)
		{
			return MakeConditionWithConstantValue(useExprFormat, cond);
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	#endregion

	#region  Build filter definition (Mongo FilterDefinition<>)
	private FilterDefinition<TDocument>? BuildFilterDefinition(IEnumerable<BuilderCondition> conditions)
	{
		FilterDefinition<TDocument>? filter = null;
		bool moveNext = true;

		using (var e = conditions.GetEnumerator())
		{
			while (moveNext && (moveNext = e.MoveNext()))
			{
				if (filter == null)
				{
					if (e.Current.Parentheses > 0)
					{
						filter = BuildFilterDefinition(e, ref moveNext, e.Current.Parentheses - 1).filter;
					}
					else
					{
						filter = BuildFilterDefinition(e.Current);
					}
				}
				else if (e.Current.IsByOr)
				{
					if (e.Current.Parentheses > 0)
					{
						filter |= BuildFilterDefinition(e, ref moveNext, e.Current.Parentheses - 1).filter;
					}
					else
					{
						filter |= BuildFilterDefinition(e.Current);
					}
				}
				else
				{
					if (e.Current.Parentheses > 0)
					{
						filter &= BuildFilterDefinition(e, ref moveNext, e.Current.Parentheses - 1).filter;
					}
					else
					{
						filter &= BuildFilterDefinition(e.Current);
					}
				}
			}

			return filter;
		}
	}
	private (FilterDefinition<TDocument> filter, int level) BuildFilterDefinition(IEnumerator<BuilderCondition> e, ref bool moveNext, int parentheses)
	{
		FilterDefinition<TDocument> filter;

		if (parentheses > 0)
		{
			var result = BuildFilterDefinition(e, ref moveNext, parentheses - 1);
			filter = result.filter;
			if (result.level < 0)
			{
				return (filter, result.level + 1);
			}
		}
		else
		{
			filter = BuildFilterDefinition(e.Current);
		}

		while (moveNext && (moveNext = e.MoveNext()))
		{
			if (e.Current.IsByOr)
			{
				if (e.Current.Parentheses > 0)
				{
					var result = BuildFilterDefinition(e, ref moveNext, e.Current.Parentheses - 1);
					filter |= result.filter;
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter |= BuildFilterDefinition(e.Current);
					if (e.Current.Parentheses < 0)
					{
						return (filter, e.Current.Parentheses + 1);
					}
				}
			}
			else
			{
				if (e.Current.Parentheses > 0)
				{
					var result = BuildFilterDefinition(e, ref moveNext, e.Current.Parentheses - 1);
					filter &= result.filter;
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter &= BuildFilterDefinition(e.Current);
					if (e.Current.Parentheses < 0)
					{
						return (filter, e.Current.Parentheses + 1);
					}
				}
			}
		}

		return (filter, 0);
	}
	private static FilterDefinition<TDocument> BuildFilterDefinition(BuilderCondition cond)
	{
		switch (cond.Operation)
		{
			case ConditionOperations.Equal:
				return Builders<TDocument>.Filter.Eq(cond.FieldPath, cond.Value);
			case ConditionOperations.NotEqual:
				return Builders<TDocument>.Filter.Ne(cond.FieldPath, cond.Value);
			case ConditionOperations.IsNull:
				return Builders<TDocument>.Filter.Eq(cond.FieldPath, (object?)null);
			default:
				throw new NotImplementedException();
		}
	}

	#endregion
}