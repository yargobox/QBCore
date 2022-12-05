using FluentAssertions;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.Controllers.Tests;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.Mongo.Tests;

[Collection(nameof(GlobalTestFixture))]
public class SelectQueryBuilder_Tests
{
	public SelectQueryBuilder_Tests(GlobalTestFixture fixture)
	{
	}

	/// <summary>
    /// Mocks IDataContext that mocks IMongoDatabase.GetCollection().AggregateAsync() to intercept its arguments during the query builder invocation.
    /// </summary>
	sealed class MongoCollectionAggregateAsyncStub
	{
		public readonly IDataContext DataContext;

		public IClientSessionHandle? AggregateSessionArg;
		public string? AggregatePipelineArg;
		public AggregateOptions? AggregateOptionsArg;
		public CancellationToken AggregateCancellationTokenArg;

		public MongoCollectionAggregateAsyncStub()
		{
			var fnAggregateAsync = void (PipelineDefinition<Document, DocumentSelectDto> pipeline, AggregateOptions options, CancellationToken cancellationToken) =>
			{
				AggregatePipelineArg = pipeline.ToString();
				AggregateOptionsArg = options;
				AggregateCancellationTokenArg = cancellationToken;
			};
			var fnAggregateWithSessionHandleAsync = void (IClientSessionHandle session, PipelineDefinition<Document, DocumentSelectDto> pipeline, AggregateOptions options, CancellationToken cancellationToken) =>
			{
				AggregateSessionArg = session;
				AggregatePipelineArg = pipeline.ToString();
				AggregateOptionsArg = options;
				AggregateCancellationTokenArg = cancellationToken;
			};

			var asyncCursor = new Mock<IAsyncCursor<DocumentSelectDto>>(MockBehavior.Loose);

			var mongoCollection = new Mock<IMongoCollection<Document>>(MockBehavior.Strict);
			mongoCollection
				.Setup(x => x.AggregateAsync<DocumentSelectDto>(It.IsAny<PipelineDefinition<Document, DocumentSelectDto>>(), It.IsAny<AggregateOptions>(), It.IsAny<CancellationToken>()))
				.Callback(fnAggregateAsync)
				.ReturnsAsync(asyncCursor.Object);
			mongoCollection
				.Setup(x => x.AggregateAsync<DocumentSelectDto>(It.IsAny<IClientSessionHandle>(), It.IsAny<PipelineDefinition<Document, DocumentSelectDto>>(), It.IsAny<AggregateOptions?>(), It.IsAny<CancellationToken>()))
				.Callback(fnAggregateWithSessionHandleAsync)
				.ReturnsAsync(asyncCursor.Object);

			var mongoDatabase = new Mock<IMongoDatabase>(MockBehavior.Strict);
			mongoDatabase
				.Setup(x => x.GetCollection<Document>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings?>()))
				.Returns(mongoCollection.Object);

