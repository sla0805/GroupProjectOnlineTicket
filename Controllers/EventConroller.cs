using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineTicket.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _db;
        public EventController(ApplicationDbContext db) { _db = db; }

        // GET: /Event or /Home/Index mapped to view
        public async Task<IActionResult> Index(string q, string date, string category)
        {
            var events = _db.Events.Include(e => e.TicketTypes).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                events = events.Where(e => EF.Functions.Like(e.Title, $"%{q}%") || EF.Functions.Like(e.Description, $"%{q}%"));

            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var dt))
                events = events.Where(e => e.EventDate.Date == dt.Date);

            if (!string.IsNullOrWhiteSpace(category))
                events = events.Where(e => e.CategoryId == int.Parse(category)); 

            var list = await events.OrderBy(e => e.EventDate).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ev = await _db.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();
            return View(ev);
        }
    }
}
