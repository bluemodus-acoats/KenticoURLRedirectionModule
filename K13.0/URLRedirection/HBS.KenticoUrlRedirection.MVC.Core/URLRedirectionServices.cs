using URLRedirection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesConfiguration
    {
        public static void AddURLRedirection(this IServiceCollection services)
        {
            services.AddSingleton<IURLRedirectionMethods, URLRedirectionMethods>();
            services.AddSingleton<IURLRedirectionRepository, URLRedirectRepository>();
        }
    }
}
