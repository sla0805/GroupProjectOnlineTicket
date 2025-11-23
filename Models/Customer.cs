using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; } // Link to IdentityUser

        public IdentityUser User { get; set; }
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        [Column(TypeName = "date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.Date;
        [Column(TypeName = "date")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow.Date;



        public ICollection<Booking> Bookings { get; set; }
    }
}
