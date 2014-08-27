using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Phone.Tasks;

namespace Sin.Utils
{
    public class LaunchUtils
    {
        public static void SendMail(String to, String subject, String body)
        {
            EmailComposeTask ect = new EmailComposeTask();
            ect.Subject = subject;
            ect.To = to;
            ect.Body = body;
            ect.Show();
        }

        public static void MarkThisApp()
        {
            (new Microsoft.Phone.Tasks.MarketplaceReviewTask()).Show();
        }

        public static void OpenBrowser(String url)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.Uri = new Uri(url);
            wbt.Show();
        }

        public static void OpenMarket(String appid=null)
        {
            Microsoft.Phone.Tasks.MarketplaceDetailTask mdt = new Microsoft.Phone.Tasks.MarketplaceDetailTask();
            if (appid != null)
                mdt.ContentIdentifier = appid;
            mdt.Show();
        }
    }
}
