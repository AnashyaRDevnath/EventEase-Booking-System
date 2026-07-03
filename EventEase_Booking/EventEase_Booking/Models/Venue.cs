using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace EventEase_Booking.Models
{
    public class Venue
    {
        [Key]
        public int VenueID { get; set; }
        
        
        [Required(ErrorMessage = "Venue name is required.")]
        [StringLength(100, ErrorMessage = "Venue name cannot exceed 100 characters.")]
        [Display(Name = "Venue Name")]
       
        /* 
     ========================Code Attribution====================
     System.ComponentModel.Dara Annotations Namespace
     Author: Microsoft
     Link: https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-10.0&redirectedfrom=MSDN 
     Date accessed: 06 May 2026
     ========================Code Attribution====================
      */
        public string VenueName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;


        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1 person.")]
        public int Capacity { get; set; }

        // Holds the venue image's Azurite blob URL (or placeholder path).
        // Not manually entered by the user; set by the controller following upload.
        // 500 characters is the maximum to allow for large blob URLs.
        [Display(Name = "Image")]
        [StringLength(500)]
        public string? ImageUrl { get; set; } = "/images/placeholder-venue.jpg";

        // Used only to accept the uploaded file from the form; it is not mapped to the database.After reading this, the controller uploads it to Azurite and saves the URL that is returned in ImageUrl.
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        // This allows the Venue to "see" its related bookings
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
