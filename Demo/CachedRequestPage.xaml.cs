using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Sin.Http;

namespace Demo
{
    public partial class CachedRequestPage : PhoneApplicationPage
    {
        CachedRequest req = new CachedRequest();
        public CachedRequestPage()
        {
            InitializeComponent();
            cbCached.Content = String.Format("启用缓存，已缓存{0}", Sin.Utils.StringUtils.HumanSize(req.CacheSize));
        }

        private void goRequest(object sender, RoutedEventArgs e)
        {
            tbResponse.Text = "正在请求...";
            req.Cached = cbCached.IsChecked.Value;
            req.GET(txtUrl.Text, (ok, co) =>
            {
                String res = co.ResponseText;
                Dispatcher.BeginInvoke(() =>
                {
                    tbResponse.Text = res;
                    cbCached.Content = String.Format("启用缓存，已缓存{0}", Sin.Utils.StringUtils.HumanSize(req.CacheSize));
                });
                return ok;
            });
        }
    }
}