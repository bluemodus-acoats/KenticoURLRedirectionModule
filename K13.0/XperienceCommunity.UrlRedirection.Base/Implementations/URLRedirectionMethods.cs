using System;
using System.Data;
using System.Linq;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Helpers;
using System.Collections.Generic;
using CMS.Localization;
using CMS.DataEngine;
using CMS;
using CMS.Core;
using XperienceCommunity.UrlRedirection;
using System.Threading.Tasks;

[assembly: RegisterImplementation(typeof(IURLRedirectionMethods), typeof(URLRedirectionMethods), Lifestyle = Lifestyle.Transient, Priority = RegistrationPriority.Fallback )]
namespace XperienceCommunity.UrlRedirection
{
    public class URLRedirectionMethods : IURLRedirectionMethods
    {
        public const string _ExactMatchPrefix = "<ExactMatch>";

        public URLRedirectionMethods()
        {

        }

        /// <summary>
        /// Gets the Culture URL settings for the given site, cached
        /// </summary>
        /// <param name="SiteID">The SiteID</param>
        /// <returns>the site's CultureUrlSettings</returns>
        public CultureUrlSettings GetCultureUrlSettings(int SiteID)
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        $"{SettingsKeyInfo.OBJECT_TYPE}|byname|CultureInUrlSettings",
                        $"{SettingsKeyInfo.OBJECT_TYPE}|byname|CultureQueryStringParam",
                        $"{SettingsKeyInfo.OBJECT_TYPE}|byname|CultureUrlFormat",
                        $"{SettingsKeyInfo.OBJECT_TYPE}|byname|SiteIdentifier"
                    });
                }
                return new CultureUrlSettings()
                {
                    Position = (CulturePosition)SettingsKeyInfoProvider.GetIntValue("CultureInUrlSettings", SiteID),
                    QueryStringParam = DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue("CultureQueryStringParam", SiteID), "lang"),
                    CultureFormat = (CultureFormat)SettingsKeyInfoProvider.GetIntValue("CultureUrlFormat", SiteID),
                    DefaultCultureCode = DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue("CMSDefaultCultureCode", SiteID), "en-US")
                };
            }, new CacheSettings(1440, "GetCultureUrlSettings", SiteID));
        }

        /// <summary>
        /// Gets a dictionary of the Culture to any Domain Alias specified for that culture.  
        /// </summary>
        /// <param name="currentSite"></param>
        /// <returns>The Culture Configurations</returns>
        public async Task<Dictionary<string, CultureConfiguration>> GetCultureConfigurationsAsync(SiteInfo currentSite)
        {
            return await CacheHelper.CacheAsync(async cs =>
            {
                Dictionary<string, CultureConfiguration> CultureConfigurations = new Dictionary<string, CultureConfiguration>();
                // Add cache dependencies
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                            $"{CultureSiteInfo.OBJECT_TYPE}|all",
                            $"{SiteDomainAliasInfo.OBJECT_TYPE}|all",
                            $"{SiteInfo.OBJECT_TYPE}|all",
                            $"{CMS.Localization.CultureInfo.OBJECT_TYPE}|all",
                    });
                }

                // Get all the Domain Aliases not linked to the current site's culture
                var domainAliases = (await SiteDomainAliasInfo.Provider.Get()
                    .WhereEquals(nameof(SiteDomainAliasInfo.SiteID), currentSite.SiteID)
                    .WhereNotEquals(nameof(SiteDomainAliasInfo.SiteDefaultVisitorCulture), currentSite.DefaultVisitorCulture)
                    .Columns(nameof(SiteDomainAliasInfo.SiteID), nameof(SiteDomainAliasInfo.SiteDomainAliasName), nameof(SiteDomainAliasInfo.SiteDefaultVisitorCulture))
                    .GetEnumerableTypedResultAsync()).ToList();
                
                // Add the main site manually, since the RedirectUrl is taking priority
                domainAliases.Add(new SiteDomainAliasInfo()
                {
                    SiteDomainAliasName = currentSite.SitePresentationURL,
                    SiteDefaultVisitorCulture = currentSite.DefaultVisitorCulture,
                });

                // Loop through cultures and build the configurations.  CultureAlias is what will be used in the URL if provided.
                foreach (var culture in CultureSiteInfoProvider.GetSiteCultures(currentSite.SiteName))
                {
                    CultureConfiguration CultureConfig = new CultureConfiguration()
                    {
                        CultureCode = culture.CultureCode,
                        CultureAlias = culture.CultureAlias
                    };

                    // if there is a domain alias, also add taht to the configuration
                    var domainAlias = domainAliases.Where(x => x.SiteDefaultVisitorCulture.Equals(culture.CultureCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (domainAlias != null)
                    {
                        CultureConfig.DomainAlias = domainAlias.SiteDomainAliasName;
                        if (CultureConfig.DomainAlias.ToLower().StartsWith("http"))
                        {
                            CultureConfig.DomainAlias = new Uri(CultureConfig.DomainAlias).Host;
                        }

                        // Indicate if it's the main domain or not if the Culture is the default visitor culture
                        CultureConfig.IsMainDomain = domainAlias.SiteDefaultVisitorCulture.Equals(currentSite.DefaultVisitorCulture, StringComparison.OrdinalIgnoreCase);
                    }
                    CultureConfigurations.Add(CultureConfig.CultureCode.ToLowerInvariant(), CultureConfig);
                }

                return CultureConfigurations;
            }, new CacheSettings(1440, "GetCultureAliasesForRedirection", currentSite.SiteName));
        }

        /// <summary>
        /// Gets a dictionary 
        /// </summary>
        /// <param name="SiteID"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<RedirectionEntry>>> GetRedirectionEntriesAsync(int SiteID)
        {
            return await CacheHelper.CacheAsync(async cs =>
            {
            Dictionary<string, List<RedirectionEntry>> PathToRedirectionEntry = new Dictionary<string, List<RedirectionEntry>>();

            if (cs.Cached)
            {
                cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                {
                    string.Format("{0}|all", RedirectionTableInfo.OBJECT_TYPE)
                });
            }
            var SiteUrlCultureSettings = GetCultureUrlSettings(SiteID);
            string[] QueryParamsToIgnore = new string[] { SiteUrlCultureSettings.QueryStringParam };
            // Build the lookup dictionary
            var tableEntries = await RedirectionTableInfo.Provider.Get()
                .WhereEquals(nameof(RedirectionTableInfo.RedirectionSiteID), SiteID)
                .WhereEquals(nameof(RedirectionTableInfo.RedirectionEnabled), true)
                .GetEnumerableTypedResultAsync();

                foreach (RedirectionTableInfo TableEntry in tableEntries)
                {
                    RedirectionEntry Entry = new RedirectionEntry(TableEntry, SiteID);
                    // This is the Key that the incoming Url must match, this will always be a non-virtual relative url, or non-virtual relative url + query string / hash
                    string Path = UrlToPathKey(Entry.IncomingUrl.OriginalUrl, TableEntry.RedirectionExactMatch, QueryParamsToIgnore);
                    if (!PathToRedirectionEntry.ContainsKey(Path))
                    {
                        PathToRedirectionEntry.Add(Path, new List<RedirectionEntry>());
                    }
                    PathToRedirectionEntry[Path].Add(Entry);
                }

                // Return the Dictionary for lookups
                return PathToRedirectionEntry;
            }, new CacheSettings(1440, "GetRedirectionEntries", SiteID));
        }

        /// <summary>
        /// Converts the Url to the Path Key for lookups
        /// </summary>
        /// <param name="Url">The URL</param>
        /// <param name="ExactMatch">If this Path Key should be an exact match or not</param>
        /// <param name="QueryParamsToIgnore">any Query Params to ignore (such as language)</param>
        /// <returns>The URL Path Key</returns>
        public string UrlToPathKey(string Url, bool ExactMatch, string[] QueryParamsToIgnore)
        {
            Dictionary<string, List<Tuple<string, bool>>> KeyValuePairs = new Dictionary<string, List<Tuple<string, bool>>>();
            return UrlToPathKey(Url, ExactMatch, QueryParamsToIgnore, ref KeyValuePairs);
        }

        /// <summary>
        /// Creates the Path Key given the Url
        /// </summary>
        /// <param name="Url">The URL, can be either absolute or relative, with Querystring/Hash should be provided</param>
        /// <param name="ExactMatch">If the Path Key should contains the QueryString/Hash</param>
        /// <returns>The Path Key, any Querystring parameters will be reordered alphabetically to ensure matching</returns>
        public string UrlToPathKey(string Url, bool ExactMatch, string[] QueryParamsToIgnore, ref Dictionary<string, List<Tuple<string, bool>>> KeyValuePairs)
        {
            string Path = MakeRelativePath(Url);
            string TempUrl = Path;
            // Remove Hash, it's never sent to the server so can't use it in matching up
            if (TempUrl.Contains('#'))
            {
                string[] Parts = TempUrl.Split('#');
                TempUrl = Parts[0];
            }
            string QueryString = "";
            if (TempUrl.Contains('?'))
            {
                string[] Parts = TempUrl.Split('?');
                QueryString = Parts[1];
                TempUrl = Parts[0];
            }

            // remove trailing slashes since will mess up matches otherwise.
            TempUrl = TempUrl.TrimEnd('/');

            // Reconstruct the Path
            if (ExactMatch)
            {
                Path = TempUrl.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(QueryString))
                {
                    // build dictionary of Query params and reorder
                    KeyValuePairs = GetQueryStringBreakdown(QueryString, QueryParamsToIgnore);
                    KeyValuePairs = OrderQueryStringDictionary(KeyValuePairs);
                    List<string> QueryStringPairs = GetQueryKeyValuePairs(KeyValuePairs);
                    Path += "?" + string.Join("&", QueryStringPairs);
                }
            }
            else
            {
                Path = TempUrl.ToLowerInvariant();
            }

            // If exact match, then putting an extra key in front so the incoming also has to have <ExactMatch>
            return (ExactMatch ? _ExactMatchPrefix + Path : Path);
        }

        /// <summary>
        /// Makes the given path relative, handling Virtual Paths (if an absolute path then returns that path)
        /// </summary>
        /// <param name="Path">The Path</param>
        /// <returns>The relative path</returns>
        public string MakeRelativePath(string Path)
        {
            if (Path.ToLower().StartsWith("http"))
            {
                return Path;
            }

            string PathWithoutEnding = Path.Split('?')[0].Split('#')[0];
            string PathEnding = Path.Substring(PathWithoutEnding.Length);
            // Make sure URL is absolute first
            if (Path.Contains("~"))
            {
                Path = Path.Replace("~", "");
            }

            // Get the Relative non virtual path, to do this must have a Virtualized relative path or one with a ~
            Path = Path.Replace("~", "");

            return "/" + Path.Split('?')[0].Split('#')[0].Trim('/').Replace("%7B", "{").Replace("%7D", "}") + PathEnding;
        }

        /// <summary>
        /// Replaces the culture with the Culture placeholder
        /// </summary>
        /// <param name="Url">The Url</param>
        /// <param name="Culture">The Culture Code</param>
        /// <returns>The Url with the culture replaced with the CulturePlaceholder</returns>
        public string ReplaceCultureWithPlaceholder(string Url, string Culture)
        {
            bool HasExactMatch = Url.Contains(_ExactMatchPrefix);
            if (HasExactMatch)
            {
                Url = Url.Replace(_ExactMatchPrefix, "");
            }
            string Hash = (Url.Contains("#") ? Url.Split('#')[1] : "");
            string Query = (Url.Contains("?") ? Url.Split('?')[1].Split('#')[0] : "");
            string[] Parts = Url.Replace("://", ":::").Split('/').Select(x => x.Split('?')[0].Split('#')[0]).ToArray();
            string UrlEnding = (!string.IsNullOrWhiteSpace(Query) ? "?" + Query : "") + (!string.IsNullOrWhiteSpace(Hash) ? "#" + Hash : "");
            bool AbsoluteUrl = Url.Replace(_ExactMatchPrefix, "").StartsWith("http", StringComparison.InvariantCultureIgnoreCase);
            string Prefix = (AbsoluteUrl ? "" : "/");

            if (Parts.Contains(Culture, StringComparer.InvariantCultureIgnoreCase))
            {
                // Find the part
                for (int p = 0; p < Parts.Length; p++)
                {
                    if (Parts[p].Equals(Culture, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Inject the culture at the proper spot in the URL
                        if (p == 0)
                        {
                            return (HasExactMatch ? _ExactMatchPrefix : "") + string.Format("{0}{1}/{2}",
                                Prefix,
                                RedirectionUrlBreakdown._CulturePlaceholder,
                                string.Join("/", Parts.Skip(p + 1)).Trim('/')).Replace(":::", "://").ToLowerInvariant() + UrlEnding;
                        }
                        else if (p == Parts.Length - 1)
                        {
                            return (HasExactMatch ? _ExactMatchPrefix : "") + string.Format("{0}{1}/{2}",
                                Prefix,
                                string.Join("/", Parts.Take(Parts.Length - 1)).Trim('/'),
                                RedirectionUrlBreakdown._CulturePlaceholder).Replace(":::", "://").ToLowerInvariant() + UrlEnding;
                        }
                        else
                        {
                            return (HasExactMatch ? _ExactMatchPrefix : "") + string.Format("{0}{1}/{2}/{3}",
                                Prefix,
                                string.Join("/", Parts.Take(p)),
                                RedirectionUrlBreakdown._CulturePlaceholder,
                                string.Join("/", Parts.Skip(p + 1))).Replace(":::", "://").ToLowerInvariant() + UrlEnding;
                        }
                    }
                }
            }
            return Url;
        }

        /// <summary>
        /// Gets the Prefixes that should not be checked for the given site from the settings
        /// </summary>
        /// <param name="SiteID">The site ID</param>
        /// <returns>The URL Prefixes not to check.</returns>
        public string[] GetPrefixesNotToCheck(int SiteID)
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        $"{SettingsKeyInfo.OBJECT_TYPE}|byname|PathsToIgnore"
                    });
                }
                return DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue("PathsToIgnore", new SiteInfoIdentifier(SiteID)), "").ToLowerInvariant().Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }, new CacheSettings(1440, "GetPrefixesNotToCheck", SiteID));
        }

        /// <summary>
        /// Joins together the Query String Params
        /// </summary>
        /// <param name="queryStringParams">Dictionary of QueryParamName, and a list of values (tuple<string value, bool IfHasEqualSign></value>)</param>
        /// <returns>A list of all the KeyValue Pairs to be joined for the query string</returns>
        public List<string> GetQueryKeyValuePairs(Dictionary<string, List<Tuple<string, bool>>> queryStringParams)
        {
            List<string> KeyValuePairs = new List<string>();
            foreach (string Key in queryStringParams.Keys)
            {
                foreach (var Value in queryStringParams[Key])
                {
                    KeyValuePairs.Add(string.Format("{0}{1}{2}",
                        Key,
                        Value.Item2 ? "=" : "",
                        Value.Item1
                        ));
                }
            }
            return KeyValuePairs;
        }

        /// <summary>
        /// Gets the Site's Cultures, cached
        /// </summary>
        /// <param name="siteID">The SiteID</param>
        /// <returns>A list of the Site Culture Codes (xx-XX)</returns>
        public async Task<List<string>> GetSiteCulturesAsync(int siteID)
        {
            return await CacheHelper.CacheAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        "cms.siteculture|all"
                    });
                }

                return (await CultureSiteInfo.Provider.Get()
                    .Source(x => x.Join<CultureInfo>($"CMS_SiteCulture.CultureID", $"CMS_Culture.CultureID"))
                    .WhereEquals(nameof(CultureSiteInfo.SiteID), siteID)
                    .Columns(nameof(CultureInfo.CultureCode))
                    .GetEnumerableTypedResultAsync())
                    .Select(x => ValidationHelper.GetString(x[nameof(CultureInfo.CultureCode)], "en-US"))
                    .ToList();
            }, new CacheSettings(1440, "UrlRedirectionGetSiteCultures", siteID));
        }


        /// <summary>
        /// Get the Kentico CultureInfo object, cached
        /// </summary>
        /// <param name="currentCulture">The Culture (xx-XX)</param>
        /// <returns>The CultureInfo obj</returns>
        public async Task<CultureInfo> GetKenticoCultureAsync(string currentCulture)
        {
            return await CacheHelper.CacheAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[] { $"cms.culture|byname|{currentCulture}" });
                }
                return await CultureInfo.Provider.GetAsync(currentCulture);
            }, new CacheSettings(1440, "URL_GetKenticoCulture", currentCulture));
        }

        /// <summary>
        /// Gets the System.Globalization.CultureInfo, cached
        /// </summary>
        /// <param name="currentCulture">teh current culture (xx-XX)</param>
        /// <returns>the System.Globalization.CultureInfo obj</returns>
        public System.Globalization.CultureInfo GetThreadCulture(string currentCulture)
        {
            return CacheHelper.Cache(cs =>
            {
                return new System.Globalization.CultureInfo(currentCulture);
            }, new CacheSettings(1440, "URL_GetThreadCulture", currentCulture));
        }

        /// <summary>
        /// Adds the RedirectParams to the RequestParams, handling merging of same-valued values
        /// </summary>
        /// <param name="RequestParams">The Request's QueryParam Dictionary</param>
        /// <param name="RedirectParams">The Redirect's QueryParam Dictionary</param>
        public void CombineQueryStringParams(ref Dictionary<string, List<Tuple<string, bool>>> RequestParams, Dictionary<string, List<Tuple<string, bool>>> RedirectParams)
        {
            foreach (string Key in RedirectParams.Keys)
            {
                if (!RequestParams.ContainsKey(Key))
                {
                    RequestParams.Add(Key, new List<Tuple<string, bool>>());
                }
                foreach (var Value in RedirectParams[Key])
                {
                    if (!RequestParams[Key].Any(x => x.Item1 == Value.Item1 && x.Item2 == Value.Item2))
                    {
                        RequestParams[Key].Add(Value);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a Dictionary of the Query Keys, plus the a list of Tuples<string KeyValue, bool HasEqualSign></string>
        /// </summary>
        /// <param name="QueryString"></param>
        /// <returns></returns>
        public Dictionary<string, List<Tuple<string, bool>>> GetQueryStringBreakdown(string QueryString, string[] QueryParamsToIgnore)
        {
            Dictionary<string, List<Tuple<string, bool>>> KeyValuePairs = new Dictionary<string, List<Tuple<string, bool>>>();
            foreach (string KeyValuePair in QueryString.Trim('?').Trim('&').Split('&'))
            {
                if (KeyValuePair.Contains("="))
                {
                    string[] KeyValueSplit = KeyValuePair.Split('=');
                    if (!QueryParamsToIgnore.Contains(KeyValueSplit[0]))
                    {
                        if (!KeyValuePairs.ContainsKey(KeyValueSplit[0]))
                        {
                            KeyValuePairs.Add(KeyValueSplit[0], new List<Tuple<string, bool>>());
                        }
                        KeyValuePairs[KeyValueSplit[0]].Add(new Tuple<string, bool>(KeyValueSplit[1], true));
                    }
                }
                else
                {
                    // Query params that have no equal, such as &SomeValue&Etc
                    if (!QueryParamsToIgnore.Contains(KeyValuePair))
                    {
                        if (!KeyValuePairs.ContainsKey(KeyValuePair))
                        {
                            KeyValuePairs.Add(KeyValuePair, new List<Tuple<string, bool>>());
                        }
                        KeyValuePairs[KeyValuePair].Add(new Tuple<string, bool>("", false));
                    }
                }
            }
            return KeyValuePairs;
        }
        public Dictionary<string, List<Tuple<string, bool>>> OrderQueryStringDictionary(Dictionary<string, List<Tuple<string, bool>>> QueryDictionary)
        {
            Dictionary<string, List<Tuple<string, bool>>> Ordered = new Dictionary<string, List<Tuple<string, bool>>>();
            foreach (string Key in QueryDictionary.Keys.OrderBy(x => x))
            {
                Ordered.Add(Key, new List<Tuple<string, bool>>());
                foreach (Tuple<string, bool> Value in QueryDictionary[Key].OrderBy(x => x.Item1).ThenBy(x => x.Item2))
                {
                    Ordered[Key].Add(Value);
                }
            }
            return Ordered;
        }

    }
}