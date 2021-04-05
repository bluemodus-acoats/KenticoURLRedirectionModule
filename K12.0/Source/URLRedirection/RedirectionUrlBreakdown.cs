using CMS.SiteProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace URLRedirection
{
    [Serializable]
    public class RedirectionUrlBreakdown
    {
        public const string _CulturePlaceholder = "{culture}";

        /// <summary>
        /// If the URL is a Virtual path (Contains a ~)
        /// </summary>
        public bool IsVirtualPath { get; set; }

        /// <summary>
        /// If the URL is secure, if not provided then is null
        /// </summary>
        public bool? IsSecure { get; set; }

        /// <summary>
        /// The Domain if provided
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The Port, if provided
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The Path without Virtual Directory, starting with /
        /// </summary>
        public string UrlPath { get; set; }

        /// <summary>
        /// The Dictionary of Query STring Keys and Values
        /// </summary>
        public Dictionary<string, List<Tuple<string, bool>>> QueryStringParams { get; set; } = new Dictionary<string, List<Tuple<string, bool>>>();

        /// <summary>
        /// The Anchor Tag value
        /// </summary>
        public string HashAnchor { get; set; }

        /// <summary>
        /// The original, full URL
        /// </summary>
        public string OriginalUrl { get; set; }

        /// <summary>
        /// The Site ID
        /// </summary>
        public int SiteID { get; set; }

        public RedirectionUrlBreakdown(string Url, int SiteID)
        {
            Url = Url.Trim();
            OriginalUrl = Url;
            this.SiteID = SiteID;
            IsVirtualPath = Url.Contains("~");

            string UrlToUri = Url;
            bool HasDomain = false;
            if (Url.ToLower().StartsWith("http"))
            {
                IsSecure = Url.ToLower().StartsWith("https");
                HasDomain = true;
            }
            else
            {
                var Site = SiteInfoProvider.GetSiteInfo(SiteID);
                string SiteDomain = !string.IsNullOrWhiteSpace(Site.SitePresentationURL) ? Site.SitePresentationURL : "http://" + Site.DomainName;

                // Not a full url, must make absolute
                UrlToUri = (!HasDomain ? SiteDomain + VirtualPathUtility.ToAbsolute(Url) : Url);
            }

            // If Virtua Path, Url must be made absolute to turn it into a new Uri
            Uri uriObj = new Uri(UrlToUri);
            Port = uriObj.Port;
            if (!string.IsNullOrWhiteSpace(uriObj.Query))
            {
                string[] ParamsToRemove = new string[] { URLRedirectionMethods.GetCultureUrlSettings(SiteID).QueryStringParam };
                QueryStringParams = URLRedirectionMethods.GetQueryStringBreakdown(uriObj.Query, ParamsToRemove);
            }
            if (Url.Contains("#"))
            {
                HashAnchor = Url.Split('#')[1];
            }
            UrlPath = URLRedirectionMethods.MakeRelativePath(uriObj.LocalPath);

            if (HasDomain)
            {
                Domain = uriObj.Host;
            }
        }
    }
}