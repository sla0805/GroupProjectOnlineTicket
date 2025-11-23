namespace OnlineTicket.Models
{
    public class CustomerDashboardVM
    {
        public int TotalBookings { get; set; }
        public List<Booking> UpcomingEvents { get; set; }
        public List<Booking> RecentBookings { get; set; }
    }
}
