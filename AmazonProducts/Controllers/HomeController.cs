using AmazonProducts.Components;
using AmazonProducts.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace AmazonProducts.Controllers
{
    public class HomeController : Controller
    {
        //Amazon API namespace
        private XNamespace ns;

        /// <summary>
        /// Page for searching Amazon products
        /// </summary>
        /// <param name="keywords">keywords for searching in Amazon API</param>
        /// <returns></returns>
        public async Task<ActionResult> Index(string keywords)
        {
            //Set culture to invariant to format numbers correctly
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            //List of currencies to show price in
            var currencies = new List<string>() { "EUR", "USD", "GBP" };
            string defaultCurrency;
            var modelList = new List<ProductViewModel>();
            ns = AmazonItems.NAMESPACE;

            //If nonempty search was submitted
            if (!string.IsNullOrEmpty(keywords))
            {
                var tuple = await GetAmazonData(keywords);
                var items = tuple.Item2;
                ViewData["Count"] = tuple.Item1;

                //construct viewmodels from xelements
                foreach (var item in items)
                {
                    var title = item.Descendants(ns + "Title").First().Value;
                    var link = item.Descendants(ns + "DetailPageURL").First().Value;
                    //Images can not exist
                    var images = item.Descendants(ns + "LargeImage").Select(x => x.Descendants(ns + "URL").First().Value);
                    var descriptionEl = item.Descendants(ns + "EditorialReview").FirstOrDefault();
                    var description = "";

                    //Description is optional
                    if (descriptionEl != null)
                    {
                        description = descriptionEl.Descendants(ns + "Content").First().Value;
                    }

                    //Get prices for all types
                    var prices = new List<Price>();
                    foreach (var priceType in new string[] { "LowestNewPrice", "LowestUsedPrice", "LowestRefurbishedPrice", "LowestCollectiblePrice" })
                    {
                        var elements = item.Descendants(ns + priceType);
                        if (elements.Count() > 0)
                        {
                            var element = elements.First();
                            prices.Add(new Price()
                            {
                                price = Convert.ToDecimal(element.Descendants(ns + "Amount").First().Value) / 100,
                                currencyCode = element.Descendants(ns + "CurrencyCode").First().Value
                            });
                        }
                    }

                    //Construct viewmodel
                    modelList.Add(new ProductViewModel()
                    {
                        Title = title,
                        Link = link,
                        Images = images,
                        Description = description,
                        Prices = prices
                    });
                }

                //All products of one locale use same currencycode
                defaultCurrency = modelList.First().Prices.First().currencyCode;
            }
            else
            {
                defaultCurrency = currencies.First();
            }

            if (!currencies.Contains(defaultCurrency))
            {
                currencies.Add(defaultCurrency);
            }

            ViewData["Currencies"] = currencies;
            ViewData["DefaultCurrency"] = defaultCurrency;
            ViewData["Keywords"] = keywords;

            return View(modelList);
        }

        /// <summary>
        /// Try to get all items for given keywords from Amazon API
        /// </summary>
        /// <param name="keywords">search keywords</param>
        /// <returns>List of XElements for found products</returns>
        private async Task<Tuple<int, IEnumerable<XElement>>> GetAmazonData(string keywords)
        {
            var items = new List<XElement>();
            var cache = MemoryCache.Default;
            var cacheKey = "Amazon:" + keywords;

            if (cache.Contains(cacheKey))
            {
                return (Tuple<int, IEnumerable<XElement>>)cache.Get(cacheKey);
            }

            //Get config
            var accessKeyId = ConfigurationManager.AppSettings["Amazon.AccessKeyId"];
            var secretKey = ConfigurationManager.AppSettings["Amazon.SecretKey"];
            var associateTag = ConfigurationManager.AppSettings["Amazon.AssociateTag"];
            var localeCode = ConfigurationManager.AppSettings["Amazon.LocaleCode"];

            var localeinfo = new AmazonLocaleHelper().GetLocaleInfo(localeCode);
            var aws = new AmazonItems(accessKeyId, secretKey, associateTag, localeinfo.httpEndpoint);

            //Build base request
            var request = new Dictionary<string, string>();
            request["ItemSearch.Shared.SearchIndex"] = "All";
            request["ItemSearch.Shared.Keywords"] = keywords;
            request["ItemSearch.Shared.ResponseGroup"] = "EditorialReview,Small,Images,OfferSummary";

            //Get initial data
            var result = await aws.Get(request);


            //Try to get total count of results
            var countEl = result.Descendants(ns + "TotalResults");
            //Lack of element indicates error
            if (!countEl.Any())
            {
                return null;
            }
            int count = Convert.ToInt32(countEl.First().Value);
            items.AddRange(result.Descendants(ns + "Item"));

            //Try to get rest of results(max 5 pages)
            request.Remove("Signature");
            if (count > 20)
            {

                request["ItemSearch.1.ItemPage"] = "2";
                request["ItemSearch.2.ItemPage"] = "3";
                result = await aws.Get(request);

                items.AddRange(result.Descendants(ns + "Item"));
            }
            else if (count > 10)
            {
                request["ItemSearch.1.ItemPage"] = "2";
                result = await aws.Get(request);

                items.AddRange(result.Descendants(ns + "Item"));
            }

            request.Remove("Signature");
            if (count > 40)
            {
                request["ItemSearch.1.ItemPage"] = "4";
                request["ItemSearch.2.ItemPage"] = "5";
                result = await aws.Get(request);

                items.AddRange(result.Descendants(ns + "Item"));
            }
            else if (count > 30)
            {
                request["ItemSearch.1.ItemPage"] = "4";
                request.Remove("ItemSearch.2.ItemPage");
                result = await aws.Get(request);

                items.AddRange(result.Descendants(ns + "Item"));
            }

            var tuple = Tuple.Create<int, IEnumerable<XElement>>(count, items);
            cache.Add(cacheKey, tuple, DateTimeOffset.Now.AddHours(1));
            return tuple;
        }

        /// <summary>
        /// AJAX request for getting conversion rate between two currencies
        /// </summary>
        /// <param name="fromCurrency">Base currency</param>
        /// <param name="toCurrency">Target currency</param>
        /// <returns>JSON of result</returns>
        public async Task<ActionResult> GetCurrencyRate(string fromCurrency, string toCurrency)
        {
            var rate = await CurrencyHelper.GetRate(fromCurrency, toCurrency);
            return Json(new {
                From = fromCurrency,
                To = toCurrency,
                Rate = rate,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}