using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineTicket.Models
{
    public class Promotion
    {
        public int PromotionId { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign key to Event (non-nullable)
        public int EventId { get; set; }
        public Event Event { get; set; }

        // Foreign key to TicketType (non-nullable)
        public int TicketTypeId { get; set; }
        public TicketType TicketType { get; set; }

        // Optional: Bookings that use this promotion
        public ICollection<Booking> Bookings { get; set; }
    }
}
