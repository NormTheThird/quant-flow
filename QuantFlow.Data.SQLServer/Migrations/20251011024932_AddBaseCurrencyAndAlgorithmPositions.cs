using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantFlow.Data.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseCurrencyAndAlgorithmPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Portfolios_UserExchangeDetails_UserExchangeDetailsId",
                table: "Portfolios");

            migrationBuilder.DropIndex(
                name: "IX_Portfolios_UserId_Mode_Exchange",
                table: "Portfolios");

            migrationBuilder.DropIndex(
                name: "IX_Portfolios_UserId_Name",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "AllowShortSelling",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "MaxPositionSizePercent",
                table: "Portfolios");

            migrationBuilder.AddColumn<Guid>(
                name: "AlgorithmPositionId",
                table: "Trades",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Portfolios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Inactive");

            migrationBuilder.AlterColumn<string>(
                name: "Mode",
                table: "Portfolios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "TestMode");

            migrationBuilder.AlterColumn<string>(
                name: "Exchange",
                table: "Portfolios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseCurrency",
                table: "Portfolios",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AlgorithmPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlgorithmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AllocatedPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxPositionSizePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 10.0m),
                    ExchangeFees = table.Column<decimal>(type: "decimal(8,6)", precision: 8, scale: 6, nullable: false, defaultValue: 0.001m),
                    AllowShortSelling = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlgorithmPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlgorithmPositions_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_AlgorithmPositionId",
                table: "Trades",
                column: "AlgorithmPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UserId_Exchange_BaseCurrency",
                table: "Portfolios",
                columns: new[] { "UserId", "Exchange", "BaseCurrency" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AlgorithmPositions_AlgorithmId",
                table: "AlgorithmPositions",
                column: "AlgorithmId");

            migrationBuilder.CreateIndex(
                name: "IX_AlgorithmPositions_PortfolioId",
                table: "AlgorithmPositions",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_AlgorithmPositions_PortfolioId_PositionName",
                table: "AlgorithmPositions",
                columns: new[] { "PortfolioId", "PositionName" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Portfolios_UserExchangeDetails_UserExchangeDetailsId",
                table: "Portfolios",
                column: "UserExchangeDetailsId",
                principalTable: "UserExchangeDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_AlgorithmPositions_AlgorithmPositionId",
                table: "Trades",
                column: "AlgorithmPositionId",
                principalTable: "AlgorithmPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Portfolios_UserExchangeDetails_UserExchangeDetailsId",
                table: "Portfolios");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_AlgorithmPositions_AlgorithmPositionId",
                table: "Trades");

            migrationBuilder.DropTable(
                name: "AlgorithmPositions");

            migrationBuilder.DropIndex(
                name: "IX_Trades_AlgorithmPositionId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Portfolios_UserId_Exchange_BaseCurrency",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "AlgorithmPositionId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "BaseCurrency",
                table: "Portfolios");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Portfolios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Inactive",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Mode",
                table: "Portfolios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "TestMode",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Exchange",
                table: "Portfolios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "AllowShortSelling",
                table: "Portfolios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                table: "Portfolios",
                type: "decimal(8,6)",
                precision: 8,
                scale: 6,
                nullable: false,
                defaultValue: 0.001m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxPositionSizePercent",
                table: "Portfolios",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 10.0m);

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UserId_Mode_Exchange",
                table: "Portfolios",
                columns: new[] { "UserId", "Mode", "Exchange" });

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UserId_Name",
                table: "Portfolios",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Portfolios_UserExchangeDetails_UserExchangeDetailsId",
                table: "Portfolios",
                column: "UserExchangeDetailsId",
                principalTable: "UserExchangeDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
