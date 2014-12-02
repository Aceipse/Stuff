using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using AutoMapper;
using Core.DomainModel;
using Core.DomainServices;
using Microsoft.Ajax.Utilities;
using Web.Models;

namespace Web.Controllers
{

    public class MangaController : Controller
    {
        private readonly IGenericRepository<Manga> _mangaRepository;
        private readonly IGenericRepository<Lists> _listRepository; 
        private readonly IUnitOfWork _unitOfWork;
        private Lists list;

        public MangaController(IUnitOfWork unitOfWork, IGenericRepository<Manga> mangaRepository, IGenericRepository<Lists> listRepository)
        {
            _unitOfWork = unitOfWork;
            _mangaRepository = mangaRepository;
            _listRepository = listRepository;
        }

        //
        // GET: /Manga/
        public ActionResult Index(string sortOrder)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.NewChaperSortParm = sortOrder == "new_chapter" ? "new_chapter_desc" : "new_chapter";

            try
            {
                list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            }
            catch (System.InvalidOperationException)
            {

                list = new Lists();
                list.Owner = (string)Session["FacebookId"];
                _listRepository.Insert(list);
                _unitOfWork.Save();
            }
            ListsViewModel listVM = Mapper.Map<ListsViewModel>(list);

            IOrderedEnumerable<Manga> mangas;
            switch (sortOrder)
            {
                case "name_desc":
                    mangas = listVM.Mangas.OrderByDescending(s=>s.Name);
                    break;
                case "new_chapter":
                    mangas = listVM.Mangas.OrderBy(s => s.NewChapter);
                    break;
                case "new_chapter_desc":
                    mangas = listVM.Mangas.OrderByDescending(s => s.NewChapter);
                    break;
                default:
                    mangas = listVM.Mangas.OrderBy(s => s.Name);
                    break;
            }
            return View(mangas);
        }

        //
        // GET: /Manga/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Manga/Create
        public ActionResult Create()
        {
            var manga = new Manga();
            var mangaViewModel = Mapper.Map<MangaViewModel>(manga);
            return View(mangaViewModel);
        }

        //
        // POST: /Manga/Create
        [HttpPost]
        public ActionResult Create(MangaViewModel mangaViewModel)
        {
            try
            {
                var manga = Mapper.Map<Manga>(mangaViewModel);
                list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
                list.Mangas.Add(manga);
                _listRepository.Update(list);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Manga/Edit/5
        public ActionResult Edit(int id)
        {
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            Manga manga = list.Mangas.Single(s => s.MangaId == id);
            var mangaViewModel = Mapper.Map<MangaViewModel>(manga);
            return View(mangaViewModel);
        }

        //
        // POST: /Manga/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, MangaViewModel mangaViewModel)
        {
            try
            {
                var manga = Mapper.Map<Manga>(mangaViewModel);
                _mangaRepository.Update(manga);
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Manga/Delete/5
        public ActionResult Delete(int id)
        {
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            var manga = list.Mangas.Single(s => s.MangaId == id);
            var mangaViewModel = Mapper.Map<MangaViewModel>(manga);
            return View(mangaViewModel);
        }

        //
        // POST: /Manga/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, MangaViewModel mangaViewModel)
        {
            try
            {
                list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
                var tempmanga = list.Mangas.Single(s => s.MangaId == id);
                _mangaRepository.DeleteByKey(tempmanga.MangaId);
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Check()
        {
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            var listofMangas =list.Mangas;
            List<Manga> todaysshows = new List<Manga>();

            WebRequest request = WebRequest.Create("http://www.mangareader.net/");
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());

            string text = reader.ReadToEnd();
           
            foreach (var manga in listofMangas)
            {
                if (text.Contains(manga.Name + " "))
                {
                    int index = text.IndexOf(manga.Name + " ") + (manga.Name.Count() + 1);
                    int index2 = text.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }, index, 10);
                    if (index2 == -1)
                    {
                        index2 = index;
                    }
                    string temp = text.Substring(index2, 3);
                    temp = temp.Replace("<", "");
                    temp = temp.Replace("\"", "");
                    temp = temp.Replace("/", "");
                    try
                    {
                        if (Convert.ToInt32(temp) > manga.Chapter)
                        {
                            manga.NewChapter = true;
                            _mangaRepository.Update(manga);
                            _unitOfWork.Save();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

            }


            return RedirectToAction("Index");
        }

        public ActionResult AddOne(Manga manga)
        {
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            list.Mangas.Single(s => s.MangaId == manga.MangaId).Chapter++;
            list.Mangas.Single(s => s.MangaId == manga.MangaId).NewChapter = false;
            _mangaRepository.Update(list.Mangas.Single(s => s.MangaId == manga.MangaId));
            _unitOfWork.Save();

            return RedirectToAction("Index", new { sortOrder = "new_chapter_desc" });
        }

        public ActionResult Import()
        {
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            XmlSerializer mySerializer = new XmlSerializer(typeof(ObservableCollection<Manga>));
            FileStream myStream = new FileStream("C:/Users/martin/Dropbox/Projects/MangaTracker/MangaTracker/bin/Debug/Mangas.txt", FileMode.Open, FileAccess.Read);

            StreamReader myReader = new StreamReader(myStream);
            ObservableCollection<Manga> savedMangas = new ObservableCollection<Manga>();

            savedMangas = (ObservableCollection<Manga>)mySerializer.Deserialize(myReader);

            var mangas = list.Mangas;
            foreach (var manga in mangas)
            {
                savedMangas.Add(manga);
            }
            ObservableCollection<Manga> newtemp = new ObservableCollection<Manga>(savedMangas.DistinctBy(x => x.Name));
            newtemp = new ObservableCollection<Manga>(newtemp.OrderBy(x => x.Name));

            while (mangas.Count != 0)
            {
                _mangaRepository.DeleteByKey(mangas.First().MangaId);
            }

            for (int i = 1; i < newtemp.Count; i++)
            {
                list.Mangas.Add(newtemp[i]);
            }
            myStream.Close();
            _listRepository.Update(list);
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
    }
}
