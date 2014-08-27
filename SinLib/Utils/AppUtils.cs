using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Utils
{
    public class AppUtils
    {
        public static String AppVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static String DeviceID
        {
            get
            {
                return TryGet("DeviceUniqueId");
            }
        }

        private static String Bytes2String(byte[] bts)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bts)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static String TryGet(String name)
        {
            try
            {
                Object obj = Microsoft.Phone.Info.DeviceExtendedProperties.GetValue(name);
                if (obj.GetType() == typeof(byte[]))
                    return Bytes2String((byte[])obj);
                else
                    return obj.ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}
