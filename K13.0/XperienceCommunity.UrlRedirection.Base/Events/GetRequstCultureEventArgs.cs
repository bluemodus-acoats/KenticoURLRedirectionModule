using CMS.Base;
using System;

namespace XperienceCommunity.UrlRedirection.Events
{
    /// <summary>
    /// Request Culture Event Arguments
    /// </summary>
    public class GetRequestCultureEventArgs : CMSEventArgs
    {
        /// <summary>
        /// The incoming Request
        /// </summary>
        public Uri Request { get; }

        /// <summary>
        /// The Site's Culture URL Settings
        /// </summary>
        public CultureUrlSettings CultureUrlSetting { get; }

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
        public bool SetCurrentCulture { get; set; }

        public GetRequestCultureEventArgs()
        {

        }

        public GetRequestCultureEventArgs(Uri request, CultureUrlSettings urlSettings)
        {
            this.Request = request;
            this.CultureUrlSetting = urlSettings;
        }

    }
}
