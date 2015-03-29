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
        private readonly IGenericRepository<Lists> _listRepository;
        private readonly IGenericRepository<Serie> _serieRepository;
        private readonly IUnitOfWork _unitOfWork;
        private Lists list;
        
        public SerieController(IUnitOfWork unitOfWork, IGenericRepository<Lists> listRepository, IGenericRepository<Serie> serieRepository, List<string> namelist)
        {
            _unitOfWork = unitOfWork;
            _listRepository = listRepository;
            _serieRepository = serieRepository;
        }

        // GET: Serie
        public ActionResult Index()
        {
            if ((string)Session["FacebookId"] == null || (string)Session["FacebookId"] == "")
            {
                return RedirectToAction("Login", "Login");
            }
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
            return View(listVM.Series.OrderBy(s=>s.Name));
        }

        // GET: Serie/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Serie/Create
        public ActionResult Create()
        {
            if ((string)Session["FacebookId"] == null || (string)Session["FacebookId"] == "")
            {
                return RedirectToAction("Login", "Login");
            }
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
                list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
                list.Series.Add(serie);
                _listRepository.Update(list);
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
            if ((string)Session["FacebookId"] == null || (string)Session["FacebookId"] == "")
            {
                return RedirectToAction("Login", "Login");
            }
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            Serie serie = list.Series.Single(s => s.SerieId == id);
            var serieViewModel = Mapper.Map<SerieViewModel>(serie);
            return View(serieViewModel);
        }

        // POST: Serie/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, SerieViewModel serieViewModel)
        {
            try
            {
                list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
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
            if ((string)Session["FacebookId"] == null || (string)Session["FacebookId"] == "")
            {
                return RedirectToAction("Login", "Login");
            }
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            var serie = list.Series.Single(s => s.SerieId == id);
            var serieViewModel = Mapper.Map<SerieViewModel>(serie);
            return View(serieViewModel);
        }

        // POST: Serie/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, SerieViewModel serieViewModel)
        {
            try
            {
                list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
                var tempserie = list.Series.Single(s => s.SerieId == id);
                _serieRepository.DeleteByKey(tempserie.SerieId);
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
            if ((string)Session["FacebookId"] == null || (string)Session["FacebookId"] == "")
            {
                return RedirectToAction("Login", "Login");
            }
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            var listofSeries = list.Series;
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
                            Serie newEp = new Serie();
                            newEp = serie;
                            newEp.Episode = Convert.ToInt32(temp);
                            todaysshows.Add(newEp);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (text.Contains(serie.Name.ToLower() + " - season " + (serie.Season + 1) + " episode "))
                {
                    int index = text.IndexOf(serie.Name.ToLower() + " - season " + (serie.Season + 1) + " episode ") + serie.Name.Count() + serie.Season.ToString().Count() + 19;
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
                        Serie newEp = new Serie();
                        newEp = serie;
                        newEp.Episode = Convert.ToInt32(temp);
                        newEp.Season = (serie.Season + 1);
                        todaysshows.Add(newEp);

                    }
                    catch (Exception)
                    {
                    }
                }

            }


            return View(todaysshows);
        }

        public ActionResult SeenOne(Serie serie)
        {
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            list.Series.Single(s => s.SerieId == serie.SerieId).Episode = serie.Episode;
            list.Series.Single(s => s.SerieId == serie.SerieId).Season = serie.Season;
            _listRepository.Update(list);
            _unitOfWork.Save();

            return RedirectToAction("Check");
        }

        public ActionResult Import()
        {
            list = _listRepository.Get().Single(s => s.Owner == (string)Session["FacebookId"]);
            XmlSerializer mySerializer = new XmlSerializer(typeof(ObservableCollection<Serie>));
            FileStream myStream = new FileStream("C:/Users/martin/Dropbox/Projects/SerieTracker/SerieTracker/bin/Debug/Series.txt", FileMode.Open, FileAccess.Read);

            StreamReader myReader = new StreamReader(myStream);
            ObservableCollection<Serie> savedSeries = new ObservableCollection<Serie>();

            savedSeries = (ObservableCollection<Serie>)mySerializer.Deserialize(myReader);

            var series = list.Series;
            foreach (var serie in series)
            {
                savedSeries.Add(serie);
            }
            ObservableCollection<Serie> newtemp = new ObservableCollection<Serie>(savedSeries.DistinctBy(x => x.Name));
            newtemp = new ObservableCollection<Serie>(newtemp.OrderBy(x => x.Name));

            while (series.Count != 0)
            {
                _serieRepository.DeleteByKey(series.First().SerieId);
            }

            for (int i = 0; i < newtemp.Count; i++)
            {
                list.Series.Add(newtemp[i]);
            }
            myStream.Close();
            _listRepository.Update(list);
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
    }
}
