using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;
using OnlineTicket.ViewModels;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public BookingController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: Booking/Create?eventId=1
    public async Task<IActionResult> Create(int eventId)
    {
        var evt = await _db.Events
            .Include(e => e.TicketTypes)
            .Include(e => e.Promotions)
             .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (evt == null) return NotFound();

        var vm = new BookingCreateVM
        {
            EventId = evt.EventId,
            EventTitle = evt.Title,
            TicketTypes = evt.TicketTypes.Select(tt => new TicketTypeDropdownItem
            {
                TicketTypeId = tt.TicketTypeId,
                Name = tt.Name,
                Price = tt.Price,
                TotalSeats = tt.TotalSeats
            }).ToList(),
            AvailablePromotions = evt.Promotions.Select(p => new PromotionDropdownItem
            {
                PromotionId = p.PromotionId,
                Name = p.Name,
                DiscountPercentage = p.DiscountPercentage,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                TicketTypeId = p.TicketTypeId
            }).ToList()
        };

        ViewBag.EventImage = evt.ImagePath;
        ViewBag.EventDate = evt.EventDate;
        ViewBag.VenueName = evt.Venue.Name;
        ViewBag.EventTitle = evt.Title;
        ViewBag.Description = evt.Description;
        ViewBag.EventOrganizer = evt.Organizer;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateVM vm)
    {

        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.GetUserAsync(User);
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (customer == null) return RedirectToAction("Index", "Event");

        var ticketType = await _db.TicketTypes.FirstOrDefaultAsync(tt => tt.TicketTypeId == vm.TicketTypeId);
        if (ticketType == null || vm.Quantity > ticketType.TotalSeats)
        {
            ModelState.AddModelError("", "Invalid quantity or ticket type.");
            return View(vm);
        }
        if (vm.Quantity <= 0 || vm.Quantity > ticketType.TotalSeats)
        {
            ModelState.AddModelError("", $"Quantity exceeds available seats ({ticketType.TotalSeats}).");
            return View(vm);
        }

        decimal totalAmount = ticketType.Price * vm.Quantity;
        decimal discountAmount = 0m;
        decimal finalAmount = totalAmount;

        // Apply promotion if selected
        if (vm.PromotionId.HasValue)
        {
            var promo = await _db.Promotions.FindAsync(vm.PromotionId.Value);
            if (promo != null)
            {
                discountAmount = Math.Round(totalAmount * (promo.DiscountPercentage / 100m), 2);
                finalAmount = totalAmount - discountAmount;
            }
        }


        var booking = new Booking
        {
            EventId = vm.EventId,
            CustomerId = customer.CustomerId,
            TicketTypeId =vm.TicketTypeId,
            Quantity = vm.Quantity,
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,  
            FinalAmount = finalAmount,
            BookingStatus = "Pending",
            PromotionId = vm.PromotionId,
            CreatedAt = DateTime.UtcNow
        };
        // Reduce seat count
        ticketType.TotalSeats -= vm.Quantity;
        if (ticketType.TotalSeats < 0) ticketType.TotalSeats = 0; // defensive
        _db.TicketTypes.Update(ticketType);

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return RedirectToAction("Create", "Payment", new { bookingId = booking.BookingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int bookingId)
    {
        // Get current logged-in user
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Event");

        // Find booking for this user
        var booking = await _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Event)
            .Include(b => b.Tickets)
            .ThenInclude(b => b.TicketType)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.Customer.UserId == user.Id);

        if (booking == null) return NotFound();

        // Only allow cancelling if booking is still pending
        if (booking.BookingStatus != "Pending")
        {
            TempData["Error"] = "Only pending bookings can be cancelled.";
            return RedirectToAction("MyBookings");
        }
        
        // Update status to Cancelled
        booking.BookingStatus = "Cancelled";

        // Optionally, free up the tickets
        if (booking.Tickets != null && booking.Tickets.Any())
        {
            _db.Tickets.RemoveRange(booking.Tickets);
        }
        booking.Event.TotalSeats += booking.Quantity;


        await _db.SaveChangesAsync();

        TempData["Success"] = "Your booking has been cancelled.";
        return RedirectToAction("MyBookings");
    }


    // GET: Booking/MyBookings
    public async Task<IActionResult> MyBookings()
    {
        var user = await _userManager.GetUserAsync(User);
        var customer = await _db.Customers
            .Include(c => c.Bookings)
                .ThenInclude(b => b.Payment)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.Tickets)
                    .ThenInclude(t => t.TicketType)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.Event)
              .Include(c => c.Bookings)
                .ThenInclude(b => b.Promotion)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (customer == null) return RedirectToAction("Index", "Event");

        var bookings = customer.Bookings.OrderByDescending(b => b.CreatedAt).ToList();

        return View(bookings);
    }

    // GET: Booking/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.Event)
            .Include(b => b.Payment)
            .Include(b => b.Tickets)
            .ThenInclude(t => t.TicketType)
            .Include(b => b.Promotion)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null) return NotFound();

        return View(booking);
    }

}
