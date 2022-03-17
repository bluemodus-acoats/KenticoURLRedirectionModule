using CMS.DataEngine;
using CMS.Helpers;
using System;

namespace XperienceCommunity.UrlRedirection
{
    /// <summary>
    /// Culture URL settings, pulled from Kentico Settings
    /// </summary>
    [Serializable]
    public class CultureUrlSettings
    {
        public CulturePosition Position { get; set; }
        public string QueryStringParam { get; set; }
        public CultureFormat CultureFormat { get; set; }
        public string DefaultCultureCode { get; set; }
        public CultureUrlSettings()
        {
        }
    }




}
