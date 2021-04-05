using URLRedirection;

namespace Microsoft.AspNetCore.Builder
{
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class URLRedirectionExtensions
    {
        public static IApplicationBuilder UseURLRedirection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<URLRedirectionMiddleware>();
        }
    }
}