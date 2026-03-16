using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExtractKeywords",
                table: "SearchEngineConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxResults",
                table: "SearchEngineConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Messages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorType",
                table: "Messages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "McpToolsJson",
                table: "Messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                table: "Messages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Text");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Messages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProviderId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SearchEnabled",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SearchStatus",
                table: "Messages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "WebSearchResultJson",
                table: "Messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Conversations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarType",
                table: "Conversations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<Guid>(
                name: "BotId",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultProviderId",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HistoryCount",
                table: "Conversations",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "HistoryType",
                table: "Conversations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Count");

            migrationBuilder.AddColumn<int>(
                name: "InputTokens",
                table: "Conversations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsStar",
                table: "Conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWithBot",
                table: "Conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OutputTokens",
                table: "Conversations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SearchEnabled",
                table: "Conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StarAt",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiStyle",
                table: "Channels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "OpenAI");

            migrationBuilder.AddColumn<string>(
                name: "ChannelType",
                table: "Channels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Default");

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Channels",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Channels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "BuiltInImageGen",
                table: "AiModels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BuiltInWebSearch",
                table: "AiModels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Selected",
                table: "AiModels",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SupportTool",
                table: "AiModels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SupportVision",
                table: "AiModels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SearchEngineConfigs_EngineType",
                table: "SearchEngineConfigs",
                column: "EngineType");

            migrationBuilder.CreateIndex(
                name: "IX_SearchEngineConfigs_IsEnabled",
                table: "SearchEngineConfigs",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DeletedAt",
                table: "Messages",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_IsStar",
                table: "Conversations",
                column: "IsStar");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_IsEnabled_SortOrder",
                table: "Channels",
                columns: new[] { "IsEnabled", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Channels_Provider",
                table: "Channels",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_AiModels_ChannelId_ModelId",
                table: "AiModels",
                columns: new[] { "ChannelId", "ModelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SearchEngineConfigs_EngineType",
                table: "SearchEngineConfigs");

            migrationBuilder.DropIndex(
                name: "IX_SearchEngineConfigs_IsEnabled",
                table: "SearchEngineConfigs");

            migrationBuilder.DropIndex(
                name: "IX_Messages_DeletedAt",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_IsStar",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Channels_IsEnabled_SortOrder",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_Channels_Provider",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_AiModels_ChannelId_ModelId",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "ExtractKeywords",
                table: "SearchEngineConfigs");

            migrationBuilder.DropColumn(
                name: "MaxResults",
                table: "SearchEngineConfigs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ErrorType",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "McpToolsJson",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SearchEnabled",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SearchStatus",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "WebSearchResultJson",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AvatarType",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "BotId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "DefaultProviderId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "HistoryCount",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "HistoryType",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "InputTokens",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "IsStar",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "IsWithBot",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "OutputTokens",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "SearchEnabled",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "StarAt",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ApiStyle",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "ChannelType",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "BuiltInImageGen",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "BuiltInWebSearch",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "Selected",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "SupportTool",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "SupportVision",
                table: "AiModels");
        }
    }
}
