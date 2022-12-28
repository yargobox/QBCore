using System.Text;
using FluentAssertions;

namespace QBCore.Extensions.Internals.Tests;

public class ConvertTo_Tests
{
	[Fact]
	public void ConvertTo_AllCases_ExpectedBehavior()
	{
		{
			SomeEnum e1 = SomeEnum.First;

			var result = ConvertTo<int>.From(e1);

			result.Should().Be(1);
		}

		{
			Enum e1 = SomeEnum.First;

			var pfn = () => ConvertTo<int>.From(e1);

			pfn.Should().Throw<ArgumentException>();
		}

		{
			Enum e1 = SomeEnum.First;

			var result = ConvertTo<int>.FromObject<SomeEnum>(e1);

			result.Should().Be(1);
		}

 		{
			var e1 = SomeEnum.First | SomeEnum.Second;
			
			var result = ConvertTo<sbyte>.From(e1);

			result.Should().Be(3);
		}

		{
			var e1 = SomeEnum.First | SomeEnum.Second;

			var result = ConvertTo<System.Data.ParameterDirection>.From(e1);

			result.Should().Be(System.Data.ParameterDirection.InputOutput);
		}

		{
			var e1 = SomeEnum.Max;

			var result = ConvertTo<byte>.FromUnchecked(e1);

			result.Should().Be(byte.MaxValue);
		}

		{
			object e1 = SomeEnum.Max;

			var result = ConvertTo<byte>.FromObjectUnchecked<SomeEnum>(e1);

			result.Should().Be(byte.MaxValue);
		}

		{
			SomeEnum? e1 = SomeEnum.Max;

			var result = ConvertTo<byte>.FromUnchecked(e1);

			result.Should().Be(byte.MaxValue);
		}

		{
			SomeEnum? e1 = null;

			var pfn1 = () => ConvertTo<int>.From(e1);
			var pfn2 = () => ConvertTo<int>.FromUnchecked(e1);
			var pfn3 = () => ConvertTo<int>.FromObject<SomeEnum?>(e1);
			var pfn4 = () => ConvertTo<int>.FromObjectUnchecked<SomeEnum?>(e1);

			pfn1.Should().Throw<ArgumentNullException>();
			pfn2.Should().Throw<ArgumentNullException>();
			pfn3.Should().Throw<ArgumentNullException>();
			pfn4.Should().Throw<ArgumentNullException>();
		}

		{
			string s = "bla-bla";

			var pfn = () => ConvertTo<StringBuilder>.From(s);

			pfn.Should().Throw<ArgumentException>();
		}
	}

	[Flags]
	enum SomeEnum : ulong
	{
		None = 0,
		First = 1,
		Second = 2,
		Max = ulong.MaxValue
	}
}