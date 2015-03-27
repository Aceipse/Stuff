using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.DomainModel;

namespace Web.Models
{
    public class ListsViewModel
    {
        public ListsViewModel()
        {
            this.Mangas = new HashSet<Manga>();
            this.Series = new HashSet<Serie>();
        }

        public int Id { get; set; }
        public string Owner { get; set; }
        public virtual ICollection<Manga> Mangas { get; set; }
        public virtual ICollection<Serie> Series { get; set; }
    }
}