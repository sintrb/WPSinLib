using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
namespace Demo
{
    public partial class ScaleableImage : UserControl
    {
        public ScaleableImage()
        {
            InitializeComponent();
        }

        public BitmapImage Source
        {
            get
            {
                return imgImage.Source as BitmapImage;
            }
            set
            {
                imgImage.Source = value;
            }
        }

        private void ToImageSize()
        {
            imgImage.Width = Source.PixelWidth;
            imgImage.Height = Source.PixelHeight;
        }
        private double thisWidth
        {
            get
            {
                return this.gdRoot.RenderSize.Width;
            }
        }
        private double thisHeigth
        {
            get
            {
                return this.gdRoot.RenderSize.Height;
            }
        }
        private void ToFitControl()
        {
            if (Source.PixelWidth <= thisWidth && Source.PixelHeight <= thisHeigth)
            {
                imgImage.Width = Source.PixelWidth;
                imgImage.Height = Source.PixelHeight;
            }
            else if((Source.PixelWidth / Source.PixelHeight)>(thisWidth / thisHeigth))
            {
                // gdRoot.W Is Max
                imgImage.Width = thisWidth;
                imgImage.Height = (int)(imgImage.Width * ((double)Source.PixelHeight / Source.PixelWidth));
            }
            else
            {
                imgImage.Height = thisHeigth;
                imgImage.Width = (int)(imgImage.Height * ((double)Source.PixelWidth / Source.PixelHeight));
            }
        }

        
        
        private void GestureListener_Tap(object sender, GestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_Tap");
            ToFitControl();
            transform.TranslateX = transform.TranslateY = 0;
        }
        private void GestureListener_DoubleTap(object sender, GestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_DoubleTap");
            transform.ScaleX = transform.ScaleY = 1;
        }
        private void GestureListener_Hold(object sender, GestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_Hold");
            transform.TranslateX = transform.TranslateY = 0;
            transform.ScaleX = transform.ScaleY = 0;
            transform.Rotation = 0;
        }

        private void GestureListener_DragStarted(object sender, DragStartedGestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_DragStarted");
        }

        private void GestureListener_DragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_DragDelta");
            transform.TranslateX += e.HorizontalChange;
            transform.TranslateY += e.VerticalChange;
        }

        private void GestureListener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_DragCompleted");
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_Flick");
        }

        //double initialAngle;
        double initialScale;
        private void GestureListener_PinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_PinchStarted");
            //initialAngle = transform.Rotation;
            initialScale = transform.ScaleX;
        }

        
        private void GestureListener_PinchDelta(object sender, PinchGestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_PinchDelta");
            //transform.Rotation = initialAngle + e.TotalAngleDelta;
            transform.ScaleX = transform.ScaleY = initialScale * e.DistanceRatio;
        }

        private void GestureListener_PinchCompleted(object sender, PinchGestureEventArgs e)
        {
            Debug.WriteLine("GestureListener_PinchCompleted");
        }

    }
}
