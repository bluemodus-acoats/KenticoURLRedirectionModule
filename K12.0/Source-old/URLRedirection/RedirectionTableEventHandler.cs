using CMS;
using CMS.DataEngine;
using CMS.SiteProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: RegisterModule(typeof(URLRedirection.RedirectionTableEventHandler))]

namespace URLRedirection
{
    public class RedirectionTableEventHandler : Module
    {
        public RedirectionTableEventHandler() : base("CustomInit")
        {

        }

        protected override void OnInit()
        {
            base.OnInit();

            RedirectionTableInfo.TYPEINFO.Events.Insert.After += Insert_After;
        }

        private void Insert_After(object sender, ObjectEventArgs e)
        {
            if(e.Object != null)
            {
                var redirectItem = (RedirectionTableInfo)e.Object;

                if(redirectItem.RedirectionSiteID == 0)
                {
                    redirectItem.RedirectionSiteID = SiteContext.CurrentSiteID;

                    redirectItem.Update();
                }
            }
        }
    }
}