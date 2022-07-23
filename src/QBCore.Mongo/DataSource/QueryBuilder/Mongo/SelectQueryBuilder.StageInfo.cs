namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed partial class SelectQueryBuilder<TDocument, TSelect>
{
	private sealed class StageInfo
	{
		public readonly QBContainer Container;
		public string LookupAs;
		public readonly List<List<QBCondition>> ConditionMap;
		public readonly List<(string fromPath, string? toPath, QBField? builderField)> ProjectBefore;
		public readonly List<(string fromPath, string? toPath, QBField? builderField)> ProjectAfter;

		public StageInfo(QBContainer container)
		{
			Container = container;
			LookupAs = "___" + container.Alias;
			ConditionMap = new List<List<QBCondition>>();
			ProjectBefore = new List<(string fromPath, string? toPath, QBField? builderField)>();
			ProjectAfter = new List<(string fromPath, string? toPath, QBField? builderField)>();
		}
	}
}