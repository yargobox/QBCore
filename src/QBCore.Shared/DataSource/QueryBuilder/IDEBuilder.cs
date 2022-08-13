using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

public interface IDEBuilder
{
	DataEntry Build<TDocument>(string fieldName);
	DataEntry Build<TDocument, TField>(string fieldName);
	DataEntry Build<TDocument>(LambdaExpression memberSelector);
	DataEntry Build<TDocument, TField>(Expression<Func<TDocument, TField>> memberSelector);
}