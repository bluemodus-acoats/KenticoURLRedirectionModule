using CMS.DataEngine;
using CMS.Modules;

namespace URLRedirection.Admin
{
    public class URLRedirectAdminInitializationModule : Module
    {
        public URLRedirectAdminInitializationModule() : base("URLRedirectAdminInitializationModule")
        {

        }

        protected override void OnInit()
        {
            base.OnInit();
            // Nuget Manifest Build
            ModulePackagingEvents.Instance.BuildNuSpecManifest.After += BuildNuSpecManifest_After;
        }

        private void BuildNuSpecManifest_After(object sender, BuildNuSpecManifestEventArgs e)
        {
            if (e.ResourceName.Equals("URLRedirection", System.StringComparison.InvariantCultureIgnoreCase))
            {
                e.Manifest.Metadata.Id = "HBS.KenticoURLRedirection.Admin";
                e.Manifest.Metadata.ProjectUrl = "https://github.com/silvertech/KenticoURLRedirectionModule";
                e.Manifest.Metadata.IconUrl = "https://github.com/silvertech/KenticoURLRedirectionModule/blob/master/silvertech-logo.png?raw=true";
                e.Manifest.Metadata.Copyright = "";
                e.Manifest.Metadata.Title = "Kentico URL Redirection";
                e.Manifest.Metadata.ReleaseNotes = "Updated to include In-Culture handling, Query String and Hash tag support, relative virtual or absolute URL, Exact Match and Culture override";
                e.Manifest.Metadata.Tags = "URL Redirection, URL, Redirect, Kentico, MVC";
            }
        }
    }
}
