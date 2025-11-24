using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;
using OnlineTicket.ViewModels;
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

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateVM vm)
    {

        //    if (!ModelState.IsValid) return View(vm);

        //    var user = await _userManager.GetUserAsync(User);
        //    var customer = await _db.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
        //    if (customer == null) return RedirectToAction("Index", "Event");

        //    var ticketType = await _db.TicketTypes.FindAsync(vm.TicketTypeId);
        //    if (ticketType == null || vm.Quantity > ticketType.TotalSeats)
        //    {
        //        ModelState.AddModelError("", "Invalid quantity or ticket type.");
        //        return View(vm);
        //    }

        //    decimal totalAmount = ticketType.Price * vm.Quantity;

        //    // Apply promotion if selected
        //    if (vm.PromotionId.HasValue)
        //    {
        //        var promo = await _db.Promotions.FindAsync(vm.PromotionId.Value);
        //        if (promo != null)
        //            totalAmount = totalAmount * (1 - (promo.DiscountPercentage / 100m));
        //    }


        //    var booking = new Booking
        //    {
        //        EventId = vm.EventId,
        //        CustomerId = customer.CustomerId,
        //        Quantity = vm.Quantity,
        //        TotalAmount = totalAmount,
        //        BookingStatus = "Pending",
        //        PromotionId = vm.PromotionId,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    _db.Bookings.Add(booking);
        //    await _db.SaveChangesAsync();

        //    return RedirectToAction("Create", "Payment", new { bookingId = booking.BookingId });
        //}

        if (!ModelState.IsValid)
        {
            var evt = await _db.Events
                .Include(e => e.TicketTypes)
                .Include(e => e.Promotions)
                .FirstOrDefaultAsync(e => e.EventId == vm.EventId);

            vm.TicketTypes = evt.TicketTypes.Select(tt => new TicketTypeDropdownItem
            {
                TicketTypeId = tt.TicketTypeId,
                Name = tt.Name,
                Price = tt.Price,
                TotalSeats = tt.TotalSeats
            }).ToList();

            vm.AvailablePromotions = evt.Promotions.Select(p => new PromotionDropdownItem
            {
                PromotionId = p.PromotionId,
                Name = p.Name,
                DiscountPercentage = p.DiscountPercentage,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                TicketTypeId = p.TicketTypeId
            }).ToList();

            return View(vm);
        }

        var user = await _userManager.GetUserAsync(User);
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (customer == null) return RedirectToAction("Index", "Event");

        var ticketType = await _db.TicketTypes.FindAsync(vm.TicketTypeId);
        if (ticketType == null)
        {
            ModelState.AddModelError("", "Invalid ticket type.");
            return View(vm);
        }

        if (vm.Quantity <= 0 || vm.Quantity > ticketType.TotalSeats)
        {
            ModelState.AddModelError("", "Quantity exceeds available seats.");
            return View(vm);
        }

        decimal subtotal = ticketType.Price * vm.Quantity;
        decimal discountAmount = 0m;
        Promotion appliedPromo = null;

        if (vm.PromotionId.HasValue)
        {
            appliedPromo = await _db.Promotions.FindAsync(vm.PromotionId.Value);
            var now = DateTime.UtcNow;

            if (appliedPromo == null 
                || !appliedPromo.IsActive 
                || appliedPromo.StartDate > now 
                || appliedPromo.EndDate < now 
                || appliedPromo.EventId != vm.EventId 
                || appliedPromo.TicketTypeId != vm.TicketTypeId)
            {
                ModelState.AddModelError("", "Promotion is invalid or not applicable.");
                return View(vm);
            }

            discountAmount = Math.Round(subtotal * (appliedPromo.DiscountPercentage / 100m), 2);
            if (discountAmount > subtotal) discountAmount = subtotal;
        }

        decimal finalAmount = subtotal - discountAmount;

        var booking = new Booking
        {
            EventId = vm.EventId,
            CustomerId = customer.CustomerId,
            Quantity = vm.Quantity,
            TotalAmount = subtotal,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            BookingStatus = "Pending",
            PromotionId = appliedPromo?.PromotionId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);

        ticketType.TotalSeats -= vm.Quantity;
        _db.TicketTypes.Update(ticketType);

        await _db.SaveChangesAsync();

        
        return RedirectToAction("Create", "Payment", new { bookingId = booking.BookingId });
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
