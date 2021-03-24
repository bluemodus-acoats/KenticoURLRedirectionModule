[![Nuget](https://img.shields.io/badge/nuget-v12.0.2-blue.svg)](https://www.nuget.org/packages/KenticoURLRedirection/)
# Kentico 12 URL Redirection Module
This module adds an interface in the CMS to allow a user to edit and manage URL Redirects in one place. The module supports multi-site instances and multi-culture sites (more details below). The source code for this module is included in this repo if you wish to clone and modify it anyway you see fit. 

It was originally created by Silver Tech, but it was forked off by HBS.  Most of what is in here is now in the Silver tech Variant, however this one has the admin and mvc split into two separate packages which prevents un-needed files from being copied into the MVC site.

## Compatibility

 - .NET 4.6.1 or greater
 - Kentico Version 12.0.29 or greater
 - Compatible with all development styles of Kentico (MVC, Portal Engine, ASPX, etc.)

## Installation Instructions

Install the latest version of the Kentico URL Redirection Admin [nuget package](https://www.nuget.org/packages/HBS.KenticoURLRedirection.Admin/) on your Kentico installation
`Install-Package HBS.KenticoUrlRedirection.Admin`

Aand if you have any MVC Sites, install the Kentico URL Redirection MVC [nuget package](https://www.nuget.org/packages/HBS.KenticoURLRedirection.MVC/)
`Install-Package HBS.KenticoUrlRedirection.MVC`

After installation, rebuild and check the event log of the site and you should see a line like this:
![Module Installed Successfully](https://github.com/KenticoDevTrev/KenticoURLRedirectionModule/blob/master/Readme%20Assets/moduleintalled-eventlog.png?raw=true)

To update the module to a more recent version, simply update the NuGet package.

## LIMITATIONS/REQUIRED SETUP
**IMPORTANT -** Please note that there are a few requirements with this module in order to determine the proper domain the redirect should use when the target is a relative path.

**Single Site and Single Culture**

If your site is a single domain and single culture, you should specify the default visitor culture as the default culture of your site. If this is not set, the redirect will use the request's domain and may default to en-US culture if not deterined elsewise.

To do this, follow these steps:

 1. Go to Applications>Configuration>Sites
 2. Edit the site
 3. Select the Culture of your site from the "Visitor Culture" drop down (in this example it is English)
 ![Primary Culture Dropdown](https://raw.githubusercontent.com/silvertech/KenticoURLRedirectionModule/master/Readme%20Assets/singlesite-singleculture.png)
 4. Click **Save**


**Single Site and Multiple Cultures - Domain Aliases**

If your site is a single site but uses different domain aliases for each culture then you must specify which language each domain if you wish the redirector to use these culture domain aliases when redirecting for the proper culture.

 1. Follow the steps for "Single Site and Single Culture" and set the main domain's culture to your primary culture
 2. Click on **Domain aliases** in the left hand tabs
 3. For each domain aliases, select the appropriate culture. In this example, **de.** is German and **fr.** is French
![Multiple Cultures Domain Aliases](https://github.com/silvertech/KenticoURLRedirectionModule/blob/master/Readme%20Assets/singlesite-domainaliases.png?raw=true)

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
| RedirectionCultureOverride | Text (10) | Drop Down List | If set, this redirection will override the user's culture during the redirect. |

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
Contributions to this module are welcome. All the source files for this module are included and you just need to add the project to a Kentico Web Application solution and you can start editing anything you like. A ZIP export of the most recent, _unsealed_, version of the module is also included in the root of this repo so you can easily import the module database items into your Kentico project through the **Sites** application. This file is named **KenticoURLRedirection_12.0.3.zip** and will be needed if you want to edit anything related to the module inside the CMS.

 Submit a pull request to the repo with your code changes as well as an updated ZIP file (if CMS changes were made) and we will review and provide feedback. We will also update the NuGet package to a new version once we approve your changes.

## Support
Any bugs can be listed as issues here in GitHub or can be sent to our email tfayas@hbs.net. We will respond as soon as we can.
