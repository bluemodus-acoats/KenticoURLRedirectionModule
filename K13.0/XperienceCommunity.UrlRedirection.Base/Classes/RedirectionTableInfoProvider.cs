using CMS.DataEngine;

namespace XperienceCommunity.UrlRedirection
{
    /// <summary>
    /// Class providing <see cref="RedirectionTableInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IRedirectionTableInfoProvider))]
    public partial class RedirectionTableInfoProvider : AbstractInfoProvider<RedirectionTableInfo, RedirectionTableInfoProvider>, IRedirectionTableInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectionTableInfoProvider"/> class.
        /// </summary>
        public RedirectionTableInfoProvider()
            : base(RedirectionTableInfo.TYPEINFO)
        {
        }
    }
}