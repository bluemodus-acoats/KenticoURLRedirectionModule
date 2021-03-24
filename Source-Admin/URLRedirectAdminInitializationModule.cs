using CMS;
using CMS.DataEngine;
using CMS.Modules;

[assembly: RegisterModule(typeof(URLRedirection.Admin.URLRedirectAdminInitializationModule))]

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
                e.Manifest.Metadata.ProjectUrl = "https://github.com/KenticoDevTrev/KenticoURLRedirectionModule";
                e.Manifest.Metadata.IconUrl = "https://raw.githubusercontent.com/Kentico/devnet.kentico.com/master/marketplace/assets/generic-integration.png";
                e.Manifest.Metadata.Copyright = "";
                e.Manifest.Metadata.Title = "Kentico URL Redirection";
                e.Manifest.Metadata.ReleaseNotes = "Fixed issue with spaced found in URL redirect causing exceptions.";
                e.Manifest.Metadata.Tags = "URL Redirection, URL, Redirect, Kentico, MVC";
            }
        }
    }
}
