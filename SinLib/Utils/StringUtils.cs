using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Text.RegularExpressions;
namespace Sin.Utils
{
    public class StringUtils
    {
        public static String MD5(String src)
        {
            return JeffWilcox.Utilities.Silverlight.MD5CryptoServiceProvider.GetMd5String(src);
        }

        public static String UrlEncode(String src)
        {
            return System.Net.HttpUtility.UrlEncode(src);
        }

        public static String UrlDecode(String ecd)
        {
             
            return System.Net.HttpUtility.UrlDecode(ecd);
        }


        static Regex NVREG = new Regex("([^=^?^&]+)=([^=^?^&]*)");
        public static Dictionary<String, String> ParseQueryString(String query)
        {
            Dictionary<String, String> q = new Dictionary<string, string>();
            foreach (Match m in NVREG.Matches(query))
            {
                q[UrlDecode(m.Groups[1].Value)] = UrlDecode(m.Groups[2].Value);
            }
            return q;
        }

        public static String Array2QueryString(Object[] args)
        {
            if (args.Length % 2 == 1)
            {
                throw new Exception("Not even length of args.length=" + args.Length);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length / 2; ++i)
            {
                if (i != 0)
                {
                    sb.Append("&");
                }
                sb.Append(String.Format("{0}={1}", UrlEncode(args[i * 2].ToString()), UrlEncode(args[i * 2 + 1].ToString())));
            }
            return sb.ToString();
        }

        public static String HumanSize(long size)
        {
            if (size < 1024)
            {
                return String.Format("{0}B", size);
            }
            else if (size < (1024 * 1024))
            {
                return String.Format("{0:F2}KB", size / 1024.0);
            }
            else if (size < (1024 * 1024 * 1024))
            {
                return String.Format("{0:F2}MB", size / (1024 * 1024.0));
            }
            else
            {
                return String.Format("{0:F2}GB", size / (1024 * 1024 * 1024.0));
            }
        }

        public static String AddParamToUrl(String url, String parms)
        {
            return String.Format("{0}{1}{2}", url, url.IndexOf('?') < 0 ? '?' : '&', parms);
        }
    }
}
