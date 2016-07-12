using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgPrism.GathererScraper.Data
{
    public class GathererScraperResult
    {
        public List<string> ScrapedSets;
        public List<string> Errors;

        public GathererScraperResult()
        {
            ScrapedSets = new List<string>();
            Errors = new List<string>();
        }
    }
}
