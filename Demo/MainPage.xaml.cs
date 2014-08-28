using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Diagnostics;

using Demo.Resources;

namespace Demo
{
    public partial class MainPage : PhoneApplicationPage
    {
        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            foreach (Button b in (from n in spMain.Children where n is Button select n))
            {
                b.Click += b_Click;
            }
        }

        void b_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine((sender as Button).Tag as String);
            NavigationService.Navigate(new Uri((sender as Button).Tag as String, UriKind.Relative));
        }
    }
}