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
        public string Name { get; set; }
        public int Afsnit { get; set; }
        public int Rating { get; set; }
        public StatusEnum Status { get; set; }
        public AirDayEnum Day { get; set; }
        public int Season { get; set; }
        public int Time { get; set; }
    }
}