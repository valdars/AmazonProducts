using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AmazonProducts.Components
{
    /// <summary>
    /// Helper class for getting currency conversion rate
    /// </summary>
    public static class CurrencyHelper
    {
        /// <summary>
        /// Get currency conversion rate using Yahoo API
        /// </summary>
        /// <param name="fromCurrency">base currency</param>
        /// <param name="toCurrency">target currency</param>
        /// <returns></returns>
        public static async Task<decimal> GetRate(string fromCurrency, string toCurrency)
        {
            //unique key for cache
            var key = String.Format("Currency:{0}>{1}", fromCurrency, toCurrency);

            var cache = MemoryCache.Default;
            if(cache.Contains(key)) {
                return (decimal)cache.Get(key);
            }

            //Try to get rates from yahoo API
            string url = String.Format("http://query.yahooapis.com/v1/public/yql?q=select * from yahoo.finance.xchange where pair in (\"{0}\")&format=json&env=store://datatables.org/alltableswithkeys", fromCurrency + toCurrency);
            string jsonResult = null;
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        jsonResult = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception){}
            }

            if (jsonResult != null)
            {
                //Parse rate out of json and add it to cache and then return
                var results = new Dictionary<string, decimal>();
                var jObject = (dynamic)JObject.Parse(jsonResult);
                decimal rate = Convert.ToDecimal(jObject.query.results.rate.Rate);
                cache.Add(key, rate, DateTimeOffset.Now.AddHours(1));
                return rate;
            }

            return 0;
        }
    }
}