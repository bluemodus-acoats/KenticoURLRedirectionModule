using CMS.DataEngine;

namespace XperienceCommunity.UrlRedirection
{
    /// <summary>
    /// Declares members for <see cref="RedirectionTableInfo"/> management.
    /// </summary>
    public partial interface IRedirectionTableInfoProvider : IInfoProvider<RedirectionTableInfo>, IInfoByIdProvider<RedirectionTableInfo>
    {
    }
}