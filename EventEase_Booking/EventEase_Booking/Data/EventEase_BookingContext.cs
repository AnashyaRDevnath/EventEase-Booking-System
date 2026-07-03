using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventEase_Booking.Models;

namespace EventEase_Booking.Data
{
    public class EventEase_BookingContext : DbContext
    {
        public EventEase_BookingContext (DbContextOptions<EventEase_BookingContext> options)
            : base(options)
        {
        }

        public DbSet<EventEase_Booking.Models.Venue> Venue { get; set; } = default!;
        public DbSet<EventEase_Booking.Models.Event> Event { get; set; } = default!;
        public DbSet<EventEase_Booking.Models.Booking> Booking { get; set; } = default!;
        public DbSet<EventType> EventTypes { get; set; }
    }
}
