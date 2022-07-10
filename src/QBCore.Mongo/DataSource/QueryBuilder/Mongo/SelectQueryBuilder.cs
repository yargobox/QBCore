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
		public string LookupAs;
		public List<List<BuilderCondition>> ConditionMap;
		public List<(string fromPath, string? toPath, BuilderField? builderField)> ProjectBefore;
		public List<(string fromPath, string? toPath, BuilderField? builderField)> ProjectAfter;

		public StageInfo()
		{
			LookupAs = string.Empty;
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

		// Add pipeline stages for each container
		//
		var stages = new OrderedDictionary<string, StageInfo>(containers.Select(x => KeyValuePair.Create(x.Name, new StageInfo() { LookupAs = "___" + x.Name })));
		stages.List[0].Value.LookupAs = string.Empty;

		// Fill the connect conditions
		//
		{
			List<BuilderCondition> builderConditions;
			foreach (var name in conditions.Where(x => x.IsConnect).Select(x => x.Name).Distinct())
			{
				builderConditions = conditions.Where(x => x.IsConnect && x.IsOnField && x.Name == name).ToList();
				if (builderConditions.Count > 0)
				{
					stages[name].ConditionMap.Add(builderConditions);
				}

				builderConditions = conditions.Where(x => x.IsConnect && !x.IsOnField && x.Name == name).ToList();
				if (builderConditions.Count > 0)
				{
					stages[name].ConditionMap.Add(builderConditions);
				}
			}
		}

		// Slice the conditions to the smallest possible parts
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
					firstPossibleStage = Math.Max(firstPossibleStage, stages.IndexOf(cond.Name));

					if (cond.IsOnField)
					{
						firstPossibleStage = Math.Max(firstPossibleStage, stages.IndexOf(cond.RefName!));
					}
				}

				stages.List[firstPossibleStage].Value.ConditionMap.Add(slice);
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
					stages[field.RefName!].LookupAs = field.Field.DBSideName;
				}
				else
				{
					path = string.Concat(
						stages[field.RefName!].LookupAs, ".",
						field.RefField!.DBSideName
					);

					// Propagate the include to the pipeline stage using the dependencies on conditions and the include fields.
					// The goal is to place the include in the first possible stage.
					// Important: any inclusion clears all other fields, so we insert it only after any mention of the entire document.
					//
					stageIndex = stages.IndexOf(field.RefName!);
					for (int i = stageIndex + 1; i < stages.Count; i++)
					{
						map = stages.List[i].Value.ConditionMap;

						if (map.Any(x => x.Any(xx => xx.Field.Name == field.RefName || (xx.RefField != null && xx.RefField.Name == field.RefName))))
						{
							stageIndex = i;
						}
					}
					stages.List[stageIndex].Value.ProjectAfter.Add((path, field.Field.DBSideName, field));
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
						map = stages.List[i].Value.ConditionMap;

						if (map.Any(x => x.Any(xx => xx.Field.FullName == field.Field.FullName)))
						{
							stageIndex = i;
						}
					}
					if (stageIndex == -1)
					{
						stages.List[0].Value.ProjectBefore.Add((path, null, field));
					}
					else
					{
						stages.List[stageIndex].Value.ProjectAfter.Add((path, null, field));
					}
				}
				else
				{
					path = string.Concat(stages[joinedDoc.RefName!].LookupAs, ".",
						string.Join(".", field.Field.Elements.Skip(joinedDoc.Field.ElementCount).Select(x => x.DBSideName)));

					// Propagate the exclude
					//
					var trueFullName = string.Join(".", field.Field.Elements.Skip(joinedDoc.Field.ElementCount).Select(x => x.Name));
					stageIndex = stages.IndexOf(joinedDoc.RefName!);
					for (int i = stageIndex + 1; i < stages.Count; i++)
					{
						map = stages.List[i].Value.ConditionMap;

						if (map.Any(x => x.Any(xx =>
								(xx.Name == joinedDoc.RefName && xx.Field.FullName == trueFullName) ||
								(xx.RefName == joinedDoc.RefName && xx.RefField!.FullName == trueFullName))))
						{
							stageIndex = i;
						}
					}
					stages.List[stageIndex].Value.ProjectAfter.Add((path, null, field));
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
					if (!stages.Any(x => x.Value.ProjectBefore.Any(xx => xx.toPath == null && xx.fromPath == elem.ElementName) ||
											x.Value.ProjectAfter.Any(xx => xx.toPath == null && xx.fromPath == elem.ElementName)))
					{
						path = elem.ElementName;

						stageIndex = -1;
						for (int i = 0; i < stages.Count; i++)
						{
							map = stages.List[i].Value.ConditionMap;

							if (map.Any(x => x.Any(xx =>
									(xx.Name == top.Name && xx.Field.FullName == path) ||
									(xx.RefName == top.Name && xx.RefField!.FullName == path))))
							{
								stageIndex = i;
							}
						}
						if (stageIndex == -1)
						{
							stages.List[0].Value.ProjectBefore.Add((path, null, null));
						}
						else
						{
							stages.List[stageIndex].Value.ProjectAfter.Add((path, null, null));
						}
					}
				}
			}

			// Exclude joined conteiners not projected to documents
			// !!!
		}

		// Func to get true field name
		//
		var getDBSideName = string (string alias, FieldPath fieldPath) =>
		{
			return fieldPath.DBSideName;
		};

		// Initial projection
		//
		OutputProjections(result, stages, 0, true);

		// Initial $match
		//
		{
			BuiltCondition? filter = null;
			foreach (var conds in stages.List[0].Value.ConditionMap)
			{
				filter = filter?.AppendByAnd(BuildConditionTree(conds, getDBSideName)!) ?? BuildConditionTree(conds, getDBSideName);
			}
			if (filter != null)
			{
				result.Add(new BsonDocument { { "$match", filter.BsonDocument } });
			}
		}

		// Initial projection after match
		//
		OutputProjections(result, stages, 0, false);

		// Lookup stages
		//
		{
			BuilderCondition? connect;
			BsonDocument lookup;
			BuiltCondition? filter;
			BsonDocument? let;
			string s;
			for (int i = 1; i < stages.Count; i++)
			{
				getDBSideName = string (string alias, FieldPath fieldPath) =>
				{
					var trueAlias = containers.Take(i).Any(x => x.Name == alias)
						? stages[alias].LookupAs
						: null;

					return string.IsNullOrEmpty(trueAlias)
						? fieldPath.DBSideName
						: string.Concat(trueAlias, ".", fieldPath.DBSideName);
				};

				// Joining container
				lookup = new BsonDocument { { "from", containers[i].DBSideName } };

				// First connect condition to localField and foreignField
				connect = stages.List[i].Value.ConditionMap.FirstOrDefault()?.FirstOrDefault(x => x.IsConnectOnField);
				if (connect != null)
				{
					lookup["foreignField"] = connect.Field.DBSideName;
					lookup["localField"] = getDBSideName(connect.RefName!, connect.RefField!);
				}

				// other conditions except the first connect one
				filter = null;
				let = null;
				foreach (var conds in stages.List[i].Value.ConditionMap)
				{
					if (filter != null)
					{
						foreach (var cond in conds.Where(x => x.IsOnField))
						{
							if (let == null) let = new BsonDocument();

							s = getDBSideName(cond.Name, cond.Field);
							let[MakeVariableName(s)] = "$" + s;
						}
						filter.AppendByAnd(BuildConditionTree(conds, getDBSideName)!);
					}
					else
					{
						foreach (var cond in conds.Where(x => x.IsOnField && x != connect))
						{
							if (let == null) let = new BsonDocument();

							s = getDBSideName(cond.Name, cond.Field);
							let[MakeVariableName(s)] = "$" + s;
						}
						filter = BuildConditionTree(conds.Where(x => x != connect), getDBSideName);
					}
				}

				if (let != null)
				{
					lookup["let"] = let;
				}

				if (filter != null)
				{
					lookup["pipeline"] = new BsonArray() { new BsonDocument { { "$match", filter.BsonDocument } } };
				}
				else if (connect == null/* containers[i].ContainerOperation == BuilderContainerOperations.CrossJoin */)
				{
					lookup["pipeline"] = new BsonArray();
				}

				lookup["as"] = stages.List[i].Value.LookupAs;


				OutputProjections(result, stages, i, true);

				result.Add(new BsonDocument { { "$lookup", lookup } });

				var leftJoinOrCrossJoin = containers[i].ContainerOperation != BuilderContainerOperations.Join;
				result.Add(new BsonDocument {
					{ "$unwind", new BsonDocument {
						{ "path", lookup["as"] },
						{ "preserveNullAndEmptyArrays", leftJoinOrCrossJoin }
					}}
				});

				OutputProjections(result, stages, i, false);
			}
		}

		return result;
	}

	
	private static void OutputProjections(List<BsonDocument> result, OrderedDictionary<string, StageInfo> stages, int stageIndex, bool beforeOrAfter)
	{
		var projectInfo = beforeOrAfter ? stages.List[stageIndex].Value.ProjectBefore : stages.List[stageIndex].Value.ProjectAfter;

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

	private BuiltCondition? BuildConditionTree(IEnumerable<BuilderCondition> conditions, Func<string, FieldPath, string> getDBSideName)
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
						filter = BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1, getDBSideName).filter;
					}
					else
					{
						filter = new BuiltCondition(BuildCondition(e.Current.IsOnField, e.Current, getDBSideName, Builder, _arguments), e.Current.IsOnField);
					}
				}
				else if (e.Current.IsByOr)
				{
					if (e.Current.Parentheses > 0)
					{
						filter.AppendByOr(BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1, getDBSideName).filter);
					}
					else
					{
						filter.AppendByOr(new BuiltCondition(BuildCondition(e.Current.IsOnField, e.Current, getDBSideName, Builder, _arguments), e.Current.IsOnField));
					}
				}
				else
				{
					if (e.Current.Parentheses > 0)
					{
						filter.AppendByAnd(BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1, getDBSideName).filter);
					}
					else
					{
						filter.AppendByAnd(new BuiltCondition(BuildCondition(e.Current.IsOnField, e.Current, getDBSideName, Builder, _arguments), e.Current.IsOnField));
					}
				}
			}

			return filter;
		}
	}
	private (BuiltCondition filter, int level) BuildConditionTree(IEnumerator<BuilderCondition> e, ref bool moveNext, int parentheses, Func<string, FieldPath, string> getDBSideName)
	{
		BuiltCondition filter;

		if (parentheses > 0)
		{
			var result = BuildConditionTree(e, ref moveNext, parentheses - 1, getDBSideName);
			filter = result.filter;
			if (result.level < 0)
			{
				return (filter, result.level + 1);
			}
		}
		else
		{
			filter = new BuiltCondition(BuildCondition(e.Current.IsOnField, e.Current, getDBSideName, Builder, _arguments), e.Current.IsOnField);
		}

		while (moveNext && (moveNext = e.MoveNext()))
		{
			if (e.Current.IsByOr)
			{
				if (e.Current.Parentheses > 0)
				{
					var result = BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1, getDBSideName);
					filter.AppendByOr(result.filter);
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter.AppendByOr(new BuiltCondition(BuildCondition(e.Current.IsOnField, e.Current, getDBSideName, Builder, _arguments), e.Current.IsOnField));
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
					var result = BuildConditionTree(e, ref moveNext, e.Current.Parentheses - 1, getDBSideName);
					filter.AppendByAnd(result.filter);
					if (result.level < 0)
					{
						return (filter, result.level + 1);
					}
				}
				else
				{
					filter.AppendByAnd(new BuiltCondition(BuildCondition(e.Current.IsOnField, e.Current, getDBSideName, Builder, _arguments), e.Current.IsOnField));
					if (e.Current.Parentheses < 0)
					{
						return (filter, e.Current.Parentheses + 1);
					}
				}
			}
		}

		return (filter, 0);
	}

	private static BsonDocument BuildCondition(bool useExprFormat, BuilderCondition cond, Func<string, FieldPath, string> getDBSideName, QBBuilder<TDocument, TSelect> builder, IReadOnlyDictionary<string, object?>? arguments)
	{
		if (cond.IsOnField)
		{
			return MakeConditionOnField(cond, getDBSideName);
		}
		else if (cond.IsOnParam)
		{
			var paramName = (string)cond.Value!;
			var param = builder.Parameters.First(x => x.Name == paramName);
			object? value;
			if (arguments == null || !arguments.TryGetValue(paramName, out value))
			{
				throw new InvalidOperationException($"Query builder parameter {paramName} is not set.");
			}

			return MakeConditionOnConst(useExprFormat, cond, getDBSideName, value);
		}
		else if (cond.IsOnConst)
		{
			return MakeConditionOnConst(useExprFormat, cond, getDBSideName);
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	#endregion
}