namespace QBCore.DataSource.QueryBuilder;

internal record BuilderCondition(string LeftTarget, string LeftField, string? RightTarget, string? RightField, object? ConstValue, ConditionOperations Operation);
