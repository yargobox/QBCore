namespace QBCore.Extensions.Reflection.Tests;

public class ExtensionsForReflection_Tests
{
	// UnitOfWork_StateUnderTest_ExpectedBehavior // Arrange // Act // Assert

	class AAA
	{
		public string NotNullableRefProperty { get; set; } = null!;
		public string? NullableRefProperty { get; set; }
		public int NotNullableValueProperty { get; set; }
		public int? NullableValueProperty { get; set; }

		public string NotNullableRefField = null!;
		public string? NullableRefField;
		public int NotNullableValueField;
		public int? NullableValueField;
	}

	[Fact]
	public void IsNullable_ForAllRefAndValueTypesNullableAndNotNullablePropertiesAndFields_ReturnsValidResult()
	{
		var notNullableRefProperty = typeof(AAA).GetProperty(nameof(AAA.NotNullableRefProperty));
		Assert.False(notNullableRefProperty?.IsNullable());

		var nullableRefProperty = typeof(AAA).GetProperty(nameof(AAA.NullableRefProperty));
		Assert.True(nullableRefProperty?.IsNullable());

		var notNullableValueProperty = typeof(AAA).GetProperty(nameof(AAA.NotNullableValueProperty));
		Assert.False(notNullableValueProperty?.IsNullable());

		var nullableValueProperty = typeof(AAA).GetProperty(nameof(AAA.NullableValueProperty));
		Assert.True(nullableValueProperty?.IsNullable());

		var notNullableRefField = typeof(AAA).GetField(nameof(AAA.NotNullableRefField));
		Assert.False(notNullableRefField?.IsNullable());

		var nullableRefField = typeof(AAA).GetField(nameof(AAA.NullableRefField));
		Assert.True(nullableRefField?.IsNullable());

		var notNullableValueField = typeof(AAA).GetField(nameof(AAA.NotNullableValueField));
		Assert.False(notNullableValueField?.IsNullable());

		var nullableValueField = typeof(AAA).GetField(nameof(AAA.NullableValueField));
		Assert.True(nullableValueField?.IsNullable());
	}
}