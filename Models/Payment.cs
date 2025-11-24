using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineTicket.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        // Foreign key to Booking
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Payment method: Visa, MasterCard,  etc.
        [Required]
        public string PaymentMethod { get; set; }

        [Required]
        [MaxLength(16)]
        public string CardNumber { get; set; }

        [Required]
        [MaxLength(5)]
        public string ExpiryDate { get; set; } // MM/YY

        [Required]
        [MaxLength(3)]
        public string CVV { get; set; }

        [Required]
        public string PaymentStatus { get; set; } = "Pending";

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }

}
