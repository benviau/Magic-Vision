using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MtgLibrarian.Models;
using MtgPrism.GathererScraper;

namespace MtgLibrarian.Controllers
{
    public class GathererScraperController : Controller
    {
        public GathererScraper _gathererScraper = new GathererScraper();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Scrape(FormCollection formCollection)
        {
            var setsToScrape = formCollection["setsToScrape"].Split(',');


            var scrapingResult = _gathererScraper.ScrapeSets();

            return View(scrapingResult);
        }
    }
}