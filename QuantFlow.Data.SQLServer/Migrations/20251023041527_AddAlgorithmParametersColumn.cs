using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantFlow.Data.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAlgorithmParametersColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlgorithmParameters",
                table: "AlgorithmPositions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlgorithmParameters",
                table: "AlgorithmPositions");
        }
    }
}
