using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XperienceCommunity.UrlRedirection
{
    /// <summary>
    /// A Redirection Entry with all applicable settings.
    /// </summary>
    [Serializable]
    public class RedirectionEntry
    {
        /// <summary>
        /// The Incoming Url breakdown
        /// </summary>
        public RedirectionUrlBreakdown IncomingUrl { get; set; }

        /// <summary>
        /// The outgoing Url breakdown
        /// </summary>
        public RedirectionUrlBreakdown RedirectionUrl { get; set; }

        /// <summary>
        /// The SiteID for this redirect
        /// </summary>
        public int SiteID { get; set; }

        /// <summary>
        /// If the Request query string should be appended to this redirect's query string
        /// </summary>
        public bool AppendQueryString { get; set; }

        /// <summary>
        /// Cultures that this redirect should apply to
        /// </summary>
        public List<string> Cultures { get; set; }

        /// <summary>
        /// The type of redirect
        /// </summary>
        public string RedirectionType { get; set; }

        /// <summary>
        /// The culture that the redirect should be overwritten to
        /// </summary>
        public string CultureOverride { get; set; }

        public RedirectionEntry(RedirectionTableInfo TableEntry, int SiteID)
        {
            try
            {
                this.SiteID = SiteID;
                AppendQueryString = TableEntry.RedirectionAppendQueryString;
                Cultures = DataHelper.GetNotEmpty(TableEntry.RedirectionCultures, "").Split(";|,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                RedirectionType = TableEntry.RedirectionType;
                // None value in UniSelector = "0"
                CultureOverride = TableEntry.RedirectionCultureOverride.Replace("0", "");
                IncomingUrl = new RedirectionUrlBreakdown(TableEntry.RedirectionOriginalURL, SiteID);
                RedirectionUrl = new RedirectionUrlBreakdown(TableEntry.RedirectionTargetURL, SiteID);
            } catch(Exception ex)
            {
                CMS.Core.Service.Resolve<IEventLogService>().LogException("URLRedirection", "ErrorParsingRrewrite", ex, additionalMessage: $"Error parsing either {TableEntry.RedirectionOriginalURL} or {TableEntry.RedirectionTargetURL}, please make sure these are valid urls.");
                // Set random urls that won't break
                IncomingUrl = new RedirectionUrlBreakdown("/invalid-"+Guid.NewGuid(), SiteID);
                RedirectionUrl = new RedirectionUrlBreakdown("/invalid-"+Guid.NewGuid(), SiteID);
            }
        }
    }
}
