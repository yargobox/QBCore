using System.Linq.Expressions;
using System.Reflection;
using QBCore.Extensions.Reflection;

namespace QBCore.Extensions.Linq.Expressions;

public static class ExtensionsForExpressions
{
	public static string GetMemberName(this LambdaExpression memberSelector)
	{
		var currentExpression = memberSelector.Body;

		for (; ; )
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

	public static string GetPropertyOrFieldName(this LambdaExpression memberSelector, bool allowEmptyName = false)
	{
		var currentExpression = memberSelector.Body;

		while (true)
		{
			switch (currentExpression.NodeType)
			{
				case ExpressionType.Parameter when allowEmptyName:
					return string.Empty;
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

	public static T? GetPropertyOrFieldInfo<T>(this LambdaExpression memberSelector, Func<PropertyInfo?, FieldInfo?, T> infoSelector, bool allowEmptyInfo = false)
	{
		var currentExpression = memberSelector.Body;

		while (true)
		{
			switch (currentExpression.NodeType)
			{
				case ExpressionType.Parameter when allowEmptyInfo:
					return default(T);
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

	public static string GetPropertyOrFieldPath(this LambdaExpression memberSelector, bool allowEmptyPath = false)
		=> string.Join(".", GetPropertyOrFieldPath<string>(memberSelector, x => x.Member.Name, allowEmptyPath));

	public static string[] GetPropertyOrFieldPathAsArray(this LambdaExpression memberSelector, bool allowEmptyPath = false)
		=> GetPropertyOrFieldPath<string>(memberSelector, x => x.Member.Name, allowEmptyPath);

	public static T[] GetPropertyOrFieldPath<T>(this LambdaExpression memberSelector, Func<MemberExpression, T> infoSelector, bool allowEmptyPath = false)
	{
		var list = new List<T>();
		var currentExpression = memberSelector.Body;

		while (true)
		{
			switch (currentExpression.NodeType)
			{
				case ExpressionType.Parameter when allowEmptyPath:
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
				default:
					throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
			}
		}
L_EXIT_WHILE:
		if (!allowEmptyPath && list.Count == 0)
		{
			throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
		}
		list.Reverse();
		return list.ToArray();
	}

	public static Expression<Func<object, object?>> MakeCommonGetter(this PropertyInfo propertyInfo)
	{
		if (propertyInfo == null)
		{
			throw new ArgumentNullException(nameof(propertyInfo));
		}

		var getMethodInfo = propertyInfo.GetMethod;
		if (getMethodInfo == null)
		{
			throw new InvalidOperationException($"The property '{propertyInfo.PropertyType.FullName} {propertyInfo.Name}' of class '{propertyInfo.DeclaringType?.FullName}' has no 'get' accessor.");
		}

		var documentParameter = Expression.Parameter(typeof(object), "item");
		var memberSelector = Expression.Lambda<Func<object, object?>>(
			Expression.Convert(
				Expression.MakeMemberAccess(
					Expression.Convert(documentParameter, propertyInfo.DeclaringType!),
					propertyInfo
				),
				typeof(object)
			),
			documentParameter
		);

		return memberSelector;
	}
	public static Expression<Func<object, object?>> MakeCommonGetter(this FieldInfo fieldInfo)
	{
		if (fieldInfo == null)
		{
			throw new ArgumentNullException(nameof(fieldInfo));
		}

		var documentParameter = Expression.Parameter(typeof(object), "item");
		var memberSelector = Expression.Lambda<Func<object, object?>>(
			Expression.Convert(
				Expression.MakeMemberAccess(
					Expression.Convert(documentParameter, fieldInfo.DeclaringType!),
					fieldInfo
				),
				typeof(object)
			),
			documentParameter
		);

		return memberSelector;
	}
	public static Expression<Action<object, object?>>? MakeCommonSetter(this PropertyInfo propertyInfo)
	{
		if (propertyInfo == null)
		{
			throw new ArgumentNullException(nameof(propertyInfo));
		}

		var setMethodInfo = propertyInfo.SetMethod;
		if (setMethodInfo == null)
		{
			return null;
		}

		var documentParameter = Expression.Parameter(typeof(object), "item");
		var valueParameter = Expression.Parameter(typeof(object), "value");
		var memberSelector = Expression.Lambda<Action<object, object?>>(
			Expression.Call(
				Expression.Convert(documentParameter,propertyInfo.DeclaringType!),
				setMethodInfo,
				Expression.Convert(valueParameter, propertyInfo.PropertyType)
			),
			documentParameter,
			valueParameter
		);

		return memberSelector;
	}
	public static Expression<Action<object, object?>>? MakeCommonSetter(this FieldInfo fieldInfo)
	{
		if (fieldInfo == null)
		{
			throw new ArgumentNullException(nameof(fieldInfo));
		}

		if (fieldInfo.IsInitOnly)
		{
			return null;
		}

		var documentParameter = Expression.Parameter(typeof(object), "item");
		var valueParameter = Expression.Parameter(typeof(object), "value");
		var field = Expression.Field(Expression.Convert(documentParameter, fieldInfo.DeclaringType!), fieldInfo);
		var value = Expression.Convert(valueParameter, fieldInfo.FieldType);
		var body = Expression.Assign(field, value);

		return Expression.Lambda<Action<object, object?>>(body, documentParameter, valueParameter);
	}
	public static Expression<Action<object, object?>>? MakeCommonSetter(this LambdaExpression memberSelector)
	{
		var stack = new List<Expression>(MemberSelectorToExpressions(memberSelector));
		stack.Reverse();

		var documentParameter = Expression.Parameter(typeof(object), "item");
		Expression navigator = stack.First().Type != typeof(object)
			? Expression.Convert(documentParameter, stack.First().Type)
			: documentParameter;
		var valueParameter = Expression.Parameter(typeof(object), "value");
		Expression currentExpression;

		for (int i = 1; i < stack.Count; i++)
		{
			currentExpression = stack[i];
			switch (currentExpression.NodeType)
			{
				case ExpressionType.MemberAccess:
					{
						var memberExpression = (MemberExpression)currentExpression;

						if (i + 1 < stack.Count && stack.Skip(i + 1).OfType<MemberExpression>().Any())
						{
							navigator = Expression.MakeMemberAccess(navigator, memberExpression.Member);
						}
						else
						{
							if (memberExpression.Member is FieldInfo fieldInfo)
							{
								if (fieldInfo.IsInitOnly)
								{
									return null;
								}

								var field = Expression.Field(navigator, fieldInfo);
								var value = Expression.Convert(valueParameter, fieldInfo.FieldType);
								var body = Expression.Assign(field, value);
								return Expression.Lambda<Action<object, object?>>(body, documentParameter, valueParameter);
							}
							else if (memberExpression.Member is PropertyInfo propertyInfo)
							{
								var setMethodInfo = propertyInfo.SetMethod;
								if (setMethodInfo == null)
								{
									return null;
								}

								var value = Expression.Convert(valueParameter, propertyInfo.PropertyType);
								var body = Expression.Call(navigator, setMethodInfo, value);
								return Expression.Lambda<Action<object, object?>>(body, documentParameter, valueParameter);
							}
						}
						break;
					}
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
					break;
				case ExpressionType.Invoke:
					var invocationExpression = (InvocationExpression)currentExpression;
					navigator = Expression.Invoke(invocationExpression, navigator);
					break;
				case ExpressionType.Parameter:
				case ExpressionType.Call:
				default: throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
			}
		}
		throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
	}
	public static Expression<Action<T, object?>>? MakeSetter<T>(this Expression<Func<T, object?>> memberSelector)
	{
		var stack = new List<Expression>(MemberSelectorToExpressions(memberSelector));
		stack.Reverse();

		var documentParameter = (ParameterExpression)stack.First();
		Expression navigator = documentParameter;
		var valueParameter = Expression.Parameter(typeof(object), "value");
		Expression currentExpression;

		for (int i = 1; i < stack.Count; i++)
		{
			currentExpression = stack[i];
			switch (currentExpression.NodeType)
			{
				case ExpressionType.MemberAccess:
					{
						var memberExpression = (MemberExpression)currentExpression;

						if (i + 1 < stack.Count && stack.Skip(i + 1).OfType<MemberExpression>().Any())
						{
							navigator = Expression.MakeMemberAccess(navigator, memberExpression.Member);
						}
						else
						{
							if (memberExpression.Member is FieldInfo fieldInfo)
							{
								if (fieldInfo.IsInitOnly)
								{
									return null;
								}

								var field = Expression.Field(navigator, fieldInfo);
								var value = Expression.Convert(valueParameter, fieldInfo.FieldType);
								var body = Expression.Assign(field, value);
								return Expression.Lambda<Action<T, object?>>(body, documentParameter, valueParameter);
							}
							else if (memberExpression.Member is PropertyInfo propertyInfo)
							{
								var setMethodInfo = propertyInfo.SetMethod;
								if (setMethodInfo == null)
								{
									return null;
								}

								var value = Expression.Convert(valueParameter, propertyInfo.PropertyType);
								var body = Expression.Call(navigator, setMethodInfo, value);
								return Expression.Lambda<Action<T, object?>>(body, documentParameter, valueParameter);
							}
						}
						break;
					}
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
					break;
				case ExpressionType.Invoke:
					var invocationExpression = (InvocationExpression)currentExpression;
					navigator = Expression.Invoke(invocationExpression, navigator);
					break;
				case ExpressionType.Parameter:
				case ExpressionType.Call:
				default: throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
			}
		}
		throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
	}
	public static Expression<Action<T, TField>>? MakeSpecificSetter<T, TField>(this Expression<Func<T, TField>> memberSelector)
	{
		var stack = new List<Expression>(MemberSelectorToExpressions(memberSelector));
		stack.Reverse();

		var documentParameter = (ParameterExpression)stack.First();
		Expression navigator = documentParameter;
		var valueParameter = Expression.Parameter(typeof(TField), "value");
		Expression currentExpression;

		for (int i = 1; i < stack.Count; i++)
		{
			currentExpression = stack[i];
			switch (currentExpression.NodeType)
			{
				case ExpressionType.MemberAccess:
					{
						var memberExpression = (MemberExpression)currentExpression;

						if (i + 1 < stack.Count && stack.Skip(i + 1).OfType<MemberExpression>().Any())
						{
							navigator = Expression.MakeMemberAccess(navigator, memberExpression.Member);
						}
						else
						{
							if (memberExpression.Member is FieldInfo fieldInfo)
							{
								if (fieldInfo.IsInitOnly)
								{
									return null;
								}

								var field = Expression.Field(navigator, fieldInfo);
								var body = Expression.Assign(field, valueParameter);
								return Expression.Lambda<Action<T, TField>>(body, documentParameter, valueParameter);
							}
							else if (memberExpression.Member is PropertyInfo propertyInfo)
							{
								var setMethodInfo = propertyInfo.SetMethod;
								if (setMethodInfo == null)
								{
									return null;
								}

								var body = Expression.Call(navigator, setMethodInfo, valueParameter);
								return Expression.Lambda<Action<T, TField>>(body, documentParameter, valueParameter);
							}
						}
						break;
					}
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
					break;
				case ExpressionType.Invoke:
					var invocationExpression = (InvocationExpression)currentExpression;
					navigator = Expression.Invoke(invocationExpression, navigator);
					break;
				case ExpressionType.Parameter:
				case ExpressionType.Call:
				default: throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
			}
		}
		throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
	}

	private static IEnumerable<Expression> MemberSelectorToExpressions(this LambdaExpression memberSelector)
	{
		if (memberSelector == null)
		{
			throw new ArgumentNullException(nameof(memberSelector));
		}

		var currentExpression = memberSelector.Body;
		for (; ; )
		{
			yield return currentExpression;

			switch (currentExpression.NodeType)
			{
				case ExpressionType.Parameter: yield break;
				case ExpressionType.MemberAccess:
					{
						var memberExpression = (MemberExpression)currentExpression;
						if (memberExpression.Expression?.NodeType == ExpressionType.MemberAccess)
						{
							currentExpression = memberExpression.Expression;
							break;
						}
						else if (memberExpression.Expression?.NodeType == ExpressionType.Parameter)
						{
							currentExpression = memberExpression.Expression;
							break;
						}
						yield break;
					}
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked: currentExpression = ((UnaryExpression)currentExpression).Operand; break;
				case ExpressionType.Invoke: currentExpression = ((InvocationExpression)currentExpression).Expression; break;
				default: throw new ArgumentException("Wrong member selector: " + memberSelector.ToString());
			}
		}
	}
}