using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OnlineTicket.Data;
using OnlineTicket.Models;
using OnlineTicket.ViewModels;

namespace OnlineTicket.Controllers
{
    [Authorize] // Ensure only logged-in users can access this
    public class OrganizerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public OrganizerController(ApplicationDbContext context,
                                   UserManager<IdentityUser> userManager,
                                   IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // Helper to get the current Organizer ID
        private async Task<int?> GetCurrentOrganizerIdAsync()
        {
            var userId = _userManager.GetUserId(User);
            // This links IdentityUser.Id to Organizer.IdentityUserId
            var organizer = await _context.Organizers
                                          .FirstOrDefaultAsync(o => o.IdentityUserId == userId);
            return organizer?.OrganizerId;
        }

        public async Task<IActionResult> Dashboard()
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue)
                return RedirectToAction("SetupProfile");

            var organizer = await _context.Organizers.FindAsync(organizerId.Value);

            // Fetch all events for this organizer
            var events = await _context.Events
                .Where(e => e.OrganizerId == organizerId.Value)
                .Include(e => e.Bookings)
                    .ThenInclude(b => b.Tickets)
                .ToListAsync();

            // Calculate event stats
            var eventStats = events.Select(e => new EventStatsViewModel
            {
                EventId = e.EventId,
                Title = e.Title,
                EventDate = e.EventDate,
                TotalTicketsSold = e.Bookings.Sum(b => b.Tickets.Count),
                TotalRevenue = e.Bookings.Sum(b => b.Payment.Amount),
                TotalSeatsAvailable = e.TotalSeats - e.Bookings.Sum(b => b.Tickets.Count)
            }).ToList();

            // Calculate Active Promotions count
            var activePromotionsCount = await _context.Promotions
                .Include(p => p.Event)
                .Where(p => p.Event.OrganizerId == organizerId.Value
                            && p.IsActive
                            && p.StartDate <= DateTime.Now
                            && p.EndDate >= DateTime.Now)
                .CountAsync();

            // Total tickets sold & revenue all time
            var totalTicketsSoldAllTime = eventStats.Sum(e => e.TotalTicketsSold);
            var totalRevenueAllTime = eventStats.Sum(e => e.TotalRevenue);

            // Build dashboard view model
            var viewModel = new OrganizerDashboardViewModel
            {
                OrganizerName = organizer.OrganizerName,
                TotalEvents = events.Count,
                TotalTicketsSoldAllTime = totalTicketsSoldAllTime,
                TotalRevenueAllTime = totalRevenueAllTime,
                ActivePromotionsCount = activePromotionsCount,
                UpcomingEvents = eventStats.Where(e => e.EventDate >= DateTime.Now).OrderBy(e => e.EventDate).ToList(),
                RevenuePerEvent = eventStats.OrderByDescending(e => e.TotalRevenue).Take(5).ToList()
            };

