using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.DomainModel;

namespace Web.Models
{
    public class MangaViewModel
    {
        public int MangaId { get; set; }
        public string Name { get; set; }
        public int Chapter { get; set; }
        public StatusEnum Status { get; set; }
        public int Rating { get; set; }
        public bool NewChapter { get; set; }
    }
}