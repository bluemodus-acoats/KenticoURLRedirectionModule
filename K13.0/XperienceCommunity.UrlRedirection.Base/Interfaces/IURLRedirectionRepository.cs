using System.Threading.Tasks;

namespace XperienceCommunity.UrlRedirection
{
    public interface IURLRedirectionRepository
    {
        /// <summary>
        /// Uses the current request and determines the Redirect Url.
        /// </summary>
        /// <param name="Url">The Absolute URL of the request</param>
        /// <returns>The Results</returns>
        Task<RedirectionResult> GetRedirectUrlAsync(string Url);
    }
}
