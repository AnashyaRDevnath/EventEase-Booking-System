using EventEase_Booking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using EventEase_Booking.Data;

namespace EventEase_Booking.Controllers
{
    public class HomeController : Controller
    {
        //Give the controller a pipeline to your database
        private readonly EventEase_BookingContext _context;

        public HomeController(EventEase_BookingContext context)
        {
            _context = context;
        }

        // Upgrade the method to 'async Task' so it can 'await' the database
        public async Task<IActionResult> Index()
        {
            // Count how many events and bookings exist in the cloud database
            ViewBag.TotalEvents = await _context.Event.CountAsync();
            ViewBag.TotalBookings = await _context.Booking.CountAsync();
            ViewBag.TotalVenues = await _context.Venue.CountAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
