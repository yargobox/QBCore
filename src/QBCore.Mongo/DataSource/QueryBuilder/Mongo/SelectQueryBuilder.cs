using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.DataSource.Options;
using QBCore.Extensions.Collections.Generic;
using QBCore.Extensions.Text;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class SelectQueryBuilder<TDocument, TSelect> : QueryBuilder<TDocument, TSelect>, ISelectQueryBuilder<TDocument, TSelect>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Select;

	public override Origin Source => new Origin(this.GetType());

	private IReadOnlyDictionary<string, object?>? _arguments;

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
		IReadOnlyDictionary<string, object?>? arguments = null,
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
		_arguments = arguments;
		var query = BuildSelectQuery();

		await Task.CompletedTask;
		throw new NotImplementedException();
		//return await Collection.CountDocumentsAsync(Builders<TDocument>.Filter.Empty, countOptions, cancellationToken);
	}

	public async IAsyncEnumerable<TSelect> SelectAsync(
		IReadOnlyCollection<QBCondition>? conditions = null,
		IReadOnlyDictionary<string, object?>? arguments = null,
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
		_arguments = arguments;
		var query = BuildSelectQuery();

		Console.WriteLine(new BsonDocument { { "aggregate", new BsonArray(query) } }.ToString());//!!!

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

	private sealed class StageInfo
	{
		public readonly string Alias;
		public string LookupAs;
		public readonly List<List<BuilderCondition>> ConditionMap;
		public readonly List<(string fromPath, string? toPath, BuilderField? builderField)> ProjectBefore;
		public readonly List<(string fromPath, string? toPath, BuilderField? builderField)> ProjectAfter;

		public StageInfo(string alias)
		{
			Alias = alias;
			LookupAs = "___" + alias;
			ConditionMap = new List<List<BuilderCondition>>();
			ProjectBefore = new List<(string fromPath, string? toPath, BuilderField? builderField)>();
			ProjectAfter = new List<(string fromPath, string? toPath, BuilderField? builderField)>();
		}
	}

	private List<BsonDocument> BuildSelectQuery()
	{
		var containers = Builder.Containers;
		var conditions = Builder.Conditions;
		var fields = Builder.Fields;
		var top = containers[0];
		var result = new List<BsonDocument>();

		// Add pipeline stages for each container and clear the first stage's LookupAs (it musn't be used)
		//
		var stages = new List<StageInfo>(containers.Select(x => new StageInfo(x.Name)));
		stages[0].LookupAs = string.Empty;

		// Fill the pipeline stages with connect conditions.
		// Connect conditions are always AND conditions (OR connect conditions are not supported by us).
		// In a pipeline stage condition map, connect conditions between fields come first.
		// Then follow the connect conditions on constant values.
		// And only after them, the regular conditions that can be given as expressions.
		//
		FillPipelineStagesWithConnectConditions(stages, conditions);

		// Slice the regular conditions to the smallest possible parts.The separator is the AND operation.
		//
		var slices = SliceConditions(conditions.Where(x => !x.IsConnect));

		// Propagate the slices to the pipeline stages using the dependencies of each condition in the slice.
		// The goal is to place the slice in the first possible stage.
		//
		{
			int firstPossibleStage;
			foreach (var slice in slices)
			{
				firstPossibleStage = 0;

				foreach (var cond in slice)
				{
					firstPossibleStage = Math.Max(firstPossibleStage, stages.FindIndex(x => x.Alias == cond.Name));

					if (cond.IsOnField)
					{
						firstPossibleStage = Math.Max(firstPossibleStage, stages.FindIndex(x => x.Alias == cond.RefName!));
					}
				}

				stages[firstPossibleStage].ConditionMap.Add(slice);
			}
		}

		// Propagate field projections
		//
		{
			string path;
			List<List<BuilderCondition>>? map;
			int stageIndex;

			foreach (var field in fields.Where(x => x.IncludeOrExclude).OrderBy(x => x.RefName ?? string.Empty).ThenBy(x => x.RefField?.ElementCount ?? 0))
			{
				if (field.RefField?.ElementCount == 0)
				{
					// project the joined document as the result field
					stages.First(x => x.Alias == field.RefName!).LookupAs = field.Field.DBSideName;
				}
				else
				{
					path = string.Concat(
						stages.First(x => x.Alias == field.RefName!).LookupAs, ".",
						field.RefField!.DBSideName
					);

					// Propagate the include to the pipeline stage using the dependencies on conditions and the include fields.
					// The goal is to place the include in the first possible stage.
					// Important: any inclusion clears all other fields, so we insert it only after any mention of the entire document.
					//
					stageIndex = stages.FindIndex(x => x.Alias == field.RefName!);
					for (int i = stageIndex + 1; i < stages.Count; i++)
					{
						map = stages[i].ConditionMap;

						if (map.Any(x => x.Any(xx => xx.Field.Name == field.RefName || (xx.RefField != null && xx.RefField.Name == field.RefName))))
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
					path = string.Concat(stages.First(x => x.Alias == joinedDoc.RefName!).LookupAs, ".",
						string.Join(".", field.Field.Elements.Skip(joinedDoc.Field.ElementCount).Select(x => x.DBSideName)));

					// Propagate the exclude
					//
					var trueFullName = string.Join(".", field.Field.Elements.Skip(joinedDoc.Field.ElementCount).Select(x => x.Name));
					stageIndex = stages.FindIndex(x => x.Alias == joinedDoc.RefName!);
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

			// Exclude elements missing in DTO compared to Document
			if (containers[0].DocumentType != typeof(TDocument))
			{
				var docMap = BsonClassMap.LookupClassMap(typeof(TDocument));
				var selMap = BsonClassMap.LookupClassMap(containers[0].DocumentType);
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
									(xx.Name == top.Name && xx.Field.FullName == path) ||
									(xx.RefName == top.Name && xx.RefField!.FullName == path))))
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

			// Exclude joined conteiners not projected to documents
			StageInfo stage;
			for (int i = 1; i < stages.Count; i++)
			{
				stage = stages[i];
				if (!stage.LookupAs.StartsWith("___")) continue;

				stageIndex = i;
				for (int j = i + 1; j < stages.Count; j++)
				{
					map = stages[i].ConditionMap;

					if (map.Any(x => x.Any(xx => xx.Name == stage.Alias || xx.RefName == stage.Alias)))
					{
						stageIndex = i;
					}
				}
				stages[stageIndex].ProjectAfter.Add((stages[i].LookupAs, null, null));
			}
		}

		// Pipeline
		//
		{
			BuilderCondition? firstConnect;
			BsonDocument lookup;
			BuiltCondition? filter;
			BsonDocument? let;
			string s;
			bool isExpr;
			int i = 0;
			var getDBSideNameBefore = string (string alias, FieldPath fieldPath) =>
			{
				var trueAlias = stages.Take(i).FirstOrDefault(x => x.Alias == alias)?.LookupAs;

				return string.IsNullOrEmpty(trueAlias)
					? fieldPath.DBSideName
					: string.Concat(trueAlias, ".", fieldPath.DBSideName);
			};
			var getDBSideNameAfter = string (string alias, FieldPath fieldPath) =>
			{
				var trueAlias = stages.Take(i + 1).FirstOrDefault(x => x.Alias == alias)?.LookupAs;

				return string.IsNullOrEmpty(trueAlias)
					? fieldPath.DBSideName
					: string.Concat(trueAlias, ".", fieldPath.DBSideName);
			};

			for ( ; i < stages.Count; i++)
			{
				OutputProjections(result, stages, i, true);

				// $lookup
				if (i > 0)
				{
					// Joining container
					lookup = new BsonDocument { { "from", containers[i].DBSideName } };

					// First connect condition to foreignField and localField
					if (containers[i].ContainerOperation == BuilderContainerOperations.LeftJoin || containers[i].ContainerOperation == BuilderContainerOperations.Join)
					{
						firstConnect = stages[i].ConditionMap.First().First(x => x.IsConnectOnField);

						lookup["foreignField"] = firstConnect.Field.DBSideName;
						lookup["localField"] = getDBSideNameBefore(firstConnect.RefName!, firstConnect.RefField!);
					}
					else
					{
						firstConnect = null;
					}

					// other connect conditions except firstConnect
					filter = null;
					let = null;
					foreach (var conds in stages[i].ConditionMap.Where(x => x.Any(xx => xx.IsConnect && xx != firstConnect)))
					{
						isExpr = false;
						foreach (var cond in conds.Where(x => x.IsOnField && x != firstConnect))
						{
							if (let == null) let = new BsonDocument();

							s = getDBSideNameBefore(cond.RefName!, cond.RefField!);
							let[MakeVariableName(s)] = "$" + s;

							isExpr = true;
						}

						filter = filter?.AppendByAnd(BuildConditionTree(isExpr, conds.Where(x => x != firstConnect), getDBSideNameBefore, _arguments)!)
									?? BuildConditionTree(isExpr, conds.Where(x => x != firstConnect), getDBSideNameBefore, _arguments);
					}
					if (let != null)
					{
						lookup["let"] = let;
					}
					if (filter != null)
					{
						lookup["pipeline"] = new BsonArray() { new BsonDocument { { "$match", filter.BsonDocument } } };
					}
					else if (firstConnect == null/* containers[i].ContainerOperation == BuilderContainerOperations.CrossJoin */)
					{
						lookup["pipeline"] = new BsonArray();
					}

					lookup["as"] = stages[i].LookupAs;

					result.Add(new BsonDocument { { "$lookup", lookup } });

					// $unwind
					{
						var leftJoinOrCrossJoin = containers[i].ContainerOperation != BuilderContainerOperations.Join;
						result.Add(new BsonDocument {
							{ "$unwind", new BsonDocument {
								{ "path", lookup["as"] },
								{ "preserveNullAndEmptyArrays", leftJoinOrCrossJoin }
							}}
						});
					}
				}

				// $match
				{
					filter = null;
					foreach (var conds in stages[i].ConditionMap.Where(x => !x.Any(xx => xx.IsConnect)))
					{
						isExpr = conds.Any(x => x.IsOnField);
						filter = filter?.AppendByAnd(BuildConditionTree(isExpr, conds, getDBSideNameAfter, _arguments)!)
									?? BuildConditionTree(isExpr, conds, getDBSideNameAfter, _arguments);
					}
					if (filter != null)
					{
						result.Add(new BsonDocument { { "$match", filter.BsonDocument } });
					}
				}

				OutputProjections(result, stages, i, false);
			}
		}

		return result;
	}

	/// <summary>
	/// Fill the pipeline stages with connect conditions.
	/// </summary>
	private static void FillPipelineStagesWithConnectConditions(List<StageInfo> stages, List<BuilderCondition> conditions)
	{
		List<BuilderCondition> builderConditions;
		foreach (var name in conditions.Where(x => x.IsConnect).Select(x => x.Name).Distinct())
		{
			builderConditions = conditions.Where(x => x.IsConnect && x.IsOnField && x.Name == name).ToList();
			if (builderConditions.Count > 0)
			{
				stages.First(x => x.Alias == name).ConditionMap.Add(builderConditions);
			}

			builderConditions = conditions.Where(x => x.IsConnect && !x.IsOnField && x.Name == name).ToList();
			if (builderConditions.Count > 0)
			{
				stages.First(x => x.Alias == name).ConditionMap.Add(builderConditions);
			}
		}
	}

	private static void OutputProjections(List<BsonDocument> result, List<StageInfo> stages, int stageIndex, bool beforeOrAfter)
	{
		var projectInfo = beforeOrAfter ? stages[stageIndex].ProjectBefore : stages[stageIndex].ProjectAfter;

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