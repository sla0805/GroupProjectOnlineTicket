using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineTicket.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string Provider { get; set; }
        public string Reference { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }

}
