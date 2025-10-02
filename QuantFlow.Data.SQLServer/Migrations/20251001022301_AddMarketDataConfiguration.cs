using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantFlow.Data.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketDataConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketDataConfiguration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SymbolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Is1mActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Is5mActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Is15mActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Is1hActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Is4hActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Is1dActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketDataConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketDataConfiguration_Symbols_SymbolId",
                        column: x => x.SymbolId,
                        principalTable: "Symbols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketDataConfiguration_Exchange",
                table: "MarketDataConfiguration",
                column: "Exchange");

            migrationBuilder.CreateIndex(
                name: "IX_MarketDataConfiguration_SymbolId",
                table: "MarketDataConfiguration",
                column: "SymbolId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketDataConfiguration_SymbolId_Exchange",
                table: "MarketDataConfiguration",
                columns: new[] { "SymbolId", "Exchange" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketDataConfiguration");
        }
    }
}
