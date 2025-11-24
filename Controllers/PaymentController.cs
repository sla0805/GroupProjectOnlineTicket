using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;
using QRCoder;
using System.Threading.Tasks;
using static QRCoder.QRCodeGenerator;

public class PaymentController : Controller
{
    private readonly ApplicationDbContext _db;

    public PaymentController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Create(int bookingId)
    {
        var booking = await _db.Bookings
                               .Include(b => b.Event)
                               //.Include(b => b.Payment)
                               .ThenInclude(e => e.Venue)
                               .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null)
            return NotFound();
        ViewBag.EventTitle = booking.Event.Title;
        ViewBag.EventImage = booking.Event.ImagePath;
        ViewBag.VenueName = booking.Event.Venue.Name;
        ViewBag.EventDate = booking.Event.EventDate;
        ViewBag.EventId = booking.Event.EventId;
        // Create a Payment object and set BookingId and Amount
        var payment = new PaymentVM
        {
            BookingId = booking.BookingId,
            Amount = booking.TotalAmount
        };


        return View(payment);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentVM payment)
    {
        var booking = await _db.Bookings
                                  .Include(b => b.Tickets)
                                  .FirstOrDefaultAsync(b => b.BookingId == payment.BookingId);
        if (booking == null) return NotFound();
        decimal finalAmount = booking.TotalAmount;
        if (!string.IsNullOrEmpty(payment.PromotionCode))
        {
            // Example: 10% discount if any promo code
            finalAmount *= 0.9m;
        }

        var pay = new Payment
        {
            BookingId = booking.BookingId,
            Amount = booking.FinalAmount,
            PaymentMethod = payment.PaymentMethod,
           
            PaymentStatus = "Paid",
            PaymentDate = DateTime.UtcNow,


        };

        _db.Payments.Add(pay);

        // Update booking status
        booking.BookingStatus = "Confirmed";
        booking.Payment = pay;
        // Generate tickets with QR
        for (int i = 0; i < booking.Quantity; i++)
        {
            var ticket = new Ticket
            {
                BookingId = booking.BookingId,
                EventId = booking.EventId,
                TicketTypeId = booking.Tickets.FirstOrDefault()?.TicketTypeId ?? 1,
                QrBase64 = QrHelper.GenerateQrBase64($"Booking:{booking.BookingId}-Ticket:{i + 1}")
            };
            _db.Tickets.Add(ticket);
        }

        await _db.SaveChangesAsync();

        return RedirectToAction("MyBookings", "Booking");
    }

   
}