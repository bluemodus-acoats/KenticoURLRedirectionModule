using System;

namespace URLRedirection
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
