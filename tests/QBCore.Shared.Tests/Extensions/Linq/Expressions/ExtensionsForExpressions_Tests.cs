using System.Linq.Expressions;

namespace QBCore.Extensions.Linq.Expressions.Tests;

public class ExtensionsForExpressions_Tests
{
	// UnitOfWork_StateUnderTest_ExpectedBehavior // Arrange // Act // Assert

	class AAA
	{
		public string PropertyA { get; set; } = null!;
		public string GetPropertyA() => PropertyA;
		public int FieldA = 0;

		public BBB EntityBBB { get; set; } = null!;
		public BBB EntityFieldBBB = null!;
	}

	class BBB
	{
		public string PropertyB { get; set; } = null!;
		public string GetPropertyB() => PropertyB;
		public int FieldB = 0;

		public AAA EntityAAA { get; set; } = null!;
		public AAA EntityFieldAAA = null!;
	}

	[Fact]
	public void GetMemberName_ForRegularUseCases_ReturnsName()
	{
		LambdaExpression pfnFromProperty = string (AAA p) => p.PropertyA;
		Assert.Equal(nameof(AAA.PropertyA), pfnFromProperty.GetMemberName());

		LambdaExpression pfnFromField = int (AAA p) => p.FieldA;
		Assert.Equal(nameof(AAA.FieldA), pfnFromField.GetMemberName());

		LambdaExpression pfnFromConvertExpr1 = object (AAA p) => p.PropertyA;
		Assert.Equal(nameof(AAA.PropertyA), pfnFromConvertExpr1.GetMemberName());

		LambdaExpression pfnFromConvertExpr2 = object (AAA p) => p.FieldA;
		Assert.Equal(nameof(AAA.FieldA), pfnFromConvertExpr2.GetMemberName());

		LambdaExpression pfnFromCallExpr = string (AAA p) => p.GetPropertyA();
		Assert.Equal(nameof(AAA.GetPropertyA), pfnFromCallExpr.GetMemberName());

		LambdaExpression pfnFromParamExpr = AAA (AAA argName) => argName;
		Assert.Equal("argName", pfnFromParamExpr.GetMemberName());
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
		Assert.Equal(nameof(AAA.PropertyA), pfnFromProperty.GetPropertyOrFieldPath());

		LambdaExpression pfnFromField = int (AAA p) => p.FieldA;
		Assert.Equal(nameof(AAA.FieldA), pfnFromField.GetPropertyOrFieldPath());

		LambdaExpression pfnFromConvertExpr1 = object (AAA p) => p.PropertyA;
		Assert.Equal(nameof(AAA.PropertyA), pfnFromConvertExpr1.GetPropertyOrFieldPath());

		LambdaExpression pfnFromConvertExpr2 = object (AAA p) => p.FieldA;
		Assert.Equal(nameof(AAA.FieldA), pfnFromConvertExpr2.GetPropertyOrFieldPath());

		LambdaExpression pfnFromParamExpr = AAA (AAA argName) => argName;
		Assert.Equal("", pfnFromParamExpr.GetPropertyOrFieldPath(true));
		Assert.Throws<ArgumentException>(() => pfnFromParamExpr.GetPropertyOrFieldPath(false));
	}

	[Fact]
	public void GetPropertyOrFieldPath_WithMultiplePathElements_ReturnsPath()
	{
		LambdaExpression pfnFromProperty = string (AAA p) => p.EntityBBB.PropertyB;
		Assert.Equal($"{nameof(AAA.EntityBBB)}.{nameof(BBB.PropertyB)}", pfnFromProperty.GetPropertyOrFieldPath());

		LambdaExpression pfnFromProperty2 = string (AAA p) => p.EntityBBB.EntityFieldAAA.PropertyA;
		Assert.Equal($"{nameof(AAA.EntityBBB)}.{nameof(BBB.EntityFieldAAA)}.{nameof(AAA.PropertyA)}", pfnFromProperty2.GetPropertyOrFieldPath());

		LambdaExpression pfnFromField = int (AAA p) => p.EntityFieldBBB.FieldB;
		Assert.Equal($"{nameof(AAA.EntityFieldBBB)}.{nameof(BBB.FieldB)}", pfnFromField.GetPropertyOrFieldPath());

		LambdaExpression pfnFromConvertExpr1 = object (AAA p) => p.EntityBBB.EntityAAA.PropertyA;
		Assert.Equal($"{nameof(AAA.EntityBBB)}.{nameof(BBB.EntityAAA)}.{nameof(AAA.PropertyA)}", pfnFromConvertExpr1.GetPropertyOrFieldPath());

		LambdaExpression pfnFromConvertExpr2 = object (AAA p) => p.EntityFieldBBB.EntityFieldAAA.FieldA;
		Assert.Equal($"{nameof(AAA.EntityFieldBBB)}.{nameof(BBB.EntityFieldAAA)}.{nameof(AAA.FieldA)}", pfnFromConvertExpr2.GetPropertyOrFieldPath());
	}
}