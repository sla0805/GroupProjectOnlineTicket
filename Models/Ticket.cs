using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace OnlineTicket.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public int? TicketTypeId { get; set; }
        public TicketType TicketType { get; set; }

        public string SeatNumber { get; set; }

        public string QRCode { get; set; }
    }
}
