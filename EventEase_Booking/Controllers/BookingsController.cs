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
    public class BookingsController : Controller
    {
        private readonly EventEase_BookingContext _context;

        public BookingsController(EventEase_BookingContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string? searchQuery)
        {
            // Store the query so we can show it back in the search box later
            ViewData["SearchQuery"] = searchQuery;

            // 1. Start with the base query (Include related tables!)
            var bookings = _context.Booking
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .AsQueryable();

            // 2. SEARCH: Filter by BookingID or Event Name
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                // Check if the user typed a number (for ID search)
                bool isIdSearch = int.TryParse(searchQuery.Trim(), out int searchId);

                bookings = bookings.Where(b =>
                    (isIdSearch && b.BookingID == searchId) ||
                    (b.Event != null && b.Event.EventName.ToLower().Contains(searchQuery.ToLower()))

                );
            }

            // 3. Execute the query and return to view
            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName");
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,VenueID,EventID,StartDate,EndDate")] Booking booking)
        {
            // 1. Logic Check: End date must be after Start date
            if (booking.StartDate >= booking.EndDate)
            {
                ModelState.AddModelError("EndDate", "The booking cannot end before it has started!");
            }

            // 2. Logic Check: Prevent overlapping bookings
            // We only check this if the dates themselves are valid
            if (ModelState.IsValid)
            {
                if (HasDoubleBooking(booking.VenueID, booking.StartDate, booking.EndDate))
                {
                    ModelState.AddModelError("", "This venue is already booked for the selected time window.");
                }
            }

            // 3. The "Success" Path
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reservation created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // 4. The "Failure" Path: Re-populate dropdowns and return to the form
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);

            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingID,VenueID,EventID,StartDate,EndDate")] Booking booking)
        {
            if (id != booking.BookingID)
            {
                return NotFound();
            }

            // 1. DATE CONSISTENCY CHECK (From PDF)
            if (booking.StartDate >= booking.EndDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after the start date.");
            }

            // 2. DOUBLE BOOKING CHECK (Only check if the dates themselves are valid first)
            if (ModelState.IsValid)
            {
                if (HasDoubleBooking(booking.VenueID, booking.StartDate, booking.EndDate, id))
                {
                    ModelState.AddModelError("", "The new times you selected conflict with an existing booking.");
                }
            }

            // 3. FINAL SAVE LOGIC
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    // Success message (optional but good for marks!)
                    TempData["SuccessMessage"] = "Booking updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingID)) { return NotFound(); }
                    else { throw; }
                }
            }

            // 4. IF WE GET HERE, SOMETHING FAILED (Validation errors)
            // Re-fill the dropdowns so the page doesn't break
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);

            return View(booking);

        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // ExcludeBookingId is supplied during Edit so that the record does not clash with itself.
        // Private: Verifies whether a venue is already reserved within the requested timeframe. 
        // Uses overlap logic: two ranges overlap when StartA < EndB AND EndA > StartB
        private bool HasDoubleBooking(int venueId, DateTime startDate, DateTime endDate, int? excludeBookingId = null)
        {
            return _context.Booking.Any(b =>
                b.VenueID == venueId &&
                (excludeBookingId == null || b.BookingID != excludeBookingId) &&
                b.StartDate < endDate &&
                b.EndDate > startDate);
        }

        // Helper - checks whether a booking with the given ID exists
        private bool BookingExists(int id)
        {
            return _context.Booking.Any(b => b.BookingID == id);
        }
    }
}
