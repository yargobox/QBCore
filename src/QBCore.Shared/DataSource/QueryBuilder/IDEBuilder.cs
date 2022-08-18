using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

public interface IDEBuilder
{
	DEInfo Build<TDocument>(string fieldName);
	DEInfo Build<TDocument, TField>(string fieldName);
	DEInfo Build<TDocument>(LambdaExpression memberSelector);
	DEInfo Build<TDocument, TField>(Expression<Func<TDocument, TField>> memberSelector);
}