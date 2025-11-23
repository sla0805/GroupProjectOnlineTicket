using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class PaymentVM
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public string Provider { get; set; } // Card, PayPal, etc.

        [Required]
        [Display(Name = "Amount to Pay")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be positive.")]
        public decimal Amount { get; set; }

        public string Reference { get; set; } // Generated automatically
    }
}
