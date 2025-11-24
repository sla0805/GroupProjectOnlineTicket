using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class PaymentVM
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Required]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required]
        [Display(Name = "Expiry Date (MM/YY)")]
        public string ExpiryDate { get; set; }

        [Required]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        // Event details for display in view
        public string EventTitle { get; set; }
        public string EventImage { get; set; }
        public string VenueName { get; set; }
        public DateTime EventDate { get; set; }
        public int EventId { get; set; }
        [MaxLength(20)]
        public string PromotionCode { get; set; }

    }
}
