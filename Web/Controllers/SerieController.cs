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

namespace Web.Models
{
    public class SerieController : Controller
    {
        private readonly IGenericRepository<Serie> _serieRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SerieController(IGenericRepository<Serie> serieRepository, IUnitOfWork unitOfWork)
        {
            _serieRepository = serieRepository;
            _unitOfWork = unitOfWork;
        }

        // GET: Serie
        public ActionResult Index()
        {
            var serie = _serieRepository.Get();
            return View(serie);
        }

        // GET: Serie/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Serie/Create
        public ActionResult Create()
        {
            var serie = new Serie();
            var serieViewModel = Mapper.Map<SerieViewModel>(serie);
            return View(serieViewModel);
        }

        // POST: Serie/Create
        [HttpPost]
        public ActionResult Create(SerieViewModel serieViewModel)
        {
            try
            {
                var serie = Mapper.Map<Serie>(serieViewModel);
                _serieRepository.Insert(serie);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Serie/Edit/5
        public ActionResult Edit(int id)
        {
            var serie = _serieRepository.GetByKey(id);
            var serieViewModel = Mapper.Map<SerieViewModel>(serie);
            return View(serieViewModel);
        }

        // POST: Serie/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, SerieViewModel serieViewModel)
        {
            try
            {
                var serie = Mapper.Map<Serie>(serieViewModel);
                _serieRepository.Update(serie);
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Serie/Delete/5
        public ActionResult Delete(int id)
        {
            var serie = _serieRepository.GetByKey(id);
            var serieViewModel = Mapper.Map<SerieViewModel>(serie);
            return View(serieViewModel);
        }

        // POST: Serie/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, SerieViewModel serieViewModel)
        {
            try
            {
                _serieRepository.DeleteByKey(id);
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
            var listofSeries = _serieRepository.Get();
            List<Serie> todaysshows = new List<Serie>();

            WebRequest request = WebRequest.Create("http://www.free-tv-video-online.me/internet/index_last_7_days.html");
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());

            string text = reader.ReadToEnd();
            text = text.ToLower();
            
            foreach (var serie in listofSeries)
            {
                if ((text.Contains(serie.Name.ToLower() + " - season " + serie.Season + " episode ")))
                {
                    int index = text.IndexOf(serie.Name.ToLower() + " - season " + serie.Season + " episode ") + serie.Name.Count() + serie.Season.ToString().Count() + 19;
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
                        if (Convert.ToInt32(temp) > serie.Episode)
                        {
                            todaysshows.Add(serie);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (text.Contains(serie.Name.ToLower() + " - season " + (serie.Season + 1) + " episode 1"))
                {
                    todaysshows.Add(serie);
                }

            }


            return View(todaysshows);
        }

        public ActionResult SeenOne(Serie serie)
        {
            serie.Episode++;
            _serieRepository.Update(serie);
            _unitOfWork.Save();

            return RedirectToAction("Check");
        }

        public ActionResult Import()
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(ObservableCollection<Serie>));
            FileStream myStream = new FileStream("C:/Users/martin/Dropbox/Projects/SerieTracker/SerieTracker/bin/Debug/Series.txt", FileMode.Open, FileAccess.Read);

            StreamReader myReader = new StreamReader(myStream);
            ObservableCollection<Serie> savedSeries = new ObservableCollection<Serie>();

            savedSeries = (ObservableCollection<Serie>)mySerializer.Deserialize(myReader);

            var  series = _serieRepository.Get();
            foreach (var serie in series)
            {
                savedSeries.Add(serie);
            }
            ObservableCollection<Serie> newtemp = new ObservableCollection<Serie>(savedSeries.DistinctBy(x => x.Name));
            newtemp = new ObservableCollection<Serie>(newtemp.OrderBy(x => x.Name));

            for (int j = 1; j <= series.Count(); j++)
            {
                _serieRepository.DeleteByKey(j);
            }

            for (int i = 1; i < newtemp.Count; i++)
            {
                _serieRepository.Insert(newtemp[i]);
            }
            myStream.Close();
            _unitOfWork.Save();
            
            return RedirectToAction("Index");
        }
    }
}
