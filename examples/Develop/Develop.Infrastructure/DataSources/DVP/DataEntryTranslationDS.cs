using Develop.DTOs;
using Develop.DTOs.DVP;
using Develop.Entities.DVP;
using Develop.Services;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;
using QBCore.ObjectFactory;

namespace Develop.DataSources;

[DsApiController]
[DataSource("dataentry-translation", typeof(PgSqlDataLayer), DataSourceOptions.SoftDelete)]
public sealed class DataEntryTranslationDS : DataSource<DataEntryTranslationID, DataEntryTranslation, DataEntryTranslationCreateDto, DataEntryTranslationSelectDto, DataEntryTranslationUpdateDto, EmptyDto, EmptyDto, DataEntryTranslationDS>, IDataEntryTranslationService, ITransient<IDataEntryTranslationService>
{
	public DataEntryTranslationDS(IServiceProvider sp) : base(sp) { }

	static void Builder(IDSBuilder builder)
	{
		builder.ServiceInterface = typeof(IDataEntryTranslationService);
	}
	static void Builder(ISqlInsertQBBuilder<DataEntryTranslation, DataEntryTranslationCreateDto> builder)
	{
		builder.AutoBuild("dvp.Translations");
	}
	static void Builder(ISqlSelectQBBuilder<DataEntryTranslation, DataEntryTranslationSelectDto> builder)
	{
		builder.Select("dvp.Translations")
			.LeftJoin<Language>("dvp.Languages")
				.Connect<Language, DataEntryTranslation>(lang => lang.LanguageId, trans => trans.LanguageId, FO.Equal)
				.Include<Language>(sel => sel.LanguageName, lang => lang.Name);
	}
	static void Builder(ISqlUpdateQBBuilder<DataEntryTranslation, DataEntryTranslationUpdateDto> builder)
	{
		builder.AutoBuild("dvp.Translations");
	}
	static void Builder(ISqlSoftDelQBBuilder<DataEntryTranslation, EmptyDto> builder)
	{
		builder.AutoBuild("dvp.Translations");
	}
	static void Builder(ISqlRestoreQBBuilder<DataEntryTranslation, EmptyDto> builder)
	{
		builder.AutoBuild("dvp.Translations");
	}
}