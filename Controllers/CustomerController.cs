using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;

namespace OnlineTicket.Controllers
{
    [Authorize(Roles = "Customer")]

    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CustomerController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

      
        // GET: Customer/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Try to get existing customer
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
            {
                // Create a temporary empty VM for first-time users
                var vm = new CustomerEditVM
                {
                    CustomerId = 0,
                    FullName = "",
                    PhoneNumber = "",
                    Email = user.Email,
                    CreatedAt = DateTime.UtcNow
                };

                return View(vm);
            }

            // Customer exists, populate VM
            var existingVm = new CustomerEditVM
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Email = user.Email,
                CreatedAt = customer.CreatedAt
            };

            return View(existingVm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CustomerEditVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                vm.Email = user.Email; // ensure email displayed
                return View(vm);
            }

            // Check if customer already exists
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
            {
                // CREATE new profile
                customer = new Customer
                {
                    UserId = user.Id,
                    FullName = vm.FullName.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(vm.PhoneNumber) ? null : vm.PhoneNumber.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _db.Customers.Add(customer);
            }
            else
            {
                // UPDATE existing profile
                customer.FullName = vm.FullName.Trim();
                customer.PhoneNumber = string.IsNullOrWhiteSpace(vm.PhoneNumber) ? null : vm.PhoneNumber.Trim();
                customer.UpdatedAt = DateTime.UtcNow;

                _db.Customers.Update(customer);
            }

            await _db.SaveChangesAsync();

            TempData["ProfileSuccess"] = "Profile saved successfully.";
            return RedirectToAction(nameof(Profile));
        }
        //Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            var customer = await _db.Customers
                .Include(c => c.Bookings)
                    .ThenInclude(b => b.Event)
                     .Include(c => c.Bookings)
                   .ThenInclude(b => b.Tickets)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
            {
                // Optionally create profile or redirect
                return RedirectToAction("Profile");
            }

            var bookings = customer.Bookings ?? new List<Booking>();

            var vm = new CustomerDashboardVM
            {
                TotalBookings = bookings.Count,
                UpcomingEvents = bookings
                    .Where(b => b.Event != null && b.Event.EventDate >= DateTime.Today)
                    .ToList(),
                RecentBookings = bookings
                    .Where(b => b.Event != null)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToList()
            };

            return View(vm);
        }


       
      
    

    }
}
