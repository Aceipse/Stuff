using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.DomainServices;
using Core.DomainModel;
using Microsoft.Ajax.Utilities;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGenericRepository<Test> _repo;
        private readonly IGenericRepository<Serie> _serieRepository;
        private readonly IGenericRepository<Manga> _mangaRepository; 
        public HomeController(IGenericRepository<Test> repo, IGenericRepository<Serie> serieRepository, IGenericRepository<Manga> mangaRepository)
        {
            _repo = repo;
            _serieRepository = serieRepository;
            _mangaRepository = mangaRepository;
        }

        public ActionResult Index()
        {
            var series =_serieRepository.Get().DistinctBy(s=>s.Name);
            var mangas = _mangaRepository.Get().DistinctBy(s => s.Name);

            var _namelist = new List<string>();
            var _manganamelist = new List<string>();
            
            foreach (Serie serie in series)
            {
                _namelist.Add(serie.Name);
            }
            foreach (Manga manga in mangas)
            {
                _manganamelist.Add(manga.Name);
            }
            Session["NameManga"] = _manganamelist;
            Session["NamesSerie"] = _namelist;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}