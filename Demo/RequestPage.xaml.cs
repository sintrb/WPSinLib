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
    public partial class RequestPage : PhoneApplicationPage
    {
        Request req = new Request();
        public RequestPage()
        {
            InitializeComponent();
        }
        private void goRequest(object sender, RoutedEventArgs e)
        {
            tbResponse.Text = "正在请求...";
            req.GET(txtUrl.Text, (ok, co) =>
            {
                String res = co.ResponseText;
                Dispatcher.BeginInvoke(() =>
                {
                    tbResponse.Text = res;
                });
                return ok;
            });
        }
    }
}