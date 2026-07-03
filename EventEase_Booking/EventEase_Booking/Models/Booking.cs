using System.ComponentModel.DataAnnotations;

namespace EventEase_Booking.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        //Foreign key linking to venue

        [Required]
        [Display(Name = "Venue")]
        /* 
      ========================Code Attribution====================
      System.ComponentModel.Dara Annotations Namespace
      Author: Microsoft
      Link: https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-10.0&redirectedfrom=MSDN 
      Date accessed: 06 May 2026
       ========================Code Attribution====================
       */
        public int VenueID { get; set; }


        public virtual Venue? Venue { get; set; }    //(Gemini, 2026)

        //Foreign key linking to event
        [Required]
        [Display(Name = "Event")]
        public int EventID { get; set; }

        public virtual Event? Event { get; set; }  //(Gemini, 2026)

        [Required(ErrorMessage = "Please select a start date and time.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }


        [Required(ErrorMessage = "Please select an end date and time.")]
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

    }
}

//Reference List
//Gemini (2026) Gemini 3 Flash (April 14 version). [Generative AI]. Available at: https://gemini.google.com/app/60af487d9a06f8e1?utm_source=app_launcher&utm_medium=owned&utm_campaign=base_all  //[Accessed 14 April 2026].Gemini (2026) Gemini 3 Flash (April 14 version). [Generative AI]. Available at: https://gemini.google.com/ [Accessed 14 April 2026].