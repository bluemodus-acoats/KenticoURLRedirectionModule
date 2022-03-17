using Microsoft.Extensions.DependencyInjection;

namespace XperienceCommunity.UrlRedirection
{
    public static class ServicesConfiguration
    {
        public static void AddUrlRedirection(this IServiceCollection services)
        {
            services.AddSingleton<IURLRedirectionMethods, URLRedirectionMethods>()
                .AddSingleton<IURLRedirectionRequestCultureRetriever, URLRedirectionRequestCultureRetriever>()
                .AddSingleton<IURLRedirectionRepository, URLRedirectRepository>();
        }
    }
}
