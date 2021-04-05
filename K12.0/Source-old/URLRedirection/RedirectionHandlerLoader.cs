using System;
using System.Linq;

using CMS.Base;
using CMS.SiteProvider;
using CMS.Helpers;
using CMS.DocumentEngine;
using System.Collections.Generic;
using CMS.EventLog;
using CMS.DataEngine;
using CMS;
using System.Threading;
using CMS.Localization;

[assembly: RegisterModule(typeof(URLRedirection.RedirectionHandlerLoader))]

namespace URLRedirection
{
    public class RedirectionHandlerLoader : Module
    {
        public RedirectionHandlerLoader() : base("RedirectionHandlerLoader")
        {

        }

        protected override void OnInit()
        {
            RequestEvents.Begin.Execute += Begin_Execute;

            base.OnInit();
        }

        private void Begin_Execute(object sender, EventArgs e)
        {
            string url = RequestContext.CurrentRelativePath + RequestContext.CurrentQueryString;
            string handler = url.Split('/')[1].ToLower();
            string[] notwanted = { "cmspages", "cmsmodules", "cmsformcontrols", "cmsadmincontrols", "admin", "getattachment", "getfile", "getmedia", "getmetafile", "app_themes", "cmsapi", "socialmediaapi", "searchapi", "formsapi", "api" };

            // only run this code if we need to perform a redirect
            if (!notwanted.Contains(handler))
            {
                var currentSite = SiteContext.CurrentSite;
                var dictCultureAlias = CacheHelper.GetItem($"RedirectionHandler_CultureAliasDictionary_{currentSite.SiteName}") as Dictionary<string, string>;

                if (dictCultureAlias == null)
                {
                    dictCultureAlias = new Dictionary<string, string>();
                    var domainAliasDict = new Dictionary<string, string>();

                    var primaryAliasInfo = new SiteDomainAliasInfo();
                    primaryAliasInfo.SiteDomainAliasName = currentSite.DomainName;
                    primaryAliasInfo.SiteDefaultVisitorCulture = currentSite.DefaultVisitorCulture;

                    var domainAliases = SiteDomainAliasInfoProvider.GetDomainAliases().Columns("SiteID", "SiteDomainAliasName", "SiteDefaultVisitorCulture").Where(x => x.SiteID == currentSite.SiteID).ToList();
                    if (domainAliases != null)
                    {
                        domainAliases.Add(primaryAliasInfo);
                        var cultureBindings = CultureSiteInfoProvider.GetCultureSites().WhereEquals("SiteID", SiteContext.CurrentSiteID);
                        foreach (var cultureBinding in cultureBindings)
                        {
                            var culture = CultureInfoProvider.GetCultureInfo(cultureBinding.CultureID);
                            if (culture != null)
                            {
                                if (String.IsNullOrEmpty(culture.CultureAlias))
                                {
                                    var domainAlias = domainAliases.Where(x => x.SiteDefaultVisitorCulture == culture.CultureCode).FirstOrDefault();
                                    if (domainAlias != null)
                                    {
                                        dictCultureAlias.Add(culture.CultureCode, "domain");
                                    }
                                }
                                else
                                {
                                    dictCultureAlias.Add(culture.CultureCode, culture.CultureAlias);
                                }
                            }
                        }
                    }
                    CacheHelper.Add("RedirectionHandler_CultureAliasDictionary", dictCultureAlias, null, DateTime.Now.AddHours(72), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                var currentCulture = LocalizationContext.CurrentCulture.CultureCode;

                //Not sure if this is a Kentico bug or not, but LocalizationContext.CurrentCulture.CultureCode sometimes does not actually show the current user's culture
                //This usually happens if this is the first request by a user on a new culture. Comparing it to the cookie value of the user always shows the correct culture
                if (!String.IsNullOrEmpty(CookieHelper.GetValue("CMSPreferredCulture")) && CookieHelper.GetValue("CMSPreferredCulture") != currentCulture)
                {
                    currentCulture = CookieHelper.GetValue("CMSPreferredCulture");
                }

                if (dictCultureAlias.ContainsKey(currentCulture))
                {
                    //Handle Redirect
                    try
                    {
                        int site = SiteContext.CurrentSiteID;
                        var cultureCode = currentCulture;
                        var cultureAlias = dictCultureAlias[cultureCode];

                        // make sure the culture has been set for the site
                        URLRedirectionMethods.Redirect(url, site, cultureCode, cultureAlias);
                    }
                    catch (ThreadAbortException threadEx)
                    {
                        //Do nothing: this exception is thrown by Response.Redirect() in the redirect method. We only want to log other kinds of exceptions
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("RedirectionMethods.Redirect", "REDIRECT_FAILED", ex, additionalMessage: "An exception occurred during the redirect process");
                    }
                }
                else
                {
                    EventLogProvider.LogInformation("UrlRedirection.RedirectionHandler.Begin_Execute", "REDIRECT_FAILED", $"The culture code: {LocalizationContext.CurrentCulture.CultureCode} was not assigned to the site. Unable to redirect URL: {RequestContext.RawURL}");
                }
            }
        }
    }
}