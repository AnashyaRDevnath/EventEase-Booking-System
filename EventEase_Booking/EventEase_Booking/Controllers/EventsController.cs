using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase_Booking.Data;
using EventEase_Booking.Models;

namespace EventEase_Booking.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventEase_BookingContext _context;

        public EventsController(EventEase_BookingContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(string? searchQuery, int? categoryId, DateTime? startDate, DateTime? endDate, bool onlyAvailable = false)
        {
            /*=== CODE ATTRIBUTION===
          Title: Read Related Data in ASP.NET Core MVC (EF Core)
          Author: Microsoft Learn
          Date: 04/06/2026
          URL: https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/read-related-data
            */

            // 1. Preserve filter values so the UI remembers what the user typed/selected
            ViewData["SearchQuery"] = searchQuery;
            ViewData["CategoryId"] = categoryId;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["OnlyAvailable"] = onlyAvailable;

            // 2. Build the Category Dropdown list from your new EventTypes table
            // This reads the categories you just typed into the database
            ViewBag.CategoryList = new SelectList(_context.EventTypes, "Id", "Name", categoryId);

            // 3. Start the base query using .AsQueryable() 
            var eventsQuery = _context.Event
                .Include(e => e.EventType) // Pulls in the category names
                .AsQueryable();

            // 4. Chain the filters step-by-step
            // Filter by Text (EventName or Description)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                // Remove accidental spaces and force lowercase to make the search bulletproof
                var cleanSearch = searchQuery.Trim().ToLower();

                eventsQuery = eventsQuery.Where(e => e.EventName.ToLower().Contains(cleanSearch));
            }

            // Filter by EventType Lookup
            if (categoryId.HasValue && categoryId > 0)
            {
                eventsQuery = eventsQuery.Where(e => e.EventTypeId == categoryId.Value);
            }

            // Filter by Start Date
            if (startDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EventDate >= startDate.Value);
            }

            // Filter by End Date
            if (endDate.HasValue)
            {
                //to pick up before midnight
                eventsQuery = eventsQuery.Where(e => e.EventDate < endDate.Value.AddDays(1));
            }

            // Filter by Availability (Using the IsAvailable boolean added earlier)
            if (onlyAvailable)
            {
                eventsQuery = eventsQuery.Where(e => e.IsAvailable == true);
            }

            // 5. Execute the final combined query and send to the View
            var finalResults = await eventsQuery.ToListAsync();

            return View(finalResults);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "Id", "Name");
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,EventName,ImageURL,EventDate,IsAvailable,EventTypeId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }



            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventID,EventName,ImageURL,EventDate,IsAvailable,EventTypeId")] Event @event)
        {
            if (id != @event.EventID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }



            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event.FindAsync(id);
            if (@event != null)
            {
                _context.Event.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventID == id);
        }
    }
}
