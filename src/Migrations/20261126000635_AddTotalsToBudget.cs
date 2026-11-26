using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalClinicWPF.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalsToBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfPayments",
                table: "Budgets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMXN",
                table: "Budgets",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalUSD",
                table: "Budgets",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfPayments",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "TotalMXN",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "TotalUSD",
                table: "Budgets");
        }
    }
}
