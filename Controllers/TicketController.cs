using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;
using QRCoder;
using System.IO;
using System.Threading.Tasks;

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
              .Include(t => t.TicketType)
              .Where(t => t.BookingId == bookingId)
              .ToListAsync();

        if (!tickets.Any()) return NotFound();

        return View(tickets);
    }
}
