using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }  // e.g., "Concert", "Theater", "Sports"

        [StringLength(500)]
        public string Description { get; set; }  // optional description

        // Navigation property: one category can have many events
        public ICollection<Event> Events { get; set; }
    }
}
