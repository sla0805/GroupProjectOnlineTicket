using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;
using QRCoder;
using System.IO;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using static QRCoder.QRCodeGenerator;

public class TicketController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public TicketController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> MyTickets(int bookingId)
    {
        var tickets = await _db.Tickets
              .Include(t => t.Event)
              .ThenInclude(e => e.Venue)
              .Include(t => t.TicketType)
              .Where(t => t.BookingId == bookingId)
              .ToListAsync();

        if (!tickets.Any()) return NotFound();

        foreach (var ticket in tickets)
        {
            if (string.IsNullOrEmpty(ticket.QrBase64))
            {
                ticket.QrBase64 = QrHelper.GenerateQrBase64($"TicketID:{ticket.TicketId}");
            }
        }

        return View(tickets);
    }

    public async Task<IActionResult> DownloadTicketsPdf(int bookingId)
    {
        var tickets = await _db.Tickets
                               .Include(t => t.Event)
                               .Include(t => t.TicketType)
                               .Include(t => t.Event.Venue)
                               .Where(t => t.BookingId == bookingId)
                               .ToListAsync();

        if (!tickets.Any()) return NotFound();
        // Ensure each ticket has a QR code
        foreach (var ticket in tickets)
        {
            if (string.IsNullOrEmpty(ticket.QrBase64))
            {
                ticket.QrBase64 = QrHelper.GenerateQrBase64($"TicketID:{ticket.TicketId}");
            }
        }
        var document = new TicketDocument(tickets);
        var pdfBytes = document.GeneratePdf();

        return File(pdfBytes, "application/pdf", $"Tickets_Booking_{bookingId}.pdf");
    }

   
}



