using System.Linq.Expressions;
using QBCore.Extensions.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public class MongoDEFactory : IDEBuilder
{
	private static readonly IDEBuilder _defaultInstance = new MongoDEFactory();
	
	public static IDEBuilder Default => _defaultInstance;

	protected MongoDEFactory() { }

	public DataEntry Build<TDocument>(string fieldName)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException(nameof(fieldName));
		}

		var doc = GetDSDocument(typeof(TDocument));
		return doc.DataEntries.GetValueOrDefault(fieldName)
			?? throw new KeyNotFoundException($"Document '{typeof(TDocument).ToPretty()}' does not have data entry '{fieldName}'.");
	}

	public DataEntry Build<TDocument, TField>(string fieldName)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException(nameof(fieldName));
		}

		var doc = GetDSDocument(typeof(TDocument));
		var dataEntry = doc.DataEntries.GetValueOrDefault(fieldName)
			?? throw new KeyNotFoundException($"Document '{typeof(TDocument).ToPretty()}' does not have DataEntry '{fieldName}'.");

		if (dataEntry.DataEntryType != typeof(TField))
		{
			throw new ArgumentException($"DataEntry '{typeof(TDocument).ToPretty()}.{fieldName}' must be of type {typeof(TField).ToPretty()}.", nameof(fieldName));
		}

		return dataEntry;
	}

	public DataEntry Build<TDocument>(LambdaExpression memberSelector)
	{
		if (memberSelector == null)
		{
			throw new ArgumentNullException(nameof(memberSelector));
		}

		var fieldName = memberSelector.GetMemberName();
		var doc = GetDSDocument(typeof(TDocument));
		return doc.DataEntries.GetValueOrDefault(fieldName)
			?? throw new KeyNotFoundException($"Document '{typeof(TDocument).ToPretty()}' does not have data entry '{fieldName}'.");
	}

	public DataEntry Build<TDocument, TField>(Expression<Func<TDocument, TField>> memberSelector)
	{
		if (memberSelector == null)
		{
			throw new ArgumentNullException(nameof(memberSelector));
		}

		var fieldName = memberSelector.GetMemberName();
		var doc = GetDSDocument(typeof(TDocument));
		var dataEntry = doc.DataEntries.GetValueOrDefault(fieldName)
			?? throw new KeyNotFoundException($"Document '{typeof(TDocument).ToPretty()}' does not have DataEntry '{fieldName}'.");

		if (dataEntry.DataEntryType != typeof(TField))
		{
			throw new ArgumentException($"DataEntry '{typeof(TDocument).ToPretty()}.{fieldName}' must be of type {typeof(TField).ToPretty()}.", nameof(fieldName));
		}

		return dataEntry;
	}

	private static IDSDocumentInfo GetDSDocument(Type documentType)
	{
		var doc = StaticFactory.Documents.GetValueOrDefault(documentType);
		if (doc == null)
		{
			doc = new DSDocument(documentType);
			var registry = (IFactoryObjectRegistry<Type, IDSDocumentInfo>)StaticFactory.Documents;
			registry.RegisterObject(documentType, doc);
		}
		return doc;
	}
}