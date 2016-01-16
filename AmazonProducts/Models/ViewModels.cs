using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace AmazonProducts.Models
{

    public class ProductViewModel
    {
        public string Title { get; set; }

        public string Link { get; set; }

        public string Description { get; set; }

        public IEnumerable<string> Images { get; set; }

        public IEnumerable<Price> Prices { get; set; }

        public decimal LowestPrice
        {
            get
            {
                if(Prices == null || !Prices.Any())
                {
                    return 0;
                }
                return Prices.Min(x => x.price);
            }
        }
    }

    public struct Price
    {
        public string type;

        public decimal price;

        public string currencyCode;
    }
}