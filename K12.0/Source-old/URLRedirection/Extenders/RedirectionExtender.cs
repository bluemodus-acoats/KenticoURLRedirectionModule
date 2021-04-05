using CMS;
using CMS.Base.Web.UI;
using CMS.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using URLRedirection.Extenders;

[assembly: RegisterCustomClass("RedirectionExtender", typeof(RedirectionExtender))]

namespace URLRedirection.Extenders
{
    public class RedirectionExtender : ControlExtender<UniGrid>
    {
        public override void OnInit()
        {
            Control.OnExternalDataBound += Control_OnExternalDataBound;
        }

        private object Control_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            if (sourceName == "targeturl")
            {
                string targeturl = parameter.ToString();
                if (targeturl.Length > 80)
                {
                    targeturl = targeturl.Substring(0, 55) + "...<strong>(Edit to see full URL)</strong>";
                }
                return targeturl;
            }
            else if (sourceName == "description")
            {
                string description = parameter.ToString();
                if (description.Length > 300)
                {
                    description = description.Substring(0, 300) + "...<strong>(Edit to see full description)</strong>";
                }
                return description;
            }

            return parameter;
        }
    }
}