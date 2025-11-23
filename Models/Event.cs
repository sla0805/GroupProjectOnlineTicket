using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace OnlineTicket.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TicketPrice { get; set; }
        public string? Description { get; set; }
        public string ImagePath { get; set; }

        // Foreign Keys
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int VenueId { get; set; }
        public Venue Venue { get; set; }

        // Organizer (IdentityUser)
        public int OrganizerId { get; set; } // FK to Organizer.OrganizerId
        public Organizer Organizer { get; set; } // Navigation property to Organizer model

        public int TotalSeats { get; set; } = 0;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<TicketType> TicketTypes { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<Promotion> Promotions { get; set; }
    }

}
