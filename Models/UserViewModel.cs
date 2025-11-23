namespace OnlineTicket.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsOrganizer { get; set; }
        public bool IsAdmin { get; set; }
    }
}
