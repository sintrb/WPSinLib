using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
namespace Sin.Media
{
    public class AnUtils
    {
        static public ObjectAnimationUsingKeyFrames GenThicknessAnimation(TimeSpan tm, Thickness from, Thickness to, int count)
        {
            ObjectAnimationUsingKeyFrames an = new ObjectAnimationUsingKeyFrames();
            double ktop = (to.Top - from.Top) / count;
            double kbottom = (to.Bottom - from.Bottom) / count;
            double kleft = (to.Left - from.Left) / count;
            double kright = (to.Right - from.Right) / count;
            for (int i = 0; i <= count; ++i)
            {
                DiscreteObjectKeyFrame dok = new DiscreteObjectKeyFrame();
                dok.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(tm.Milliseconds * i / (double)count));
                dok.Value = new Thickness(from.Left + i * kleft, from.Top + i * ktop, from.Right + i * kright, from.Bottom + i * kbottom);
                an.KeyFrames.Add(dok);
            }
            return an;
        }
    }
}
