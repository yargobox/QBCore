using System.Linq.Expressions;

namespace QBCore.Extensions.Linq.Expressions.Tests;

public class ExtensionsForExpressions_Tests
{
	// UnitOfWork_StateUnderTest_ExpectedBehavior // Arrange // Act // Assert

	class AAA
	{
		public string PropertyA { get; set; } = null!;
		public string GetPropertyA() => PropertyA;
		public int FieldA;

		public BBB EntityBBB { get; set; } = null!;
		public BBB EntityFieldBBB = null!;
	}

	class BBB
	{
		public string PropertyB { get; set; } = null!;
		public string GetPropertyB() => PropertyB;
		public int FieldB;

		public AAA EntityAAA { get; set; } = null!;
		public AAA EntityFieldAAA = null!;
	}

	[Fact]
	public void GetMemberName_ForRegularUseCases_ReturnsName()
	{
		LambdaExpression pfnFromProperty = string (AAA p) => p.PropertyA;
		Assert.Equal(pfnFromProperty.GetMemberName(), nameof(AAA.PropertyA));

		LambdaExpression pfnFromField = int (AAA p) => p.FieldA;
		Assert.Equal(pfnFromField.GetMemberName(), nameof(AAA.FieldA));

		LambdaExpression pfnFromConvertExpr1 = object (AAA p) => p.PropertyA;
		Assert.Equal(pfnFromConvertExpr1.GetMemberName(), nameof(AAA.PropertyA));

		LambdaExpression pfnFromConvertExpr2 = object (AAA p) => p.FieldA;
		Assert.Equal(pfnFromConvertExpr2.GetMemberName(), nameof(AAA.FieldA));

		LambdaExpression pfnFromCallExpr = string (AAA p) => p.GetPropertyA();
		Assert.Equal(pfnFromCallExpr.GetMemberName(), nameof(AAA.GetPropertyA));

		LambdaExpression pfnFromParamExpr = AAA (AAA argName) => argName;
		Assert.Equal(pfnFromParamExpr.GetMemberName(), "argName");
	}

	[Fact]
	public void GetMemberName_WithWrongLambdaArg_ThrowsArgumentException()
	{
		LambdaExpression pfnFromWrongLambda = string (AAA p) => "";
		Assert.Throws<ArgumentException>(() => pfnFromWrongLambda.GetMemberName());
	}

	[Fact]
	public void GetPropertyOrFieldPath_WithSinglePathElements_ReturnsPath()
	{
		LambdaExpression pfnFromProperty = string (AAA p) => p.PropertyA;
		Assert.Equal(pfnFromProperty.GetPropertyOrFieldPath(), nameof(AAA.PropertyA));

		LambdaExpression pfnFromField = int (AAA p) => p.FieldA;
		Assert.Equal(pfnFromField.GetPropertyOrFieldPath(), nameof(AAA.FieldA));

		LambdaExpression pfnFromConvertExpr1 = object (AAA p) => p.PropertyA;
		Assert.Equal(pfnFromConvertExpr1.GetPropertyOrFieldPath(), nameof(AAA.PropertyA));

		LambdaExpression pfnFromConvertExpr2 = object (AAA p) => p.FieldA;
		Assert.Equal(pfnFromConvertExpr2.GetPropertyOrFieldPath(), nameof(AAA.FieldA));

		LambdaExpression pfnFromParamExpr = AAA (AAA argName) => argName;
		Assert.Equal(pfnFromParamExpr.GetPropertyOrFieldPath(true), "");
		Assert.Throws<ArgumentException>(() => pfnFromParamExpr.GetPropertyOrFieldPath(false));
	}

	[Fact]
	public void GetPropertyOrFieldPath_WithMultiplePathElements_ReturnsPath()
	{
		LambdaExpression pfnFromProperty = string (AAA p) => p.EntityBBB.PropertyB;
		Assert.Equal(pfnFromProperty.GetPropertyOrFieldPath(), $"{nameof(AAA.EntityBBB)}.{nameof(BBB.PropertyB)}");

		LambdaExpression pfnFromProperty2 = string (AAA p) => p.EntityBBB.EntityFieldAAA.PropertyA;
		Assert.Equal(pfnFromProperty2.GetPropertyOrFieldPath(), $"{nameof(AAA.EntityBBB)}.{nameof(BBB.EntityFieldAAA)}.{nameof(AAA.PropertyA)}");

		LambdaExpression pfnFromField = int (AAA p) => p.EntityFieldBBB.FieldB;
		Assert.Equal(pfnFromField.GetPropertyOrFieldPath(), $"{nameof(AAA.EntityFieldBBB)}.{nameof(BBB.FieldB)}");

		LambdaExpression pfnFromConvertExpr1 = object (AAA p) => p.EntityBBB.EntityAAA.PropertyA;
		Assert.Equal(pfnFromConvertExpr1.GetPropertyOrFieldPath(), $"{nameof(AAA.EntityBBB)}.{nameof(BBB.EntityAAA)}.{nameof(AAA.PropertyA)}");

		LambdaExpression pfnFromConvertExpr2 = object (AAA p) => p.EntityFieldBBB.EntityFieldAAA.FieldA;
		Assert.Equal(pfnFromConvertExpr2.GetPropertyOrFieldPath(), $"{nameof(AAA.EntityFieldBBB)}.{nameof(BBB.EntityFieldAAA)}.{nameof(AAA.FieldA)}");
	}
}