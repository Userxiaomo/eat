using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModelMappingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ToModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HideUpstream = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModelMappings_ChannelId_FromModel",
                table: "ModelMappings",
                columns: new[] { "ChannelId", "FromModel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelMappings");
        }
    }
}
