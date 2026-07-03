using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase_Booking.Data;
using EventEase_Booking.Models;
using EventEase_Booking.Services;

namespace EventEase_Booking.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEase_BookingContext _context;
        private readonly BlobService _blobService;

        public VenuesController(EventEase_BookingContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;

        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueID,VenueName,Location,Capacity,ImageFile")] Venue venue)
        {
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null && venue.ImageFile.Length > 0)
                    {
                        // Upload image to Azurite and store the returned blob URL
                        venue.ImageUrl = await _blobService.UploadImageAsync(venue.ImageFile);
                    }
                    else
                    {
                        // Fall back to the default placeholder if no file was uploaded
                        venue.ImageUrl = "/images/placeholder-venue.jpg";
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    // Validation errors from BlobService (file type, size, etc.)
                    ModelState.AddModelError("ImageFile", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    // Azurite connection or upload errors
                    ModelState.AddModelError("", $"Image upload failed: {ex.Message}");
                }
            }
            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueID,VenueName,Location,Capacity,ImageURL")] Venue venue)
        {
            if (id != venue.VenueID) return NotFound();

            ModelState.Remove("ImageFile"); // Remove file from basic validation

            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null && venue.ImageFile.Length > 0)
                    {
                        // 1. Delete the old image from Azurite first
                        await _blobService.DeleteImageAsync(venue.ImageUrl);

                        // 2. Upload the new image
                        venue.ImageUrl = await _blobService.UploadImageAsync(venue.ImageFile);
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Update failed: {ex.Message}");
                }
            }
            return View(venue);

        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound(); //graceful exit for missing ID
            }

            var venue = await _context.Venue
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null)
            {
                return NotFound(); //graceful exit for ID that doesnt exist in DB
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Load the venue WITH its bookings
            var venue = await _context.Venue
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(m => m.VenueID == id);

            if (venue == null)
            {
                return NotFound();
            }

            // 2. VALIDATION: Block deletion if active bookings exist
            if (venue.Bookings.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete '{venue.VenueName}' - it has {venue.Bookings.Count} active booking(s). " +
                                            "Please remove all linked bookings before deleting this venue.";

                // Redirect back to the Delete view so the "Blocked State" is shown
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            // 3. STORAGE CLEANUP: Delete the image from Azure if it exists
            if (!string.IsNullOrEmpty(venue.ImageUrl) && venue.ImageUrl.StartsWith("http://127.0.0.1"))
            {
                await _blobService.DeleteImageAsync(venue.ImageUrl);
            }

            // 4. DATABASE DELETION
            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Venue '{venue.VenueName}' was deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueID == id);
        }
    }
}
