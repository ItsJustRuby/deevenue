using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deevenue.Infrastructure.Db.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DataProtectionKeys",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                FriendlyName = table.Column<string>(type: "text", nullable: true),
                Xml = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "JobResults",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                JobId = table.Column<Guid>(type: "uuid", nullable: false),
                JobTypeName = table.Column<string>(type: "text", nullable: false),
                ErrorText = table.Column<string>(type: "text", nullable: false),
                InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobResults", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Media",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Rating = table.Column<char>(type: "character(1)", nullable: false),
                ContentType = table.Column<string>(type: "text", nullable: false),
                Width = table.Column<int>(type: "integer", nullable: false),
                Height = table.Column<int>(type: "integer", nullable: false),
                FileSize = table.Column<decimal>(type: "numeric", nullable: false),
                InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Media", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Tags",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Rating = table.Column<char>(type: "character(1)", nullable: false),
                Aliases = table.Column<string[]>(type: "text[]", nullable: false, defaultValue: new string[0])
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tags", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "MediumHashes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MediumId = table.Column<Guid>(type: "uuid", nullable: false),
                Hash = table.Column<string>(type: "character(32)", fixedLength: true, maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MediumHashes", x => x.Id);
                table.ForeignKey(
                    name: "FK_MediumHashes_Media_MediumId",
                    column: x => x.MediumId,
                    principalTable: "Media",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ThumbnailSheets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MediumId = table.Column<Guid>(type: "uuid", nullable: false),
                ThumbnailCount = table.Column<int>(type: "integer", nullable: false),
                InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ThumbnailSheets", x => x.Id);
                table.ForeignKey(
                    name: "FK_ThumbnailSheets_Media_MediumId",
                    column: x => x.MediumId,
                    principalTable: "Media",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MediumTagAbsences",
            columns: table => new
            {
                MediumId = table.Column<Guid>(type: "uuid", nullable: false),
                TagId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MediumTagAbsences", x => new { x.MediumId, x.TagId });
                table.ForeignKey(
                    name: "FK_MediumTagAbsences_Media_MediumId",
                    column: x => x.MediumId,
                    principalTable: "Media",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MediumTagAbsences_Tags_TagId",
                    column: x => x.TagId,
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MediumTags",
            columns: table => new
            {
                MediumId = table.Column<Guid>(type: "uuid", nullable: false),
                TagId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MediumTags", x => new { x.MediumId, x.TagId });
                table.ForeignKey(
                    name: "FK_MediumTags_Media_MediumId",
                    column: x => x.MediumId,
                    principalTable: "Media",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MediumTags_Tags_TagId",
                    column: x => x.TagId,
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TagImplications",
            columns: table => new
            {
                ImplyingTagId = table.Column<Guid>(type: "uuid", nullable: false),
                ImpliedTagId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TagImplications", x => new { x.ImpliedTagId, x.ImplyingTagId });
                table.ForeignKey(
                    name: "FK_TagImplications_Tags_ImpliedTagId",
                    column: x => x.ImpliedTagId,
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TagImplications_Tags_ImplyingTagId",
                    column: x => x.ImplyingTagId,
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_MediumHashes_Hash",
            table: "MediumHashes",
            column: "Hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MediumHashes_MediumId",
            table: "MediumHashes",
            column: "MediumId");

        migrationBuilder.CreateIndex(
            name: "IX_MediumTagAbsences_TagId",
            table: "MediumTagAbsences",
            column: "TagId");

        migrationBuilder.CreateIndex(
            name: "IX_MediumTags_TagId",
            table: "MediumTags",
            column: "TagId");

        migrationBuilder.CreateIndex(
            name: "IX_TagImplications_ImplyingTagId",
            table: "TagImplications",
            column: "ImplyingTagId");

        migrationBuilder.CreateIndex(
            name: "IX_Tags_Name",
            table: "Tags",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ThumbnailSheets_MediumId",
            table: "ThumbnailSheets",
            column: "MediumId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DataProtectionKeys");

        migrationBuilder.DropTable(
            name: "JobResults");

        migrationBuilder.DropTable(
            name: "MediumHashes");

        migrationBuilder.DropTable(
            name: "MediumTagAbsences");

        migrationBuilder.DropTable(
            name: "MediumTags");

        migrationBuilder.DropTable(
            name: "TagImplications");

        migrationBuilder.DropTable(
            name: "ThumbnailSheets");

        migrationBuilder.DropTable(
            name: "Tags");

        migrationBuilder.DropTable(
            name: "Media");
    }
}
