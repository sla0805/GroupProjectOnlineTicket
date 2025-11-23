using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.ViewModels
{
    public class PromotionViewModel
    {
        public int PromotionId { get; set; }

        // Name of the promotion
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Promotion code (coupon code)
        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        // Discount percentage as decimal (5,2)
        [Required]
        [Range(0.01, 100.00, ErrorMessage = "Discount must be between 0.01 and 100.00")]
        public decimal DiscountPercentage { get; set; }

        // Start / End dates
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Active status
        public bool IsActive { get; set; } = true;

        // REQUIRED: Event selection
        [Required(ErrorMessage = "Please select an event.")]
        public int EventId { get; set; }
        public SelectList? Events { get; set; }

        // REQUIRED: TicketType selection
        [Required(ErrorMessage = "Please select a ticket type.")]
        public int TicketTypeId { get; set; }
        public SelectList? TicketTypes { get; set; }

        public string? EventName { get; set; }
        public string? TicketTypeName { get; set; }
    }
}