			DataContext = new MongoDataContext(mongoDatabase.Object);
		}
	}


	[Fact]
	public async Task SelectAsync_WithSelectBuilder_1_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_1);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		using var cursor = await qbSelect.SelectAsync();

		stub.AggregateSessionArg.Should().BeNull();
		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"$and\" : [{ \"_id\" : 0 }, { \"_id\" : 1 }] }, { \"_id\" : 2 }, { \"_id\" : 3 }] } }]");
		stub.AggregateOptionsArg.Should().BeNull();
		stub.AggregateCancellationTokenArg.Should().BeEquivalentTo(default(CancellationToken));
	}

	[Fact]
	public async Task SelectAsync_WithSelectBuilder_2_WithSkipAndTakeAndQueryStringCallback_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_2);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		string? queryString = null;
		var options = new DataSourceSelectOptions
		{
			QueryStringCallback = x => queryString = x
		};

		using var cursor = await qbSelect.SelectAsync(987, 123, options);

		stub.AggregateSessionArg.Should().BeNull();
		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"_id\" : 0 }, { \"$and\" : [{ \"_id\" : 1 }, { \"_id\" : 2 }, { \"_id\" : 3 }] }, { \"_id\" : 4 }] } }, { \"$skip\" : NumberLong(987) }, { \"$limit\" : NumberLong(123) }]");
		queryString.Should().BeEquivalentTo($"db.document.aggregate({stub.AggregatePipelineArg});");
		stub.AggregateOptionsArg.Should().BeNull();
		stub.AggregateCancellationTokenArg.Should().BeEquivalentTo(default(CancellationToken));
	}

	[Fact]
	public async Task SelectAsync_WithSelectBuilder_3_WithOptionsAndQueryStringCallbackAsync_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_3);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		string? queryString = null;
		var options = new DataSourceSelectOptions
		{
			QueryStringCallbackAsync = async x =>
			{
				Interlocked.Exchange(ref queryString, x);
				await Task.CompletedTask;
			},
			NativeOptions = new AggregateOptions { Comment = "test" },
			NativeClientSession = new Mock<IClientSessionHandle>().Object
		};
		using var cancellationTokenSource = new CancellationTokenSource();

		using var cursor = await qbSelect.SelectAsync(0, -1, options, cancellationTokenSource.Token);

		stub.AggregateSessionArg.Should().BeSameAs(options.NativeClientSession);
		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"_id\" : 0 }, { \"$and\" : [{ \"_id\" : 1 }, { \"_id\" : 2 }, { \"$or\" : [{ \"_id\" : 3 }, { \"_id\" : 4 }] }] }, { \"_id\" : 5 }] } }]");
		queryString.Should().BeEquivalentTo($"db.document.aggregate({stub.AggregatePipelineArg});");
		stub.AggregateOptionsArg.Should().BeSameAs(options.NativeOptions);
		stub.AggregateCancellationTokenArg.Should().BeEquivalentTo(cancellationTokenSource.Token);
	}

	[Fact]
	public async Task SelectAsync_WithSelectBuilder_4_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_4);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		using var cursor = await qbSelect.SelectAsync();

		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"_id\" : 0 }, { \"$and\" : [{ \"_id\" : 1 }, { \"_id\" : 2 }] }], \"_id\" : 3 } }]");
	}

	[Fact]
	public async Task SelectAsync_WithSelectBuilder_5_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_5);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		using var cursor = await qbSelect.SelectAsync();

		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"$or\" : [{ \"$and\" : [{ \"_id\" : 0 }, { \"_id\" : 1 }] }, { \"_id\" : 2 }], \"_id\" : 3 }, { \"_id\" : 4 }] } }]");
	}

	[Fact]
	public async Task SelectAsync_WithSelectBuilder_6_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_6);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		using var cursor = await qbSelect.SelectAsync();

		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"_id\" : 0 }, { \"_id\" : 1, \"$or\" : [{ \"_id\" : 2 }, { \"_id\" : 3 }] }] } }]");
	}

	[Fact]
	public async Task SelectAsync_WithSelectBuilder_7_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_7);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		using var cursor = await qbSelect.SelectAsync();

		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"_id\" : 0 }, { \"_id\" : 1, \"$or\" : [{ \"$and\" : [{ \"_id\" : 2 }, { \"_id\" : 3 }] }, { \"_id\" : 4 }] }, { \"_id\" : 5 }] } }]");
	}

	[Fact]
	public async Task SelectAsync_WithSelectBuilder_8_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_8);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		using var cursor = await qbSelect.SelectAsync();

		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"_id\" : 0 }, { \"_id\" : 1, \"$or\" : [{ \"_id\" : 3 }, { \"_id\" : 4 }] }] } }]");
	}

	[Fact]
	public async Task SelectAsync_WithSelectBuilder_9_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_9);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		using var cursor = await qbSelect.SelectAsync();

		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"_id\" : 0 }, { \"_id\" : 1, \"$or\" : [{ \"$and\" : [{ \"_id\" : 2 }, { \"_id\" : 3 }] }, { \"_id\" : 4 }] }] } }]");
	}

	[Fact]
	public async Task SelectAsync_WithSelectBuilder_10_PerformsExpectedQuery()
	{
		var qbFactory = CreateQBFactory<int, Document, DocumentSelectDto>(DocumentSelectDto.SelectBuilder_10);
		var stub = new MongoCollectionAggregateAsyncStub();
		var qbSelect = qbFactory.CreateQBSelect<Document, DocumentSelectDto>(stub.DataContext);

		using var cursor = await qbSelect.SelectAsync();

		stub.AggregatePipelineArg.Should().BeEquivalentTo("[{ \"$match\" : { \"$or\" : [{ \"$or\" : [{ \"_id\" : 0 }, { \"_id\" : 1, \"$or\" : [{ \"$and\" : [{ \"_id\" : 2 }, { \"_id\" : 3 }] }, { \"_id\" : 4 }] }, { \"_id\" : 5 }], \"$and\" : [{ \"_id\" : 6 }, { \"_id\" : 7 }] }, { \"_id\" : 8 }] } }]");
	}

	static IQueryBuilderFactory CreateQBFactory<TKey, TDocument, TSelect>(Action<IQBMongoSelectBuilder<Document, DocumentSelectDto>> selectBuilderMethod)
	{
		var dsTypeInfo = new DSTypeInfo
		(
			Concrete: typeof(NotSupported),
			Interface: typeof(NotSupported),
			TKey: typeof(TKey),
			TDocument: typeof(TDocument),
			TCreate: typeof(NotSupported),
			TSelect: typeof(TSelect),
			TUpdate: typeof(NotSupported),
			TDelete: typeof(NotSupported),
			TRestore: typeof(NotSupported)
		);

		return new MongoQBFactory(dsTypeInfo, DataSourceOptions.CanSelect, null, selectBuilderMethod, null, null, null, null, true);
	}

	public class Document
	{
		[DeId] public int Id { get; set; }
		[DeDeleted] public DateTimeOffset Deleted { get; set; }
	}

	public class DocumentSelectDto
	{
		[DeId] public int Id { get; set; }
		[DeDeleted] public DateTimeOffset Deleted { get; set; }

		public static void SelectBuilder_1(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")

				// 0 & 1 | 2 | 3 => (0 & 1) | 2 | 3

				.Condition(sel => sel.Id, 0, FO.Equal)
				.Condition(sel => sel.Id, 1, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 2, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 3, FO.Equal)
			;
		}

		public static void SelectBuilder_2(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")

				// 0 | 1 & 2 & 3 | 4 => 0 | (1 & 2 & 3) | 4

				.Condition(sel => sel.Id, 0, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 1, FO.Equal)
				.Condition(sel => sel.Id, 2, FO.Equal)
				.Condition(sel => sel.Id, 3, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 4, FO.Equal)
			;
		}

		public static void SelectBuilder_3(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")
				
				// 0 | 1 & 2 & (3 | 4) | 5 => 0 | (1 & 2 & (3 | 4)) | 5

				.Condition(sel => sel.Id, 0, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 1, FO.Equal)
				.Condition(sel => sel.Id, 2, FO.Equal)
				.Begin()
					.Condition(sel => sel.Id, 3, FO.Equal)
					.Or()
					.Condition(sel => sel.Id, 4, FO.Equal)
				.End()
				.Or()
				.Condition(sel => sel.Id, 5, FO.Equal)
			;
		}

		public static void SelectBuilder_4(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")
				
				// (0 | 1 & 2) & 3 => (0 | (1 & 2)) & 3

	 			.Begin()
					.Condition(sel => sel.Id, 0, FO.Equal)
					.Or()
					.Condition(sel => sel.Id, 1, FO.Equal)
					.Condition(sel => sel.Id, 2, FO.Equal)
				.End()
				.Condition(sel => sel.Id, 3, FO.Equal)
			;
		}

		public static void SelectBuilder_5(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")

				// (0 & 1 | 2) & 3 | 4 => (((0 & 1) | 2) & 3) | 4

				.Begin()
					.Condition(sel => sel.Id, 0, FO.Equal)
					.Condition(sel => sel.Id, 1, FO.Equal)
					.Or()
					.Condition(sel => sel.Id, 2, FO.Equal)
				.End()
				.Condition(sel => sel.Id, 3, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 4, FO.Equal)
			;
		}

		public static void SelectBuilder_6(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")
				
				// 0 | 1 & (2 | 3) => 0 | (1 & (2 | 3))

				.Condition(sel => sel.Id, 0, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 1, FO.Equal)
				.Begin()
					.Condition(sel => sel.Id, 2, FO.Equal)
					.Or()
					.Condition(sel => sel.Id, 3, FO.Equal)
				.End()
			;
		}

		public static void SelectBuilder_7(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")
				
				// 0 | 1 & (2 & 3 | 4) | 5 => 0 | (1 & ((2 & 3) | 4)) | 5

				.Condition(sel => sel.Id, 0, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 1, FO.Equal)
				.Begin()
					.Condition(sel => sel.Id, 2, FO.Equal)
					.Condition(sel => sel.Id, 3, FO.Equal)
					.Or()
					.Condition(sel => sel.Id, 4, FO.Equal)
				.End()
				.Or()
				.Condition(sel => sel.Id, 5, FO.Equal)
			;
		}

		public static void SelectBuilder_8(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")
				
				// 0 | 1 & (3 | 4) => 0 | (1 & (3 | 4))

				.Condition(sel => sel.Id, 0, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 1, FO.Equal)
				.Begin()
					//.Condition(sel => sel.Id, 2, FO.Equal)
					.Condition(sel => sel.Id, 3, FO.Equal)
					.Or()
					.Condition(sel => sel.Id, 4, FO.Equal)
				.End()
			;
		}

		public static void SelectBuilder_9(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")
				
				// 0 | 1 & (2 & 3 | 4) => 0 | (1 & ((2 & 3) | 4))

				.Condition(sel => sel.Id, 0, FO.Equal)
				.Or()
				.Condition(sel => sel.Id, 1, FO.Equal)
				.Begin()
					.Condition(sel => sel.Id, 2, FO.Equal)
					.Condition(sel => sel.Id, 3, FO.Equal)
					.Or()
					.Condition(sel => sel.Id, 4, FO.Equal)
				.End()
			;
		}

		public static void SelectBuilder_10(IQBMongoSelectBuilder<Document, DocumentSelectDto> builder)
		{
			builder
				.Select("document")
				
				// (0 | 1 & (2 & 3 | 4) | 5) & (6 & 7) | 8 => ((0 | (1 & ((2 & 3) | 4)) | 5) & (6 & 7)) | 8

				.Begin().Begin()
					.Begin()
						.Condition(sel => sel.Id, 0, FO.Equal)
						.Or()
						.Condition(sel => sel.Id, 1, FO.Equal)
						.Begin()
							.Condition(sel => sel.Id, 2, FO.Equal)
							.Condition(sel => sel.Id, 3, FO.Equal)
							.Or()
							.Condition(sel => sel.Id, 4, FO.Equal)
						.End()
						.Or()
						.Condition(sel => sel.Id, 5, FO.Equal)
					.End()
					.Begin()
						.Condition(sel => sel.Id, 6, FO.Equal)
						.Condition(sel => sel.Id, 7, FO.Equal)
					.End()
					.Or()
					.Condition(sel => sel.Id, 8, FO.Equal)
				.End().End()
			;
		}
	}


	/// <summary>
    /// We need a DS definition to make QBCore register document types: Document and DocumentSelectDto
    /// </summary>
	[DsApiController, DataSource(typeof(MongoDataLayer))]
	public sealed class TestDS : DataSource<int, Document, NotSupported, DocumentSelectDto, NotSupported, NotSupported, NotSupported, TestDS>
	{
		public TestDS(IServiceProvider serviceProvider) : base(serviceProvider) { }

		static void DSInfoBuilder(IDSBuilder builder)
		{
			// We must hint at one of select builders, otherwise the InvalidOperationException will be thrown.
			builder.SelectBuilder = DocumentSelectDto.SelectBuilder_1;
		}
	}


	public interface ITestDSInfo : IDataSource<int, Document, EmptyDto, DocumentSelectDto, EmptyDto, EmptyDto, EmptyDto> { }

	[DataSource]
	public sealed class TestDSInfo : DataSource<int, Document, EmptyDto, DocumentSelectDto, EmptyDto, EmptyDto, EmptyDto, TestDSInfo>, ITestDSInfo
	{
		public TestDSInfo(IServiceProvider serviceProvider) : base(serviceProvider) { }

		static void DSInfoBuilder(IDSBuilder builder)
		{
			builder.Name = "[DS]";
			builder.Options |= DataSourceOptions.CanSelect;
			builder.DataContextName = "OtherThanDefault";
			builder.DataLayer = typeof(MongoDataLayer);
			builder.BuildAutoController = false;
			builder.ControllerName = "[DS:guessPlural]";
			builder.IsServiceSingleton = true;
			builder.ServiceInterface = typeof(ITestDSInfo);		
			builder.InsertBuilder = InsertBuilder;
			builder.SelectBuilder = SelectBuilder;
			builder.UpdateBuilder = UpdateBuilder;
			builder.SoftDelBuilder = SoftDelBuilder;
			builder.RestoreBuilder = RestoreBuilder;
		}

		static void InsertBuilder(IQBMongoInsertBuilder<Document, EmptyDto> qb) => qb.Insert("document");
		static void SelectBuilder(IQBMongoSelectBuilder<Document, DocumentSelectDto> qb) => qb.Select("document");
		static void UpdateBuilder(IQBMongoUpdateBuilder<Document, EmptyDto> qb) => qb.Update("document");
		static void SoftDelBuilder(IQBMongoSoftDelBuilder<Document, EmptyDto> qb) => qb.Update("document");
		static void RestoreBuilder(IQBMongoRestoreBuilder<Document, EmptyDto> qb) => qb.Update("document");
	}


	public interface ITestDSInfo2 : IDataSource<int, Document, EmptyDto, DocumentSelectDto, EmptyDto, EmptyDto, EmptyDto> { }

	[DsApiController("UniqueDSControllerName"), DataSource("UniqueDSName", typeof(MongoDataLayer), DataSourceOptions.SoftDelete)]
	public sealed class TestDSInfo2 : DataSource<int, Document, EmptyDto, DocumentSelectDto, EmptyDto, EmptyDto, EmptyDto, TestDSInfo>, ITestDSInfo2
	{
		public TestDSInfo2(IServiceProvider serviceProvider) : base(serviceProvider) { }

		static void InsertBuilder(IQBMongoInsertBuilder<Document, EmptyDto> qb) => qb.Insert("document");
		static void SelectBuilder(IQBMongoSelectBuilder<Document, DocumentSelectDto> qb) => qb.Select("document");
		static void UpdateBuilder(IQBMongoUpdateBuilder<Document, EmptyDto> qb) => qb.Update("document");
		static void SoftDelBuilder(IQBMongoSoftDelBuilder<Document, EmptyDto> qb) => qb.Update("document");
		static void RestoreBuilder(IQBMongoRestoreBuilder<Document, EmptyDto> qb) => qb.Update("document");
	}
}