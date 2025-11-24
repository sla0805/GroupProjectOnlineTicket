using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTicket.Data.Migrations
{
    /// <inheritdoc />
    public partial class addnewpromotionfieldinbooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "Bookings");
        }
    }
}
