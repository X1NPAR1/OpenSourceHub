using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenSourceHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiProviderSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaudeApiKey",
                table: "AppSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaudeModel",
                table: "AppSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeepSeekApiKey",
                table: "AppSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeepSeekModel",
                table: "AppSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeminiApiKey",
                table: "AppSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeminiModel",
                table: "AppSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ClaudeApiKey", "ClaudeModel", "DeepSeekApiKey", "DeepSeekModel", "GeminiApiKey", "GeminiModel" },
                values: new object[] { null, "claude-sonnet-4-5", null, "deepseek-chat", null, "gemini-2.0-flash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaudeApiKey",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "ClaudeModel",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "DeepSeekApiKey",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "DeepSeekModel",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GeminiApiKey",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GeminiModel",
                table: "AppSettings");
        }
    }
}