            return View(viewModel);
        }


        // GET: /Organizer/SetupProfile
        [HttpGet]
        public async Task<IActionResult> SetupProfile()
        {
            var userId = _userManager.GetUserId(User);
            var userEmail = User.Identity.Name;

            // Check if profile already exists
            var existingProfile = await _context.Organizers.FirstOrDefaultAsync(o => o.IdentityUserId == userId);
            if (existingProfile != null)
            {
                return RedirectToAction("Dashboard");
            }

            // Pre-fill the form with the user's login email
            var model = new OrganizerProfileViewModel
            {
                Email = userEmail
            };

            return View(model);
        }

        // POST: /Organizer/SetupProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupProfile(OrganizerProfileViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                // Prevent duplicate creation
                var existingProfile = await _context.Organizers.FirstOrDefaultAsync(o => o.IdentityUserId == userId);
                if (existingProfile != null)
                {
                    return RedirectToAction("Dashboard");
                }

                // Create the new Organizer profile
                var organizer = new Organizer
                {
                    IdentityUserId = userId,
                    OrganizerName = model.OrganizerName,
                    Address = model.Address,
                    Phone = model.Phone,
                    Email = model.Email,
                };

                _context.Organizers.Add(organizer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your organizer profile has been created successfully! You can now start hosting events.";
                return RedirectToAction("Dashboard");
            }

            // If model is invalid, return the view with errors
            return View(model);
        }

        // GET: /Organizer/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue)
            {
                // This user hasn't set up their profile yet, send them to setup.
                return RedirectToAction("SetupProfile");
            }

            // Find the organizer's profile in the database
            var organizer = await _context.Organizers.FindAsync(organizerId.Value);
            if (organizer == null)
            {
                return NotFound();
            }

            // Map the data from the (Organizer) database model to the (OrganizerProfileViewModel) view model
            var model = new OrganizerProfileViewModel
            {
                OrganizerName = organizer.OrganizerName,
                Address = organizer.Address,
                Phone = organizer.Phone,
                Email = organizer.Email
            };

            return View(model);
        }

        // POST: /Organizer/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(OrganizerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return the view with validation errors
            }

            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue)
            {
                return Unauthorized();
            }

            // Get the existing organizer profile from the database to update it
            var organizer = await _context.Organizers.FindAsync(organizerId.Value);
            if (organizer == null)
            {
                return NotFound();
            }

            // Map the updated values from the view model back to the database model
            organizer.OrganizerName = model.OrganizerName;
            organizer.Address = model.Address;
            organizer.Phone = model.Phone;
            organizer.Email = model.Email;

            try
            {
                _context.Organizers.Update(organizer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (DbUpdateException ex)
            {
                // Log the error
                ModelState.AddModelError("", "Unable to save changes. Please try again.");
                return View(model);
            }
        }
        // -------------------------------
        // Events - List / Create / Edit / Delete
        // Views location: Views/Organizer/Events/{Index,Create,Edit,Delete,TicketTypes}.cshtml
        // -------------------------------

        /* LIST: /Organizer/Events
           Supports: search, category filter, venue filter, status, pagination
        */
        public async Task<IActionResult> Events(string search, int? categoryId, int? venueId, string status, int page = 1, int pageSize = 10)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return RedirectToAction("SetupProfile");

            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Tickets)
                .Where(e => e.OrganizerId == organizerId.Value)
                .OrderByDescending(e => e.EventDate)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.Title.Contains(search));
            }

            // Category filter
            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            // Venue filter
            if (venueId.HasValue)
            {
                query = query.Where(e => e.VenueId == venueId.Value);
            }

            // Status filter
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(e => e.Status == status);
            }

            // Pagination
            var totalItems = await query.CountAsync();
            var events = await query
                .OrderByDescending(e => e.EventDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Prepare dropdowns as SelectList for Tag Helpers
            ViewBag.CategoryList = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
                "CategoryId",
                "Name",
                categoryId
            );

            ViewBag.VenueList = new SelectList(
                await _context.Venues.OrderBy(v => v.Name).ToListAsync(),
                "VenueId",
                "Name",
                venueId
            );

            ViewBag.StatusList = new SelectList(
                new List<SelectListItem>
                {
            new SelectListItem { Value = "All", Text = "All" },
            new SelectListItem { Value = "Active", Text = "Active" },
            new SelectListItem { Value = "Cancelled", Text = "Cancelled" }
                }, "Value", "Text", status ?? "All"
            );

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View("Events/Index", events);
        }

        // GET: /Organizer/CreateEvent
        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return RedirectToAction("SetupProfile");

            var vm = new EventCreateViewModel
            {
                EventDate = DateTime.Now,
                Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync(),
                Venues = await _context.Venues
                    .OrderBy(v => v.Name)
                    .Select(v => new SelectListItem
                    {
                        Value = v.VenueId.ToString(),
                        Text = v.Name
                    })
                    .ToListAsync()
            };

            return View("Events/Create", vm);
        }

        // POST: /Organizer/CreateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventCreateViewModel model)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return Unauthorized();

            if (!ModelState.IsValid)
            {
                // reload dropdowns if validation fails
                model.Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                model.Venues = await _context.Venues
                    .OrderBy(v => v.Name)
                    .Select(v => new SelectListItem
                    {
                        Value = v.VenueId.ToString(),
                        Text = v.Name
                    })
                    .ToListAsync();

                return View("Events/Create", model);
            }

            string imagePath = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var wwwRoot = _hostEnvironment.WebRootPath;
                var uploadFolder = Path.Combine(wwwRoot, "images", "events");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var fullPath = Path.Combine(uploadFolder, fileName);
                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fs);
                }
                imagePath = "/images/events/" + fileName;
            }

            var newEvent = new Event
            {
                Title = model.Title,
                EventDate = model.EventDate,
                TicketPrice = model.TicketPrice,
                Description = model.Description ?? "",
                TotalSeats = model.TotalSeats,
                CategoryId = model.CategoryId,
                VenueId = model.VenueId,
                OrganizerId = organizerId.Value,
                ImagePath = imagePath,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event created successfully!";
            return RedirectToAction("AddTicketTypes", new { eventId = newEvent.EventId });
        }



        /* EDIT (GET): /Organizer/EditEvent/{id}
           Loads event into EventEditViewModel (only if it belongs to current organizer)
        */
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return RedirectToAction("SetupProfile");

            var ev = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id && e.OrganizerId == organizerId.Value);

            if (ev == null) return NotFound();

            var vm = new EventEditViewModel
            {
                EventId = ev.EventId,
                Title = ev.Title,
                EventDate = ev.EventDate,
                TicketPrice = ev.TicketPrice,
                Description = ev.Description,
                TotalSeats = ev.TotalSeats,
                CategoryId = ev.CategoryId,
                VenueId = ev.VenueId,
                ExistingImagePath = ev.ImagePath,
                Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name, Selected = (c.CategoryId == ev.CategoryId) })
                    .ToListAsync(),
                Venues = await _context.Venues
                    .OrderBy(v => v.Name)
                    .Select(v => new SelectListItem { Value = v.VenueId.ToString(), Text = v.Name, Selected = (v.VenueId == ev.VenueId) })
                    .ToListAsync()
            };

            return View("Events/Edit", vm);
        }

        /* EDIT (POST): /Organizer/EditEvent
           Updates event (only if belongs to current organizer)
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EventEditViewModel model)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return Unauthorized();

            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                    .ToListAsync();
                model.Venues = await _context.Venues
                    .OrderBy(v => v.Name)
                    .Select(v => new SelectListItem { Value = v.VenueId.ToString(), Text = v.Name })
                    .ToListAsync();
                return View("Events/Edit", model);
            }

            var ev = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == model.EventId && e.OrganizerId == organizerId.Value);

            if (ev == null) return NotFound();

            // Replace image if new uploaded
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var wwwRoot = _hostEnvironment.WebRootPath;
                // delete old image if exists
                if (!string.IsNullOrEmpty(ev.ImagePath))
                {
                    var oldFile = Path.Combine(wwwRoot, ev.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                }

                var uploadFolder = Path.Combine(wwwRoot, "images", "events");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var fullPath = Path.Combine(uploadFolder, fileName);
                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fs);
                }
                ev.ImagePath = "/images/events/" + fileName;
            }

            // update fields
            ev.Title = model.Title;
            ev.EventDate = model.EventDate;
            ev.TicketPrice = model.TicketPrice;
            ev.Description = model.Description;
            ev.TotalSeats = model.TotalSeats;
            ev.CategoryId = model.CategoryId;
            ev.VenueId = model.VenueId;

            _context.Events.Update(ev);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event updated successfully.";
            return RedirectToAction(nameof(Events));
        }

        /* DELETE (GET confirmation): /Organizer/DeleteEvent/{id}
           Shows confirmation view (only owner can see)
        */
        [HttpGet]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return RedirectToAction("SetupProfile");

            var ev = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id && e.OrganizerId == organizerId.Value);

            if (ev == null) return NotFound();
            return View("Events/Delete", ev);
        }

        /* DELETE (POST): /Organizer/DeleteEventConfirmed/{id}
           Performs deletion (ensures only owner)
        */
        [HttpPost, ActionName("DeleteEventConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEventConfirmed(int id)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return Unauthorized();

            var ev = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id && e.OrganizerId == organizerId.Value);

            if (ev == null) return NotFound();

            // delete image file if exists
            if (!string.IsNullOrEmpty(ev.ImagePath))
            {
                var wwwRoot = _hostEnvironment.WebRootPath;
                var file = Path.Combine(wwwRoot, ev.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(file)) System.IO.File.Delete(file);
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event deleted.";
            return RedirectToAction(nameof(Events));
        }

        /* TICKET TYPES: List + Add + Delete (per-event)
           Views: Views/Organizer/Events/TicketTypes.cshtml
        */
        [HttpGet]
        public async Task<IActionResult> AddTicketTypes(int eventId)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return RedirectToAction("SetupProfile");

            // Verify the event belongs to this organizer
            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.OrganizerId == organizerId.Value);

            if (ev == null) return NotFound();

            return View("Events/AddTicketTypes", ev); // Pass Event including its TicketTypes
        }


        /* POST: /Organizer/AddTicketType
           Adds a ticket type to an event (only if organizer owns the event)
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketTypes(int eventId, string name, decimal price, int totalSeats)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return Unauthorized();

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId && e.OrganizerId == organizerId.Value);
            if (ev == null) return NotFound();

            var tt = new TicketType
            {
                EventId = eventId,
                Name = name,
                Price = price,
                TotalSeats = totalSeats
            };

            _context.TicketTypes.Add(tt);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ticket type added.";
            return RedirectToAction(nameof(AddTicketTypes), new { eventId });
        }



        //Edit Ticket Get

        // GET: Show all ticket types for an event in cards
        [HttpGet]
        public async Task<IActionResult> EditTicketTypes(int eventId)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return RedirectToAction("SetupProfile");

            // Verify the event belongs to this organizer
            var ev = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.OrganizerId == organizerId.Value);

            if (ev == null) return NotFound();

            return View("Events/EditTicketTypes", ev); // Pass Event including its TicketTypes
        }

        // POST: Update a single ticket type
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTicketType(int ticketTypeId, string name, decimal price, int totalSeats)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return Unauthorized();

            var tt = await _context.TicketTypes
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TicketTypeId == ticketTypeId && t.Event.OrganizerId == organizerId.Value);

            if (tt == null) return NotFound();

            // Optional: prevent reducing seats below already sold tickets
            var soldTicketsCount = await _context.Tickets.CountAsync(t => t.TicketTypeId == tt.TicketTypeId);
            if (totalSeats < soldTicketsCount)
            {
                TempData["ErrorMessage"] = $"Cannot reduce total seats below already sold tickets ({soldTicketsCount}).";
                return RedirectToAction(nameof(EditTicketTypes), new { eventId = tt.EventId });
            }

            tt.Name = name;
            tt.Price = price;
            tt.TotalSeats = totalSeats;

            _context.TicketTypes.Update(tt);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ticket type updated successfully.";
            return RedirectToAction(nameof(EditTicketTypes), new { eventId = tt.EventId });
        }

        // POST: Delete a ticket type
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTicketType(int ticketTypeId)
        {
            var organizerId = await GetCurrentOrganizerIdAsync();
            if (!organizerId.HasValue) return Unauthorized();

            var tt = await _context.TicketTypes
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TicketTypeId == ticketTypeId && t.Event.OrganizerId == organizerId.Value);

            if (tt == null)
            {
                TempData["ErrorMessage"] = "Ticket type not found.";
                return RedirectToAction(nameof(EditTicketTypes), new { eventId = tt.EventId });
            }

            var hasSoldTickets = await _context.Tickets.AnyAsync(t => t.TicketTypeId == ticketTypeId);
            if (hasSoldTickets)
            {
                TempData["ErrorMessage"] = "Cannot delete ticket type that already has sold tickets.";
                return RedirectToAction(nameof(EditTicketTypes), new { eventId = tt.EventId });
            }

            _context.TicketTypes.Remove(tt);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ticket type deleted successfully.";
            return RedirectToAction(nameof(AddTicketTypes), new { eventId = tt.EventId });
        }

        // GET: /Organizer/Promotions
        public async Task<IActionResult> Promotions(int eventId = 0)
        {
            // Fetch promotions with Event and TicketType
            var query = _context.Promotions
                .Include(p => p.Event)
                .Include(p => p.TicketType)
                .AsQueryable();

            if (eventId > 0)
                query = query.Where(p => p.EventId == eventId);

            // Fetch events for dropdown filter
            ViewBag.Events = new SelectList(
                await _context.Events.ToListAsync(),
                "EventId", "Title", eventId
            );

            var promotions = await query.ToListAsync();

            // Map to PromotionViewModel
            var promoViewModels = promotions.Select(p => new PromotionViewModel
            {
                PromotionId = p.PromotionId,
                Name = p.Name,
                Code = p.Code,
                DiscountPercentage = p.DiscountPercentage,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                EventId = p.EventId,
                TicketTypeId = p.TicketTypeId,
                EventName = p.Event.Title,
                TicketTypeName = p.TicketType.Name
            }).ToList();

            return View("PromoList", promoViewModels);
        }



        // GET: /Organizer/AddPromotion
        [HttpGet]
        public async Task<IActionResult> AddPromotion()
        {
            var vm = new PromotionViewModel
            {
                Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "Title"),
                TicketTypes = new SelectList(await _context.TicketTypes.ToListAsync(), "TicketTypeId", "Name"),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                IsActive = true
            };

            return View(vm);
        }


        // POST: /Organizer/AddPromotion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPromotion(PromotionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // repopulate SelectLists if validation fails
                model.Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "Title", model.EventId);
                model.TicketTypes = new SelectList(await _context.TicketTypes.ToListAsync(), "TicketTypeId", "Name", model.TicketTypeId);

                return View(model);
            }

            var promo = new Promotion
            {
                Name = model.Name,
                Code = model.Code,
                DiscountPercentage = model.DiscountPercentage,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive,
                EventId = model.EventId,
                TicketTypeId = model.TicketTypeId
            };

            _context.Promotions.Add(promo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Promotion added successfully!";
            return RedirectToAction("Promotions");
        }


        // GET: /Organizer/EditPromotion/{id}
        [HttpGet]
        public async Task<IActionResult> EditPromotion(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            var vm = new PromotionViewModel
            {
                PromotionId = promo.PromotionId,
                Name = promo.Name,
                Code = promo.Code,
                DiscountPercentage = promo.DiscountPercentage,
                StartDate = promo.StartDate,
                EndDate = promo.EndDate,
                IsActive = promo.IsActive,
                EventId = promo.EventId,
                TicketTypeId = promo.TicketTypeId,

                Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "Title", promo.EventId),
                TicketTypes = new SelectList(await _context.TicketTypes.ToListAsync(), "TicketTypeId", "Name", promo.TicketTypeId)
            };

            return View(vm);
        }


        // POST: /Organizer/EditPromotion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPromotion(PromotionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Events = new SelectList(await _context.Events.ToListAsync(), "EventId", "Title");
                model.TicketTypes = new SelectList(await _context.TicketTypes.ToListAsync(), "TicketTypeId", "Name");
                return View(model);
            }

            var promo = await _context.Promotions.FindAsync(model.PromotionId);
            if (promo == null) return NotFound();

            promo.Name = model.Name;
            promo.Code = model.Code;
            promo.DiscountPercentage = model.DiscountPercentage;
            promo.StartDate = model.StartDate;
            promo.EndDate = model.EndDate;
            promo.IsActive = model.IsActive;
            promo.EventId = model.EventId;
            promo.TicketTypeId = model.TicketTypeId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Promotion updated successfully!";
            return RedirectToAction("Promotions");
        }


        // POST: /Organizer/DeletePromotion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            _context.Promotions.Remove(promo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Promotion deleted successfully!";
            return RedirectToAction("Promotions");
        }


        // VALIDATE PROMO DURING BOOKING
        public async Task<Promotion> ValidatePromo(string code, int eventId, int ticketTypeId)
        {
            var now = DateTime.UtcNow;

            return await _context.Promotions
                .FirstOrDefaultAsync(p =>
                    p.Code == code &&
                    p.EventId == eventId &&
                    p.TicketTypeId == ticketTypeId &&
                    p.IsActive &&
                    p.StartDate <= now &&
                    p.EndDate >= now
                );
        }


    }
}