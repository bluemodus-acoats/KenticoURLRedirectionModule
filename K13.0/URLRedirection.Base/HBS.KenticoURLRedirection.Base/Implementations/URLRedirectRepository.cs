using System;
using System.Linq;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Helpers;
using System.Collections.Generic;
using System.Threading;
using CMS.Localization;
using URLRedirection.Events;
using System.Text.RegularExpressions;
using CMS.Core;
using URLRedirection;
using CMS;

[assembly: RegisterImplementation(typeof(IURLRedirectionRepository), typeof(URLRedirectRepository), Lifestyle = Lifestyle.Transient, Priority = RegistrationPriority.SystemDefault)]

namespace URLRedirection
{
    public class URLRedirectRepository : IURLRedirectionRepository
    {
        public RedirectionResult GetRedirectUrl(string AbsoluteUrl)
        {
            IEventLogService EventLog = Service.Resolve<IEventLogService>();

            try
            {
                IURLRedirectionMethods URLRedirectHelper = Service.Resolve<IURLRedirectionMethods>();
                ISiteService SiteService = Service.Resolve<ISiteService>();
                
                var currentSite = SiteService.CurrentSite;

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

                string[] PrefixesNotToCheck = URLRedirectHelper.GetPrefixesNotToCheck(currentSite.SiteID);
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
                Dictionary<string, List<RedirectionEntry>> PathToRedirectionEntry = URLRedirectHelper.GetRedirectionEntries(currentSite.SiteID);
                Dictionary<string, CultureConfiguration> CultureConfigs = (currentSite is SiteInfo ? URLRedirectHelper.GetCultureConfigurations((SiteInfo)currentSite) : new Dictionary<string, CultureConfiguration>());
                CultureUrlSettings SiteCultureUrlSettings = URLRedirectHelper.GetCultureUrlSettings(currentSite.SiteID);
                List<string> SiteCultures = URLRedirectHelper.GetSiteCultures(currentSite.SiteID);

                #region "Get Current Culture"

                GetRequestCultureEventArgs Args = new GetRequestCultureEventArgs(RequestUrlUri, SiteCultureUrlSettings)
                {
                    CurrentCulture = "",
                    SetCurrentCulture = true,
                };
                string currentCulture = "";
                bool LangFoundInRequestQueryString = false;
                bool SetCurrentCulture = true;
                string UrlCulture = "";
                using (var RequestCultureHandler = UrlRedirectionEvents.GetRequestCulture.StartEvent(Args))
                {
                    // Try to determine the UrlCultureCode, if not already present from event hook
                    if (string.IsNullOrWhiteSpace(Args.UrlCultureCode))
                    {
                        string PathToCheck = "/" + Args.Request.AbsolutePath.Split('?')[0].Trim('/') + "/";
                        string MatchPattern = (Args.CultureUrlSetting.CultureFormat == CultureFormat.LanguageDashRegion ? @"\/[a-zA-Z]{2}-[a-zA-Z]{2}\/" : @"\/[a-zA-Z]{2}\/");

                        switch (Args.CultureUrlSetting.Position)
                        {
                            case CulturePosition.Prefix:
                                Regex UrlCultureMatchPrefix = new Regex("^" + MatchPattern);
                                PathToCheck = URLRedirectHelper.MakeRelativePath("~" + PathToCheck);
                                var MatchesPrefix = UrlCultureMatchPrefix.Matches(PathToCheck);
                                if (MatchesPrefix.Count > 0)
                                {
                                    Args.UrlCultureCode = MatchesPrefix[0].Value.Trim('/');
                                }
                                break;
                            case CulturePosition.PrefixBeforeVirtual:
                                Regex UrlCultureMatchPrefixBeforeVirtual = new Regex("^" + MatchPattern);
                                var MatchesPrefixVirtual = UrlCultureMatchPrefixBeforeVirtual.Matches(PathToCheck);
                                if (MatchesPrefixVirtual.Count > 0)
                                {
                                    Args.UrlCultureCode = MatchesPrefixVirtual[0].Value.Trim('/');
                                }
                                break;
                            case CulturePosition.Postfix:
                                Regex UrlCultureMatchPostfix = new Regex(MatchPattern + "$");
                                var MatchesPostfix = UrlCultureMatchPostfix.Matches(PathToCheck);
                                if (MatchesPostfix.Count > 0)
                                {
                                    Args.UrlCultureCode = MatchesPostfix[0].Value.Trim('/');
                                }
                                break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(Args.CurrentCulture))
                    {
                        // Get the culture
                        Args.CurrentCulture = (LocalizationContext.CurrentCulture != null ? LocalizationContext.CurrentCulture.CultureCode : SiteCultureUrlSettings.DefaultCultureCode);

                        // Next check the Domain, if it's a culture specific domain alias, then prioritize that.
                        var CultureConfigForUrl = CultureConfigs.Where(x => ValidationHelper.GetString(x.Value.DomainAlias, "").IndexOf(RequestUrlUri.Host, StringComparison.InvariantCultureIgnoreCase) >= 0).Select(x => x.Value).FirstOrDefault();
                        if (CultureConfigForUrl != null && !CultureConfigForUrl.IsMainDomain)
                        {
                            Args.CurrentCulture = CultureConfigForUrl.CultureCode;
                        }

                        // Next Priority is cookie value
                        if (!string.IsNullOrEmpty(CookieHelper.GetValue("CMSPreferredCulture")) && CookieHelper.GetValue("CMSPreferredCulture") != Args.CurrentCulture)
                        {
                            //Not sure if this is a Kentico bug or not, but LocalizationContext.CurrentCulture.CultureCode sometimes does not actually show the current user's culture
                            //This usually happens if this is the first request by a user on a new culture. Comparing it to the cookie value of the user always shows the correct culture
                            Args.CurrentCulture = CookieHelper.GetValue("CMSPreferredCulture");
                        }

                        // Next priority is the UrlCultureCode if set
                        if (!string.IsNullOrWhiteSpace(Args.UrlCultureCode))
                        {
                            string match = SiteCultures.Where(x => x.StartsWith(Args.UrlCultureCode, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(match))
                            {
                                Args.CurrentCulture = match;
                            }
                        }

                        // Last Priority is the Url Parameter
                        if (!string.IsNullOrWhiteSpace(SiteCultureUrlSettings.QueryStringParam) && AbsoluteUrl.Contains(SiteCultureUrlSettings.QueryStringParam))
                        {
                            string QueryCulture = URLHelper.GetUrlParameter(AbsoluteUrl, SiteCultureUrlSettings.QueryStringParam);
                            LangFoundInRequestQueryString = true;
                            if (!string.IsNullOrWhiteSpace(QueryCulture) && !QueryCulture.Equals(Args.CurrentCulture, StringComparison.InvariantCultureIgnoreCase))
                            {
                                Args.CurrentCulture = QueryCulture;
                            }
                        }
                    }
                    // Set Url Culture code if it wasn't found.
                    if (string.IsNullOrWhiteSpace(Args.UrlCultureCode))
                    {
                        Args.UrlCultureCode = Args.CurrentCulture;
                    }

                    RequestCultureHandler.FinishEvent();

                    // Use Argument values now that logic is finished
                    currentCulture = DataHelper.GetNotEmpty(DataHelper.GetNotEmpty(Args.CurrentCulture, SiteCultureUrlSettings.DefaultCultureCode), SiteCultures.First());
                    SetCurrentCulture = Args.SetCurrentCulture;
                    UrlCulture = DataHelper.GetNotEmpty(Args.UrlCultureCode, currentCulture);
                }

                // Set the Thread and UI Contexts
                if (SetCurrentCulture)
                {
                    // Set their culture also in the current thread.
                    System.Globalization.CultureInfo NewThreadCulture = URLRedirectHelper.GetThreadCulture(currentCulture);
                    CultureInfo NewCulture = URLRedirectHelper.GetKenticoCulture(currentCulture);
                    if (NewThreadCulture != null)
                    {
                        Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(currentCulture);
                        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(currentCulture);
                    }
                    if (NewCulture != null)
                    {
                        LocalizationContext.CurrentCulture = CultureInfo.Provider.Get(currentCulture);
                        LocalizationContext.CurrentUICulture = CultureInfo.Provider.Get(currentCulture);
                    }
                }

                #endregion

                // Generated during Path Key Generation, but also used in building the request
                Dictionary<string, List<Tuple<string, bool>>> RequestKeyValuePairs = new Dictionary<string, List<Tuple<string, bool>>>();

                // Generate the Path Keys, these both include Exact Matches, Relative Matches, With/Without Domain and with Culture swapped
                string Prefix = RequestUrlUri.AbsoluteUri.Substring(0, RequestUrlUri.AbsoluteUri.IndexOf(RequestUrlUri.AbsolutePath));

                string ExactMatchPathKey = URLRedirectHelper.UrlToPathKey(AbsoluteUrl, true, new string[] { SiteCultureUrlSettings.QueryStringParam }, ref RequestKeyValuePairs);
                string ExactMatchPathKeyWithoutDomain = ExactMatchPathKey.Replace(Prefix, "");
                string ExactMatchPathKeyWithCulture = URLRedirectHelper.ReplaceCultureWithPlaceholder(ExactMatchPathKey, UrlCulture);
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
                    EventLog.LogInformation("UrlRedirection.RedirectionHandler.Begin_Execute", "REDIRECT_FAILED", $"The culture code: {LocalizationContext.CurrentCulture.CultureCode} was not assigned to the site. Unable to redirect URL: {RequestContext.RawURL}");
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
                    string RelativePath = (RedirectionUrl.ToLower().StartsWith("http") ? URLRedirectHelper.MakeRelativePath(new Uri(RedirectionUrl).LocalPath) : URLRedirectHelper.MakeRelativePath(RedirectionUrl));
                    
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

                    if (RedirectDomain.ToLower().StartsWith("http"))
                    {
                        IsSecure = RedirectDomain.ToLower().StartsWith("https");
                        RedirectDomain = new Uri(RedirectDomain).Host;
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
                        URLRedirectHelper.CombineQueryStringParams(ref RequestKeyValuePairs, Redirect.QueryStringParams);
                        QueryParamsForRedirect = RequestKeyValuePairs;
                    }
                    else
                    {
                        QueryParamsForRedirect = Redirect.QueryStringParams;
                    }

                    // Append Culture in query string if the incoming request had it, or the Redirect overwrites it, and a Culture specific domain was not found, nor was the culture set in the URL iself.
                    if (!CultureWasSetInUrl && (CultureOverwrittenByRedirect || LangFoundInRequestQueryString) && !string.IsNullOrWhiteSpace(SiteCultureUrlSettings.QueryStringParam))
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
                        List<string> QueryKeyValuePairs = URLRedirectHelper.GetQueryKeyValuePairs(QueryParamsForRedirect);
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
                    EventLog.LogException("RedirectionMethods.Redirect", "REDIRECT_FAILED", ex, additionalMessage: "An exception occurred during the redirect process");
                }
            }
            catch (Exception ex)
            {
                EventLog.LogException("URLRedirect", "GeneralError", ex, additionalMessage: "For " + AbsoluteUrl);
            }

            return new RedirectionResult()
            {
                RedirectionFound = false
            };
        }
    }
}
