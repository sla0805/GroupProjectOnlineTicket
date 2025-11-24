using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.ViewModels
{
    // ===============================
    // Admin Dashboard ViewModel
    // ===============================
    public class AdminDashboardVM
    {
        public int ActiveUsersCount { get; set; }

        // Tickets sold per event
        public List<TicketsPerEventVM> TicketsPerEvent { get; set; }

        // Revenue per event
        public List<RevenuePerEventVM> RevenuePerEvent { get; set; }

        // Revenue per month
        public List<RevenuePerMonthVM> RevenuePerMonth { get; set; }
    }

    public class TicketsPerEventVM
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public int TotalTicketsSold { get; set; }
    }

    public class RevenuePerEventVM
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RevenuePerMonthVM
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
    }

    // ===============================
    // Venue CRUD ViewModel
    // ===============================
    public class VenueVM
    {
        public int VenueId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string City { get; set; }
    }

    // ===============================
    // User Management ViewModel
    // ===============================
    public class UserWithStatsVM
    {
        public string UserId { get; set; }
        public string Email { get; set; }

        // For Customers
        public int BookingsCount { get; set; }

        // For Organizers
        public int HostedEventsCount { get; set; }

        public bool IsEnabled { get; set; }

        //Customer Fields
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

        // Organizer fields
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

    }

    // ===============================
    // Optional: Event Summary for Admin
    // ===============================
    public class EventSummaryVM
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
