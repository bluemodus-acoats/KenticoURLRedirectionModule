using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace XperienceCommunity.UrlRedirection
{
    public class URLRedirectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IURLRedirectionRepository _uRLRedirectionRepository;

        public URLRedirectionMiddleware(RequestDelegate next, IURLRedirectionRepository uRLRedirectionRepository)
        {
            _next = next;
            _uRLRedirectionRepository = uRLRedirectionRepository;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var Req = httpContext.Request;
            string AbsoluteUrl = $"{Req.Scheme}://{Req.Host}{Req.Path}{Req.QueryString}";
            var UrlRedirectResult = await _uRLRedirectionRepository.GetRedirectUrlAsync(AbsoluteUrl);
            if (UrlRedirectResult.RedirectionFound)
            {
                httpContext.Response.Redirect(UrlRedirectResult.RedirectUrl, UrlRedirectResult.RedirectType == 301);
            } else
            {
                await _next(httpContext);
            }
        }

    }
}
