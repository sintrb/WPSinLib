using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sin.Http
{
    public delegate void CallbackResponse(HttpWebResponse res);
    public delegate void CallbackString(String str);
    public delegate void CallbackStream(Stream stm);
    public delegate void CallbackImg(BitmapSource img);
    public delegate void CallbackJson(JObject json);

    /// <summary>
    /// HTTP请求保持器
    /// </summary>
    public class HttpHolder
    {
        public bool AutoRefere = true;
        public bool UseCache = true;
        public String RefereUrl = null;
        public SafeDictionary Cookies = new SafeDictionary();

        private void BeginRequest(HttpWebRequest req, CallbackResponse cbk)
        {
            req.BeginGetResponse((asyncResult) =>
            {
                Debug.WriteLine("res: " + req.RequestUri.ToString());
                try
                {
                    HttpWebResponse res = req.EndGetResponse(asyncResult) as HttpWebResponse;
                    using (res)
                    {
                        if (AutoRefere)
                            RefereUrl = req.RequestUri.ToString();
                        if (cbk != null)
                        {
                            cbk(res);
                        }
                        if (res.Cookies != null)
                        {
                            foreach (Cookie ck in res.Cookies)
                            {
                                Debug.WriteLine(String.Format("{0}={1}, {2}--{3}", ck.Name, ck.Value, ck.Domain, ck.Path));
                            }

                            lock (this.Cookies)
                            {
                                foreach (Cookie ck in res.Cookies)
                                {
                                    this.Cookies[ck.Name] = ck.Value;
                                }
                            }
                            Debug.WriteLine("Now Cookie Size: " + this.Cookies.Count);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (cbk != null)
                        cbk(null);
                }
            }, req);
        }

        /// <summary>
        /// 获取HTTP响应
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="form">表单，为null时采用GET方法，不为null时采用POST方法</param>
        /// <param name="headers">附加的请求头信息</param>
        /// <param name="cbk">请求回调</param>
        public void GetResponse(String url, Dictionary<String, String> form = null, Dictionary<String, String> headers = null, CallbackResponse cbk = null)
        {
            if (!UseCache)
            {
                String cachekey = String.Format("_k{0}", System.DateTime.Now.Ticks);
                if (url.IndexOf('?') > 0 && url.EndsWith("?"))
                {
                    url = url + cachekey + "=" + cachekey;
                }
                else if (url.IndexOf('?') > 0)
                {
                    url = url + "&" + cachekey + "=" + cachekey;
                }
                else
                {
                    url = url + "?" + cachekey + "=" + cachekey;
                }
            }
            Debug.WriteLine("req: " + url);
            Uri uri = new Uri(url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.AllowReadStreamBuffering = true;

            CookieContainer cc = new CookieContainer();
            if (this.Cookies.Count > 0)
            {
                lock (this.Cookies)
                {
                    Dictionary<String, String>.Enumerator em = Cookies.Dict.GetEnumerator();
                    while (em.MoveNext())
                    {
                        cc.Add(uri, new Cookie()
                        {
                            Name = em.Current.Key,
                            Value = em.Current.Value
                        });
                    }
                }
            }
            req.CookieContainer = cc;
            try
            {
                if (RefereUrl != null)
                    req.Headers["Referer"] = RefereUrl;
            }
            catch
            {
                Debug.WriteLine("--------------Referer: " + RefereUrl);
            }
            if (headers != null)
            {
                Dictionary<String, String>.Enumerator em = headers.GetEnumerator();
                System.Diagnostics.Debug.WriteLine("headers:");
                while (em.MoveNext())
                {
                    try
                    {
                        req.Headers[em.Current.Key] = Utils.StringUtils.UrlEncode(em.Current.Value);
                        System.Diagnostics.Debug.WriteLine(String.Format("\t{0}={1}", em.Current.Key, em.Current.Value));
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("\tFail: {0}={1}", em.Current.Key, em.Current.Value));
                    }
                }
            }

            if (form != null)
            {
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                StringBuilder sb = new StringBuilder();
                Dictionary<String, String>.Enumerator em = form.GetEnumerator();
                System.Diagnostics.Debug.WriteLine("form:");
                while (em.MoveNext())
                {
                    sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(em.Current.Key), HttpUtility.UrlEncode(em.Current.Value));
                    System.Diagnostics.Debug.WriteLine(String.Format("\t{0}={1}", em.Current.Key, em.Current.Value));
                }
                req.ContentLength = sb.Length;
                req.BeginGetRequestStream((asyncResult)=>{
                    using (Stream stream = req.EndGetRequestStream(asyncResult) as Stream)
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.Write(sb.ToString());
                        writer.Flush();
                    }
                    BeginRequest(req, cbk);
                }, req);
            }
            else
            {
                req.Method = "GET";
                BeginRequest(req, cbk);
            }
        }

        /// <summary>
        /// 获取HTTP流响应
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="form">表单，为null时采用GET方法，不为null时采用POST方法</param>
        /// <param name="headers">附加的请求头信息</param>
        /// <param name="cbk">请求回调</param>
        public void GetStream(String url, Dictionary<String, String> form = null, Dictionary<String, String> headers = null, CallbackStream cbk = null)
        {
            GetResponse(url, form, headers, (res) =>
            {
                if (cbk != null)
                {
                    if (res != null)
                        cbk(res.GetResponseStream());
                    else
                        cbk(null);
                }
            });
        }

        /// <summary>
        /// 获取HTTP字符串响应
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="form">表单，为null时采用GET方法，不为null时采用POST方法</param>
        /// <param name="headers">附加的请求头信息</param>
        /// <param name="cbk">请求回调</param>
        public void GetString(String url, Dictionary<String, String> form = null, Dictionary<String, String> headers = null, CallbackString cbk = null)
        {
            GetStream(url, form, headers, (stm) =>
            {
                if (cbk != null)
                {
                    if (stm != null)
                    {
                        using (stm)
                        {
                            StreamReader sr = new StreamReader(stm);
                            String str = sr.ReadToEnd();
                            Debug.WriteLine(str);
                            cbk(str);
                        }
                    }
                    else
                        cbk(null);
                }
            });
        }

        /// <summary>
        /// 获取HTTP字符串响应
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="form">表单，为null时采用GET方法，不为null时采用POST方法</param>
        /// <param name="headers">附加的请求头信息</param>
        /// <param name="cbk">请求回调</param>
        public void GetImage(String url, Dictionary<String, String> form = null, Dictionary<String, String> headers = null, CallbackImg cbk = null)
        {
            GetStream(url, form, headers, (stm) =>
            {
                if (cbk != null)
                {
                    if (stm != null)
                    {
                        using (stm)
                        {
                            BitmapImage img = new BitmapImage();
                            img.SetSource(stm);
                            cbk(img);
                        }
                    }
                    else
                        cbk(null);
                }
            });
        }

        public void RequestJson(Dispatcher disp, String url, Dictionary<String, String> form = null, Dictionary<String, String> headers = null, CallbackJson cbk = null)
        {
            GetString(url, form, headers, (s) =>
            {
                if (cbk != null)
                {
                    if (s != null)
                    {
                        disp.BeginInvoke(() =>
                        {
                            try
                            {
                                if (s != null)
                                {
                                    JObject jo = JObject.Parse(s);
                                    cbk(jo);
                                }
                                else
                                {
                                    cbk(null);
                                }
                            }
                            catch
                            {
                                cbk(null);
                            }
                        });
                    }
                    else
                    {
                        disp.BeginInvoke(() =>
                        {
                            cbk(null);
                        });
                    }
                }
            });
        }

        public void Reset()
        {
            this.RefereUrl = null;
            this.Cookies.Clear();
        }
    }
}
