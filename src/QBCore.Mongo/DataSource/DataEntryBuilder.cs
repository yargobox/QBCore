using System.Linq.Expressions;
using QBCore.Extensions.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

internal sealed class DataEntryBuilder : IDataEntryBuilder
{
	public IDataEntry Build<TDocument>(string fieldName)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException(nameof(fieldName));
		}

		var doc = GetDSDocument(typeof(TDocument));
		return doc.DataEntries.GetValueOrDefault(fieldName)
			?? throw new KeyNotFoundException($"Document '{typeof(TDocument).ToPretty()}' does not have data entry '{fieldName}'.");
	}

	public IDataEntry Build<TDocument, TField>(string fieldName)
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

	public IDataEntry Build<TDocument>(LambdaExpression memberSelector)
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

	public IDataEntry Build<TDocument, TField>(Expression<Func<TDocument, TField>> memberSelector)
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

	private static IDSDocument GetDSDocument(Type documentType)
	{
		var doc = StaticFactory.DocumentsPool.GetValueOrDefault(documentType);
		if (doc == null)
		{
			doc = new DSDocument(documentType);
			var registry = (IFactoryObjectRegistry<Type, IDSDocument>)StaticFactory.DocumentsPool;
			registry.RegisterObject(documentType, doc);
		}
		return doc;
	}
}