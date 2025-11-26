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
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }

}
