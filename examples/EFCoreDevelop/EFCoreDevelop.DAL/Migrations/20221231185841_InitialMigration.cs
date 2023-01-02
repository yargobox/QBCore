using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EFCoreDevelop.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dvp");

            migrationBuilder.EnsureSchema(
                name: "com");

            migrationBuilder.CreateTable(
                name: "Languages",
                schema: "dvp",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.LanguageId);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "dvp",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "com",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Login = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false, collation: "uk-UA-x-icu"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Translations",
                schema: "dvp",
                columns: table => new
                {
                    RefId = table.Column<int>(type: "integer", nullable: false),
                    RefKey = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false, collation: "uk-UA-x-icu"),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => new { x.RefId, x.LanguageId, x.RefKey });
                    table.ForeignKey(
                        name: "FK_Translations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalSchema: "dvp",
                        principalTable: "Languages",
                        principalColumn: "LanguageId");
                });

            migrationBuilder.CreateTable(
                name: "Apps",
                schema: "dvp",
                columns: table => new
                {
                    AppId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apps", x => x.AppId);
                    table.ForeignKey(
                        name: "FK_Apps_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "dvp",
                        principalTable: "Projects",
                        principalColumn: "ProjectId");
                });

            migrationBuilder.CreateTable(
                name: "FuncGroups",
                schema: "dvp",
                columns: table => new
                {
                    FuncGroupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuncGroups", x => x.FuncGroupId);
                    table.ForeignKey(
                        name: "FK_FuncGroups_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "dvp",
                        principalTable: "Projects",
                        principalColumn: "ProjectId");
                });

            migrationBuilder.CreateTable(
                name: "AppObjects",
                schema: "dvp",
                columns: table => new
                {
                    AppObjectId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FuncGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppObjects", x => x.AppObjectId);
                    table.ForeignKey(
                        name: "FK_AppObjects_FuncGroups_FuncGroupId",
                        column: x => x.FuncGroupId,
                        principalSchema: "dvp",
                        principalTable: "FuncGroups",
                        principalColumn: "FuncGroupId");
                });

            migrationBuilder.CreateTable(
                name: "FuncGroupsByApps",
                schema: "dvp",
                columns: table => new
                {
                    AppsAppId = table.Column<int>(type: "integer", nullable: false),
                    FuncGroupsFuncGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuncGroupsByApps", x => new { x.AppsAppId, x.FuncGroupsFuncGroupId });
                    table.ForeignKey(
                        name: "FK_FuncGroupsByApps_Apps_AppsAppId",
                        column: x => x.AppsAppId,
                        principalSchema: "dvp",
                        principalTable: "Apps",
                        principalColumn: "AppId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FuncGroupsByApps_FuncGroups_FuncGroupsFuncGroupId",
                        column: x => x.FuncGroupsFuncGroupId,
                        principalSchema: "dvp",
                        principalTable: "FuncGroups",
                        principalColumn: "FuncGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenericObjects",
                schema: "dvp",
                columns: table => new
                {
                    GenericObjectId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FuncGroupId = table.Column<int>(type: "integer", nullable: false),
                    AppObjectId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenericObjects", x => x.GenericObjectId);
                    table.ForeignKey(
                        name: "FK_GenericObjects_AppObjects_AppObjectId",
                        column: x => x.AppObjectId,
                        principalSchema: "dvp",
                        principalTable: "AppObjects",
                        principalColumn: "AppObjectId");
                    table.ForeignKey(
                        name: "FK_GenericObjects_FuncGroups_FuncGroupId",
                        column: x => x.FuncGroupId,
                        principalSchema: "dvp",
                        principalTable: "FuncGroups",
                        principalColumn: "FuncGroupId");
                });

            migrationBuilder.CreateTable(
                name: "AOListeners",
                schema: "dvp",
                columns: table => new
                {
                    AOListenerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenericObjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AOListeners", x => x.AOListenerId);
                    table.ForeignKey(
                        name: "FK_AOListeners_GenericObjects_GenericObjectId",
                        column: x => x.GenericObjectId,
                        principalSchema: "dvp",
                        principalTable: "GenericObjects",
                        principalColumn: "GenericObjectId");
                });

            migrationBuilder.CreateTable(
                name: "CDSNodes",
                schema: "dvp",
                columns: table => new
                {
                    CDSNodeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenericObjectId = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CDSNodes", x => x.CDSNodeId);
                    table.ForeignKey(
                        name: "FK_CDSNodes_CDSNodes_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "dvp",
                        principalTable: "CDSNodes",
                        principalColumn: "CDSNodeId");
                    table.ForeignKey(
                        name: "FK_CDSNodes_GenericObjects_GenericObjectId",
                        column: x => x.GenericObjectId,
                        principalSchema: "dvp",
                        principalTable: "GenericObjects",
                        principalColumn: "GenericObjectId");
                });

            migrationBuilder.CreateTable(
                name: "DataEntries",
                schema: "dvp",
                columns: table => new
                {
                    DataEntryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenericObjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataEntries", x => x.DataEntryId);
                    table.ForeignKey(
                        name: "FK_DataEntries_GenericObjects_GenericObjectId",
                        column: x => x.GenericObjectId,
                        principalSchema: "dvp",
                        principalTable: "GenericObjects",
                        principalColumn: "GenericObjectId");
                });

            migrationBuilder.CreateTable(
                name: "QueryBuilders",
                schema: "dvp",
                columns: table => new
                {
                    QueryBuilderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenericObjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryBuilders", x => x.QueryBuilderId);
                    table.ForeignKey(
                        name: "FK_QueryBuilders_GenericObjects_GenericObjectId",
                        column: x => x.GenericObjectId,
                        principalSchema: "dvp",
                        principalTable: "GenericObjects",
                        principalColumn: "GenericObjectId");
                });

            migrationBuilder.CreateTable(
                name: "CDSConditions",
                schema: "dvp",
                columns: table => new
                {
                    CDSConditionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CDSNodeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CDSConditions", x => x.CDSConditionId);
                    table.ForeignKey(
                        name: "FK_CDSConditions_CDSNodes_CDSNodeId",
                        column: x => x.CDSNodeId,
                        principalSchema: "dvp",
                        principalTable: "CDSNodes",
                        principalColumn: "CDSNodeId");
                });

            migrationBuilder.CreateTable(
                name: "DataEntriesByTranslations",
                schema: "dvp",
                columns: table => new
                {
                    RefId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    RefKey = table.Column<string>(type: "text", nullable: false, collation: "uk-UA-x-icu"),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataEntriesByTranslations", x => new { x.RefId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_DataEntriesByTranslations_DataEntries_RefId",
                        column: x => x.RefId,
                        principalSchema: "dvp",
                        principalTable: "DataEntries",
                        principalColumn: "DataEntryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataEntriesByTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalSchema: "dvp",
                        principalTable: "Languages",
                        principalColumn: "LanguageId");
                });

            migrationBuilder.CreateTable(
                name: "QBAggregations",
                schema: "dvp",
                columns: table => new
                {
                    QBAggregationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QueryBuilderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QBAggregations", x => x.QBAggregationId);
                    table.ForeignKey(
                        name: "FK_QBAggregations_QueryBuilders_QueryBuilderId",
                        column: x => x.QueryBuilderId,
                        principalSchema: "dvp",
                        principalTable: "QueryBuilders",
                        principalColumn: "QueryBuilderId");
                });

            migrationBuilder.CreateTable(
                name: "QBColumns",
                schema: "dvp",
                columns: table => new
                {
                    QBColumnId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QueryBuilderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QBColumns", x => x.QBColumnId);
                    table.ForeignKey(
                        name: "FK_QBColumns_QueryBuilders_QueryBuilderId",
                        column: x => x.QueryBuilderId,
                        principalSchema: "dvp",
                        principalTable: "QueryBuilders",
                        principalColumn: "QueryBuilderId");
                });

            migrationBuilder.CreateTable(
                name: "QBConditions",
                schema: "dvp",
                columns: table => new
                {
                    QBConditionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QueryBuilderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QBConditions", x => x.QBConditionId);
                    table.ForeignKey(
                        name: "FK_QBConditions_QueryBuilders_QueryBuilderId",
                        column: x => x.QueryBuilderId,
                        principalSchema: "dvp",
                        principalTable: "QueryBuilders",
                        principalColumn: "QueryBuilderId");
                });

            migrationBuilder.CreateTable(
                name: "QBJoinConditions",
                schema: "dvp",
                columns: table => new
                {
                    QBJoinConditionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QueryBuilderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QBJoinConditions", x => x.QBJoinConditionId);
                    table.ForeignKey(
                        name: "FK_QBJoinConditions_QueryBuilders_QueryBuilderId",
                        column: x => x.QueryBuilderId,
                        principalSchema: "dvp",
                        principalTable: "QueryBuilders",
                        principalColumn: "QueryBuilderId");
                });

            migrationBuilder.CreateTable(
                name: "QBObjects",
                schema: "dvp",
                columns: table => new
                {
                    QBObjectId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QueryBuilderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QBObjects", x => x.QBObjectId);
                    table.ForeignKey(
                        name: "FK_QBObjects_QueryBuilders_QueryBuilderId",
                        column: x => x.QueryBuilderId,
                        principalSchema: "dvp",
                        principalTable: "QueryBuilders",
                        principalColumn: "QueryBuilderId");
                });

            migrationBuilder.CreateTable(
                name: "QBParameters",
                schema: "dvp",
                columns: table => new
                {
                    QBParameterId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QueryBuilderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QBParameters", x => x.QBParameterId);
                    table.ForeignKey(
                        name: "FK_QBParameters_QueryBuilders_QueryBuilderId",
                        column: x => x.QueryBuilderId,
                        principalSchema: "dvp",
                        principalTable: "QueryBuilders",
                        principalColumn: "QueryBuilderId");
                });

            migrationBuilder.CreateTable(
                name: "QBSortOrders",
                schema: "dvp",
                columns: table => new
                {
                    QBSortOrderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, collation: "uk-UA-x-icu"),
                    Desc = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true, collation: "uk-UA-x-icu"),
                    Inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QueryBuilderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QBSortOrders", x => x.QBSortOrderId);
                    table.ForeignKey(
                        name: "FK_QBSortOrders_QueryBuilders_QueryBuilderId",
                        column: x => x.QueryBuilderId,
                        principalSchema: "dvp",
                        principalTable: "QueryBuilders",
                        principalColumn: "QueryBuilderId");
                });

            migrationBuilder.InsertData(
                schema: "dvp",
                table: "Languages",
                columns: new[] { "LanguageId", "Deleted", "Desc", "Name", "Updated" },
                values: new object[,]
                {
                    { 1, null, null, "en", null },
                    { 2, null, null, "uk", null }
                });

            migrationBuilder.InsertData(
                schema: "dvp",
                table: "Projects",
                columns: new[] { "ProjectId", "Deleted", "Desc", "Name", "Updated" },
                values: new object[] { 1, null, "", "General", null });

            migrationBuilder.InsertData(
                schema: "com",
                table: "Users",
                columns: new[] { "UserId", "Deleted", "Desc", "Login", "Name", "Updated" },
                values: new object[] { 1, null, "Default admin account", "Admin", "Admin", null });

            migrationBuilder.InsertData(
                schema: "dvp",
                table: "Apps",
                columns: new[] { "AppId", "Deleted", "Desc", "Name", "ProjectId", "Updated" },
                values: new object[] { 1, null, "Застосунок для обліку та розробки застосунків на основі QBCore", "Develop", 1, null });

            migrationBuilder.InsertData(
                schema: "dvp",
                table: "FuncGroups",
                columns: new[] { "FuncGroupId", "Deleted", "Desc", "Name", "ProjectId", "Updated" },
                values: new object[,]
                {
                    { 1, null, null, "COM", 1, null },
                    { 2, null, null, "DVP", 1, null }
                });

            migrationBuilder.InsertData(
                schema: "dvp",
                table: "Translations",
                columns: new[] { "LanguageId", "RefId", "RefKey", "Deleted", "Desc", "Name", "Updated" },
                values: new object[,]
                {
                    { 1, 1, "DataEntry", null, null, "Id.", null },
                    { 2, 1, "DataEntry", null, null, "Ід.", null },
                    { 1, 2, "DataEntry", null, null, "Project", null },
                    { 2, 2, "DataEntry", null, null, "Проект", null },
                    { 1, 3, "DataEntry", null, null, "Description", null },
                    { 2, 3, "DataEntry", null, null, "Опис", null },
                    { 1, 8, "DataEntry", null, null, "Id.", null },
                    { 2, 8, "DataEntry", null, null, "Ід.", null },
                    { 1, 9, "DataEntry", null, null, "Application", null },
                    { 2, 9, "DataEntry", null, null, "Застосунок", null },
                    { 2, 10, "DataEntry", null, null, "Опис", null }
                });

            migrationBuilder.InsertData(
                schema: "dvp",
                table: "AppObjects",
                columns: new[] { "AppObjectId", "Deleted", "Desc", "FuncGroupId", "Name", "Updated" },
                values: new object[,]
                {
                    { 1, null, null, 2, "Projects", null },
                    { 2, null, null, 2, "Apps", null },
                    { 3, null, null, 2, "FuncGroups", null },
                    { 4, null, null, 2, "AppObjects", null }
                });

            migrationBuilder.InsertData(
                schema: "dvp",
                table: "FuncGroupsByApps",
                columns: new[] { "AppsAppId", "FuncGroupsFuncGroupId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 }
                });

            migrationBuilder.InsertData(
                schema: "dvp",
                table: "GenericObjects",
                columns: new[] { "GenericObjectId", "AppObjectId", "Deleted", "Desc", "FuncGroupId", "Name", "Updated" },
                values: new object[,]
                {
                    { 1, 1, null, null, 2, "Projects", null },
                    { 2, 2, null, null, 2, "Apps", null },
                    { 3, 3, null, null, 2, "FuncGroups", null },
                    { 4, 4, null, null, 2, "AppObjects", null }
                });

            migrationBuilder.InsertData(
                schema: "dvp",
                table: "DataEntries",
                columns: new[] { "DataEntryId", "Deleted", "Desc", "GenericObjectId", "Name", "Updated" },
                values: new object[,]
                {
                    { 1, null, null, 1, "ProjectId", null },
                    { 2, null, null, 1, "Name", null },
                    { 3, null, null, 1, "Desc", null },
                    { 4, null, null, 1, "Inserted", null },
                    { 5, null, null, 1, "Updated", null },
                    { 6, null, null, 1, "Deleted", null },
                    { 7, null, null, 2, "ProjectId", null },
                    { 8, null, null, 2, "AppId", null },
                    { 9, null, null, 2, "Name", null },
                    { 10, null, null, 2, "Desc", null },
                    { 11, null, null, 2, "Inserted", null },
                    { 12, null, null, 2, "Updated", null },
                    { 13, null, null, 2, "Deleted", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AOListeners_Deleted",
                schema: "dvp",
                table: "AOListeners",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AOListeners_GenericObjectId",
                schema: "dvp",
                table: "AOListeners",
                column: "GenericObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AppObjects_Deleted",
                schema: "dvp",
                table: "AppObjects",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppObjects_FuncGroupId",
                schema: "dvp",
                table: "AppObjects",
                column: "FuncGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Apps_Deleted",
                schema: "dvp",
                table: "Apps",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Apps_ProjectId",
                schema: "dvp",
                table: "Apps",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CDSConditions_CDSNodeId",
                schema: "dvp",
                table: "CDSConditions",
                column: "CDSNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CDSConditions_Deleted",
                schema: "dvp",
                table: "CDSConditions",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CDSNodes_Deleted",
                schema: "dvp",
                table: "CDSNodes",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CDSNodes_GenericObjectId",
                schema: "dvp",
                table: "CDSNodes",
                column: "GenericObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CDSNodes_ParentId",
                schema: "dvp",
                table: "CDSNodes",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataEntries_Deleted",
                schema: "dvp",
                table: "DataEntries",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DataEntries_GenericObjectId",
                schema: "dvp",
                table: "DataEntries",
                column: "GenericObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DataEntriesByTranslations_Deleted",
                schema: "dvp",
                table: "DataEntriesByTranslations",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DataEntriesByTranslations_LanguageId",
                schema: "dvp",
                table: "DataEntriesByTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_FuncGroups_Deleted",
                schema: "dvp",
                table: "FuncGroups",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FuncGroups_ProjectId",
                schema: "dvp",
                table: "FuncGroups",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FuncGroupsByApps_FuncGroupsFuncGroupId",
                schema: "dvp",
                table: "FuncGroupsByApps",
                column: "FuncGroupsFuncGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GenericObjects_AppObjectId",
                schema: "dvp",
                table: "GenericObjects",
                column: "AppObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GenericObjects_Deleted",
                schema: "dvp",
                table: "GenericObjects",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GenericObjects_FuncGroupId",
                schema: "dvp",
                table: "GenericObjects",
                column: "FuncGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Deleted",
                schema: "dvp",
                table: "Languages",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Name",
                schema: "dvp",
                table: "Languages",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Deleted",
                schema: "dvp",
                table: "Projects",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                schema: "dvp",
                table: "Projects",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QBAggregations_Deleted",
                schema: "dvp",
                table: "QBAggregations",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QBAggregations_QueryBuilderId",
                schema: "dvp",
                table: "QBAggregations",
                column: "QueryBuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_QBColumns_Deleted",
                schema: "dvp",
                table: "QBColumns",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QBColumns_QueryBuilderId",
                schema: "dvp",
                table: "QBColumns",
                column: "QueryBuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_QBConditions_Deleted",
                schema: "dvp",
                table: "QBConditions",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QBConditions_QueryBuilderId",
                schema: "dvp",
                table: "QBConditions",
                column: "QueryBuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_QBJoinConditions_Deleted",
                schema: "dvp",
                table: "QBJoinConditions",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QBJoinConditions_QueryBuilderId",
                schema: "dvp",
                table: "QBJoinConditions",
                column: "QueryBuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_QBObjects_Deleted",
                schema: "dvp",
                table: "QBObjects",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QBObjects_QueryBuilderId",
                schema: "dvp",
                table: "QBObjects",
                column: "QueryBuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_QBParameters_Deleted",
                schema: "dvp",
                table: "QBParameters",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QBParameters_QueryBuilderId",
                schema: "dvp",
                table: "QBParameters",
                column: "QueryBuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_QBSortOrders_Deleted",
                schema: "dvp",
                table: "QBSortOrders",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QBSortOrders_QueryBuilderId",
                schema: "dvp",
                table: "QBSortOrders",
                column: "QueryBuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryBuilders_Deleted",
                schema: "dvp",
                table: "QueryBuilders",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QueryBuilders_GenericObjectId",
                schema: "dvp",
                table: "QueryBuilders",
                column: "GenericObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_Deleted",
                schema: "dvp",
                table: "Translations",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_LanguageId",
                schema: "dvp",
                table: "Translations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Deleted",
                schema: "com",
                table: "Users",
                column: "Deleted",
                filter: "\"Deleted\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                schema: "com",
                table: "Users",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AOListeners",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "CDSConditions",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "DataEntriesByTranslations",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "FuncGroupsByApps",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "QBAggregations",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "QBColumns",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "QBConditions",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "QBJoinConditions",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "QBObjects",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "QBParameters",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "QBSortOrders",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "Translations",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "com");

            migrationBuilder.DropTable(
                name: "CDSNodes",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "DataEntries",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "Apps",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "QueryBuilders",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "Languages",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "GenericObjects",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "AppObjects",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "FuncGroups",
                schema: "dvp");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "dvp");
        }
    }
}
