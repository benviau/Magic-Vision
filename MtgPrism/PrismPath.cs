using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MtgPrism
{
    public static class PrismPath
    {
        public class Output
        {
            public static class Files
            {
                public static string ACTIVITY_LOG = HttpContext.Current.Server.MapPath("~/output/gatherer_scraper/logs/activity.txt");
            }
            public static class Folders
            {

                public static string NAME_IMAGE = HttpContext.Current.Server.MapPath("~/output/gatherer_scraper/name_image_pairs/");
            }
        }
        public class Search
        {
            public static string GATHERER_URL = "http://gatherer.wizards.com/Pages/Search/Default.aspx";
        }
    }
}
