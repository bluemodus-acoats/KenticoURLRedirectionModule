using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XperienceCommunity.UrlRedirection
{
    public interface IURLRedirectionResultLogger
    {
        Task LogUnsuccessfulMatchAsync(string absoluteUrl, List<string> possibleMatchKeys, Dictionary<string, List<RedirectionEntry>> pathToRedirectionEntry);
        Task LogUnsuccessfulCultureMatchAsync(string absoluteUrl, List<string> possibleMatchKeys, List<RedirectionEntry> foundEntries, string currentCulture);
        Task LogSuccessfulMatchAsync(string absoluteUrl, string urlToRedirect, List<string> possibleMatchKeys, string currentCulture, List<RedirectionEntry> foundEntries);
    }
}
