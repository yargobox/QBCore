namespace QBCore.DataSource.QueryBuilder.Mongo;

internal record BuilderParameter(string Name, string TargetName, string TargetParameter, Type UnderlyingType, string DbType, bool IsNullable, System.Data.ParameterDirection Direction);