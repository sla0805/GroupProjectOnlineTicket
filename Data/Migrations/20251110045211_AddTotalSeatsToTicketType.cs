using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTicket.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalSeatsToTicketType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalSeats",
                table: "TicketTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalSeats",
                table: "TicketTypes");
        }
    }
}
