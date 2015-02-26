using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;

using Sin.Ext;

namespace Sin.Utils
{
    public class ExShortUtils
    {
        static List<String> Status = new List<string>();
        public static String AppTitle = null;
        public static bool ToastOnStatus = true;
        static public void RefTray(String status, bool IsIndeterminate = true)
        {
            if (Microsoft.Phone.Shell.SystemTray.ProgressIndicator == null)
            {
                Microsoft.Phone.Shell.SystemTray.ProgressIndicator = new Microsoft.Phone.Shell.ProgressIndicator();
            }
            Microsoft.Phone.Shell.SystemTray.ProgressIndicator.Text = status;
            Microsoft.Phone.Shell.SystemTray.ProgressIndicator.IsIndeterminate = IsIndeterminate;
            Microsoft.Phone.Shell.SystemTray.ProgressIndicator.IsVisible = true;
        }
        static public void StopTray()
        {
            if (Microsoft.Phone.Shell.SystemTray.ProgressIndicator != null)
            {
                Microsoft.Phone.Shell.SystemTray.ProgressIndicator.IsIndeterminate = false;
                if (AppTitle != null)
                {
                    Microsoft.Phone.Shell.SystemTray.ProgressIndicator.Text = AppTitle;
                }
                else
                {
                    Microsoft.Phone.Shell.SystemTray.ProgressIndicator.IsVisible = false;
                }
            }
        }

        static public void RefStatus(String status)
        {
            RefTray(status, true);
        }

        static public void RefMessage(String message)
        {
            RefTray(message, false);
        }

        static public void AddStatus(String status)
        {
            lock (Status)
            {
                Status.Push(status);
                if(String.IsNullOrEmpty(preMsg))
                    RefStatus(status);
            }
        }
        static public void RemStatus()
        {
            lock (Status)
            {
                Status.Pop();
                if (String.IsNullOrEmpty(Status.Peek()))
                {
                    if (String.IsNullOrEmpty(preMsg))
                    {
                        StopTray();
                    }
                    else
                        RefMessage(preMsg);
                }
                else
                {
                    RefStatus(Status.Peek());
                }
            }
        }

        static public void MsgStatus(String msg)
        {
            preMsg = msg;
            RefMessage(preMsg);
            Timer.Stop();
            Timer.Start();
        }

        static private DispatcherTimer _Timer;
        static private DispatcherTimer Timer
        {
            get
            {
                if (_Timer == null)
                {
                    _Timer = new DispatcherTimer();
                    _Timer.Tick += _Timer_Tick;
                    _Timer.Interval = System.TimeSpan.FromSeconds(3.0);
                }
                return _Timer;
            }
        }

        static void _Timer_Tick(object sender, EventArgs e)
        {
            preMsg = null;
            if (String.IsNullOrEmpty(Status.Peek()))
            {
                StopTray();
            }
            else
            {
                RefStatus(Status.Peek());
            }
        }

        static public void Doing(String state, bool add=true)
        {
            if (add)
                AddStatus(state);
            else
                RefStatus(state);
        }

        static public void Done()
        {
            RemStatus();
        }

        //private static long lastToastTime = 0;
        public static void ShowToast(String tst)
        {
            ShowToast(tst, "", null);
        }
        private static String preMsg = null;
        public static void ShowToast(String tst, String title, SolidColorBrush brush)
        {
            if (Microsoft.Phone.Shell.SystemTray.IsVisible && ToastOnStatus)
            {
                MsgStatus(tst);
            }
            else
            {
                ShortUtils.ShowToast(tst);
            }
        }


        public static bool IsTrialMode
        {
            get
            {
                return Microsoft.Xna.Framework.GamerServices.Guide.IsTrialMode;
            }
        }

        public static Visibility BuyTheAppVisibility
        {
            get
            {
                return !IsTrialMode ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public static bool GotoBuyTheApp(String tips, String title)
        {
            if (MessageBox.Show(tips, title, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Sin.Utils.LaunchUtils.OpenMarket();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
