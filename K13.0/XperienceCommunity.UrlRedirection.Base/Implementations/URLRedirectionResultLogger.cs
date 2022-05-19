using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XperienceCommunity.UrlRedirection
{
    public class URLRedirectionResultLogger : IURLRedirectionResultLogger
    {
        public Task LogSuccessfulMatchAsync(string absoluteUrl, string urlToRedirect, List<string> possibleMatchKeys, string currentCulture, List<RedirectionEntry> foundEntries)
        {
            // Do nothing, should implement your own logger
            return Task.FromResult((object)null);
        }

        public Task LogUnsuccessfulCultureMatchAsync(string absoluteUrl, List<string> possibleMatchKeys, List<RedirectionEntry> foundEntries, string currentCulture)
        {
            // Do nothing, should implement your own logger
            return Task.FromResult((object)null);
        }

        public Task LogUnsuccessfulMatchAsync(string absoluteUrl, List<string> possibleMatchKeys, Dictionary<string, List<RedirectionEntry>> pathToRedirectionEntry)
        {
            // Do nothing, should implement your own logger
            return Task.FromResult((object)null);
        }
    }
}
