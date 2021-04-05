using URLRedirection.Events;

namespace URLRedirection
{
    /// <summary>
    /// Url Redirection Module Event Hooks
    /// </summary>
    public static class UrlRedirectionEvents
    {
        /// <summary>
        /// Overwrites the handling of finding a request's culture and Url Culture so it can properly find redirects and set the culture.
        /// By Default, Culture is determined in this priority: 1: LocalizationContext, 2: Culture Specific Domain Alias, 3: Cookie Value, 4: Culture in Url path 5: Language Url Parameter
        /// By Default, The UrlCulture is determined in this priority: 1: Pattern of /{xx-xx}/ or /{xx}/ depending on Site Url Setting, 2: The Culture determined
        /// </summary>
        public static GetRequestCultureEventHandler GetRequestCulture;


        static UrlRedirectionEvents()
        {
            GetRequestCulture = new GetRequestCultureEventHandler()
            {
                Name = "UrlRedirection.GetRequestCulture"
            };
        }
    }
}
