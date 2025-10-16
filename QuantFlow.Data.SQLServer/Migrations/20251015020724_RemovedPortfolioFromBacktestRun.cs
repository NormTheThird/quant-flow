using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantFlow.Data.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class RemovedPortfolioFromBacktestRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BacktestRun_Portfolios_PortfolioId",
                table: "BacktestRun");

            migrationBuilder.RenameColumn(
                name: "PortfolioId",
                table: "BacktestRun",
                newName: "PortfolioEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_BacktestRun_PortfolioId",
                table: "BacktestRun",
                newName: "IX_BacktestRun_PortfolioEntityId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "BacktestRun",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Exchange",
                table: "BacktestRun",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_BacktestRun_Portfolios_PortfolioEntityId",
                table: "BacktestRun",
                column: "PortfolioEntityId",
                principalTable: "Portfolios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BacktestRun_Portfolios_PortfolioEntityId",
                table: "BacktestRun");

            migrationBuilder.RenameColumn(
                name: "PortfolioEntityId",
                table: "BacktestRun",
                newName: "PortfolioId");

            migrationBuilder.RenameIndex(
                name: "IX_BacktestRun_PortfolioEntityId",
                table: "BacktestRun",
                newName: "IX_BacktestRun_PortfolioId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "BacktestRun",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Exchange",
                table: "BacktestRun",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_BacktestRun_Portfolios_PortfolioId",
                table: "BacktestRun",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
