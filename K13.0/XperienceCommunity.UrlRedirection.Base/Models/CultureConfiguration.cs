using System;

namespace XperienceCommunity.UrlRedirection
{
    /// <summary>
    /// Culture Domain Configurations
    /// </summary>
    [Serializable]
    public class CultureConfiguration
    {
        public string CultureCode { get; set; }
        public string CultureAlias { get; set; }
        public string DomainAlias { get; set; }
        public bool IsMainDomain { get; set; } = false;
        public CultureConfiguration()
        {

        }
    }
}
