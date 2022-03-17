using System;
using System.Linq;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Helpers;
using System.Collections.Generic;
using System.Threading;
using CMS.Localization;
using System.Text.RegularExpressions;
using CMS.Core;
using CMS;
using XperienceCommunity.UrlRedirection;
using XperienceCommunity.UrlRedirection.Events;
using System.Threading.Tasks;

[assembly: RegisterImplementation(typeof(IURLRedirectionRepository), typeof(URLRedirectRepository), Lifestyle = Lifestyle.Transient, Priority = RegistrationPriority.SystemDefault)]

namespace XperienceCommunity.UrlRedirection
{
    public class URLRedirectRepository : IURLRedirectionRepository
    {
        private readonly IURLRedirectionMethods _uRLRedirectionMethods;
        private readonly ISiteService _siteService;
        private readonly IEventLogService _eventLogService;
        private readonly IURLRedirectionRequestCultureRetriever _uRLRedirectionRequestCultureRetriever;

        public URLRedirectRepository(IURLRedirectionMethods uRLRedirectionMethods,
            ISiteService siteService,
            IEventLogService eventLogService,
            IURLRedirectionRequestCultureRetriever uRLRedirectionRequestCultureRetriever)
        {
            _uRLRedirectionMethods = uRLRedirectionMethods;
            _siteService = siteService;
            _eventLogService = eventLogService;
            _uRLRedirectionRequestCultureRetriever = uRLRedirectionRequestCultureRetriever;
        }
        public async Task<RedirectionResult> GetRedirectUrlAsync(string AbsoluteUrl)
        {

            try
            {
                
                var currentSite = _siteService.CurrentSite;

                // Possible that the current site may be null on intitial request
                if (currentSite == null)
                {
                    return new RedirectionResult()
                    {
                        RedirectionFound = false
                    };
                }

                // Get URL and parse it so we can analyze
                Uri RequestUrlUri = new Uri(AbsoluteUrl);

                RedirectionUrlBreakdown CurrentRequestEntry = new RedirectionUrlBreakdown(AbsoluteUrl, SiteContext.CurrentSiteID);

                string[] PrefixesNotToCheck = _uRLRedirectionMethods.GetPrefixesNotToCheck(currentSite.SiteID);
                string[] CurrentUrlPathParts = CurrentRequestEntry.UrlPath.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // only run this code if we need to perform a redirect
                if (CurrentUrlPathParts.Length > 0 && PrefixesNotToCheck.Contains(CurrentUrlPathParts[0], StringComparer.InvariantCultureIgnoreCase))
                {
                    return new RedirectionResult()
                    {
                        RedirectionFound = false
                    };
                }

                // Get Url Aliases, Site Culture Configurations, and Culture URL Settings and build current Url into a RedirecitonUrlBreakdown
                Dictionary<string, List<RedirectionEntry>> PathToRedirectionEntry = await _uRLRedirectionMethods.GetRedirectionEntriesAsync(currentSite.SiteID);
                Dictionary<string, CultureConfiguration> CultureConfigs = (currentSite is SiteInfo ? await _uRLRedirectionMethods.GetCultureConfigurationsAsync((SiteInfo)currentSite) : new Dictionary<string, CultureConfiguration>());
                CultureUrlSettings SiteCultureUrlSettings = _uRLRedirectionMethods.GetCultureUrlSettings(currentSite.SiteID);
                List<string> SiteCultures = await _uRLRedirectionMethods.GetSiteCulturesAsync(currentSite.SiteID);

                #region "Get Current Culture"

                var cultureResults = await _uRLRedirectionRequestCultureRetriever.GetCultureResultsAsync(AbsoluteUrl, SiteCultureUrlSettings);
                var currentCulture = cultureResults.CurrentCulture;
                var urlCulture = cultureResults.UrlCultureCode;
                // Set the Thread and UI Contexts
                if (cultureResults.SetCurrentCulture)
                {
                    // Set their culture also in the current thread.
                    System.Globalization.CultureInfo NewThreadCulture = _uRLRedirectionMethods.GetThreadCulture(currentCulture);
                    CultureInfo newCulture = await _uRLRedirectionMethods.GetKenticoCultureAsync(currentCulture);
                    if (NewThreadCulture != null && (
                        !(Thread.CurrentThread?.CurrentUICulture?.Name ?? "").Equals(currentCulture) ||
                        !(Thread.CurrentThread?.CurrentCulture?.Name ?? "").Equals(currentCulture)
                        )
                        )
                    {
                        Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(currentCulture);
                        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(currentCulture);
                    }
                    if (newCulture != null && (
                        !(LocalizationContext.CurrentUICulture?.CultureCode ?? "").Equals(currentCulture) ||
                        !(LocalizationContext.CurrentCulture?.CultureCode ?? "").Equals(currentCulture)
                        )
                        )
                    {
                        LocalizationContext.CurrentCulture = newCulture;
                        LocalizationContext.CurrentUICulture = newCulture;
                    }
                }

                #endregion

                // Generated during Path Key Generation, but also used in building the request
                Dictionary<string, List<Tuple<string, bool>>> RequestKeyValuePairs = new Dictionary<string, List<Tuple<string, bool>>>();

                // Generate the Path Keys, these both include Exact Matches, Relative Matches, With/Without Domain and with Culture swapped
                string Prefix = RequestUrlUri.AbsoluteUri.Substring(0, RequestUrlUri.AbsoluteUri.IndexOf(RequestUrlUri.AbsolutePath));

                string ExactMatchPathKey = _uRLRedirectionMethods.UrlToPathKey(AbsoluteUrl, true, new string[] { SiteCultureUrlSettings.QueryStringParam }, ref RequestKeyValuePairs);
                string ExactMatchPathKeyWithoutDomain = ExactMatchPathKey.Replace(Prefix, "");
                string ExactMatchPathKeyWithCulture = _uRLRedirectionMethods.ReplaceCultureWithPlaceholder(ExactMatchPathKey, urlCulture);
                string ExactMatchPathKeyWithCultureWithoutDomain = ExactMatchPathKeyWithCulture.Replace(Prefix, "");

                string RelativeMatchPathKey = ExactMatchPathKey.Split('?')[0].Replace(URLRedirectionMethods._ExactMatchPrefix, "");
                string RelativeMatchPathKeyWithCulture = ExactMatchPathKeyWithCulture.Split('?')[0].Replace(URLRedirectionMethods._ExactMatchPrefix, "");
                string RelativeMatchPathKeyWithoutDomain = RelativeMatchPathKey.Replace(Prefix, "");
                string RelativeMatchPathKeyWithCultureWithoutDomain = RelativeMatchPathKeyWithCulture.Replace(Prefix, "");

                // Make a list of all possible lookup values
                List<string> PossibleMatchKeys = new List<string>(new string[] {
                    ExactMatchPathKey,
                    RelativeMatchPathKey,
                    ExactMatchPathKeyWithCulture,
                    RelativeMatchPathKeyWithCulture,
                    ExactMatchPathKeyWithoutDomain,
                    RelativeMatchPathKeyWithoutDomain,
                    ExactMatchPathKeyWithCultureWithoutDomain,
                    RelativeMatchPathKeyWithCultureWithoutDomain
                    });
                PossibleMatchKeys = PossibleMatchKeys.Distinct().ToList();

                // Now search the site's Redirection Entries for a match
                List<RedirectionEntry> FoundEntries = new List<RedirectionEntry>();
                FoundEntries.AddRange(PathToRedirectionEntry.Where(x => PossibleMatchKeys.Contains(x.Key)).SelectMany(x => x.Value));

                // None found, exit
                if (FoundEntries.Count == 0)
                {
                    return new RedirectionResult() {
                        RedirectionFound = false
                    };
                }

                // Look through Found entries for the one partainig to the culture
                var CultureEntry = FoundEntries.Where(x => x.Cultures.Count() == 0 || x.Cultures.Contains(currentCulture, StringComparer.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (CultureEntry == null)
                {
                    // None for this culture found
                    _eventLogService.LogInformation("XperienceCommunity.UrlRedirection.RedirectionHandler.Begin_Execute", "REDIRECT_FAILED", $"The culture code: {LocalizationContext.CurrentCulture.CultureCode} was not assigned to the site. Unable to redirect URL: {RequestContext.RawURL}");
                    return new RedirectionResult()
                    {
                        RedirectionFound = false
                    };
                }

                // Handle culture override
                bool CultureOverwrittenByRedirect = false;
                if (!string.IsNullOrWhiteSpace(CultureEntry.CultureOverride) && !CultureEntry.CultureOverride.Equals(currentCulture, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentCulture = CultureEntry.CultureOverride;
                    CultureOverwrittenByRedirect = true;
                }

                //Handle Redirect
                try
                {
                    #region "Path Handling"

                    RedirectionUrlBreakdown Redirect = CultureEntry.RedirectionUrl;
                    string RedirectionUrl = Redirect.UrlPath;
                    string RelativePath = (RedirectionUrl.ToLower().StartsWith("http") ? _uRLRedirectionMethods.MakeRelativePath(new Uri(RedirectionUrl).LocalPath) : _uRLRedirectionMethods.MakeRelativePath(RedirectionUrl));
                    
                    // No longer support Virtual Directories
                    string VirtalDirectoryPrefix = "";

                    // Replacing the {culture} placeholder if provided
                    string ReplacementDomain = "";
                    string RedirectDomain = "";
                    bool IsSecure = false;

                    #endregion

                    #region "Culture Handling"

                    // Handle Culture in the Redirection URL and and changing the domain to a culture specific domain
                    string CultureUrlRepresentation = "";
                    switch (SiteCultureUrlSettings.CultureFormat)
                    {
                        case CultureFormat.LanguageDashRegion:
                        default:
                            CultureUrlRepresentation = currentCulture;
                            break;
                        case CultureFormat.Language:
                            CultureUrlRepresentation = currentCulture.Split('-')[0];
                            break;
                    }
                    bool SetCulturePlaceholder = true;
                    bool CultureWasSetInUrl = false;
                    switch (SiteCultureUrlSettings.Position)
                    {
                        case CulturePosition.None:
                            break;
                        case CulturePosition.Prefix:
                            if (!RelativePath.Contains(RedirectionUrlBreakdown._CulturePlaceholder))
                            {
                                RelativePath = string.Format("/{0}/{1}",
                                    CultureUrlRepresentation,
                                    RelativePath.Trim('/')
                                    );
                                CultureWasSetInUrl = true;
                            }
                            break;
                        case CulturePosition.PrefixBeforeVirtual:
                            SetCulturePlaceholder = false;
                            VirtalDirectoryPrefix = string.Format("/{0}/{1}",
                                CultureUrlRepresentation,
                                VirtalDirectoryPrefix.Trim('/'));
                            CultureWasSetInUrl = true;
                            break;
                        case CulturePosition.Postfix:
                            // Append it onto the end
                            if (!RelativePath.Contains(RedirectionUrlBreakdown._CulturePlaceholder))
                            {
                                RelativePath = string.Format("/{0}/{1}",
                                    RelativePath.Trim('/'),
                                    CultureUrlRepresentation);
                                CultureWasSetInUrl = true;
                            }
                            break;
                    }


                    if (SetCulturePlaceholder && RelativePath.Contains(RedirectionUrlBreakdown._CulturePlaceholder))
                    {
                        CultureWasSetInUrl = true;
                        RelativePath = RelativePath.Replace(RedirectionUrlBreakdown._CulturePlaceholder, CultureUrlRepresentation);
                    }

                    #endregion

                    #region "Domain Handling"

                    string CultureAlias = currentCulture;
                    if (CultureConfigs.ContainsKey(currentCulture.ToLowerInvariant()))
                    {
                        var CultureConfig = CultureConfigs[currentCulture.ToLowerInvariant()];
                        CultureAlias = CultureConfig.CultureAlias;
                        ReplacementDomain = CultureConfig.DomainAlias;
                    }

                    // Only replace the domain if one isn't specified, and one exists.  Also only specify the http/https to the current request if it's not specified.
                    if (!string.IsNullOrWhiteSpace(Redirect.Domain))
                    {
                        // Use the domain and security specified in the redirection URL
                        RedirectDomain = Redirect.Domain;
                    }
                    else if (!string.IsNullOrWhiteSpace(ReplacementDomain))
                    {
                        // Use the Culture Domain Alias for the domain and if it's secure or not
                        RedirectDomain = ReplacementDomain;
                    }
                    else
                    {
                        // use current request as the domain/http/https
                        RedirectDomain = RequestUrlUri.Host;
                        IsSecure = AbsoluteUrl.ToLower().StartsWith("https");
                    }

                    if (Redirect.OriginalUrl.ToLower().StartsWith("http"))
                    {
                        IsSecure = Redirect.OriginalUrl.ToLower().StartsWith("https");
                        RedirectDomain = new Uri(Redirect.OriginalUrl).Host;
                    }
                    else
                    {
                        IsSecure = AbsoluteUrl.ToLower().StartsWith("https");
                    }


                    #endregion

                    #region "QueryString Handling"

                    // Appending Query String values
                    Dictionary<string, List<Tuple<string, bool>>> QueryParamsForRedirect = new Dictionary<string, List<Tuple<string, bool>>>();
                    if (CultureEntry.AppendQueryString)
                    {
                        _uRLRedirectionMethods.CombineQueryStringParams(ref RequestKeyValuePairs, Redirect.QueryStringParams);
                        QueryParamsForRedirect = RequestKeyValuePairs;
                    }
                    else
                    {
                        QueryParamsForRedirect = Redirect.QueryStringParams;
                    }

                    // Append Culture in query string if the incoming request had it, or the Redirect overwrites it, and a Culture specific domain was not found, nor was the culture set in the URL iself.
                    if (!CultureWasSetInUrl && (CultureOverwrittenByRedirect || cultureResults.LangFoundInRequestQueryString) && !string.IsNullOrWhiteSpace(SiteCultureUrlSettings.QueryStringParam))
                    {
                        if (!QueryParamsForRedirect.ContainsKey(SiteCultureUrlSettings.QueryStringParam))
                        {
                            QueryParamsForRedirect.Add(SiteCultureUrlSettings.QueryStringParam, new List<Tuple<string, bool>>());
                        }
                        QueryParamsForRedirect[SiteCultureUrlSettings.QueryStringParam].Clear();
                        QueryParamsForRedirect[SiteCultureUrlSettings.QueryStringParam].Add(new Tuple<string, bool>(CultureUrlRepresentation, true));
                    }

                    // Build Query String
                    string QueryString = "";
                    if (QueryParamsForRedirect.Keys.Count > 0)
                    {
                        List<string> QueryKeyValuePairs = _uRLRedirectionMethods.GetQueryKeyValuePairs(QueryParamsForRedirect);
                        QueryString = "?" + string.Join("&", QueryKeyValuePairs);
                    }

                    #endregion

                    #region "Hash Handling"

                    // Handle Hash
                    string Hash = "";
                    if (!string.IsNullOrWhiteSpace(Redirect.HashAnchor))
                    {
                        Hash = "#" + Redirect.HashAnchor;
                    }

                    #endregion

                    #region "Redirection"

                    // Build the URL
                    string UrlToRedirect = string.Format("{0}://{1}{2}{3}{4}{5}{6}",
                        IsSecure ? "https" : "http",
                        RedirectDomain,
                        (CultureEntry.RedirectionUrl.Port != 80 && CultureEntry.RedirectionUrl.Port != 443) ? ":" + CultureEntry.RedirectionUrl.Port : "",
                        (!string.IsNullOrWhiteSpace(VirtalDirectoryPrefix.Trim('/')) ? "/" + VirtalDirectoryPrefix.Trim('/') : ""),
                        (!string.IsNullOrWhiteSpace(RelativePath.Trim('/')) ? "/" + RelativePath.Trim('/') : ""),
                        QueryString,
                        Hash);

                    // Redirect
                    switch (CultureEntry.RedirectionType)
                    {
                        case "301":
                            return new RedirectionResult()
                            {
                                RedirectionFound = true,
                                RedirectUrl = UrlToRedirect,
                                RedirectType = 301
                            };
                        case "302":
                        default:
                            return new RedirectionResult()
                            {
                                RedirectionFound = true,
                                RedirectUrl = UrlToRedirect,
                                RedirectType = 302
                            };
                    }

                    #endregion
                }
               
                catch (Exception ex)
                {
                    _eventLogService.LogException("XperienceCommunity.UrlRedirection.RedirectionMethods.Redirect", "REDIRECT_FAILED", ex, additionalMessage: "An exception occurred during the redirect process");
                }
            }
            catch (Exception ex)
            {
                _eventLogService.LogException("XperienceCommunity.UrlRedirection", "GeneralError", ex, additionalMessage: "For " + AbsoluteUrl);
            }

            return new RedirectionResult()
            {
                RedirectionFound = false
            };
        }
    }
}
