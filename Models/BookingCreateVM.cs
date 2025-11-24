using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class BookingCreateVM
    {

        public int EventId { get; set; }
        public string EventTitle { get; set; }

        [Required]
        public int TicketTypeId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public int? PromotionId { get; set; }

        public List<TicketTypeDropdownItem> TicketTypes { get; set; } = new();
        public List<PromotionDropdownItem> AvailablePromotions { get; set; } = new();
    }

    public class TicketTypeDropdownItem
    {
        public int TicketTypeId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int TotalSeats { get; set; }
    }

    public class PromotionDropdownItem
    {
        public int PromotionId { get; set; }
        public string Name { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TicketTypeId { get; set; }

    }
}
