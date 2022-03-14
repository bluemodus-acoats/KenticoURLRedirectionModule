
# Kentico 13 URL Redirection Module
This module adds an interface in the CMS to allow a user to edit and manage URL Redirects in one place. The module supports multi-site instances and multi-culture sites (more details below). The source code for this module is included in this repo if you wish to clone and modify it anyway you see fit. 

It was originally created by Silver Tech, but it was forked off by HBS and upgraded to 13.

## Compatibility

 - .Net Core 3.1 / .Net 5.0 (for KX 13 MVC Core)
 - Kentico Version 13.0.0 or greater

## Installation Instructions (Both)

Install the latest version of the XperienceCommunity.UrlRedirection.Admin [nuget package](https://www.nuget.org/packages/XperienceCommunity.UrlRedirection.Admin) on your Kentico installation
`Install-Package XperienceCommunity.UrlRedirection.Admin`

Since this package depends on the Kentico.Xperience.Libraries, this nuget package will install, please make sure to upgrade the `Kentico.Xperience.Libraries` nuget package to the version that matches your Kentico Solution's hotfix!

## Installation (MVC.Net Core)
On your MVC.Net Core Sites, install the XperienceCommunity.UrlRedirection [nuget package](https://www.nuget.org/packages/HBS.KenticoURLRedirection.MVC.Core/)
`Install-Package https://nuget.org/packages/XperienceCommunity.UrlRedirection`

In your startup's `ConfigureServices(IServiceCollection services)` method, add: `services.AddURLRedirection();`

Also in your startup's `Configure(IApplicationBuilder app)` method, add: `app.UseURLRedirection();` where you wish to wire up this middleware.  I put it before the UseEndPoints myself.


## LIMITATIONS/REQUIRED SETUP
**IMPORTANT -** Please note that there are a few requirements with this module in order to determine the proper domain the redirect should use when the target is a relative path.

**Single Site and Single Culture**

If your site is a single domain and single culture, you should specify the default visitor culture as the default culture of your site. If this is not set, the redirect will use the request's domain and may default to en-US culture if not determined elsewise.

To do this, follow these steps:

 1. Go to Applications>Configuration>Sites
 2. Edit the site
 3. Select the Culture of your site from the "Visitor Culture" drop down (in this example it is English)
 ![Primary Culture Dropdown](https://github.com/KenticoDevTrev/KenticoURLRedirectionModule/blob/master/K12.0/Readme%20Assets/singlesite-singleculture.png?raw=true)
 4. Click **Save**


**Single Site and Multiple Cultures - Domain Aliases**

If your site is a single site but uses different domain aliases for each culture then you must specify which language each domain if you wish the redirector to use these culture domain aliases when redirecting for the proper culture.

 1. Follow the steps for "Single Site and Single Culture" and set the main domain's culture to your primary culture
 2. Click on **Domain aliases** in the left hand tabs
 3. For each domain aliases, select the appropriate culture. In this example, **de.** is German and **fr.** is French
![Multiple Cultures Domain Aliases](https://github.com/KenticoDevTrev/KenticoURLRedirectionModule/blob/master/K12.0/Readme%20Assets/singlesite-domainaliases.png?raw=true)

## Module Overview
The Kentico URL Redirection module contains one class, **Redirection Table**, that is not customizable. The class' fields are as follows:

| Field Name  | Data Type | Form Control | Descrpiton |
|--|--|--|--|
| RedirectionTableID | Integer | N/A (Not Editable) | Unique ID of the redirection item |
| RedirectionEnabled | bool | Checkbox | Only enabled redirects will be leveraged in redirection lookups.  Uncheck to disable. |
| RedirectionOriginalURL | Text (2000) | Text Box | URL Alias that will be redirected. Ex: **/original-url** .  You can us {culture} as a placeholder for the culture in your URL, and you can add Query String parameters (coupled with Exact Match) to require a match on query string values additionally. |
| RedirectionExactMatch | bool | Check box | If true, the Original Url must match exactly, including the query strings and/or hash. |
| RedirectionTargetURL | Text (2000) | URL Selector | Internal alias or External URL that the Original URL field will be redirected to. Ex: **/target-url** Ex: **https://www.external-domain.com** |
| RedirectionAppendQueryString | bool | Checkbox | If true, any query string parameters the user hits the Origin URL with will be added to the Target URL's listing.  Duplications are automatically handled. |
| RedirectionDescription | Long Text | Text Area | A field that allows a content editor to describe a redirect or enter the purpose of a redirect |
| RedirectionSiteID | Integer | Site Selector | Drop down list that allows a user to specify which site the redirect is for. Default is the current site the user is on. |
| RedirectionType | Text (3) | Drop Down List | Allows the user to specify if the redirect is a 301 or 302 redirect. |
| RedirectionCultures | Text (4000) | Multiple Choice | Allows the user to specify which cultures the redirection should be enabled for. The default selected will be the Cultures available for the current site, although all cultures currently assigned to the site will be shown. |
| RedirectionCultureOverride | Text (10) | Drop Down List | If set, this redirection will override the user's culture during the redirect. This feature may no longer work on .Net Core sites |

The module can be accessed by going to **Applications>Custom>URL Redirection**.

## URL Redirect Settings
In Settings > URLS and SEO > Redirections, there are the following settings:

| Name  | Data Type | Form Control | Descrpiton |
|--|--|--|--|
| Path Prefixes to Ignore | LongText | TextArea | One per line, any requests with a prefix of any specified will not be processed, useful for speeding up non-page requests |
| Culture In Url Settings | int (enum) | Drop down list | If your site uses Culture in the URLs, this can be configured to automatically detect and handle cultures through it.  Options are None, Prefix, Prefix before Virtual directory, and Postfixed  |
| Culture Query String Param | Text | Textbox | If set, the URL Redirect will honor cultures passed through this query string.  |
| Culture Format (No Alias) | int (enum) | Drop down list | What culture format in the URL your site uses, options are xx-XX and xx (example: en-US or en) |

## Event Hooks
This module has an `UrlRedirectionEvents.GetRequestCulture` Event that you can hook into using normal Kentico Global Event Hooks in a custom loader module.  This allows you to customize how you determine the request's culture, and any culture existing in the URL.  It handles these automatically as best as possible, but may require customization through these Event Hooks to meet your specific requirements.

## Exact Matches and Hash
Exact Match will match based on the URL and the Query String, but since Hash values are not passed to the server, they will are not tracked for matching.

## License
This project uses a standard MIT license which can be found [here](https://github.com/KenticoDevTrev/KenticoURLRedirectionModule/blob/master/LICENSE).

## Contribution
Contributions to this module are welcome. All the source files for this module are included and you just need to add the project to a Kentico Web Application solution and you can start editing anything you like. 

 Submit a pull request to the repo with your code changes as well as an updated ZIP file (if CMS changes were made) and we will review and provide feedback. We will also update the NuGet package to a new version once we approve your changes.

## Support
Any bugs can be listed as issues here in GitHub or can be sent to our email tfayas@hbs.net. We will respond as soon as we can.
