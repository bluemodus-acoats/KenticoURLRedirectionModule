using System.Threading.Tasks;

namespace XperienceCommunity.UrlRedirection
{
    public interface IURLRedirectionRequestCultureRetriever
    {
        Task<UrlRedirectionCultureResult> GetCultureResultsAsync(string requestAbsoluteUrl, CultureUrlSettings siteCultureUrlSettings);
    }
}
