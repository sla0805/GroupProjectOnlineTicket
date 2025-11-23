using System.ComponentModel.DataAnnotations;

namespace OnlineTicket.Models
{
    public class CustomerVM
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }// from IdentityUser


        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
