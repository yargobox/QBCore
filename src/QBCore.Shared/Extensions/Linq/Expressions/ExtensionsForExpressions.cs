using System.Linq.Expressions;

namespace QBCore.Extensions.Linq.Expressions;

public static class ExtensionsForExpressions
{
	public static string GetMemberName(this LambdaExpression memberSelector)
	{
		var currentExpression = memberSelector.Body;

		while (true)
		{
			switch (currentExpression.NodeType)
			{
				case ExpressionType.Parameter:
					return ((ParameterExpression)currentExpression).Name ?? throw new Exception("Wrong member selector: " + memberSelector.ToString());
				case ExpressionType.MemberAccess:
					return ((MemberExpression)currentExpression).Member.Name;
				case ExpressionType.Call:
					return ((MethodCallExpression)currentExpression).Method.Name;
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
					currentExpression = ((UnaryExpression)currentExpression).Operand;
					break;
				case ExpressionType.Invoke:
					currentExpression = ((InvocationExpression)currentExpression).Expression;
					break;
				case ExpressionType.ArrayLength:
					return "Length";
				default:
					throw new Exception("Wrong member selector: " + memberSelector.ToString());
			}
		}
	}
}