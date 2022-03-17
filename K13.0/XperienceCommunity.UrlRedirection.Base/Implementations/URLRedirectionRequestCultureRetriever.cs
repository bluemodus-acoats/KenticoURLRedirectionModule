using CMS.Base;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XperienceCommunity.UrlRedirection
{
    public class URLRedirectionRequestCultureRetriever : IURLRedirectionRequestCultureRetriever
    {
        private readonly IURLRedirectionMethods _uRLRedirectionMethods;
        private readonly ISiteService _siteService;

        public URLRedirectionRequestCultureRetriever(IURLRedirectionMethods uRLRedirectionMethods,
            ISiteService siteService)
        {
            _uRLRedirectionMethods = uRLRedirectionMethods;
            _siteService = siteService;
        }

        public async Task<UrlRedirectionCultureResult> GetCultureResultsAsync(string requestAbsoluteUrl, CultureUrlSettings siteCultureUrlSettings)
        {
            var requestUrlUri = new Uri(requestAbsoluteUrl);
            var currentSite = _siteService.CurrentSite;
            var cultureConfigs = currentSite is SiteInfo siteInfo ? await _uRLRedirectionMethods.GetCultureConfigurationsAsync(siteInfo) : new Dictionary<string, CultureConfiguration>();
            var siteCultures = await _uRLRedirectionMethods.GetSiteCulturesAsync(currentSite.SiteID);
            var results = new UrlRedirectionCultureResult()
            {
                SetCurrentCulture = true,
            };

            // Try to determine the UrlCultureCode, if not already present from event hook
            
                string PathToCheck = "/" + requestUrlUri.AbsolutePath.Split('?')[0].Trim('/') + "/";
                string MatchPattern = (siteCultureUrlSettings.CultureFormat == CultureFormat.LanguageDashRegion ? @"\/[a-zA-Z]{2}-[a-zA-Z]{2}\/" : @"\/[a-zA-Z]{2}\/");

                switch (siteCultureUrlSettings.Position)
                {
                    case CulturePosition.Prefix:
                        Regex UrlCultureMatchPrefix = new Regex("^" + MatchPattern);
                        PathToCheck = _uRLRedirectionMethods.MakeRelativePath("~" + PathToCheck);
                        var MatchesPrefix = UrlCultureMatchPrefix.Matches(PathToCheck);
                        if (MatchesPrefix.Count > 0)
                        {
                            results.UrlCultureCode = MatchesPrefix[0].Value.Trim('/');
                        }
                        break;
                    case CulturePosition.PrefixBeforeVirtual:
                        Regex UrlCultureMatchPrefixBeforeVirtual = new Regex("^" + MatchPattern);
                        var MatchesPrefixVirtual = UrlCultureMatchPrefixBeforeVirtual.Matches(PathToCheck);
                        if (MatchesPrefixVirtual.Count > 0)
                        {
                        results.UrlCultureCode = MatchesPrefixVirtual[0].Value.Trim('/');
                        }
                        break;
                    case CulturePosition.Postfix:
                        Regex UrlCultureMatchPostfix = new Regex(MatchPattern + "$");
                        var MatchesPostfix = UrlCultureMatchPostfix.Matches(PathToCheck);
                        if (MatchesPostfix.Count > 0)
                        {
                        results.UrlCultureCode = MatchesPostfix[0].Value.Trim('/');
                        }
                        break;
                }
            
            if (string.IsNullOrWhiteSpace(results.CurrentCulture))
            {
                // Get the culture
                results.CurrentCulture = (LocalizationContext.CurrentCulture != null ? LocalizationContext.CurrentCulture.CultureCode : siteCultureUrlSettings.DefaultCultureCode);

                // Next check the Domain, if it's a culture specific domain alias, then prioritize that.
                var CultureConfigForUrl = cultureConfigs.Where(x => ValidationHelper.GetString(x.Value.DomainAlias, "").IndexOf(requestUrlUri.Host, StringComparison.InvariantCultureIgnoreCase) >= 0).Select(x => x.Value).FirstOrDefault();
                if (CultureConfigForUrl != null && !CultureConfigForUrl.IsMainDomain)
                {
                    results.CurrentCulture = CultureConfigForUrl.CultureCode;
                }

                // Next Priority is cookie value
                if (!string.IsNullOrEmpty(CookieHelper.GetValue("CMSPreferredCulture")) && CookieHelper.GetValue("CMSPreferredCulture") != results.CurrentCulture)
                {
                    //Not sure if this is a Kentico bug or not, but LocalizationContext.CurrentCulture.CultureCode sometimes does not actually show the current user's culture
                    //This usually happens if this is the first request by a user on a new culture. Comparing it to the cookie value of the user always shows the correct culture
                    results.CurrentCulture = CookieHelper.GetValue("CMSPreferredCulture");
                }

                // Next priority is the UrlCultureCode if set
                if (!string.IsNullOrWhiteSpace(results.UrlCultureCode))
                {
                    string match = siteCultures.Where(x => x.StartsWith(results.UrlCultureCode, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(match))
                    {
                        results.CurrentCulture = match;
                    }
                }

                // Last Priority is the Url Parameter
                if (!string.IsNullOrWhiteSpace(siteCultureUrlSettings.QueryStringParam) && requestAbsoluteUrl.Contains(siteCultureUrlSettings.QueryStringParam))
                {
                    string QueryCulture = URLHelper.GetUrlParameter(requestAbsoluteUrl, siteCultureUrlSettings.QueryStringParam);
                    results.LangFoundInRequestQueryString = true;
                    if (!string.IsNullOrWhiteSpace(QueryCulture) && !QueryCulture.Equals(results.CurrentCulture, StringComparison.InvariantCultureIgnoreCase))
                    {
                        results.CurrentCulture = QueryCulture;
                    }
                }
            }
            // Set Url Culture code if it wasn't found.
            if (string.IsNullOrWhiteSpace(results.UrlCultureCode))
            {
                results.UrlCultureCode = results.CurrentCulture;
            }

            return results;
        }
    }
}
