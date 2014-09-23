using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace Demo
{
    public class ImageItem
    {
        private BitmapImage _Image = new BitmapImage(new Uri("Assets/aa.png", UriKind.Relative));
        public BitmapImage Image
        {
            get
            {
                return _Image;
            }
        }
        public String Title;
    }
    
    public partial class ImagesViewerPage : PhoneApplicationPage
    {
        public ImagesViewerPage()
        {
            InitializeComponent();
            List<ImageItem> imgs = new List<ImageItem>();
            for (int i = 0; i < 3; ++i)
            {
                imgs.Add(new ImageItem()
                {
                    Title = "Img"+i
                });
            }

            pMain.ItemsSource = imgs;
        }
    }
}