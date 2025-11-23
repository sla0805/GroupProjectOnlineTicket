using OnlineTicket.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.ViewModels
{
    // For listing ticket types in TicketTypes view
    public class TicketTypeViewModel
    {
        public int TicketTypeId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int TotalSeats { get; set; }

        public int TicketsSold { get; set; }  // optional, calculated in controller
        public int TicketsAvailable { get; set; } // optional, calculated in controller
    }

    // For adding a new ticket type
    public class TicketTypeCreateViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public List<TicketType> ExistingTicketTypes { get; set; } = new();

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int TotalSeats { get; set; }
    }

    // For editing an existing ticket type
    public class TicketTypeEditViewModel
    {
        public int TicketTypeId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int TotalSeats { get; set; }

        public int TicketsSold { get; set; }  // optional, for display
    }
}
