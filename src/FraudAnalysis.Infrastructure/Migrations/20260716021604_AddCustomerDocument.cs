using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FraudAnalysis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_document",
                table: "transactions",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_document",
                table: "transactions");
        }
    }
}
