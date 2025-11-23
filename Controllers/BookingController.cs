using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;
using QRCoder;
using System.Drawing.Imaging;

namespace OnlineTicket.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Booking form
        [HttpGet]
        public async Task<IActionResult> BookingCreate(int eventId)
        {
            var evt = await _db.Events.FindAsync(eventId);
            if (evt == null) return NotFound();

            var vm = new BookingCreateVM
            {

                EventId = evt.EventId,
                Title = evt.Title,
                TicketTypes = evt.TicketTypes.ToList()

            };
            return View(vm);
        }

        // Submit booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookingCreate(BookingCreateVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Event"] = await _db.Events.FindAsync(vm.EventId);
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

            var ticketType = await _db.TicketTypes.FindAsync(vm.TicketTypeId);
            if (ticketType == null) return BadRequest();

            // availability check
            var bookedCount = await _db.Tickets.CountAsync(t => t.TicketTypeId == ticketType.TicketTypeId);
            if (ticketType.TotalSeats > 0 && bookedCount + vm.Quantity > ticketType.TotalSeats)
            {
                ModelState.AddModelError("", "Not enough seats.");
                vm.TicketTypes = await _db.TicketTypes.Where(t => t.EventId == vm.EventId).ToListAsync();
                return View(vm);
            }

            var booking = new Booking
            {
                CustomerId = customer.CustomerId,
                EventId = vm.EventId,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = vm.Quantity * ticketType.Price,
                PaymentStatus = "Pending"
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            // Generate tickets and QR codes
            for (int i = 0; i < vm.Quantity; i++)
            {
               // var qrBase64 = GenerateQRCodeBase64($"B-{booking.BookingId}|T-{i}|E-{vm.EventId}");

                var ticket = new Ticket
                {
                    BookingId = booking.BookingId,
                    EventId = vm.EventId,
                    TicketTypeId = vm.TicketTypeId,
                    SeatNumber = $"Seat-{i + 1}",
                    QRCode = GenerateQRCode($"{booking.BookingId}-{i + 1}")
                };
                _db.Tickets.Add(ticket);
            }
            await _db.SaveChangesAsync();

            return RedirectToAction("MyBookings");
        }

        [Authorize(Roles = "Customer")]

        // Customer booking history
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

            var bookings = await _db.Bookings
                .Include(b => b.Event)
                .Include(b => b.Tickets)
                .Where(b => b.CustomerId == customer.CustomerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        private string GenerateQRCode(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var bitmap = qrCode.GetGraphic(20);
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
        }
    }
}
