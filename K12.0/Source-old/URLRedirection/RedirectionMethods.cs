using System;
using System.Data;
using System.Linq;
using URLRedirection;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Helpers;
using CMS.DocumentEngine;
using CMS.Base.Web.UI;
using System.Collections.Generic;
using CMS.Localization;

namespace URLRedirection
{
    public class URLRedirectionMethods
    {
        public static void RedirectDomain(string url, int site, string cultureCode)
        {
            url = url.Contains("?") ? url.Split('?')[0] : url;
            url = url.TrimEnd('/');
            var redirectInfo = RedirectionTableInfoProvider.GetRedirectionTables().WhereEquals("RedirectionSiteID", site).WhereContains("RedirectionCultures", cultureCode).And(RedirectionTableInfoProvider.GetRedirectionTables().WhereEquals("RedirectionOriginalURL", url).Or().WhereEquals("RedirectionOriginalURL", $"{url}/")).FirstOrDefault();
            if (redirectInfo != null)
            {
                string finalUrl = redirectInfo.RedirectionTargetURL;
                if (!finalUrl.Contains("http"))
                {
                    if (cultureCode != "en-US")
                    {
                        var translatedDocument = DocumentHelper.GetDocuments().AllCultures().WhereEquals("NodeAliasPath", finalUrl).WhereEquals("DocumentCulture", cultureCode).FirstOrDefault();
                        if (translatedDocument != null && !String.IsNullOrWhiteSpace(translatedDocument.DocumentUrlPath))
                        {
                            finalUrl = "~/" + translatedDocument.DocumentUrlPath;
                        }
                        else
                        {
                            finalUrl = finalUrl.StartsWith("/") ? "~" + finalUrl : finalUrl;
                            finalUrl = finalUrl.StartsWith("~") ? finalUrl : "~/" + finalUrl;
                        }
                    }
                    else
                    {
                        finalUrl = finalUrl.StartsWith("/") ? "~" + finalUrl : finalUrl;
                        finalUrl = finalUrl.StartsWith("~") ? finalUrl : "~/" + finalUrl;
                    }
                }
                if (redirectInfo.RedirectionType == "301")
                {
                    URLHelper.RedirectPermanent(UrlResolver.ResolveUrl(finalUrl), SiteInfoProvider.GetSiteName(site));
                }
                else
                {
                    URLHelper.Redirect(UrlResolver.ResolveUrl(finalUrl));
                }
            }
        }

        public static void Redirect(string url, int site, string cultureCode, string cultureAlias)
        {
            if (cultureAlias == "domain")
            {
                RedirectDomain(url, site, cultureCode);
                return;
            }
            url = url.Contains("?") ? url.Split('?')[0] : url;
            url = url.EndsWith("/") ? url : url + "/";
            if (cultureCode != "en-US" && url.StartsWith($"/{cultureAlias}/"))
            {
                url = url.Replace($"/{cultureAlias}/", "/");
            }
            url = url.TrimEnd('/');

            var redirectInfo = RedirectionTableInfoProvider.GetRedirectionTables().WhereContains("RedirectionCultures", cultureCode).And(RedirectionTableInfoProvider.GetRedirectionTables().WhereEquals("RedirectionOriginalURL", url).Or().WhereEquals("RedirectionOriginalURL", $"{url}/")).FirstOrDefault();

            if (redirectInfo != null)
            {
                var finalUrl = redirectInfo.RedirectionTargetURL;
                if (!finalUrl.Contains("http"))
                {
                    finalUrl = finalUrl.StartsWith("~") ? finalUrl.TrimStart('~') : finalUrl;
                    finalUrl = finalUrl.StartsWith("/") ? finalUrl : $"/{finalUrl}";
                    if (cultureCode != "en-US")
                    {
                        var translatedDocument = DocumentHelper.GetDocuments().AllCultures().WhereEquals("NodeAliasPath", finalUrl).WhereEquals("DocumentCulture", cultureCode).FirstOrDefault();
                        if (translatedDocument != null && !String.IsNullOrWhiteSpace(translatedDocument.DocumentUrlPath))
                        {
                            finalUrl = $"~/{cultureAlias}{translatedDocument.DocumentUrlPath}";
                        }
                        else
                        {
                            //finalUrl = finalUrl.StartsWith($"/{cultureAlias}/") ? finalUrl : $"~/{cultureAlias}/{finalUrl}";
                            finalUrl = finalUrl.StartsWith("/") ? "~" + finalUrl : finalUrl;
                            finalUrl = finalUrl.StartsWith("~") ? finalUrl : "~/" + finalUrl;
                            finalUrl = finalUrl.Replace("~/", $"~/{cultureAlias}/");
                        }
                    }
                    else
                    {
                        finalUrl = finalUrl.StartsWith("/") ? "~" + finalUrl : finalUrl;
                        finalUrl = finalUrl.StartsWith("~") ? finalUrl : "~/" + finalUrl;
                    }
                }
                if (redirectInfo.RedirectionType == "301")
                {
                    URLHelper.RedirectPermanent(UrlResolver.ResolveUrl(finalUrl), SiteInfoProvider.GetSiteName(site));
                }
                else
                {
                    URLHelper.Redirect(UrlResolver.ResolveUrl(finalUrl));
                }
            }
        }
    }
}