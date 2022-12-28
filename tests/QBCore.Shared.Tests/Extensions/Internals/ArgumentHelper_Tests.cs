using System.Collections;
using FluentAssertions;

namespace QBCore.Extensions.Internals.Tests;

public class ArgumentHelper_Tests
{
	[Fact]
	public void PrepareAsValueOrCollection_AllCases_ExpectedBehavior()
	{
		Type? compliment;
		object? value;
		bool isCollection;

		{
			value = 99;

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int), typeof(int), out compliment);

			isCollection.Should().BeFalse();
			compliment.Should().BeNull();
			value.Should().BeOfType<int>().And.BeEquivalentTo(99);
		}

		{
			value = 99;

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int?), typeof(int), out compliment);

			isCollection.Should().BeFalse();
			compliment.Should().Be(typeof(int));
			value.Should().BeOfType<int>().And.BeEquivalentTo(99);
		}

		{
			value = new int[] { 1, 2, 3 };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int>>().And.BeEquivalentTo(new int[] { 1, 2, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new int[] { 1, 2, 3 };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int?), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().Be(typeof(int));
			value.Should().BeAssignableTo<ICollection<int>>().And.BeEquivalentTo(new int[] { 1, 2, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new int?[] { 1, 2, 3 };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int?), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int?>>().And.BeEquivalentTo(new int?[] { 1, 2, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new int?[] { 1, 2, 3 };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int>>().And.BeEquivalentTo(new int[] { 1, 2, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new int?[] { 1, null, 3 };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int?), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int?>>().And.BeEquivalentTo(new int?[] { 1, null, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new int?[] { 1, null, 3 };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int>>();
			value.Enumerating(x => x.As<ICollection<int>>()).Should().Throw<ArgumentNullException>();
		}

		{
			value = new byte[] { 1, 2, 3 };

			var pfn = () => ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int), typeof(int), out compliment);

			pfn.Should().Throw<ArgumentException>();
		}

		{
			value = new List<string?>() { "1", "2", null };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(string), typeof(string), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<string>>().And.BeEquivalentTo(new string?[] { "1", "2", null }, c => c.WithStrictOrdering());
		}

		{
			value = YieldInt123();

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int>>().And.BeEquivalentTo(new int[] { 1, 2, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = YieldInt123();

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int?), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().Be(typeof(int));
			value.Should().BeAssignableTo<ICollection<int>>().And.BeEquivalentTo(new int[] { 1, 2, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new TestICollection(new int[] { 1, 2, 3 });

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int>>().And.BeEquivalentTo(new int[] { 1, 2, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new TestICollection(new int?[] { 1, null, 3 });

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int?), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int?>>().And.BeEquivalentTo(new int?[] { 1, null, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new TestICollectionT<int?>(new int?[] { 1, 2, 3 });

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int>>().And.BeEquivalentTo(new int[] { 1, 2, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new object[] { 1, 2, 3 };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(int), typeof(int), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<int>>().And.BeEquivalentTo(new int[] { 1, 2, 3 }, c => c.WithStrictOrdering());
		}

		{
			value = new object[] { 1, 2, 3 };

			var pfn = () => ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(long), typeof(long), out compliment);

			pfn.Should().Throw<ArgumentException>();
		}

		{
			value = new SomeEnum[] { SomeEnum.First, SomeEnum.Second };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum), typeof(SomeEnum), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<SomeEnum>>().And.BeEquivalentTo(new SomeEnum[] { SomeEnum.First, SomeEnum.Second }, c => c.WithStrictOrdering());
		}

		{
			value = new SomeEnum[] { SomeEnum.First, SomeEnum.Second };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum?), typeof(SomeEnum), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().Be(typeof(SomeEnum));
			value.Should().BeAssignableTo<ICollection<SomeEnum>>().And.BeEquivalentTo(new SomeEnum[] { SomeEnum.First, SomeEnum.Second }, c => c.WithStrictOrdering());
		}

		{
			value = new SomeEnum?[] { SomeEnum.First, null, SomeEnum.Second };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum?), typeof(SomeEnum), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<SomeEnum?>>().And.BeEquivalentTo(new SomeEnum?[] { SomeEnum.First, null, SomeEnum.Second }, c => c.WithStrictOrdering());
		}

		{
			value = new SomeEnum?[] { SomeEnum.First, SomeEnum.Second };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum), typeof(SomeEnum), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<SomeEnum>>().And.BeEquivalentTo(new SomeEnum[] { SomeEnum.First, SomeEnum.Second }, c => c.WithStrictOrdering());
		}

		{
			value = new SomeEnum?[] { SomeEnum.First, null, SomeEnum.Second };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum), typeof(SomeEnum), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<SomeEnum>>();
			value.Enumerating(x => x.As<ICollection<SomeEnum>>()).Should().Throw<ArgumentNullException>();
		}

		{
			value = new object[] { SomeEnum.First, SomeEnum.Second };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum), typeof(SomeEnum), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<SomeEnum>>().And.BeEquivalentTo(new SomeEnum[] { SomeEnum.First, SomeEnum.Second }, c => c.WithStrictOrdering());
		}

		{
			value = new object?[] { SomeEnum.First, null, SomeEnum.Second };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum?), typeof(SomeEnum), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<SomeEnum?>>().And.BeEquivalentTo(new SomeEnum?[] { SomeEnum.First, null, SomeEnum.Second }, c => c.WithStrictOrdering());
		}

		{
			value = new object?[] { SomeEnum.First, null, SomeEnum.Second };

			isCollection = ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum), typeof(SomeEnum), out compliment);

			isCollection.Should().BeTrue();
			compliment.Should().BeNull();
			value.Should().BeAssignableTo<ICollection<SomeEnum>>();
			value.Enumerating(x => x.As<ICollection<SomeEnum>>()).Should().Throw<ArgumentNullException>();
		}
	}

	[Fact]
	public void ConvertMethods_AllCases_ExpectedBehavior()
	{
		Type? compliment;
		object? value;

		{
			value = SomeEnum.Second;

			var result = ArgumentHelper.ConvertToValue<ulong>(value);

			result.Should().Be(2);
		}

		{
			value = ulong.MaxValue;

			var result = ArgumentHelper.ConvertUncheckedToValue<int>(value);

			result.Should().Be(-1);
		}

		{
			value = DateTime.Now.Ticks;

			var pfn = () => ArgumentHelper.ConvertUncheckedToValue<DateTime>(value);

			pfn.Should().Throw<ArgumentException>();
		}

		{
			value = new object[] { SomeEnum.First, SomeEnum.Second };

			ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum), typeof(SomeEnum), out compliment);
			var col = ArgumentHelper.ConvertUncheckedToCollection<ulong>(value, compliment ?? typeof(SomeEnum));

			col.Should().BeEquivalentTo(new ulong[] { 1, 2 }, c => c.WithStrictOrdering());
		}

		{
			value = new object[] { SomeEnum.First, SomeEnum.Second };

			ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum?), typeof(SomeEnum), out compliment);
			var col = ArgumentHelper.ConvertUncheckedToCollection<short>(value, compliment ?? typeof(SomeEnum?));

			col.Should().BeEquivalentTo(new short[] { 1, 2 }, c => c.WithStrictOrdering());
		}

		{
			value = new object?[] { SomeEnum.First, null, SomeEnum.Second };

			ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum?), typeof(SomeEnum), out compliment);
			var col = ArgumentHelper.ConvertToCollection<ulong?>(value, compliment ?? typeof(SomeEnum?));

			col.Should().BeEquivalentTo(new ulong?[] { 1, null, 2 }, c => c.WithStrictOrdering());
		}

		{
			value = new object?[] { SomeEnum.First, SomeEnum.Second };

			ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum?), typeof(SomeEnum), out compliment);
			var col = ArgumentHelper.ConvertToCollection<ulong>(value, compliment ?? typeof(SomeEnum?));

			col.Should().BeEquivalentTo(new ulong[] { 1, 2 }, c => c.WithStrictOrdering());
		}

		{
			value = new object?[] { SomeEnum.First, null, SomeEnum.Second };

			ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(SomeEnum?), typeof(SomeEnum), out compliment);
			var col1 = ArgumentHelper.ConvertToCollection<ulong>(value, compliment ?? typeof(SomeEnum?));
			var col2 = ArgumentHelper.ConvertUncheckedToCollection<ulong>(value, compliment ?? typeof(SomeEnum?));

			col1.Enumerating(x => x).Should().Throw<ArgumentNullException>();
			col2.Enumerating(x => x).Should().Throw<ArgumentNullException>();
		}

		{
			value = new ulong[] { 10, ulong.MaxValue };

			ArgumentHelper.PrepareAsValueOrCollection(ref value, typeof(ulong?), typeof(ulong), out compliment);
			var col1 = ArgumentHelper.ConvertUncheckedToCollection<int>(value, compliment ?? typeof(ulong?));
			var col2 = ArgumentHelper.ConvertToCollection<int>(value, compliment ?? typeof(ulong?));

			col1.Should().BeEquivalentTo(new int[] { 10, -1 }, c => c.WithStrictOrdering());
			col2.Enumerating(x => x).Should().Throw<OverflowException>();
		}
	}

	[Flags]
	private enum SomeEnum : byte
	{
		None = 0,
		First = 1,
		Second = 2
	}
	private static IEnumerable<int> YieldInt123()
	{
		yield return 1; yield return 2; yield return 3;
	}
	private static IEnumerable YieldObject123()
	{
		yield return 1; yield return 2; yield return 3;
	}
	private class TestICollection : ICollection
	{
		readonly object?[] _col;
		public TestICollection(IEnumerable e) => _col = e.Cast<object?>().ToArray();
		public int Count => _col.Length;
		public bool IsReadOnly => true;
		public bool IsSynchronized => false;
		public object SyncRoot => new object();
		public void Add(object? item) => throw new NotSupportedException();
		public void Clear() => throw new NotSupportedException();
		public bool Contains(object? item) => _col.Contains(item);
		public void CopyTo(Array array, int arrayIndex) => _col.CopyTo(array, arrayIndex);
		public IEnumerator GetEnumerator()
		{
			foreach (var p in  _col) yield return p;
		}
		public bool Remove(object? item) => throw new NotSupportedException();
		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var p in  _col) yield return p;
		}
	}
	private class TestICollectionT<T> : ICollection<T>
	{
		readonly T[] _col;
		public TestICollectionT(IEnumerable e) => _col = (e as IEnumerable<T> ?? e.Cast<T>()).ToArray();
		public int Count => _col.Length;
		public bool IsReadOnly => true;
		public void Add(T item) => throw new NotSupportedException();
		public void Clear() => throw new NotSupportedException();
		public bool Contains(T item) => _col.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => _col.CopyTo(array, arrayIndex);
		public IEnumerator<T> GetEnumerator()
		{
			foreach (var p in  _col) yield return p;
		}
		public bool Remove(T item) => throw new NotSupportedException();
		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var p in  _col) yield return p;
		}
	}
}