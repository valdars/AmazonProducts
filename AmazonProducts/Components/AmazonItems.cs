using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace AmazonProducts.Components
{
    /// <summary>
    /// Amazon product API
    /// </summary>
    public class AmazonItems
    {
        public const string NAMESPACE = "http://webservices.amazon.com/AWSECommerceService/2011-08-01";

        private string accessKeyId;
        private string secretKey;
        private string associateTag;
        private string endpoint;

        /// <summary>
        /// API error for last request
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Query string used for signing
        /// </summary>
        public string CanonicalizedQueryString { get; private set; }

        /// <summary>
        /// Final url of last request
        /// </summary>
        public string Url { get; private set; }

        public AmazonItems(string accessKeyId_, string secretKey_, string associateTag_, string endpoint_)
        {
            accessKeyId = accessKeyId_;
            secretKey = secretKey_;
            associateTag = associateTag_;
            endpoint = endpoint_;
        }

        /// <summary>
        /// Do request to API 
        /// </summary>
        /// <param name="args">Request arguments</param>
        /// <returns>XDocument of result XML or null</returns>
        public async Task<XDocument> Get(Dictionary<string, string> args)
        {
            Error = null;

            var request = CreateRequest(args);
            var doc = await Request(request);
            return doc;
        }

        /// <summary>
        /// Creates API URL for given request arguments
        /// </summary>
        /// <param name="args">Request arguments</param>
        /// <returns>URL to use in API request</returns>
        public string CreateRequest(Dictionary<string, string> args)
        {
            //Add all required parameters
            if (!args.ContainsKey("Service"))
            {
                args["Service"] = "AWSECommerceService";
            }

            if (!args.ContainsKey("Operation"))
            {
                args["Operation"] = "ItemSearch";
            }

            if (!args.ContainsKey("Timestamp"))
            {
                args["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }

            if (!args.ContainsKey("AWSAccessKeyId"))
            {
                args["AWSAccessKeyId"] = accessKeyId;
            }

            if (!args.ContainsKey("AssociateTag"))
            {
                args["AssociateTag"] = associateTag;
            }

            args["Signature"] = CreateSignature(args);

            //Construct query part of URL from arguments
            var query = String.Join("&", args.Select(x => x.Key + "=" + Encode(x.Value)));

            return endpoint + "?" + query;
        }

        /// <summary>
        /// Create request signature
        /// </summary>
        /// <param name="args">Request parameters to sign</param>
        /// <returns>Signature string for request</returns>
        public string CreateSignature(Dictionary<string, string> args)
        {
            //Arguments must be sorted by char value
            SortedDictionary<string, string> sortedArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach(var item in args)
            {
                //Encode and sort arguments
                sortedArgs.Add(Encode(item.Key), Encode(item.Value));
            }

            //Build query string
            var canonicalizedQueryString = String.Join("&", sortedArgs.Select(x => x.Key + "=" + x.Value).ToArray());
            CanonicalizedQueryString = canonicalizedQueryString;

            var uri = new Uri(endpoint);
            //Build final string to sign
            var signString = String.Format(
                "GET\n{0}\n{1}\n{2}", 
                uri.Host,
                uri.AbsolutePath,
                canonicalizedQueryString
            );

            //Encrypt using SHA256 and API secret key
            var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(signString));

            //Convert into escaped base64
            var base64 = Convert.ToBase64String(hash);
            var encodedBase64 = Uri.EscapeDataString(base64);

            return base64;
        }

        /// <summary>
        /// Get XML from given URL
        /// </summary>
        /// <param name="url">Target URL for request</param>
        /// <returns>XDocument of returned XML</returns>
        private async Task<XDocument> Request(string url)
        {
            Url = url;
            XDocument doc = null;
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        doc = XDocument.Parse(result);
                    }
                    else
                    {
                        Error = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (HttpRequestException ex)
                {
                    var t = ex;
                }
                catch (Exception ex)
                {
                    var t = ex;
                }
            }
            return doc;
        }

        /// <summary>
        /// Encode string for using in signature
        /// </summary>
        /// <param name="value">string to encode</param>
        /// <returns></returns>
        public static string Encode(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                //Encode string according to API specification
                value = Uri.EscapeDataString(value);

                //Convert all escape entities into uppercase according to API specification
                value = System.Text.RegularExpressions.Regex.Replace(value, "%[a-z0-9]{2}", (match) =>
                {
                    return match.Value.ToUpper();
                });
            }

            return value;
        }
    }
}