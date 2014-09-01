using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly IUnitOfWork _unitOfWork;

        public MangaController(IUnitOfWork unitOfWork, IGenericRepository<Manga> mangaRepository)
        {
            _unitOfWork = unitOfWork;
            _mangaRepository = mangaRepository;
        }

        //
        // GET: /Manga/
        public ActionResult Index()
        {
            var manga = _mangaRepository.Get();
            return View(manga);
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
                _mangaRepository.Insert(manga);
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
            var manga = _mangaRepository.GetByKey(id);
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
                var Manga = Mapper.Map<Manga>(mangaViewModel);
                _mangaRepository.Update(Manga);
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
            var manga = _mangaRepository.GetByKey(id);
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
                _mangaRepository.DeleteByKey(id);
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
            var listofMangas = _mangaRepository.Get();
            List<Manga> todaysshows = new List<Manga>();

            WebRequest request = WebRequest.Create("http://www.mangareader.net/");
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());

            string text = reader.ReadToEnd();
            text = text.ToLower();

            foreach (var manga in listofMangas)
            {
                if (text.Contains(manga.Name + " "))
                {
                    int index = text.IndexOf(manga.Name + " ") + manga.Name.Count() + 1;
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
            manga.Chapter++;
            manga.NewChapter = false;
            _mangaRepository.Update(manga);
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        public ActionResult Import()
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(ObservableCollection<Manga>));
            FileStream myStream = new FileStream("C:/Users/martin/Dropbox/Projects/MangaTracker/MangaTracker/bin/Debug/Mangas.txt", FileMode.Open, FileAccess.Read);

            StreamReader myReader = new StreamReader(myStream);
            ObservableCollection<Manga> savedMangas = new ObservableCollection<Manga>();

            savedMangas = (ObservableCollection<Manga>)mySerializer.Deserialize(myReader);

            var mangas = _mangaRepository.Get();
            foreach (var manga in mangas)
            {
                savedMangas.Add(manga);
            }
            ObservableCollection<Manga> newtemp = new ObservableCollection<Manga>(savedMangas.DistinctBy(x => x.Name));
            newtemp = new ObservableCollection<Manga>(newtemp.OrderBy(x => x.Name));

            for (int j = 1; j <= mangas.Count(); j++)
            {
                _mangaRepository.DeleteByKey(j);
            }

            for (int i = 1; i < newtemp.Count; i++)
            {
                _mangaRepository.Insert(newtemp[i]);
            }
            myStream.Close();
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
    }
}
