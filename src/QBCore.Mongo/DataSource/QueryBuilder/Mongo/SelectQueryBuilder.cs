using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed partial class SelectQueryBuilder<TDoc, TSelect> : QueryBuilder<TDoc, TSelect>, ISelectQueryBuilder<TDoc, TSelect>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Select;

	public SelectQueryBuilder(SelectQBBuilder<TDoc, TSelect> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public IMongoCollection<TDoc> Collection
	{
		get
		{
			if (_collection == null)
			{
				var top = Builder.Containers.FirstOrDefault(x => x.ContainerOperation == ContainerOperations.Select);
				if (top?.ContainerType == ContainerTypes.Table || top?.ContainerType == ContainerTypes.View)
				{
					_collection = _mongoDbContext.AsMongoDatabase().GetCollection<TDoc>(top!.DBSideName);
				}
				else
				{
					throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TSelect).ToPretty()}'.");
				}
			}
			return _collection;
		}
	}
	IMongoCollection<TDoc>? _collection;

	public IQueryable<TDoc> AsQueryable(DataSourceQueryableOptions? options = null)
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

	private (List<BsonDocument> query, AggregateOptions? aggregateOptions, IClientSessionHandle? clientSessionHandle) CountImpl(DataSourceCountOptions? options)
	{
		if (options != null)
		{
			if (options.Skip < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(options.Skip));
			}
			if (options.NativeOptions != null && options.NativeOptions is not AggregateOptions)
			{
				throw new ArgumentException(nameof(options.NativeOptions));
			}
			if (options.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
			{
				throw new ArgumentException(nameof(options.NativeClientSession));
			}
			if (options.NativeSelectQuery != null && options.NativeSelectQuery is not List<BsonDocument>)
			{
				throw new ArgumentException(nameof(options.NativeSelectQuery));
			}
		}

		_ = Collection;

		var aggregateOptions = (AggregateOptions?)options?.NativeOptions;
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;
		var selectQuery = (List<BsonDocument>?)options?.NativeSelectQuery;
		List<BsonDocument> query;
		bool isCountQuery = false;

		if (selectQuery != null)
		{
			// selecQuery is our counting aggregation query?
			{
				var lastElem = selectQuery.LastOrDefault();
				if (lastElem != null && lastElem.BsonType == BsonType.Document && lastElem.Contains("$group"))
				{
					var groupElem = lastElem["$group"];
					if (groupElem.BsonType == BsonType.Document && groupElem.AsBsonDocument.Contains("_id") && groupElem.AsBsonDocument["_id"] == BsonNull.Value)
					{
						if (groupElem.AsBsonDocument.Contains("n"))
						{
							var nElem = groupElem.AsBsonDocument["n"];
							if (nElem.BsonType == BsonType.Document && nElem.AsBsonDocument.Contains("$sum"))
							{
								isCountQuery = nElem.AsBsonDocument["$sum"] == 1;
							}
						}
					}
				}
			}

			if (isCountQuery)
			{
				// update, remove, or insert $skip
				var index = selectQuery.FindLastIndex(x => x.BsonType == BsonType.Document && x.Contains("$skip"));
				if (index >= 0)
				{
					if (options?.Skip > 0)
					{
						selectQuery.ElementAt(index)["$skip"] = options.Skip;
					}
					else
					{
						selectQuery.RemoveAt(index);
					}
				}
				else if (options?.Skip > 0)
				{
					index = selectQuery.FindLastIndex(x => x.BsonType == BsonType.Document && x.Contains("$limit"));
					if (index < 0)
					{
						selectQuery.Insert(selectQuery.Count - 1, new BsonDocument { { "$skip", options.Skip } });
					}
					else
					{
						selectQuery.Insert(index, new BsonDocument { { "$skip", options.Skip } });
					}
				}

				// update, remove, or insert $limit
				index = selectQuery.FindLastIndex(x => x.BsonType == BsonType.Document && x.Contains("$limit"));
				if (index >= 0)
				{
					if (options?.CountNoMoreThan >= 0)
					{
						selectQuery.ElementAt(index)["$limit"] = options.CountNoMoreThan;
					}
					else
					{
						selectQuery.RemoveAt(index);
					}
				}
				else if (options?.CountNoMoreThan >= 0)
				{
					index = selectQuery.FindLastIndex(x => x.BsonType == BsonType.Document && x.Contains("$skip"));
					if (index < 0)
					{
						selectQuery.Insert(selectQuery.Count - 1, new BsonDocument { { "$limit", options.CountNoMoreThan } });
					}
					else
					{
						selectQuery.Insert(index + 1, new BsonDocument { { "$limit", options.CountNoMoreThan } });
					}
				}
			}
			else
			{
				// remove $limit
				var index = selectQuery.FindLastIndex(x => x.BsonType == BsonType.Document && x.Contains("$limit"));
				if (index >= 0) selectQuery.RemoveAt(index);

				// remove $skip
				index = selectQuery.FindLastIndex(x => x.BsonType == BsonType.Document && x.Contains("$skip"));
				selectQuery.RemoveAt(index);

				// remove all $sort
				while ((index = selectQuery.FindLastIndex(x => x.BsonType == BsonType.Document && x.Contains("$sort"))) >= 0)
				{
					selectQuery.RemoveAt(index);
				}

				// remove all last $project
				for (var i = selectQuery.Count - 1; i >= 0; i = selectQuery.Count - 1)
				{
					var elem = selectQuery.ElementAt(i);
					if (elem.BsonType == BsonType.Document && elem.Contains("$project"))
					{
						selectQuery.RemoveAt(i);
						continue;
					}
					break;
				}
			}

			query = selectQuery;
		}
		else
		{
			// build query from scratch
			var stages = BuildSelectPipelineStages(Builder.Containers, Builder.Connects, Builder.Conditions, Builder.Fields, Array.Empty<QBSortOrder>());
			
			query = BuildSelectQuery(stages, Builder.Parameters);
		}

		if (!isCountQuery)
		{
			// add $skip and $limit
			if (options != null)
			{
				if (options.Skip > 0)
				{
					query.Add(new BsonDocument { { "$skip", options.Skip } });
				}
				if (options.CountNoMoreThan >= 0)
				{
					query.Add(new BsonDocument { { "$limit", options.CountNoMoreThan } });
				}
			}

			// add the counting aggregation
			query.Add(new BsonDocument {
							{ "$group", new BsonDocument {
									{ "_id", BsonNull.Value },
									{ "n", new BsonDocument {
											{ "$sum", 1 }
										}
									}
							} } });
		}

		return (query, aggregateOptions, clientSessionHandle);
	}

	public async Task<long> CountAsync(DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		var prep = CountImpl(options);
		
		if (options != null)
		{
			if (options.QueryStringCallbackAsync != null)
			{
				var queryString = string.Concat("db.", Builder.Containers.First().DBSideName, ".aggregate(", new BsonArray(prep.query).ToString(), ");");
				await options.QueryStringCallbackAsync(queryString).ConfigureAwait(false);
			}
			else if (options.QueryStringCallback != null)
			{
				var queryString = string.Concat("db.", Builder.Containers.First().DBSideName, ".aggregate(", new BsonArray(prep.query).ToString(), ");");
				options.QueryStringCallback(queryString);
			}
		}

		var cursor = prep.clientSessionHandle == null
			? await Collection.AggregateAsync<TSelect>(prep.query, prep.aggregateOptions, cancellationToken)
			: await Collection.AggregateAsync<TSelect>(prep.clientSessionHandle, prep.query, prep.aggregateOptions, cancellationToken);
		
		var bsonTotalCount = (await cursor.FirstAsync(cancellationToken)).ToBsonDocument();
		return bsonTotalCount["n"].ToInt64();
	}
	
	public long Count(DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		var prep = CountImpl(options);

		if (options != null)
		{
			if (options.QueryStringCallback != null)
			{
				var queryString = string.Concat("db.", Builder.Containers.First().DBSideName, ".aggregate(", new BsonArray(prep.query).ToString(), ");");
				options.QueryStringCallback(queryString);
			}
			else if (options.QueryStringCallbackAsync != null)
			{
				throw new NotSupportedException($"Incompatible options of select query builder '{typeof(TSelect).ToPretty()}': '{nameof(DataSourceCountOptions.QueryStringCallbackAsync)}' is not supported in sync method.");
			}
		}

		var cursor = prep.clientSessionHandle == null
			? Collection.Aggregate<TSelect>(prep.query, prep.aggregateOptions, cancellationToken)
			: Collection.Aggregate<TSelect>(prep.clientSessionHandle, prep.query, prep.aggregateOptions, cancellationToken);

		var bsonTotalCount = cursor.First(cancellationToken).ToBsonDocument();
		return bsonTotalCount["n"].ToInt64();
	}

	private (List<BsonDocument> query, AggregateOptions? aggregateOptions, IClientSessionHandle? clientSessionHandle) SelectImpl(long skip, int take, DataSourceSelectOptions? options)
	{
		if (skip < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(skip));
		}
		if (options != null)
		{
			if (options.NativeOptions != null && options.NativeOptions is not AggregateOptions)
			{
				throw new ArgumentException(nameof(options.NativeOptions));
			}
			if (options.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
			{
				throw new ArgumentException(nameof(options.NativeClientSession));
			}
		}

		_ = Collection;

		var aggregateOptions = (AggregateOptions?)options?.NativeOptions;
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;
		var stages = BuildSelectPipelineStages(Builder.Containers, Builder.Connects, Builder.Conditions, Builder.Fields, Builder.SortOrders);
		var query = BuildSelectQuery(stages, Builder.Parameters);

		if (skip > 0L)
		{
			query.Add(new BsonDocument { { "$skip", skip } });
		}
		if (take >= 0)
		{
			query.Add(new BsonDocument { { "$limit", (uint)take + (options?.ObtainLastPageMark == true ? 1U : 0U) } });
		}

		return (query, aggregateOptions, clientSessionHandle);
	}

	public async Task<IDSAsyncCursor<TSelect>> SelectAsync(long skip = 0L, int take = -1, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		var prep = SelectImpl(skip, take, options);

		if (options != null)
		{
			if (options.NativeSelectQueryCallback != null)
			{
				options.NativeSelectQueryCallback(prep.query);
			}
			if (options.QueryStringCallbackAsync != null)
			{
				var queryString = string.Concat("db.", Builder.Containers.First().DBSideName, ".aggregate(", new BsonArray(prep.query).ToString(), ");");
				await options.QueryStringCallbackAsync(queryString).ConfigureAwait(false);
			}
			else if (options.QueryStringCallback != null)
			{
				var queryString = string.Concat("db.", Builder.Containers.First().DBSideName, ".aggregate(", new BsonArray(prep.query).ToString(), ");");
				options.QueryStringCallback(queryString);
			}
		}

		var cursor = prep.clientSessionHandle == null
			? await Collection.AggregateAsync<TSelect>(prep.query, prep.aggregateOptions, cancellationToken)
			: await Collection.AggregateAsync<TSelect>(prep.clientSessionHandle, prep.query, prep.aggregateOptions, cancellationToken);

		return options?.ObtainLastPageMark == true
			? new DSAsyncCursorWithLastPageMark<TSelect>(cursor, take, cancellationToken)
			: new DSAsyncCursor<TSelect>(cursor, cancellationToken);
	}

	public IDSAsyncCursor<TSelect> Select(long skip = 0L, int take = -1, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		var prep = SelectImpl(skip, take, options);

		if (options != null)
		{
			if (options.NativeSelectQueryCallback != null)
			{
				options.NativeSelectQueryCallback(prep.query);
			}
			if (options.QueryStringCallback != null)
			{
				var queryString = string.Concat("db.", Builder.Containers.First().DBSideName, ".aggregate(", new BsonArray(prep.query).ToString(), ");");
				options.QueryStringCallback(queryString);
			}
			else if (options.QueryStringCallbackAsync != null)
			{
				throw new NotSupportedException($"Incompatible options of select query builder '{typeof(TSelect).ToPretty()}': '{nameof(DataSourceSelectOptions.QueryStringCallbackAsync)}' is not supported in sync method.");
			}
		}

		var cursor = prep.clientSessionHandle == null
			? Collection.Aggregate<TSelect>(prep.query, prep.aggregateOptions, cancellationToken)
			: Collection.Aggregate<TSelect>(prep.clientSessionHandle, prep.query, prep.aggregateOptions, cancellationToken);

		return options?.ObtainLastPageMark == true
			? new DSAsyncCursorWithLastPageMark<TSelect>(cursor, take, cancellationToken)
			: new DSAsyncCursor<TSelect>(cursor, cancellationToken);
	}

	private static List<StageInfo> BuildSelectPipelineStages(IReadOnlyList<QBContainer> containers, IReadOnlyList<QBCondition> connects, IReadOnlyList<QBCondition> conditions, IReadOnlyList<QBField> fields, IReadOnlyList<QBSortOrder> sortOrders)
	{
		// Add pipeline stages for each container and clear the first stage's LookupAs (it musn't be used)
		//
		var stages = new List<StageInfo>(containers.Select(x => new StageInfo(x,
			x.ContainerOperation != ContainerOperations.Unwind ? StageOperations.Lookup : StageOperations.Unwind)));
		stages[0].LookupAs = string.Empty;

		// Fill the pipeline stages with connect conditions.
		// Connect conditions are always AND conditions (OR connect conditions or expressions are not supported by us).
		// In a pipeline stage condition map, connect conditions between fields come first.
		// Then follow the connect conditions on constant values. And only after them, the regular conditions as expressions.
		//
		FillPipelineStagesWithConnectConditions(stages, connects);

		// Slice the regular conditions to the smallest possible parts. The separator is the AND operation.
		//
		var slices = SliceConditions(conditions);

		// Fill the pipeline stages with conditions (condition slices) using the dependencies of each condition in the slice.
		// The goal is to place the slice in the first possible stage.
		//
		FillPipelineStagesWithConditionSlices(stages, slices);

		// Fill the pipeline stages with information for the $project and $addField commands
		//
		FillPipelineStagesWithProjections(stages, fields);

		// Fill the pipeline stages with information for the $sort commands
		//
		FillPipelineStagesWithSortOrders(stages, sortOrders);

		return stages;
	}

	private static List<BsonDocument> BuildSelectQuery(List<StageInfo> stages, IReadOnlyList<QBParameter> parameters)
	{
		var result = new List<BsonDocument>();
		StageInfo stage;
		QBCondition? firstConnect;
		BsonDocument lookup;
		BuiltCondition? filter;
		BsonDocument? let;
		string s;
		bool isExpr;

		// Declare func to get DB-side field names. They are depend on 'stages' and 'stageIndex'
		int stageIndex = 0;
		var getDBSideNameInsideLookup = string (string alias, DEPath fieldPath) =>
		{
			var trueAlias = stages.Take(stageIndex).FirstOrDefault(x => x.Container.Alias == alias)?.LookupAs;
			if (trueAlias == null)
			{
				if (stages[stageIndex].Container.Alias == alias)
				{
					return fieldPath.GetDBSideName();
				}
				throw new KeyNotFoundException($"Collection '{alias}' is not yet defined at this stage.");
			}
			else if (trueAlias.Length == 0)
			{
				return fieldPath.GetDBSideName();
			}
			return string.Concat(trueAlias, ".", fieldPath.GetDBSideName());
		};
		var getDBSideNameAfterLookup = string (string alias, DEPath fieldPath) =>
		{
			var trueAlias = stages.Take(stageIndex + 1).FirstOrDefault(x => x.Container.Alias == alias)?.LookupAs;
			if (trueAlias == null)
			{
				throw new KeyNotFoundException($"Collection '{alias}' is not yet defined at this stage.");
			}
			else if (trueAlias.Length == 0)
			{
				return fieldPath.GetDBSideName();
			}
			return string.Concat(trueAlias, ".", fieldPath.GetDBSideName());
		};

		for ( ; stageIndex < stages.Count; stageIndex++)
		{
			stage = stages[stageIndex];

			OutputProjections(result, stage.ProjectBefore);

			// $lookup
			if (stageIndex > 0)
			{
				// Joining container
				lookup = new BsonDocument { { "from", stage.Container.DBSideName } };

				// First connect condition to foreignField and localField
				if (stage.Container.ContainerOperation == ContainerOperations.LeftJoin || stage.Container.ContainerOperation == ContainerOperations.Join)
				{
					firstConnect = stage.ConditionMap.First().First(x => x.IsConnectOnField);

					lookup["foreignField"] = firstConnect.Field.GetDBSideName();
					lookup["localField"] = getDBSideNameInsideLookup(firstConnect.RefAlias!, firstConnect.RefField!);
				}
				else
				{
					firstConnect = null;
				}

				// other connect conditions except firstConnect
				filter = null;
				let = null;
				foreach (var conds in stage.ConditionMap.Where(x => x.Any(xx => xx.IsConnect && xx != firstConnect)))
				{
					isExpr = false;
					foreach (var cond in conds.Where(x => x.IsOnField && x != firstConnect))
					{
						if (let == null) let = new BsonDocument();

						s = getDBSideNameInsideLookup(cond.RefAlias!, cond.RefField!);
						let[MakeVariableName(s)] = "$" + s;

						isExpr = true;
					}

					filter = filter?.AppendByAnd(BuildConditionTree(isExpr, conds.Where(x => x != firstConnect), getDBSideNameInsideLookup, parameters)!)
								?? BuildConditionTree(isExpr, conds.Where(x => x != firstConnect), getDBSideNameInsideLookup, parameters);
				}
				if (let != null)
				{
					lookup["let"] = let;
				}
				if (filter != null)
				{
					lookup["pipeline"] = new BsonArray() { new BsonDocument { { "$match", filter.BsonDocument } } };
				}
				else if (firstConnect == null/* stage.Container.ContainerOperation == BuilderContainerOperations.CrossJoin */)
				{
					lookup["pipeline"] = new BsonArray();
				}

				lookup["as"] = stages[stageIndex].LookupAs;

				result.Add(new BsonDocument { { "$lookup", lookup } });

				// $unwind
				{
					var leftJoinOrCrossJoin = stage.Container.ContainerOperation != ContainerOperations.Join;
					result.Add(new BsonDocument {
						{ "$unwind", new BsonDocument {
							{ "path", "$" + lookup["as"] },
							{ "preserveNullAndEmptyArrays", leftJoinOrCrossJoin }
						}}
					});
				}
			}

			// $match
			{
				filter = null;
				foreach (var conds in stage.ConditionMap.Where(x => !x.Any(xx => xx.IsConnect)))
				{
					isExpr = conds.Any(x => x.IsOnField);
					filter = filter?.AppendByAnd(BuildConditionTree(isExpr, conds, getDBSideNameAfterLookup, parameters)!)
								?? BuildConditionTree(isExpr, conds, getDBSideNameAfterLookup, parameters);
				}
				if (filter != null)
				{
					result.Add(new BsonDocument { { "$match", filter.BsonDocument } });
				}
			}

			OutputSortOrders(result, stage.SortBeforeProject);
			OutputProjections(result, stage.ProjectAfter);
			OutputSortOrders(result, stage.SortAfterProject);
		}

		return result;
	}

	/// <summary>
	/// Fill the pipeline stages with connect conditions.
	/// </summary>
	private static void FillPipelineStagesWithConnectConditions(List<StageInfo> stages, IReadOnlyList<QBCondition> connects)
	{
		List<QBCondition> builderConditions;
		foreach (var alias in connects.Select(x => x.Alias).Distinct())
		{
			builderConditions = connects.Where(x => x.IsOnField && x.Alias == alias).ToList();
			if (builderConditions.Count > 0)
			{
				stages.First(x => x.Container.Alias == alias).ConditionMap.Add(builderConditions);
			}

			builderConditions = connects.Where(x => !x.IsOnField && x.Alias == alias).ToList();
			if (builderConditions.Count > 0)
			{
				stages.First(x => x.Container.Alias == alias).ConditionMap.Add(builderConditions);
			}
		}
	}

	/// <summary>
	/// Fill the pipeline stages with conditions (condition slices)
	/// </summary>
	private static void FillPipelineStagesWithConditionSlices(List<StageInfo> stages, List<List<QBCondition>> slices)
	{
		int firstPossibleStage;
		foreach (var slice in slices)
		{
			firstPossibleStage = 0;

			foreach (var cond in slice)
			{
				firstPossibleStage = Math.Max(firstPossibleStage, stages.FindIndex(x => x.Container.Alias == cond.Alias));

				if (cond.IsOnField)
				{
					firstPossibleStage = Math.Max(firstPossibleStage, stages.FindIndex(x => x.Container.Alias == cond.RefAlias!));
				}
			}

			stages[firstPossibleStage].ConditionMap.Add(slice);
		}
	}

	/// <summary>
	/// Fill the pipeline stages with projections
	/// </summary>
	private static void FillPipelineStagesWithProjections(List<StageInfo> stages, IReadOnlyList<QBField> fields)
	{
		string path;
		List<List<QBCondition>>? map;
		int stageIndex;

		// Fill the includes and change the LookupAs names for the joining documents the result of which is directly projected into the field.
		foreach (var field in fields.Where(x => x.IncludeOrExclude).OrderBy(x => x.RefAlias ?? string.Empty).ThenBy(x => x.RefField?.Count ?? 0))
		{
			stageIndex = stages.FindIndex(x => x.Container.Alias == field.RefAlias);

			if (field.RefField != null && field.RefField.DataEntryType == stages[stageIndex].Container.DocumentType)
			{
				// project the joining document as the result field
				stages[stageIndex].LookupAs = field.Field.GetDBSideName();
			}
			else
			{
				path = string.Concat(
					stages[stageIndex].LookupAs, ".",
					field.RefField!.GetDBSideName()
				);

				// Propagate the include to the pipeline stage using the dependencies on conditions and the include fields.
				// The goal is to place the include in the first possible stage.
				// Important: any inclusion clears all other fields, so we can insert it only after any mention of the entire document.
				//
				for (int i = stageIndex + 1; i < stages.Count; i++)
				{
					map = stages[i].ConditionMap;

					if (map.Any(x => x.Any(xx => xx.Alias == field.RefAlias || (xx.RefAlias != null && xx.RefAlias == field.RefAlias))))
					{
						stageIndex = i;
					}
				}
				stages[stageIndex].ProjectAfter.Add((path, field.Field.GetDBSideName(), field));
			}
		}

		foreach (var field in fields.Where(x => !x.IncludeOrExclude))
		{
			// Is this exclude for the entire joined document?
			var joinedDoc = fields.FirstOrDefault(x =>
				// search in includes
				x.IncludeOrExclude
				// for the entire document: (store) => store
				&& x.RefField!.DataEntryType == stages.First(xx => xx.Container.Alias == x.RefAlias).Container.DocumentType
				// in this case our exclude must be a part of it: (sel) => sel.Store.LogoImg, (sel) => sel.Store
				&& field.Field.Path.StartsWith(x.Field.Path)
				&& field.Field.Path.Length > x.Field.Path.Length + 1
				&& field.Field.Path[x.Field.Path.Length] == '.'
			);
			if (joinedDoc == null)
			{
				path = field.Field.GetDBSideName();

				// Propagate the exclude
				//
				stageIndex = -1;
				for (int i = 0; i < stages.Count; i++)
				{
					map = stages[i].ConditionMap;

					if (map.Any(x => x.Any(xx => xx.Field.Path == field.Field.Path)))
					{
						stageIndex = i;
					}
				}
				if (stageIndex == -1)
				{
					stages[0].ProjectBefore.Add((path, null, field));
				}
				else
				{
					stages[stageIndex].ProjectAfter.Add((path, null, field));
				}
			}
			else
			{
				stageIndex = stages.FindIndex(x => x.Container.Alias == joinedDoc.RefAlias!);

				path = string.Concat(stages[stageIndex].LookupAs, ".",
					string.Join(".", field.Field.Skip(joinedDoc.Field.Count).Select(x => field.Field.Cast<MongoDEInfo>().Select(x => x.DBSideName))));

				// Propagate the exclude
				//
				var trueFullName = string.Join(".", field.Field.Skip(joinedDoc.Field.Count).Select(x => x.Name));
				for (int i = stageIndex + 1; i < stages.Count; i++)
				{
					map = stages[i].ConditionMap;

					if (map.Any(x => x.Any(xx =>
							(xx.Alias == joinedDoc.RefAlias && xx.Field.Path == trueFullName) ||
							(xx.RefAlias == joinedDoc.RefAlias && xx.RefField!.Path == trueFullName))))
					{
						stageIndex = i;
					}
				}
				stages[stageIndex].ProjectAfter.Add((path, null, field));
			}
		}

		// Exclude Bson-elements missing in DTO compared to Document
		if (stages[0].Container.DocumentType != typeof(TDoc))
		{
			var docMap = BsonClassMap.LookupClassMap(typeof(TDoc));
			var selMap = BsonClassMap.LookupClassMap(stages[0].Container.DocumentType);
			var topAlias = stages[0].Container.Alias;
			// 
			foreach (var elem in docMap.AllMemberMaps.ExceptBy(selMap.AllMemberMaps.Select(x => x.MemberName), x => x.MemberName))
			{
				// this exclude has not been added before?
				if (!stages.Any(x => x.ProjectBefore.Any(xx => xx.toPath == null && xx.fromPath == elem.ElementName) ||
										x.ProjectAfter.Any(xx => xx.toPath == null && xx.fromPath == elem.ElementName)))
				{
					path = elem.ElementName;

					stageIndex = -1;
					for (int i = 0; i < stages.Count; i++)
					{
						map = stages[i].ConditionMap;

						if (map.Any(x => x.Any(xx =>
								(xx.Alias == topAlias && xx.Field.Path == path) ||
								(xx.RefAlias == topAlias && xx.RefField!.Path == path))))
						{
							stageIndex = i;
						}
					}
					if (stageIndex == -1)
					{
						stages[0].ProjectBefore.Add((path, null, null));
					}
					else
					{
						stages[stageIndex].ProjectAfter.Add((path, null, null));
					}
				}
			}
		}

		// Exclude joining conteiners not projected to documents
		StageInfo stage;
		for (int i = 1; i < stages.Count; i++)
		{
			stage = stages[i];
			if (!stage.LookupAs.StartsWith("___")) continue;

			stageIndex = i;
			for (int j = i + 1; j < stages.Count; j++)
			{
				map = stages[j].ConditionMap;

				if (map.Any(x => x.Any(xx => xx.Alias == stage.Container.Alias || xx.RefAlias == stage.Container.Alias)))
				{
					stageIndex = j;
				}
			}
			stages[stageIndex].ProjectAfter.Add((stages[i].LookupAs, null, null));
		}
	}
	
	/// <summary>
	/// Fill the last pipeline stage with $sort if any
	/// </summary>
	private static void FillPipelineStagesWithSortOrders(List<StageInfo> stages, IReadOnlyList<QBSortOrder> sortOrders)
	{
		if (sortOrders.Count == 0)
		{
			return;
		}

		var lastStage = stages[stages.Count - 1];

		if (sortOrders.Any(x => x.Alias.Length > 0))
		{
			lastStage.SortBeforeProject = new List<(string path, SO sortOrder)>(sortOrders.Count);

			StageInfo stage;
			string path;

			foreach (var so in sortOrders)
			{
				if (so.Alias.Length > 0)
				{
					stage = stages.First(x => x.Container.Alias == so.Alias);
					path = stage.LookupAs.Length == 0
						? string.Concat(stage.LookupAs, ".", so.Field.GetDBSideName())
						: so.Field.GetDBSideName();
				}
				else
				{
					path = so.Field.GetDBSideName();
				}

				lastStage.SortBeforeProject.Add((path, so.SortOrder));
			}
		}
		else
		{
			lastStage.SortAfterProject = new List<(string path, SO sortOrder)>(sortOrders.Count);

			foreach (var so in sortOrders)
			{
				lastStage.SortAfterProject.Add((so.Field.GetDBSideName(), so.SortOrder));
			}
		}
	}

	private static void OutputProjections(List<BsonDocument> result, List<(string fromPath, string? toPath, QBField? builderField)> projectInfo)
	{
		BsonDocument? projection = null;
		foreach (var projField in projectInfo.Where(x => x.toPath == null && x.builderField?.OptionalExclusion == true))
		{
			if (projection == null) projection = new BsonDocument();

			var condStmt = new BsonArray();
			var eqStmt = new BsonArray();

			eqStmt.Add(new BsonDocument { { "$type", "$" + projField.fromPath } });
			
			if (projField.builderField!.Field.DataEntryType == typeof(string))
			{
				eqStmt.Add("string");

				condStmt.Add(new BsonDocument { { "$eq", eqStmt } });
				condStmt.Add(string.Empty);
				condStmt.Add(BsonNull.Value);
			}
			else
			{
				eqStmt.Add("binData");

				condStmt.Add(new BsonDocument { { "$eq", eqStmt } });
				//condStmt.Add(new BsonJavaScript("Binary()"));
				condStmt.Add(new BsonBinaryData(Array.Empty<byte>()));
				//condStmt.Add(new BsonDocumentWrapper(Array.Empty<byte>(), BsonSerializer.SerializerRegistry.GetSerializer(typeof(byte[]))));
			}
			condStmt.Add(BsonNull.Value);

			projection[projField.fromPath] = new BsonDocument { { "$cond", condStmt } };
		}
		if (projection != null)
		{
			result.Add(new BsonDocument { { "$addFields", projection } });
		}
		
		projection = null;
		foreach (var projField in projectInfo.Where(x => x.toPath != null || x.builderField == null || !x.builderField.OptionalExclusion))
		{
			if (projection == null) projection = new BsonDocument();

			if (projField.toPath == null)
			{
				projection[projField.fromPath] = 0;
			}
			else
			{
				projection[projField.toPath] = projField.fromPath;
			}
		}
		if (projection != null)
		{
			result.Add(new BsonDocument { { "$project", projection } });
		}
	}

	private static void OutputSortOrders(List<BsonDocument> result, List<(string path, SO sortOrder)>? sortInfo)
	{
		if (sortInfo == null || sortInfo.Count == 0)
		{
			return;
		}

		var entries = new BsonDocument();
		foreach (var sortEntry in sortInfo)
		{
			if (sortEntry.sortOrder.HasFlag(SO.Rank))
			{
				entries.Add("___textScore", new BsonDocument { { "$meta", "textScore" } });
			}
			else
			{
				entries.Add(
					sortEntry.path,
					sortEntry.sortOrder switch
					{
						SO.Ascending => 1,
						SO.Descending => -1,
						_ => throw new NotSupportedException()
					}
				);
			}
		}

		result.Add(new BsonDocument { { "$sort", entries } });
	}
}