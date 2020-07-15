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
        public RedirectionTableEventHandler() : base("UrlRedirection_RedirectionTableEventHandler")
        {

        }

        protected override void OnInit()
        {
            base.OnInit();
        }
    }
}