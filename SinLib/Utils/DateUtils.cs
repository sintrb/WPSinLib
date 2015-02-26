using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Utils
{
    public class DateUtils
    {
        static DateTime D1970 = new DateTime(1970, 1, 1);
        public static DateTime Stmp2Date(long stamp)
        {
            TimeSpan span = new TimeSpan(stamp * 10000000) + System.TimeZoneInfo.Local.BaseUtcOffset;
            return D1970.Add(span); ;
        }
        public static long Date2Stmp(DateTime date)
        {
            TimeSpan span = DateTime.Now - D1970 - System.TimeZoneInfo.Local.BaseUtcOffset;
            return span.Ticks / 10000000;
        }

        public static String TodayString
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        public static String NowString
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }
    }
}
