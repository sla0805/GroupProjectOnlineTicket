using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;


namespace OnlineTicket.ViewModels
{
    public class EventCreateViewModel
    {
        [Required (ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }


        [Required(ErrorMessage = "Event Date is required.")]
        public DateTime EventDate { get; set; }


        [Required(ErrorMessage = "Ticket Price is required.")]
        [Range(0, 999999)]
        public decimal TicketPrice { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Event image is required.")]
        public IFormFile? ImageFile { get; set; }


        [Required(ErrorMessage = "Please select one category.")]
        public int CategoryId { get; set; }


        [Required(ErrorMessage = "Please select one venue.")]
        public int VenueId { get; set; }


        [Required(ErrorMessage = "Total seats are required.")]
        public int TotalSeats { get; set; }


        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IEnumerable<SelectListItem>? Venues { get; set; }
    }
}