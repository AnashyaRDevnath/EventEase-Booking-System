using System.ComponentModel.DataAnnotations;

namespace EventEase_Booking.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }
        [Required]
        [Display(Name = "Event Name")]

        /* 
     ========================Code Attribution====================
     System.ComponentModel.Dara Annotations Namespace
     Author: Microsoft
     Link: https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-10.0&redirectedfrom=MSDN 
     Date accessed: 06 May 2026
      ========================Code Attribution====================
      */

        public string EventName { get; set; }
        public string ImageURL { get; set; }
        public DateTime EventDate { get; set; }

        //For the availability filter
        public bool IsAvailable { get; set; }

        //To link to the new EventType table
        public int? EventTypeId { get; set; }
        public EventType? EventType { get; set; }



    }
}
