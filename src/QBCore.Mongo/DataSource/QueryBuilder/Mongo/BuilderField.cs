using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal record BuilderField
(
	bool IncludeOrExclude,
	FieldPath Field,
	string RefName,
	FieldPath? RefField
);