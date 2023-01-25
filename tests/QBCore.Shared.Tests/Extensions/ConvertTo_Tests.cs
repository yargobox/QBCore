using System.Text;
using FluentAssertions;

namespace QBCore.Extensions.Internals.Tests;

public class ConvertTo_Tests
{
	[Flags]
	enum SomeEnum : ulong
	{
		None = 0,
		First = 1,
		Second = 2,
		Max = ulong.MaxValue
	}

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



	class Dto1
	{
		public int PublicIntProp { get; set; } = 1000;
		public int? PublicNullableIntProp { get; set; } = 1010;
		private int PrivateIntProp { get; set; } = 1020;
		protected int ProtectedIntProp { get; set; } = 1030;
		internal int InternalIntProp { get; set; } = 1040;
		public string? PublicStringProp { get; set; } = "1050";
		public int PublicReadOnlyIntProp => 1060;
		public int PublicWriteOnlyIntProp { set => _publicWriteOnlyIntValue = value; }
		private int _publicWriteOnlyIntValue = 1070;
		public static int PublicStaticIntProp { get; set; } = 1080;
		public int PublicInitOnlyIntProp { get; init; } = 1090;
		public int PublicInitOnlyWriteOnlyIntProp { init => _publicInitOnlyWriteOnlyIntValue = value; }
		private int _publicInitOnlyWriteOnlyIntValue = 1090;

		public int PublicIntField = 1100;
		public int? PublicNullableIntField = 1110;
		private int PrivateIntField = 1120;
		protected int ProtecetdIntField = 1130;
		internal int InternalIntField = 1140;
		public string? PublicStringField = "1150";
		public readonly int PublicReadonlyIntField = 1160;
		public static int PublicStaticIntField = 1170;
		public const int PublicContsIntField = 1180;

		public int GetPrivateIntProp() => PrivateIntProp;
		public int GetProtectedIntProp() => ProtectedIntProp;
		public int GetPublicWriteOnlyIntProp() => _publicWriteOnlyIntValue;
		public int GetPublicInitOnlyWriteOnlyIntProp() => _publicInitOnlyWriteOnlyIntValue;
		public int GetPrivateIntField() => PrivateIntField;
		public int GetProtecetdIntField() => ProtecetdIntField;
	}

	class Dto1ToReceiveThemAll
	{
		public int? PublicIntProp { get; set; } = 5000;
		public int PublicNullableIntProp { get; set; } = 5010;
		public int PrivateIntProp { get; set; } = 5020;
		public int ProtectedIntProp { get; set; } = 5030;
		public int InternalIntProp { get; set; } = 5040;
		public string? PublicStringProp { get; set; } = "5050";
		public int PublicReadOnlyIntProp { get; set; } = 5060;
		public int PublicWriteOnlyIntProp { get; set; } = 5070;
		public int PublicStaticIntProp { get; set; } = 5080;
		public int PublicInitOnlyIntProp { get; set; } = 5090;
		public int PublicInitOnlyWriteOnlyIntProp { get; set; } = 5100;
		public int PublicIntField { get; set; } = 5110;
		public int? PublicNullableIntField { get; set; } = 5120;
		public int PrivateIntField { get; set; } = 5130;
		public int ProtecetdIntField { get; set; } = 5140;
		public int InternalIntField { get; set; } = 5150;
		public string? PublicStringField = "5160";
		public int PublicReadonlyIntField = 5170;
		public int PublicStaticIntField { get; set; } = 5180;
		public int PublicContsIntField { get; set; } = 5190;
	}

	class Dto2
	{
		public int PublicIntProp { get; set; } = 5000;

		public Dto2() { }

		public Dto2(Dto1 other)
		{
			PublicIntProp = 999;
		}
	}

	class Dto3
	{
		public int PublicIntProp { get; } = 5000;
		public int? PublicNullableIntProp { get; } = 5010;
		public int SomeDefaultValue { get; } = 5020;

		public Dto3() { }

		public Dto3(int PublicIntProp, bool PublicNullableIntProp, int someDefaultValue = -2)
		{
			this.PublicIntProp = PublicIntProp;
			this.SomeDefaultValue = someDefaultValue;
		}

		public Dto3(int PublicIntProp, int publicNullableIntProp, int someDefaultValue = -1)
		{
			this.PublicIntProp = PublicIntProp;
			this.PublicNullableIntProp = publicNullableIntProp;
			this.SomeDefaultValue = someDefaultValue;
		}
	}

