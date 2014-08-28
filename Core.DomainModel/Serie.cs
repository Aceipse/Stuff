using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public class Serie
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

    public enum StatusEnum
    {
        Airing,
        Completed,
        NA
    }

    public enum AirDayEnum
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday,
        NA
    }
}
