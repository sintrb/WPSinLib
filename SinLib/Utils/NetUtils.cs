using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Utils
{
    public class NetUtils
    {
        /// <summary>
        /// 获取局域网地址（WiFi地址，私有）
        /// </summary>
        /// <returns>存在返回地址，不存在返回null</returns>
        public static String GetLANAddress()
        {
            foreach (var hn in Windows.Networking.Connectivity.NetworkInformation.GetHostNames())
            {
                if (hn.IPInformation != null && !String.IsNullOrEmpty(hn.DisplayName))
                {
                    String ip = hn.DisplayName.Trim();
                    if (ip.StartsWith("10.") || // A class, 10.0.0.0--10.255.255.255 
                        ip.StartsWith("172.16.") ||    // B 172.16.0.0--172.31.255.255 
                            ip.StartsWith("172.17.") ||
                            ip.StartsWith("172.18.") ||
                            ip.StartsWith("172.19.") ||
                            ip.StartsWith("172.20.") ||
                            ip.StartsWith("172.21.") ||
                            ip.StartsWith("172.22.") ||
                            ip.StartsWith("172.23.") ||
                            ip.StartsWith("172.24.") ||
                            ip.StartsWith("172.25.") ||
                            ip.StartsWith("172.26.") ||
                            ip.StartsWith("172.27.") ||
                            ip.StartsWith("172.28.") ||
                            ip.StartsWith("172.29.") ||
                            ip.StartsWith("172.30.") ||
                            ip.StartsWith("172.31.") ||
                            ip.StartsWith("172.") ||
                        ip.StartsWith("192.168.") || // C 192.168.0.0--192.168.255.255
                        ip.StartsWith("202.120."))   // D 202.120.0.0 --202.120.255.255
                        return ip;
                }
            }
            return null;
        }
    }
}
