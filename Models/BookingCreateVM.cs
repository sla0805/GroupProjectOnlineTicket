using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class BookingCreateVM
    {
        [Required]
        public int EventId { get; set; } 

        public string Title { get; set; }  //EventName

        [Required]
        [Range(1, 10, ErrorMessage = "You can book 1 to 10 tickets at a time.")]
        public int Quantity { get; set; }  

        [Required]
        public int TicketTypeId { get; set; }  
        public string TicketTypeName { get; set; }  
        public decimal TicketPrice { get; set; }  

        public decimal TotalAmount { get; set; } 

        public string CustomerFullName { get; set; }  
        public string CustomerPhone { get; set; }     

        public string PaymentMethod { get; set; }  // e.g., Card, PayPal

        public int? PromotionId { get; set; }
        public string PromotionCode { get; set; }

        public List<TicketType> TicketTypes { get; set; } = new();
    }
}
