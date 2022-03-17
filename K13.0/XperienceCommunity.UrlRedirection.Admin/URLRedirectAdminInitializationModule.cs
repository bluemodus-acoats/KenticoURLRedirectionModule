using CMS;
using CMS.DataEngine;
using CMS.Modules;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System.Collections.Generic;

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
                e.Manifest.Metadata.Id = "XperienceCommunity.UrlRedirection.Admin";
                e.Manifest.Metadata.SetIconUrl("https://raw.githubusercontent.com/Kentico/devnet.kentico.com/master/marketplace/assets/HBS-Logo.png");
                e.Manifest.Metadata.SetProjectUrl("https://github.com/KenticoDevTrev/KenticoURLRedirectionModule");
                e.Manifest.Metadata.Title = "Xperience URL Redirection";
                e.Manifest.Metadata.ReleaseNotes = "Updated to use new Base package";
                e.Manifest.Metadata.Tags = "URL Redirection, URL, Redirect, Kentico, MVC";
                // Add dependencies
                List<PackageDependency> NetStandardDependencies = new List<PackageDependency>()
                {
                    new PackageDependency("Kentico.Xperience.Libraries", new VersionRange(new NuGetVersion("13.0.0")), new string[] { }, new string[] {"Build","Analyzers"}),
                    new PackageDependency("XperienceCommunity.UrlRedirection.Base", new VersionRange(new NuGetVersion("13.0.8")), new string[] { }, new string[] {"Build","Analyzers"})
                };
                PackageDependencyGroup PackageGroup = new PackageDependencyGroup(new NuGet.Frameworks.NuGetFramework(".NETStandard2.0"), NetStandardDependencies);
                e.Manifest.Metadata.DependencyGroups = new PackageDependencyGroup[] { PackageGroup };
            }
        }
    }
}
