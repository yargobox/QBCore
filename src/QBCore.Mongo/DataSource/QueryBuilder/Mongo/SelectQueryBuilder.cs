using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Collections.Generic;
using QBCore.Extensions.Text;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class SelectQueryBuilder<TDocument, TSelect> : QueryBuilder<TDocument, TSelect>, ISelectQueryBuilder<TDocument, TSelect>
{
	private sealed class StageInfo
	{
		public readonly BuilderContainer Container;
		public string LookupAs;
		public readonly List<List<BuilderCondition>> ConditionMap;
		public readonly List<(string fromPath, string? toPath, BuilderField? builderField)> ProjectBefore;
		public readonly List<(string fromPath, string? toPath, BuilderField? builderField)> ProjectAfter;

		public StageInfo(BuilderContainer container)
		{
			Container = container;
			LookupAs = "___" + container.Name;
			ConditionMap = new List<List<BuilderCondition>>();
			ProjectBefore = new List<(string fromPath, string? toPath, BuilderField? builderField)>();
			ProjectAfter = new List<(string fromPath, string? toPath, BuilderField? builderField)>();
		}
	}

	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Select;

	public override Origin Source => new Origin(this.GetType());

	public SelectQueryBuilder(QBBuilder<TDocument, TSelect> building, IDataContext dataContext)
		: base(building, dataContext)
	{
	}

	public IMongoCollection<TDocument> Collection
	{
		get
		{
			if (_collection == null)
			{
				var top = Builder.Containers.FirstOrDefault(x => x.ContainerOperation == BuilderContainerOperations.Select);
				if (top?.ContainerType == BuilderContainerTypes.Table || top?.ContainerType == BuilderContainerTypes.View)
				{
					_collection = _mongoDbContext.DB.GetCollection<TDocument>(top!.DBSideName);
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

	public async Task<long> CountAsync(DataSourceCountOptions? options = null, CancellationToken cancellationToken = default)
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
		var query = BuildSelectQuery(Builder, null!);//!!!

		await Task.CompletedTask;
		throw new NotImplementedException();
		//return await Collection.CountDocumentsAsync(Builders<TDocument>.Filter.Empty, countOptions, cancellationToken);
	}

	public async IAsyncEnumerable<TSelect> SelectAsync(long? skip = null, int? take = null, DataSourceSelectOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		if (skip < 0)
		{
			throw new ArgumentException(nameof(skip));
		}
		if (take < 0)
		{
			throw new ArgumentException(nameof(take));
		}
		if (take == 0)
		{
			yield break;
		}
		if (options?.NativeOptions != null && options.NativeOptions is not AggregateOptions)
		{
			throw new ArgumentException(nameof(options.NativeOptions));
		}
		if (options?.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
		{
			throw new ArgumentException(nameof(options.NativeClientSession));
		}
		if (options?.PreparedNativeQuery != null && options.PreparedNativeQuery is not List<BsonDocument>)
		{
			throw new ArgumentException(nameof(options.PreparedNativeQuery));
		}

		_ = Collection;

		var aggrOptions = (AggregateOptions?)options?.NativeOptions;
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;
		var preparedQuery = (List<BsonDocument>?)options?.PreparedNativeQuery;

		List<BsonDocument> query;

		if (preparedQuery != null)
		{
			var skipIndex = preparedQuery.FindLastIndex(x => x.Contains("$skip"));
			var limitIndex = preparedQuery.FindLastIndex(x => x.Contains("$limit"));

			if (skip != null)
			{
				if (skipIndex >= 0)
				{
					preparedQuery[skipIndex]["$skip"] = skip;
				}
				else if (limitIndex >= 0)
				{
					preparedQuery.Insert(limitIndex, new BsonDocument { { "$skip", skip } });
				}
				else
				{
					preparedQuery.Add(new BsonDocument { { "$skip", skip } });
				}
			}
			else if (skipIndex >= 0)
			{
				preparedQuery.RemoveAt(skipIndex);
			}

			if (take != null)
			{
				if (limitIndex >= 0)
				{
					preparedQuery[limitIndex]["$limit"] = take;
				}
				else if (skipIndex >= 0)
				{
					preparedQuery.Insert(skipIndex + 1, new BsonDocument { { "$limit", take } });
				}
				else
				{
					preparedQuery.Add(new BsonDocument { { "$limit", take } });
				}
			}
			else if (limitIndex >= 0)
			{
				preparedQuery.RemoveAt(limitIndex);
			}

			query = preparedQuery;
		}
		else
		{
			query = BuildSelectQuery(Builder, null!);//!!!

			if (skip != null)
			{
				query.Add(new BsonDocument { { "$skip", skip } });
			}
			if (take != null)
			{
				query.Add(new BsonDocument { { "$limit", take } });
			}
		}

		if (options?.GetNativeQuery != null)
		{
			options.GetNativeQuery(query);
		}
		if (options?.GetQueryString != null)
		{
			options.GetQueryString(new BsonArray(query).ToString());
		}

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

	public static List<BsonDocument> BuildSelectQuery(QBBuilder<TDocument, TSelect> builder, IReadOnlyDictionary<string, object?>? arguments)
	{
		var result = new List<BsonDocument>();

		// Add pipeline stages for each container and clear the first stage's LookupAs (it musn't be used)
		//
		var stages = new List<StageInfo>(builder.Containers.Select(x => new StageInfo(x)));
		stages[0].LookupAs = string.Empty;

		// Fill the pipeline stages with connect conditions.
		// Connect conditions are always AND conditions (OR connect conditions or expressions are not supported by us).
		// In a pipeline stage condition map, connect conditions between fields come first.
		// Then follow the connect conditions on constant values. And only after them, the regular conditions as expressions.
		//
		FillPipelineStagesWithConnectConditions(stages, builder.Connects);

		// Slice the regular conditions to the smallest possible parts. The separator is the AND operation.
		//
		var slices = SliceConditions(builder.Conditions);

		// Fill the pipeline stages with conditions (condition slices) using the dependencies of each condition in the slice.
		// The goal is to place the slice in the first possible stage.
		//
		FillPipelineStagesWithConditionSlices(stages, slices);

		// Fill the pipeline stages with information for the $project and $addField commands
		//
		FillPipelineStagesWithProjections(stages, builder.Fields);

		// Pipelines
		//

		StageInfo stage;
		BuilderCondition? firstConnect;
		BsonDocument lookup;
		BuiltCondition? filter;
		BsonDocument? let;
		string s;
		bool isExpr;

		// Declare func to get DB-side field names. They are depend on 'stages' and 'stageIndex'
		int stageIndex = 0;
		var getDBSideNameInsideLookup = string (string alias, FieldPath fieldPath) =>
		{
			var trueAlias = stages.Take(stageIndex).FirstOrDefault(x => x.Container.Name == alias)?.LookupAs;
			if (trueAlias == null)
			{
				if (stages[stageIndex].Container.Name == alias)
				{
					return fieldPath.DBSideName;
				}
				throw new KeyNotFoundException($"Collection '{alias}' is not yet defined at this stage.");
			}
			else if (trueAlias.Length == 0)
			{
				return fieldPath.DBSideName;
			}
			return string.Concat(trueAlias, ".", fieldPath.DBSideName);
		};
		var getDBSideNameAfterLookup = string (string alias, FieldPath fieldPath) =>
		{
			var trueAlias = stages.Take(stageIndex + 1).FirstOrDefault(x => x.Container.Name == alias)?.LookupAs;
			if (trueAlias == null)
			{
				throw new KeyNotFoundException($"Collection '{alias}' is not yet defined at this stage.");
			}
			else if (trueAlias.Length == 0)
			{
				return fieldPath.DBSideName;
			}
			return string.Concat(trueAlias, ".", fieldPath.DBSideName);
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
				if (stage.Container.ContainerOperation == BuilderContainerOperations.LeftJoin || stage.Container.ContainerOperation == BuilderContainerOperations.Join)
				{
					firstConnect = stage.ConditionMap.First().First(x => x.IsConnectOnField);

					lookup["foreignField"] = firstConnect.Field.DBSideName;
					lookup["localField"] = getDBSideNameInsideLookup(firstConnect.RefName!, firstConnect.RefField!);
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

						s = getDBSideNameInsideLookup(cond.RefName!, cond.RefField!);
						let[MakeVariableName(s)] = "$" + s;

						isExpr = true;
					}

					filter = filter?.AppendByAnd(BuildConditionTree(isExpr, conds.Where(x => x != firstConnect), getDBSideNameInsideLookup, arguments)!)
								?? BuildConditionTree(isExpr, conds.Where(x => x != firstConnect), getDBSideNameInsideLookup, arguments);
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
					var leftJoinOrCrossJoin = stage.Container.ContainerOperation != BuilderContainerOperations.Join;
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
					filter = filter?.AppendByAnd(BuildConditionTree(isExpr, conds, getDBSideNameAfterLookup, arguments)!)
								?? BuildConditionTree(isExpr, conds, getDBSideNameAfterLookup, arguments);
				}
				if (filter != null)
				{
					result.Add(new BsonDocument { { "$match", filter.BsonDocument } });
				}
			}

			OutputProjections(result, stage.ProjectAfter);
		}

		return result;
	}

	/// <summary>
	/// Fill the pipeline stages with connect conditions.
	/// </summary>
	private static void FillPipelineStagesWithConnectConditions(List<StageInfo> stages, List<BuilderCondition> connects)
	{
		List<BuilderCondition> builderConditions;
		foreach (var name in connects.Select(x => x.Name).Distinct())
		{
			builderConditions = connects.Where(x => x.IsOnField && x.Name == name).ToList();
			if (builderConditions.Count > 0)
			{
				stages.First(x => x.Container.Name == name).ConditionMap.Add(builderConditions);
			}

			builderConditions = connects.Where(x => !x.IsOnField && x.Name == name).ToList();
			if (builderConditions.Count > 0)
			{
				stages.First(x => x.Container.Name == name).ConditionMap.Add(builderConditions);
			}
		}
	}

	/// <summary>
	/// Fill the pipeline stages with conditions (condition slices)
	/// </summary>
	private static void FillPipelineStagesWithConditionSlices(List<StageInfo> stages, List<List<BuilderCondition>> slices)
	{
		int firstPossibleStage;
		foreach (var slice in slices)
		{
			firstPossibleStage = 0;

			foreach (var cond in slice)
			{
				firstPossibleStage = Math.Max(firstPossibleStage, stages.FindIndex(x => x.Container.Name == cond.Name));

				if (cond.IsOnField)
				{
					firstPossibleStage = Math.Max(firstPossibleStage, stages.FindIndex(x => x.Container.Name == cond.RefName!));
				}
			}

			stages[firstPossibleStage].ConditionMap.Add(slice);
		}
	}

	/// <summary>
	/// Fill the pipeline stages with projections
	/// </summary>
	private static void FillPipelineStagesWithProjections(List<StageInfo> stages, List<BuilderField> fields)
	{
		string path;
		List<List<BuilderCondition>>? map;
		int stageIndex;

		// Fill the includes and change the LookupAs names for the joining documents the result of which is directly projected into the field.
		foreach (var field in fields.Where(x => x.IncludeOrExclude).OrderBy(x => x.RefName ?? string.Empty).ThenBy(x => x.RefField?.ElementCount ?? 0))
		{
			if (field.RefField?.ElementCount == 0)
			{
				// project the joining document as the result field
				stages.First(x => x.Container.Name == field.RefName!).LookupAs = field.Field.DBSideName;
			}
			else
			{
				stageIndex = stages.FindIndex(x => x.Container.Name == field.RefName!);

				path = string.Concat(
					stages[stageIndex].LookupAs, ".",
					field.RefField!.DBSideName
				);

				// Propagate the include to the pipeline stage using the dependencies on conditions and the include fields.
				// The goal is to place the include in the first possible stage.
				// Important: any inclusion clears all other fields, so we can insert it only after any mention of the entire document.
				//
				for (int i = stageIndex + 1; i < stages.Count; i++)
				{
					map = stages[i].ConditionMap;

					if (map.Any(x => x.Any(xx => xx.Name == field.RefName || (xx.RefName != null && xx.RefName == field.RefName))))
					{
						stageIndex = i;
					}
				}
				stages[stageIndex].ProjectAfter.Add((path, field.Field.DBSideName, field));
			}
		}

		foreach (var field in fields.Where(x => !x.IncludeOrExclude))
		{
			// Is this exclude for the entire joined document?
			var joinedDoc = fields.FirstOrDefault(x =>
				// search in includes
				x.IncludeOrExclude
				// for the entire document: (store) => store
				&& x.RefField?.ElementCount == 0
				// in this case our exclude must be a part of it: (sel) => sel.Store.LogoImg, (sel) => sel.Store
				&& field.Field.FullName.StartsWith(x.Field.FullName)
				&& field.Field.FullName.Length > x.Field.FullName.Length + 1
				&& field.Field.FullName[x.Field.FullName.Length] == '.'
			);
			if (joinedDoc == null)
			{
				path = field.Field.DBSideName;

				// Propagate the exclude
				//
				stageIndex = -1;
				for (int i = 0; i < stages.Count; i++)
				{
					map = stages[i].ConditionMap;

					if (map.Any(x => x.Any(xx => xx.Field.FullName == field.Field.FullName)))
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
				path = string.Concat(stages.First(x => x.Container.Name == joinedDoc.RefName!).LookupAs, ".",
					string.Join(".", field.Field.Elements.Skip(joinedDoc.Field.ElementCount).Select(x => x.DBSideName)));

				// Propagate the exclude
				//
				var trueFullName = string.Join(".", field.Field.Elements.Skip(joinedDoc.Field.ElementCount).Select(x => x.Name));
				stageIndex = stages.FindIndex(x => x.Container.Name == joinedDoc.RefName!);
				for (int i = stageIndex + 1; i < stages.Count; i++)
				{
					map = stages[i].ConditionMap;

					if (map.Any(x => x.Any(xx =>
							(xx.Name == joinedDoc.RefName && xx.Field.FullName == trueFullName) ||
							(xx.RefName == joinedDoc.RefName && xx.RefField!.FullName == trueFullName))))
					{
						stageIndex = i;
					}
				}
				stages[stageIndex].ProjectAfter.Add((path, null, field));
			}
		}

		// Exclude Bson-elements missing in DTO compared to Document
		if (stages[0].Container.DocumentType != typeof(TDocument))
		{
			var docMap = BsonClassMap.LookupClassMap(typeof(TDocument));
			var selMap = BsonClassMap.LookupClassMap(stages[0].Container.DocumentType);
			var topAlias = stages[0].Container.Name;
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
								(xx.Name == topAlias && xx.Field.FullName == path) ||
								(xx.RefName == topAlias && xx.RefField!.FullName == path))))
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

				if (map.Any(x => x.Any(xx => xx.Name == stage.Container.Name || xx.RefName == stage.Container.Name)))
				{
					stageIndex = j;
				}
			}
			stages[stageIndex].ProjectAfter.Add((stages[i].LookupAs, null, null));
		}
	}
	private static void OutputProjections(List<BsonDocument> result, List<(string fromPath, string? toPath, BuilderField? builderField)> projectInfo)
	{
		BsonDocument? projection = null;
		foreach (var projField in projectInfo.Where(x => x.toPath == null && x.builderField?.OptionalExclusion == true))
		{
			if (projection == null) projection = new BsonDocument();

			var condStmt = new BsonArray();
			var eqStmt = new BsonArray();

			eqStmt.Add(new BsonDocument { { "$type", "$" + projField.fromPath } });
			
			if (projField.builderField!.Field.FieldType == typeof(string))
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
}