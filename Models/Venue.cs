using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}
