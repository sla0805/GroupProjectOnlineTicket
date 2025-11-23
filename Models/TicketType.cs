using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineTicket.Models
{
    public class TicketType
    {
        public int TicketTypeId { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public string Name { get; set; }// e.g., VIP, Regular
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int TotalSeats { get; set; }  // Total seats for this type

        public ICollection<Ticket> Tickets { get; set; }

    }

}
