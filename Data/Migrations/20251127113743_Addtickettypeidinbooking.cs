using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTicket.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtickettypeidinbooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TicketTypeId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TicketTypeId",
                table: "Bookings",
                column: "TicketTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_TicketTypes_TicketTypeId",
                table: "Bookings",
                column: "TicketTypeId",
                principalTable: "TicketTypes",
                principalColumn: "TicketTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_TicketTypes_TicketTypeId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TicketTypeId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TicketTypeId",
                table: "Bookings");
        }
    }
}
