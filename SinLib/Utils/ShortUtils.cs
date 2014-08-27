using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coding4Fun.Toolkit.Controls;
namespace Sin.Utils
{
    public class ShortUtils
    {
        public static void ShowToast(String tst)
        {
            ToastPrompt toast = new ToastPrompt();
            toast.Title = tst;
            toast.TextOrientation = System.Windows.Controls.Orientation.Horizontal;
            toast.Show();
        }
    }
}
