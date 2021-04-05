using CMS.Base;
using CMS.SiteProvider;
using System;
using System.Collections.Generic;

namespace URLRedirection
{
    public interface IURLRedirectionMethods
    {
        void CombineQueryStringParams(ref Dictionary<string, List<Tuple<string, bool>>> RequestParams, Dictionary<string, List<Tuple<string, bool>>> RedirectParams);
        Dictionary<string, CultureConfiguration> GetCultureConfigurations(SiteInfo currentSite);
        CultureUrlSettings GetCultureUrlSettings(int SiteID);
        CMS.Localization.CultureInfo GetKenticoCulture(string currentCulture);
        string[] GetPrefixesNotToCheck(int SiteID);
        List<string> GetQueryKeyValuePairs(Dictionary<string, List<Tuple<string, bool>>> queryStringParams);
        Dictionary<string, List<Tuple<string, bool>>> GetQueryStringBreakdown(string QueryString, string[] QueryParamsToIgnore);
        Dictionary<string, List<RedirectionEntry>> GetRedirectionEntries(int SiteID);
        List<string> GetSiteCultures(int siteID);
        System.Globalization.CultureInfo GetThreadCulture(string currentCulture);
        string MakeRelativePath(string Path);
        Dictionary<string, List<Tuple<string, bool>>> OrderQueryStringDictionary(Dictionary<string, List<Tuple<string, bool>>> QueryDictionary);
        string ReplaceCultureWithPlaceholder(string Url, string Culture);
        string UrlToPathKey(string Url, bool ExactMatch, string[] QueryParamsToIgnore);
        string UrlToPathKey(string Url, bool ExactMatch, string[] QueryParamsToIgnore, ref Dictionary<string, List<Tuple<string, bool>>> KeyValuePairs);
    }
}