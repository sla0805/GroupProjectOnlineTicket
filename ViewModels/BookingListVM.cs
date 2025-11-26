using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.ViewModels
{
    public class BookingListVM
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public string EventName { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PromotionCode { get; set; }
        public decimal FinalAmount { get; set; }
    }

}
