using CMS;
using CMS.DataEngine;
using CMS.SiteProvider;

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

            RedirectionTableInfo.TYPEINFO.Events.Insert.Before += InsertUpdate_Before;
            RedirectionTableInfo.TYPEINFO.Events.Update.Before += InsertUpdate_Before;
        }

        private void InsertUpdate_Before(object sender, ObjectEventArgs e)
        {
            RedirectionTableInfo TableObj = (RedirectionTableInfo)e.Object;
            TableObj.RedirectionOriginalURL = TableObj.RedirectionOriginalURL.Trim();
            TableObj.RedirectionTargetURL = TableObj.RedirectionTargetURL.Trim();

            // Set Site ID
            if (TableObj.RedirectionSiteID == 0)
            {
                TableObj.RedirectionSiteID = SiteContext.CurrentSiteID;
                TableObj.Update();
            }
        }

       
    }
}