using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace OnlineTicket.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public int CustomerId { get; set; } 
        public Customer Customer { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]  
        public decimal TotalAmount { get; set; }
        public string BookingStatus { get; set; } = "Pending";

        public int? PromotionId { get; set; }
        public Promotion Promotion { get; set; }

        public Payment Payment { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
    }

}
