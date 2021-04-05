using CMS.DataEngine;
using CMS.Helpers;
using System;

namespace URLRedirection
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
        public CultureUrlSettings(int SiteID)
        {
            SiteInfoIdentifier SiteIdentifier = new SiteInfoIdentifier(SiteID);
            Position = (CulturePosition) SettingsKeyInfoProvider.GetIntValue("CultureInUrlSettings", SiteIdentifier);
            QueryStringParam = DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue("CultureQueryStringParam", SiteIdentifier), "lang");
            CultureFormat = (CultureFormat)SettingsKeyInfoProvider.GetIntValue("CultureUrlFormat", SiteIdentifier);
            DefaultCultureCode = DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue("CMSDefaultCultureCode", SiteIdentifier), "en-US");
        }
    }




}
