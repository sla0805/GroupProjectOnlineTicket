using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class CustomerEditVM
    {
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } // read-only from IdentityUser

        [Display(Name = "Member since")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
