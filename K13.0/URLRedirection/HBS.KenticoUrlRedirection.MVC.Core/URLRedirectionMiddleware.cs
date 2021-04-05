using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace URLRedirection
{
    public class URLRedirectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IURLRedirectionRepository uRLRedirectionRepository;

        public URLRedirectionMiddleware(RequestDelegate next, IURLRedirectionRepository uRLRedirectionRepository)
        {
            _next = next;
            this.uRLRedirectionRepository = uRLRedirectionRepository;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var Req = httpContext.Request;
            string AbsoluteUrl = $"{Req.Scheme}://{Req.Host}{Req.Path}{Req.QueryString}";
            var UrlRedirectResult = uRLRedirectionRepository.GetRedirectUrl(AbsoluteUrl);
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
