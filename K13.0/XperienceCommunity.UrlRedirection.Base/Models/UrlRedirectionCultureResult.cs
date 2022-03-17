using System;
using System.Collections.Generic;
using System.Text;

namespace XperienceCommunity.UrlRedirection
{
    public class UrlRedirectionCultureResult
    {
        /// <summary>
        /// The Request's Culture, ex /MyRequest?lang=es-ES would have a CurrentCulture of es-ES as that's what the person is requesting in the query string
        /// </summary>
        public string CurrentCulture { get; set; }

        /// <summary>
        /// Set this if the Culture is found in the current request's URL, this will allow the Path Keys to replace it with {culture} for matching, ex /en-US/MyRequest?lang=es-ES would have a UrlCultureCode of "en-US"
        /// </summary>
        public string UrlCultureCode { get; set; }

        /// <summary>
        /// True by default, if set to false, will not set the Thread.CurrentThread.CurrentUICulture, Thread.CurrentThread.CurrentCulture, LocalizationContext.CurrentCulture, and LocalizationContext.CurrentUICulture to the found culture
        /// </summary>
        public bool SetCurrentCulture { get; set; } = true;

        /// <summary>
        /// If a language was found in the URL parameter, impacts redirect handling with language specific domains or not.
        /// </summary>
        public bool LangFoundInRequestQueryString { get; set; } = false;
    }
}
