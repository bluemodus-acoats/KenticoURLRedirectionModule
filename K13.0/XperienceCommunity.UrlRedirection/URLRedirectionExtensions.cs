using Microsoft.AspNetCore.Builder;

namespace XperienceCommunity.UrlRedirection
{
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class URLRedirectionExtensions
    {
        public static IApplicationBuilder UseUrlRedirection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<URLRedirectionMiddleware>();
        }
    }
}