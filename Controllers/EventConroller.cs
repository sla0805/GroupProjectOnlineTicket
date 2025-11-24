using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Data;
using OnlineTicket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineTicket.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _db;

        public EventController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Event/Index
        public async Task<IActionResult> Index(int? categoryId, int? venueId, DateTime? date, string search, int page = 1, int pageSize = 9)
        {
            var query = _db.Events
                           .Include(e => e.Venue)
                           .AsQueryable();

            if (categoryId.HasValue) query = query.Where(e => e.CategoryId == categoryId.Value);
            if (venueId.HasValue) query = query.Where(e => e.VenueId == venueId.Value);
            if (date.HasValue) query = query.Where(e => e.EventDate.Date == date.Value.Date);
            if (!string.IsNullOrEmpty(search)) query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search));

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var events = await query.OrderBy(e => e.EventDate)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            ViewBag.CategoryList = _db.Categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name }).ToList();
            ViewBag.VenueList = _db.Venues.Select(v => new SelectListItem { Value = v.VenueId.ToString(), Text = v.Name }).ToList();
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CategoryId = categoryId;
            ViewBag.VenueId = venueId;
            ViewBag.DateString = date?.ToString("yyyy-MM-dd");
            ViewBag.Search = search;

            return View(events);
        }


        // GET: /Event/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var evt = await _db.Events
                               .Include(e => e.Category)
                               .Include(e => e.Venue)
                               .Include(e => e.TicketTypes)
                               .FirstOrDefaultAsync(e => e.EventId == id);

            if (evt == null)
                return NotFound();

            return View(evt);
        }
    }
}
