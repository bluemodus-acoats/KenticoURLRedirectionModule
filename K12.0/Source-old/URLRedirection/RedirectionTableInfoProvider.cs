using System;
using System.Data;
using System.Linq;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace URLRedirection
{    
    /// <summary>
    /// Class providing <see cref="RedirectionTableInfo"/> management.
    /// </summary>
    public partial class RedirectionTableInfoProvider : AbstractInfoProvider<RedirectionTableInfo, RedirectionTableInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="RedirectionTableInfoProvider"/>.
        /// </summary>
        public RedirectionTableInfoProvider()
            : base(RedirectionTableInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="RedirectionTableInfo"/> objects.
        /// </summary>
        public static ObjectQuery<RedirectionTableInfo> GetRedirectionTables()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="RedirectionTableInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="RedirectionTableInfo"/> ID.</param>
        public static RedirectionTableInfo GetRedirectionTableInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="RedirectionTableInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="RedirectionTableInfo"/> to be set.</param>
        public static void SetRedirectionTableInfo(RedirectionTableInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
            UpdateCache();
        }


        /// <summary>
        /// Deletes specified <see cref="RedirectionTableInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="RedirectionTableInfo"/> to be deleted.</param>
        public static void DeleteRedirectionTableInfo(RedirectionTableInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
            UpdateCache();
        }


        /// <summary>
        /// Deletes <see cref="RedirectionTableInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="RedirectionTableInfo"/> ID.</param>
        public static void DeleteRedirectionTableInfo(int id)
        {
            RedirectionTableInfo infoObj = GetRedirectionTableInfo(id);
            DeleteRedirectionTableInfo(infoObj);
            UpdateCache();
        }

        private static void UpdateCache()
        {
            var lstOfRedirects = GetRedirectionTables().ToList();
            CacheHelper.Add("RedirectionTables_CachedList", lstOfRedirects, null, DateTime.Now.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration);
        }
    }
}