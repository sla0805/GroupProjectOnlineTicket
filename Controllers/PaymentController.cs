using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;

namespace OnlineTicket.Controllers
{
    [Authorize(Roles = "Customer")]

    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PaymentController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Show payment page
        [HttpGet]
        public async Task<IActionResult> Pay(int bookingId)
        {
            var booking = await _db.Bookings
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return NotFound();

            var vm = new PaymentVM
            {
                BookingId = booking.BookingId,
                Amount = booking.TotalAmount
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(PaymentVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            string paymentReference = Guid.NewGuid().ToString(); // dummy reference

            var payment = new Payment
            {
                BookingId = vm.BookingId,
                Amount = vm.Amount,
                Provider = vm.Provider,
                Reference = paymentReference,
                PaidAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);

            var booking = await _db.Bookings.FindAsync(vm.BookingId);
            booking.PaymentStatus = "Paid";
            booking.Payment = payment;

            await _db.SaveChangesAsync();

            TempData["PaymentSuccess"] = "Payment successful!";
            return RedirectToAction("MyBookings", "Customer");
        }

    }
}
