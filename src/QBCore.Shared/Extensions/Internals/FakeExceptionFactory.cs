namespace QBCore.Extensions.Internals
{
	public abstract class FakeExceptionFactory<ExceptionArea>
	{
		public static ExceptionArea Make => default(ExceptionArea)!;
	}

	namespace EX
	{
		public sealed class Shared : FakeExceptionFactory<Shared> { private Shared() { } }
		public sealed class Configuration : FakeExceptionFactory<Configuration> { private Configuration() { } }
		public sealed class ObjectFactory : FakeExceptionFactory<ObjectFactory> { private ObjectFactory() { } }
		public sealed class DataSource : FakeExceptionFactory<DataSource> { private DataSource() { } }
		public sealed class QueryBuilder : FakeExceptionFactory<QueryBuilder> { private QueryBuilder() { } }
	}
}