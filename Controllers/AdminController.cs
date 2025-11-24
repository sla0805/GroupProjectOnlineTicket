using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;
using OnlineTicket.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineTicket.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==============================
        // DASHBOARD
        // ==============================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            // -----------------------------
            // Total tickets sold per event
            // -----------------------------
            var ticketsPerEvent = await _context.Tickets
                .GroupBy(t => new { t.EventId, t.Event.Title })
                .Select(g => new TicketsPerEventVM
                {
                    EventId = g.Key.EventId,
                    Title = g.Key.Title,
                    TotalTicketsSold = g.Count()
                })
                .ToListAsync();

            // -----------------------------
            // Total revenue per event
            // -----------------------------
            var revenuePerEvent = await _context.Bookings
                .GroupBy(b => new { b.EventId, b.Event.Title })
                .Select(g => new RevenuePerEventVM
                {
                    EventId = g.Key.EventId,
                    Title = g.Key.Title,
                    TotalRevenue = g.Sum(b => b.TotalAmount)
                })
                .ToListAsync();

            // -----------------------------
            // Total revenue per month
            // -----------------------------
            var revenuePerMonth = await _context.Bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new RevenuePerMonthVM
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToListAsync();

            // -----------------------------
            // Active users count (all users)
            // -----------------------------
            var activeUsersCount = await _userManager.Users.CountAsync();

            // Build dashboard view model
            var dashboardVM = new AdminDashboardVM
            {
                TicketsPerEvent = ticketsPerEvent,
                RevenuePerEvent = revenuePerEvent,
                RevenuePerMonth = revenuePerMonth,
                ActiveUsersCount = activeUsersCount
            };

            return View(dashboardVM);
        }


        // ==============================
        // VENUE CRUD
        // ==============================
        public IActionResult CreateVenue()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVenue(VenueVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var venue = new Venue
            {
                Name = model.Name,
                Address = model.Address,
                City = model.City
            };
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Venue created successfully!";
            return RedirectToAction(nameof(ListVenues));
        }

        public async Task<IActionResult> ListVenues(string searchString)
        {
            var venuesQuery = _context.Venues.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                venuesQuery = venuesQuery.Where(v =>
                    v.Name.Contains(searchString) ||
                    v.Address.Contains(searchString) ||
                    v.City.Contains(searchString)
                );
            }

            var venues = await venuesQuery.ToListAsync();

            var model = venues.Select(v => new VenueVM
            {
                VenueId = v.VenueId,
                Name = v.Name,
                Address = v.Address,
                City = v.City
            }).ToList();

            ViewData["CurrentFilter"] = searchString;

            return View(model);
        }



        public async Task<IActionResult> EditVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            var model = new VenueVM
            {
                VenueId = venue.VenueId,
                Name = venue.Name,
                Address = venue.Address,
                City = venue.City
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVenue(VenueVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var venue = await _context.Venues.FindAsync(model.VenueId);
            if (venue == null) return NotFound();

            venue.Name = model.Name;
            venue.Address = model.Address;
            venue.City = model.City;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Venue updated successfully!";
            return RedirectToAction(nameof(ListVenues));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Venue deleted successfully!";
            return RedirectToAction(nameof(ListVenues));
        }

        // ==============================
        // USER MANAGEMENT
        // ==============================

        // List all customers with bookings count
        public async Task<IActionResult> Customers(string search, int page = 1, int pageSize = 10)
        {
            // Include related IdentityUser data
            var customersQuery = _context.Customers
                .Include(c => c.User)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                customersQuery = customersQuery.Where(c =>
                    c.FullName.ToLower().Contains(search) ||
                    c.User.Email.ToLower().Contains(search) ||
                    (c.PhoneNumber != null && c.PhoneNumber.Contains(search))
                );
            }

            // Project to UserWithStatsVM
            var customerList = await customersQuery
                .Select(c => new UserWithStatsVM
                {
                    UserId = c.UserId,
                    FullName = c.FullName,
                    Email = c.User.Email,
                    PhoneNumber = c.PhoneNumber,
                    BookingsCount = c.Bookings.Count,
                    IsEnabled = c.User.LockoutEnd == null || c.User.LockoutEnd <= DateTimeOffset.UtcNow
                })
                .OrderBy(c => c.FullName)
                .ToListAsync();

            // PAGINATION
            var paged = PagedResult<UserWithStatsVM>.CreateFromList(customerList, page, pageSize);

            ViewBag.Search = search;

            return View(paged);
        }

        //Delete Customer account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Customer not specified.");

            // Get the Identity user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            // Get the Customer entry
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            // Delete the Identity user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to delete customer.";
                return RedirectToAction("Customers");
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Customer account deleted successfully.";
            return RedirectToAction("Customers");
        }


        // List all organizers with hosted events count
        public async Task<IActionResult> Organizers(string search, int page = 1, int pageSize = 10)
        {
            // Step 1: Get all users in the "Organizer" role
            var organizersInRole = await _userManager.GetUsersInRoleAsync("Organizer");
            var organizerUserIds = organizersInRole.Select(u => u.Id).ToList();

            // Step 2: Fetch all organizer details including events in a single query
            var organizers = await _context.Organizers
                .Where(o => organizerUserIds.Contains(o.IdentityUserId))
                .Include(o => o.Events)
                .ToListAsync();

            // Step 3: Map data to UserWithStatsVM
            var list = organizersInRole.Select(u =>
            {
                var organizerEntry = organizers.FirstOrDefault(o => o.IdentityUserId == u.Id);

                int eventCount = organizerEntry?.Events?.Count ?? 0;
                bool isEnabled = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow;

                return new UserWithStatsVM
                {
                    UserId = u.Id,
                    OrganizerId = organizerEntry?.OrganizerId ?? 0,
                    OrganizerName = organizerEntry?.OrganizerName ?? "",
                    Address = organizerEntry?.Address ?? "",
                    Phone = organizerEntry?.Phone ?? "",
                    Email = u.Email,
                    HostedEventsCount = eventCount,
                    IsEnabled = isEnabled
                };
            }).ToList();

            // Step 4: Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                list = list
                    .Where(x => (x.OrganizerName?.ToLower().Contains(search) ?? false) ||
                                (x.Email?.ToLower().Contains(search) ?? false) ||
                                (x.Address?.ToLower().Contains(search) ?? false) ||
                                (x.Phone?.ToLower().Contains(search) ?? false))
                    .ToList();
            }

            // Step 5: Pagination
            var paged = PagedResult<UserWithStatsVM>.CreateFromList(list, page, pageSize);
            ViewBag.Search = search;

            return View(paged);
        }

        //Delete Organizer

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrganizer(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Organizer not specified.");

            // Get the Identity user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            // Get the Organizer entry
            var organizer = await _context.Organizers.FirstOrDefaultAsync(o => o.IdentityUserId == userId);
            if (organizer != null)
            {
                _context.Organizers.Remove(organizer);
            }

            // Delete the Identity user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to delete organizer.";
                return RedirectToAction("Organizers");
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Organizer account deleted successfully.";
            return RedirectToAction("Organizers");
        }

        // Event list with search and pagination
        public async Task<IActionResult> Events(string search, int page = 1, int pageSize = 10)
        {
            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(search) ||
                    e.Category.Name.ToLower().Contains(search) ||
                    e.Venue.Name.ToLower().Contains(search) ||
                    e.Organizer.OrganizerName.ToLower().Contains(search));
            }

            var eventsList = await query
                .OrderByDescending(e => e.EventDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            var pagedResult = new PagedResult<Event>
            {
                Items = eventsList,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalCount
            };

            ViewBag.Search = search;

            return View(pagedResult);
        }

        // Delete Event
        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Events");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                // Enable account
                user.LockoutEnd = null;
            }
            else
            {
                // Disable account (lock indefinitely)
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }

            await _userManager.UpdateAsync(user);
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
