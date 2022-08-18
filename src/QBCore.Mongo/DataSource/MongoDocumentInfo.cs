using System.Reflection;
using MongoDB.Bson.Serialization;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

internal sealed class MongoDocumentInfo : DSDocumentInfo
{
	public readonly BsonClassMap ClassMap;

	public MongoDocumentInfo(Type documentType) : base(documentType)
		=> ClassMap = BsonClassMap.LookupClassMap(DocumentType);

	protected override DEInfo CreateDataEntryInfo(MemberInfo memberInfo, DataEntryFlags flags, ref object? methodSharedContext)
	{
		if (methodSharedContext is not BsonClassMap classMap)
		{
			methodSharedContext = classMap = BsonClassMap.LookupClassMap(DocumentType);
		}

		return new MongoDataEntry(this, memberInfo, flags, classMap);
	}

	MethodInfo _registerClassMapMethodInfo = typeof(BsonClassMap)
		.GetMethods(BindingFlags.Static | BindingFlags.Public)
		.SingleOrDefault(x => x.Name == nameof(BsonClassMap<DEInfo>.RegisterClassMap)
			&& x.IsGenericMethodDefinition && x.GetParameters().Length == 1
			&& x.GetParameters()[0].ParameterType == typeof(Action<>).MakeGenericType(typeof(BsonClassMap<>).MakeGenericType(x.GetGenericArguments()[0])))
		?? throw new ApplicationException("Could not get MethodInfo for the BsonClassMap<T>.RegisterClassMap(Action<T>) method.");

	protected override void PreBuild()
	{
		base.PreBuild();

		var concreteClassMapType = typeof(BsonClassMap<>).MakeGenericType(DocumentType);
		var actionType = typeof(Action<>).MakeGenericType(concreteClassMapType);
		var builder = FactoryHelper.FindBuilder(concreteClassMapType, DocumentType, null);
		if (builder?.GetType() == actionType)
		{
			var concreteRegisterClassMapMethodInfo = _registerClassMapMethodInfo.MakeGenericMethod(DocumentType);
			concreteRegisterClassMapMethodInfo.Invoke(null, new object?[] { builder });
		}
	}
}