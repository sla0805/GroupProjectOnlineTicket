using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineTicket.Models
{
    public class Organizer
    {
        // Primary Key - Database will automatically generate this ID (Identity Column)
        [Key]
        public int OrganizerId { get; set; }

        [Required]
        [StringLength(100)]
        public string OrganizerName { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        [Phone]
        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [EmailAddress]
        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        // Foreign Key to AspNetUsers.Id (the IdentityUser PK)
        [Required]
        public string IdentityUserId { get; set; }

        // Navigation Property for the 1:1 relationship with IdentityUser
        public IdentityUser User { get; set; }

        // Navigation Property for the 1:N relationship with Event
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}