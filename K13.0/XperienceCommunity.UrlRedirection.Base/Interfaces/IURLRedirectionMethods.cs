using CMS.Base;
using CMS.SiteProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XperienceCommunity.UrlRedirection
{
    public interface IURLRedirectionMethods
    {
        void CombineQueryStringParams(ref Dictionary<string, List<Tuple<string, bool>>> RequestParams, Dictionary<string, List<Tuple<string, bool>>> RedirectParams);
        Task<Dictionary<string, CultureConfiguration>> GetCultureConfigurationsAsync(SiteInfo currentSite);
        CultureUrlSettings GetCultureUrlSettings(int SiteID);
        Task<CMS.Localization.CultureInfo> GetKenticoCultureAsync(string currentCulture);
        string[] GetPrefixesNotToCheck(int SiteID);
        List<string> GetQueryKeyValuePairs(Dictionary<string, List<Tuple<string, bool>>> queryStringParams);
        Dictionary<string, List<Tuple<string, bool>>> GetQueryStringBreakdown(string QueryString, string[] QueryParamsToIgnore);
        Task<Dictionary<string, List<RedirectionEntry>>> GetRedirectionEntriesAsync(int SiteID);
        Task<List<string>> GetSiteCulturesAsync(int siteID);
        System.Globalization.CultureInfo GetThreadCulture(string currentCulture);
        string MakeRelativePath(string Path);
        Dictionary<string, List<Tuple<string, bool>>> OrderQueryStringDictionary(Dictionary<string, List<Tuple<string, bool>>> QueryDictionary);
        string ReplaceCultureWithPlaceholder(string Url, string Culture);
        string UrlToPathKey(string Url, bool ExactMatch, string[] QueryParamsToIgnore);
        string UrlToPathKey(string Url, bool ExactMatch, string[] QueryParamsToIgnore, ref Dictionary<string, List<Tuple<string, bool>>> KeyValuePairs);
    }
}