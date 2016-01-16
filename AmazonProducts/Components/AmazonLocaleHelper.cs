using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonProducts.Components
{
    /// <summary>
    /// Helper class for getting info about Amazon API locales
    /// </summary>
    public class AmazonLocaleHelper
    {
        //All possible locales
        public IEnumerable<LocaleInfo> LocaleInfos { get; private set; }

        /// <summary>
        /// Add all possible locales
        /// </summary>
        public AmazonLocaleHelper()
        {
            List<LocaleInfo> list = new List<LocaleInfo>();

            list.Add(new LocaleInfo()
            {
                locale = "BR",
                httpEndpoint = "http://webservices.amazon.com.br/onca/xml",
                httpsEndpoint = "https://webservices.amazon.com.br/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "CA",
                httpEndpoint = "http://webservices.amazon.ca/onca/xml",
                httpsEndpoint = "https://webservices.amazon.ca/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "CN",
                httpEndpoint = "http://webservices.amazon.cn/onca/xml",
                httpsEndpoint = "https://webservices.amazon.cn/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "DE",
                httpEndpoint = "http://webservices.amazon.de/onca/xml",
                httpsEndpoint = "https://webservices.amazon.de/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "ES",
                httpEndpoint = "http://webservices.amazon.es/onca/xml",
                httpsEndpoint = "https://webservices.amazon.es/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "FR",
                httpEndpoint = "http://webservices.amazon.fr/onca/xml",
                httpsEndpoint = "https://webservices.amazon.fr/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "IN",
                httpEndpoint = "http://webservices.amazon.in/onca/xml",
                httpsEndpoint = "https://webservices.amazon.in/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "IT",
                httpEndpoint = "http://webservices.amazon.it/onca/xml",
                httpsEndpoint = "https://webservices.amazon.it/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "JP",
                httpEndpoint = "http://webservices.amazon.co.jp/onca/xml",
                httpsEndpoint = "https://webservices.amazon.co.jp/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "MX",
                httpEndpoint = "http://webservices.amazon.mx/onca/xml",
                httpsEndpoint = "https://webservices.amazon.mx/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "UK",
                httpEndpoint = "http://webservices.amazon.co.uk/onca/xml",
                httpsEndpoint = "https://webservices.amazon.co.uk/onca/xml"
            });
            list.Add(new LocaleInfo()
            {
                locale = "US",
                httpEndpoint = "http://webservices.amazon.com/onca/xml",
                httpsEndpoint = "https://webservices.amazon.com/onca/xml"
            });
            LocaleInfos = list;
        }

        /// <summary>
        /// Get locale info for given code
        /// </summary>
        /// <param name="locale"></param>
        /// <returns></returns>
        public LocaleInfo GetLocaleInfo(string locale)
        {
            return LocaleInfos.FirstOrDefault(x => x.locale == locale);
        }

        public struct LocaleInfo
        {
            public string locale;
            public string httpEndpoint;
            public string httpsEndpoint;
        }
    }
}