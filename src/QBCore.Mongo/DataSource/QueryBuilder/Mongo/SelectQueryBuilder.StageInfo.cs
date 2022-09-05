namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed partial class SelectQueryBuilder<TDocument, TSelect>
{
	private enum StageOperations
	{
		Lookup = 0,
		Unwind = 1
	}

	private sealed class StageInfo
	{
		public readonly StageOperations StageOperation;
		public readonly QBContainer Container;
		public string LookupAs;
		public readonly List<List<QBCondition>> ConditionMap;
		public readonly List<(string fromPath, string? toPath, QBField? builderField)> ProjectBefore;
		public readonly List<(string fromPath, string? toPath, QBField? builderField)> ProjectAfter;
		public List<(string path, SO sortOrder)>? SortBeforeProject;
		public List<(string path, SO sortOrder)>? SortAfterProject;

		public StageInfo(QBContainer container, StageOperations stageOperation)
		{
			StageOperation = stageOperation;
			Container = container;
			LookupAs = "___" + container.Alias;
			ConditionMap = new List<List<QBCondition>>();
			ProjectBefore = new List<(string fromPath, string? toPath, QBField? builderField)>();
			ProjectAfter = new List<(string fromPath, string? toPath, QBField? builderField)>();
		}
	}
}