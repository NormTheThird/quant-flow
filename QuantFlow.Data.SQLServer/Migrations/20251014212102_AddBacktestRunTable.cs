using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantFlow.Data.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class AddBacktestRunTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BacktestRunEntityId",
                table: "Trades",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BacktestRun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AlgorithmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Exchange = table.Column<int>(type: "int", nullable: false),
                    Timeframe = table.Column<int>(type: "int", nullable: false),
                    BacktestStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BacktestEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InitialBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AlgorithmParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(8,6)", precision: 8, scale: 6, nullable: false),
                    FinalBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalReturnPercent = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    MaxDrawdownPercent = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    SharpeRatio = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    TotalTrades = table.Column<int>(type: "int", nullable: false),
                    WinningTrades = table.Column<int>(type: "int", nullable: false),
                    LosingTrades = table.Column<int>(type: "int", nullable: false),
                    WinRatePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AverageTradeReturnPercent = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    ExecutionDurationTicks = table.Column<long>(type: "bigint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BacktestRun_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BacktestRun_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_BacktestRunEntityId",
                table: "Trades",
                column: "BacktestRunEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_PortfolioId",
                table: "BacktestRun",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_Status",
                table: "BacktestRun",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_UserId",
                table: "BacktestRun",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_BacktestRun_BacktestRunEntityId",
                table: "Trades",
                column: "BacktestRunEntityId",
                principalTable: "BacktestRun",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trades_BacktestRun_BacktestRunEntityId",
                table: "Trades");

            migrationBuilder.DropTable(
                name: "BacktestRun");

            migrationBuilder.DropIndex(
                name: "IX_Trades_BacktestRunEntityId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "BacktestRunEntityId",
                table: "Trades");
        }
    }
}
