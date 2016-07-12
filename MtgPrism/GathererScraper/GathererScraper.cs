using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScrapySharp.Core;
using ScrapySharp.Html.Parsing;
using ScrapySharp.Network;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Html.Forms;
using Newtonsoft.Json;
using System.IO;
using MtgPrism.GathererScraper.Data;

namespace MtgPrism.GathererScraper
{
    public class GathererScraper
    {
        public List<MtgSet> ScrapedSets;

        private ScrapingBrowser Browser;
        private Uri CurrentScrapingUrl;
        private List<string> RecentSetNames = new List<string>(){
                "Magic 2013",
                "Return to Ravnica",
                "Gatecrash",
                "Dragon's Maze",
                "Magic 2014 Core Set",
                "Theros",
                "Born of the Gods",
                "Journey into Nyx",
                "Magic 2015 Core Set",
                "Khans of Tarkir",
                "Fate Reforged",
                "Dragons of Tarkir",
                "Magic Origins",
                "Battle for Zendikar",
                "Oath of the Gatewatch",
                "Shadows over Innistrad"
        };

        public GathererScraper()
        {
            ScrapedSets = new List<MtgSet>();
            Browser = new ScrapingBrowser();
            Browser.AllowAutoRedirect = true; // Browser has many settings you can access in setup
            Browser.AllowMetaRedirect = true;
            Browser.IgnoreCookies = true;
        }

        public GathererScraperResult ScrapeSets()
        {
            var scraperResult = new GathererScraperResult();

            foreach (string setName in RecentSetNames)
            {
                ScrapeSet(setName, ref scraperResult);
            }

            return scraperResult;
        }

        public GathererScraperResult ScrapeSets(string[] setsToScrape)
        {
            var scraperResult = new GathererScraperResult();

            foreach (string setName in setsToScrape)
            {
                ScrapeSet(setName, ref scraperResult);
            }

            return scraperResult;
        }

        private void ScrapeSet(string setToScrape, ref GathererScraperResult scraperResult)
        {
            CurrentScrapingUrl = new Uri(PrismPath.Search.GATHERER_URL + "?set=[\"" + setToScrape + "\"]");


            var cardNameImagePairs = new Dictionary<string, string>();

            try
            {
                // abort scraping if we've done this set before
                var hasAlreadyScraped = File.Exists(PrismPath.Output.Folders.NAME_IMAGE + setToScrape + ".json");
                if (hasAlreadyScraped)
                {
                    throw new Exception("Already scraped " + setToScrape + ". Will not scrape this set again.");
                }

                // navigate to scraping page
                var pageToScrape = Browser.NavigateToPage(CurrentScrapingUrl);
                cardNameImagePairs = new Dictionary<string, string>();

                // scrape all result pages for card image urls
                while (pageToScrape != null)
                {
                    var cardsFromPage = GetCardNameImageFromPage(pageToScrape);

                    foreach (var cardFromPage in cardsFromPage)
                    {
                        cardNameImagePairs[cardFromPage.Key] = cardFromPage.Value;
                    }

                    pageToScrape = GetNextPage(pageToScrape);
                }

                // save to file
                SaveSetToFile(setToScrape, cardNameImagePairs);

                scraperResult.ScrapedSets.Add(setToScrape);
            }
            catch (Exception e)
            {
                scraperResult.Errors.Add(e.Message);
                LogError(e);
            }
        }

        private void SaveSetToFile(string setName, Dictionary<string, string> cardNameImagePairs)
        {
            if (cardNameImagePairs.Any()) { 
                var cardNameImageJson = JsonConvert.SerializeObject(cardNameImagePairs);

                File.WriteAllText(PrismPath.Output.Folders.NAME_IMAGE + setName + ".json", cardNameImageJson);
            }
            else
            {
                LogError(new Exception("No cardNameImagePairs found for set: \"" + setName + "\". Aborting saving this set."));
            }
        }

        private Dictionary<string, string> GetCardNameImageFromPage(WebPage pageToScrape)
        {
            var cardNameImagePairs = new Dictionary<string, string>();
            List<HtmlNode> cardStrips = pageToScrape.Html.CssSelect(".cardItem").ToList();

            foreach (HtmlNode cardStrip in cardStrips)
            {
                var image = cardStrip.CssSelect(".leftCol img").FirstOrDefault();
                var cardTitle = cardStrip.CssSelect(".middleCol .cardTitle a").FirstOrDefault();
                var imageRelativeUrl = image.Attributes["src"].Value;
                var imageAbsoluteUrl = GetAbsoluteUrlString(imageRelativeUrl);
                var title = cardTitle.InnerHtml;

                cardNameImagePairs.Add(title, imageAbsoluteUrl);
            };

            return cardNameImagePairs;
        }

        private WebPage GetNextPage(WebPage currentPage)
        {
            var pagingControls = currentPage.Html.CssSelect(".pagingcontrols>div").First();
            var nextPageUrl =
                pagingControls.LastChild.Attributes["href"] != null ?
                pagingControls.LastChild.Attributes["href"].Value : default(string);

            WebPage nextPage = null;

            if (nextPageUrl != default(string))
            {
                var nextPageAbsoluteUrl = GetAbsoluteUrlString(nextPageUrl);
                nextPage =
                    nextPageAbsoluteUrl != default(string) ?
                    Browser.NavigateToPage(new Uri(nextPageAbsoluteUrl)) : null;
            }

            return nextPage;
        }

        private string GetAbsoluteUrlString(string url)
        {
            var decodedUrl =
                System.Web.HttpUtility.HtmlDecode(
                    System.Web.HttpUtility.UrlDecode(url)
                );
            var uri = new Uri(decodedUrl, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
                uri = new Uri(CurrentScrapingUrl, uri);
            return uri.ToString();
        }

        private static void LogError(Exception ex)
        {
            var ErrorDate = new DateTime();

            File.WriteAllText(PrismPath.Output.Files.ACTIVITY_LOG, @"" +
                "========================================\n" +
                "Error Date: " + ErrorDate + "\n" +
                ex.Message + "\n" +
                "----------------------------------------\n" +
                "Exception Details: \n" +
                ex.StackTrace + "\n" +
                "========================================\n"
                );
        }
    }
}
