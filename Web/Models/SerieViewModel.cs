using System.ComponentModel.DataAnnotations;
using Core.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class SerieViewModel
    {
        public int SerieId { get; set; }
              [Required(ErrorMessage = "A serie must have a name")]
        public string Name { get; set; }
              [Required(ErrorMessage = "You have to indicate your progress in the serie")]
              [Range(0, 1000, ErrorMessage = "A serie can't have that amount of episodes")]
        public int Episode { get; set; }
        [Range(1, 5, ErrorMessage = "Ratings go from 1 to 5")]
        public int Rating { get; set; }
        public StatusEnum Status { get; set; }
        public AirDayEnum Day { get; set; }
        [Required(ErrorMessage = "You have to indicate your progress in the serie")]
        [Range(0, 40, ErrorMessage = "A serie can't have that amount of seasons")]
        public int Season { get; set; }
         [Range(0, 300, ErrorMessage = "A serie can't have that time")]
        public int Time { get; set; }
    }
}