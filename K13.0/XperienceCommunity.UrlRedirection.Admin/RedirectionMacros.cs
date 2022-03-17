using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;
using System;
using System.Data;

[assembly: RegisterExtension(typeof(URLRedirection.RedirectionMacros), typeof(StringNamespace))]

namespace URLRedirection
{
    public class RedirectionMacros : MacroMethodContainer
    {
        [MacroMethod(typeof(string), "Get all active cultures on the site", 0)]
        public static object GetAllActiveCultures(EvaluationContext context, params object[] parameters)
        {
            //Get the cultures from the cache if they exist
            string strCultures = CacheHelper.GetItem("GetAllActiveCulturesQueryStringCache") as string;
            if (string.IsNullOrWhiteSpace(strCultures))
            {
                //Build Query
                var cultureQuery = new DataQuery("cms.culture.selectsitecultures");
                cultureQuery.AddColumns("CultureCode");
                QueryDataParameters queryParameters = new QueryDataParameters
                {
                    { "@SiteID", SiteContext.CurrentSiteID }
                };
                cultureQuery.Parameters = queryParameters;

                DataSet cultures = cultureQuery.Result;

                //Store all cultures in a string with each culture separated by |
                if (!DataHelper.DataSourceIsEmpty(cultures))
                {
                    foreach (DataRow culture in cultures.Tables[0].Rows)
                    {
                        strCultures += culture["CultureCode"] + "|";
                    }
                }

                //Remove trailing |
                strCultures = strCultures.Remove(strCultures.Length - 1);

                //Store resulting string in the cache
                CacheHelper.Add("GetAllActiveCulturesQueryStringCache", strCultures, null, DateTime.Now.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration);
            }

            return strCultures;
        }
    }
}