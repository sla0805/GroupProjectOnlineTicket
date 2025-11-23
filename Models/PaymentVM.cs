using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class PaymentVM
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
        public string PaymentStatus { get; set; } = "Pending";

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string EventTitle { get; set; }
        public string EventImage { get; set; }
        public string VenueName { get; set; }
        public DateTime EventDate { get; set; }

    }
}
