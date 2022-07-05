using System.Linq.Expressions;
using System.Reflection;

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
					return ((ParameterExpression)currentExpression).Name ?? throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
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
					throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
			}
		}
	}

	public static string GetPropertyOrFieldName(this LambdaExpression memberSelector)
	{
		var currentExpression = memberSelector.Body;

		while (true)
		{
			switch (currentExpression.NodeType)
			{
				case ExpressionType.MemberAccess:
					return ((MemberExpression)currentExpression).Member.Name;
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
					currentExpression = ((UnaryExpression)currentExpression).Operand;
					break;
				case ExpressionType.Invoke:
					currentExpression = ((InvocationExpression)currentExpression).Expression;
					break;
				default:
					throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
			}
		}
	}

	public static T GetPropertyOrFieldInfo<T>(this LambdaExpression memberSelector, Func<PropertyInfo?, FieldInfo?, T> infoSelector)
	{
		var currentExpression = memberSelector.Body;

		while (true)
		{
			switch (currentExpression.NodeType)
			{
				case ExpressionType.MemberAccess:
					var member = ((MemberExpression)currentExpression).Member;
					if (member is PropertyInfo propertyInfo)
					{
						return infoSelector(propertyInfo, null);
					}
					else if (member is FieldInfo fieldInfo)
					{
						return infoSelector(null, fieldInfo);
					}
					throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
					currentExpression = ((UnaryExpression)currentExpression).Operand;
					break;
				case ExpressionType.Invoke:
					currentExpression = ((InvocationExpression)currentExpression).Expression;
					break;
				default:
					throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
			}
		}
	}

	public static string GetPropertyOrFieldPath(this LambdaExpression memberSelector)
		=> string.Join(".", GetPropertyOrFieldPath<string>(memberSelector, x => x.Member.Name));

	public static string[] GetPropertyOrFieldPathAsArray(this LambdaExpression memberSelector)
		=> GetPropertyOrFieldPath<string>(memberSelector, x => x.Member.Name);

	public static T[] GetPropertyOrFieldPath<T>(this LambdaExpression memberSelector, Func<MemberExpression, T> infoSelector)
	{
		var list = new List<T>();
		var currentExpression = memberSelector.Body;

		while (true)
		{
			switch (currentExpression.NodeType)
			{
				case ExpressionType.Parameter:
					goto L_EXIT_WHILE;
				case ExpressionType.MemberAccess:
					{
						var memberExpression = (MemberExpression)currentExpression;

						list.Add(infoSelector(memberExpression));
						if (memberExpression.Expression?.NodeType == ExpressionType.MemberAccess)
						{
							currentExpression = memberExpression.Expression;
							break;
						}

						goto L_EXIT_WHILE;
					}
				case ExpressionType.Call:
					throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
					currentExpression = ((UnaryExpression)currentExpression).Operand;
					break;
				case ExpressionType.Invoke:
					currentExpression = ((InvocationExpression)currentExpression).Expression;
					break;
				case ExpressionType.ArrayLength:
					throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
				default:
					throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
			}
		}
L_EXIT_WHILE:
		if (list.Count == 0)
		{
			throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
		}
		list.Reverse();
		return list.ToArray();
	}
}