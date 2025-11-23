using OnlineTicket.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineTicket.ViewModels
{
    // --- Dashboard View Models ---
    public class EventStatsViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalSeatsAvailable { get; set; }
    }

    public class OrganizerDashboardViewModel
    {
        public string OrganizerName { get; set; }
        public int TotalEvents { get; set; }
        public int TotalTicketsSoldAllTime { get; set; }
        public decimal TotalRevenueAllTime { get; set; }
        public int ActivePromotionsCount { get; set; }
        public List<EventStatsViewModel> UpcomingEvents { get; set; } = new List<EventStatsViewModel>();
        public List<EventStatsViewModel> RevenuePerEvent { get; set; } = new List<EventStatsViewModel>();
    }


    // --- Organizer Profile View Models ---
    public class OrganizerProfileViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Company / Organizer Name")]
        public string OrganizerName { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Business Address")]
        public string Address { get; set; }

        [Phone]
        [Required]
        [StringLength(20)]
        [Display(Name = "Contact Phone Number (e.g., 077-123-4567)")]
        public string Phone { get; set; }

        [EmailAddress]
        [Required]
        [StringLength(256)]
        [Display(Name = "Public Contact Email")]
        public string Email { get; set; }
    }
}