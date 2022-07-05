namespace QBCore.DataSource.QueryBuilder;

public interface IQBBuilder
{
}

public interface IQBBuilder<TDocument, TProjection> : IQBBuilder
{
}

public interface IQBInsertBuilder<TDocument, TCreate> : IQBBuilder<TDocument, TCreate>
{
}

public interface IQBSelectBuilder<TDocument, TSelect> : IQBBuilder<TDocument, TSelect>
{
}

public interface IQBUpdateBuilder<TDocument, TUpdate> : IQBBuilder<TDocument, TUpdate>
{
}

public interface IQBDeleteBuilder<TDocument, TDelete> : IQBBuilder<TDocument, TDelete>
{
}

public interface IQBSoftDelBuilder<TDocument, TDelete> : IQBBuilder<TDocument, TDelete>
{
}

public interface IQBRestoreBuilder<TDocument, TDelete> : IQBBuilder<TDocument, TDelete>
{
}