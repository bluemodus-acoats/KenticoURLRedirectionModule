using System;

namespace XperienceCommunity.UrlRedirection
{
    [Serializable]
    /// <summary>
    /// Where the Culture is in the URL
    /// </summary>
    public enum CulturePosition
    {
        None, Prefix, PrefixBeforeVirtual, Postfix, QueryString
    }
}