	readonly record struct Dto4
	{
		public readonly int PublicInitOnlyIntProp { get; init; } = 6000;
		public readonly int PublicReadOnlyIntProp { get; } = 6010;
		public readonly int PublicReadonlyIntField = 6020;

		public Dto4() { }
		public Dto4(int publicReadOnlyIntProp, int publicReadonlyIntField)
		{
			PublicReadOnlyIntProp = publicReadOnlyIntProp;
			PublicReadonlyIntField = publicReadonlyIntField;
		}
	}

	[Fact]
	public void MapFrom_ForClassicPOCOClasses_ExpectedBehavior()
	{
		var dto1 = new Dto1
		{
			PublicIntProp = 10,
			PublicNullableIntProp = 20,
			InternalIntProp = 30,
			PublicStringProp = "40",
			PublicWriteOnlyIntProp = 50,
			PublicInitOnlyIntProp = 70,
			PublicInitOnlyWriteOnlyIntProp = 80,
			PublicIntField = 90,
			PublicNullableIntField = 100,
			InternalIntField = 110,
			PublicStringField = "120"
		};

		{
			var p = ConvertTo<Dto1>.MapFrom(dto1);

			p.Should().NotBeSameAs(dto1);
			p.PublicIntProp.Should().Be(10);
			p.PublicNullableIntProp.Should().Be(20);
			p.GetPrivateIntProp().Should().Be(1020);
			p.GetProtectedIntProp().Should().Be(1030);
			p.InternalIntProp.Should().Be(1040);
			p.PublicStringProp.Should().BeEquivalentTo("40");
			p.PublicReadOnlyIntProp.Should().Be(1060);
			p.GetPublicWriteOnlyIntProp().Should().Be(1070);
			Dto1.PublicStaticIntProp.Should().Be(1080);
			p.PublicInitOnlyIntProp.Should().Be(70);
			p.GetPublicInitOnlyWriteOnlyIntProp().Should().Be(1090);
			p.PublicIntField.Should().Be(90);
			p.PublicNullableIntField.Should().Be(100);
			p.GetPrivateIntField().Should().Be(1120);
			p.GetProtecetdIntField().Should().Be(1130);
			p.InternalIntField.Should().Be(1140);
			p.PublicStringField.Should().BeEquivalentTo("120");
			p.PublicReadonlyIntField.Should().Be(1160);
			Dto1.PublicStaticIntField.Should().Be(1170);
		}

		{
			var p = ConvertTo<Dto1ToReceiveThemAll>.MapFrom(dto1);

			p.PublicIntProp.Should().Be(10);
			p.PublicNullableIntProp.Should().Be(20);
			p.PrivateIntProp.Should().Be(5020);
			p.ProtectedIntProp.Should().Be(5030);
			p.InternalIntProp.Should().Be(5040);
			p.PublicStringProp.Should().BeEquivalentTo("40");
			p.PublicReadOnlyIntProp.Should().Be(1060);
			p.PublicWriteOnlyIntProp.Should().Be(5070);
			Dto1.PublicStaticIntProp.Should().Be(1080);
			p.PublicInitOnlyIntProp.Should().Be(70);
			p.PublicInitOnlyWriteOnlyIntProp.Should().Be(5100);
			p.PublicIntField.Should().Be(90);
			p.PublicNullableIntField.Should().Be(100);
			p.PrivateIntField.Should().Be(5130);
			p.ProtecetdIntField.Should().Be(5140);
			p.InternalIntField.Should().Be(5150);
			p.PublicStringField.Should().BeEquivalentTo("120");
			p.PublicReadonlyIntField.Should().Be(1160);
			Dto1.PublicStaticIntField.Should().Be(1170);
			p.PublicContsIntField.Should().Be(5190);
		}

		{
			var p = ConvertTo<Dto2>.MapFrom(dto1);

			p.PublicIntProp.Should().Be(999);
		}

		{
			var p = ConvertTo<Dto3>.MapFrom(dto1);

			p.PublicIntProp.Should().Be(10);
			p.PublicNullableIntProp.Should().Be(20);
			p.SomeDefaultValue.Should().Be(-1);
		}

		{
			var p = ConvertTo<Dto4>.MapFrom(dto1);

			p.PublicInitOnlyIntProp.Should().Be(70);
			p.PublicReadOnlyIntProp.Should().Be(1060);
			p.PublicReadonlyIntField.Should().Be(1160);
		}
	}
}