using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantFlow.Data.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAlgorithmAndEffectivenessTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Algorithms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlgorithmType = table.Column<int>(type: "int", nullable: false),
                    AlgorithmSource = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "1.0"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Algorithms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlgorithmEffectiveness",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlgorithmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timeframe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReliabilityStars = table.Column<int>(type: "int", nullable: false),
                    OpportunityStars = table.Column<int>(type: "int", nullable: false),
                    RecommendedStars = table.Column<int>(type: "int", nullable: false),
                    ReliabilityReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OpportunityReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AverageWinRate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    AverageReturnPerTrade = table.Column<decimal>(type: "decimal(6,4)", precision: 6, scale: 4, nullable: true),
                    AverageTradesPerMonth = table.Column<int>(type: "int", nullable: true),
                    AverageSharpeRatio = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AverageMaxDrawdown = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    AverageStopLossPercent = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    BestFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvoidWhen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalBacktestsRun = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "System")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlgorithmEffectiveness", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlgorithmEffectiveness_Algorithms_AlgorithmId",
                        column: x => x.AlgorithmId,
                        principalTable: "Algorithms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlgorithmEffectiveness_AlgorithmId_Timeframe",
                table: "AlgorithmEffectiveness",
                columns: new[] { "AlgorithmId", "Timeframe" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AlgorithmEffectiveness_RecommendedStars",
                table: "AlgorithmEffectiveness",
                column: "RecommendedStars");

            migrationBuilder.CreateIndex(
                name: "IX_Algorithms_AlgorithmSource",
                table: "Algorithms",
                column: "AlgorithmSource");

            migrationBuilder.CreateIndex(
                name: "IX_Algorithms_IsEnabled",
                table: "Algorithms",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Algorithms_Name",
                table: "Algorithms",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlgorithmEffectiveness");

            migrationBuilder.DropTable(
                name: "Algorithms");
        }
    }
}
