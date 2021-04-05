using System;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Helpers;
using CMS.DataEngine;
using CMS;
using System.Threading;
using System.Web;
using CMS.Core;

[assembly: RegisterModule(typeof(URLRedirection.RedirectionHandlerLoader))]

namespace URLRedirection
{
    public class RedirectionHandlerLoader : Module
    {

        public RedirectionHandlerLoader() : base("UrlRedirection_RedirectionHandlerLoader")
        {

        }

        protected override void OnInit()
        {
            RequestEvents.Begin.Execute += Begin_Execute;

            base.OnInit();
        }

        private void Begin_Execute(object sender, EventArgs e)
        {
            var Logger = Service.Resolve<IEventLogService>();
            try
            {
                var currentSite = SiteContext.CurrentSite;

                // Possible that the current site may be null on intitial request
                if (currentSite == null)
                {
                    return;
                }
                URLRedirectRepository URLRedirectionRepo = new URLRedirectRepository();
                var Result = URLRedirectionRepo.GetRedirectUrl(HttpContext.Current.Request.Url.AbsoluteUri);
                if(Result.RedirectionFound)
                {
                    try
                    {
                        if(Result.RedirectType == 301)
                        {
                            URLHelper.RedirectPermanent(Result.RedirectUrl);
                        } else
                        {
                            URLHelper.Redirect(Result.RedirectUrl);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        //Do nothing: this exception is thrown by Response.Redirect() in the redirect method. We only want to log other kinds of exceptions
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException("RedirectionMethods.Redirect", "REDIRECT_FAILED", ex, additionalMessage: "An exception occurred during the redirect process");
                    }
                }

                
            }
            catch (ThreadAbortException)
            {
                //Do nothing: this exception is thrown by Response.Redirect() in the redirect method. We only want to log other kinds of exceptions
            }
            catch (Exception ex)
            {
                Logger.LogException("URLRedirect", "GeneralError", ex, additionalMessage: "For " + HttpContext.Current.Request.Url.ToString());
            }
        }




















    }
}