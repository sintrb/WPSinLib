using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sin.Http
{
    public class CallbackObject
    {
        public HttpWebRequest Request { get; set; }
        public HttpWebResponse Response { get; set; }

        private Stream _ResponseStream = null;
        public Stream ResponseStream
        {
            get
            {
                if (_ResponseStream == null)
                    _ResponseStream = Response.GetResponseStream();
                return _ResponseStream;
            }
            set
            {
                _ResponseStream = value;
            }
        }

        private String _ResponseText = null;
        public String ResponseText
        {
            get
            {
                if (_ResponseText == null)
                {
                    using (this.Response)
                    {
                        Stream stm = ResponseStream;
                        StreamReader sr = new StreamReader(stm);
                        _ResponseText = sr.ReadToEnd();
                        if (_ResponseText == null)
                            _ResponseText = "";
                        stm.Close();
                    }
                    Debug.WriteLine("TXT: " + _ResponseText);
                }
                return _ResponseText;
            }
            set
            {
                _ResponseText = value;
            }
        }

        private JObject _ResponseJson = null;
        public JObject ResponseJson
        {
            get
            {
                if (_ResponseJson == null)
                {
                    try
                    {
                        _ResponseJson = JObject.Parse(ResponseText);
                    }
                    catch
                    {

                    }
                }
                return _ResponseJson;
            }
        }
    }
    public delegate bool RequestCallback(bool ok, CallbackObject co);

    public class Request
    {
        private Dictionary<String, String> _Headers = null;
        public Dictionary<String, String> Headers
        {
            get
            {
                if (_Headers == null)
                    _Headers = new Dictionary<string, string>();
                return _Headers;
            }
        }

        public void GET(String url, RequestCallback cbk)
        {
            ReadyRequest(url, "GET", null, cbk);
        }

        public void PUT(String url, Object data, RequestCallback cbk)
        {
            ReadyRequest(url, "PUT", data, cbk);
        }

        public void POST(String url, Object data, RequestCallback cbk)
        {
            ReadyRequest(url, "POST", data, cbk);
        }

        public void DELETE(String url, RequestCallback cbk)
        {
            ReadyRequest(url, "DELETE", null, cbk);
        }

        virtual public void ReadyRequest(String url, String method, Object data, RequestCallback cbk)
        {
            Debug.WriteLine("REQ: " + method + " " + url);
            Uri uri = new Uri(url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.AllowReadStreamBuffering = true;
            req.Method = method;

            if (_Headers != null && _Headers.Count > 0)
            {
                Dictionary<String, String>.Enumerator em = _Headers.GetEnumerator();
                System.Diagnostics.Debug.WriteLine("headers.count = " + Headers.Count);
                while (em.MoveNext())
                {
                    try
                    {
                        req.Headers[em.Current.Key] = em.Current.Value;
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("\tFail: {0}={1}", em.Current.Key, em.Current.Value));
                    }
                }
            }

            if (data == null)
            {
            }
            else if (data is Dictionary<String, Object>)
            {
                Dictionary<String, Object>.Enumerator em = (data as Dictionary<String, Object>).GetEnumerator();
                StringBuilder sb = new StringBuilder();
                while (em.MoveNext())
                {
                    sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(em.Current.Key), em.Current.Value != null ? Sin.Utils.StringUtils.UrlEncode(em.Current.Value.ToString()) : "");
                }
                data = sb.ToString();
                // form
                req.ContentType = "application/x-www-form-urlencoded";
            }
            else if (data is String)
            {
                req.ContentType = "multipart/form-data";
            }
            else
            {
                throw new Exception("Unknow data type " + data.GetType().ToString() + ": " + data.ToString());
            }


            if (data != null)
            {
                Debug.WriteLine("DAT: " + data);
                req.ContentLength = ((String)data).Length;
                req.BeginGetRequestStream((asyncResult) =>
                {
                    using (Stream stream = req.EndGetRequestStream(asyncResult) as Stream)
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        writer.Write((String)data);
                        writer.Flush();
                    }
                    BeginRequest(req, cbk);
                }, req);
            }
            else
            {
                BeginRequest(req, cbk);
            }
        }

        private void BeginRequest(HttpWebRequest req, RequestCallback cbk)
        {
            
            req.BeginGetResponse((asyncResult) =>
            {
                CallbackObject co = new CallbackObject()
                {
                    Request = req
                };
                
                try
                {
                    co.Response = req.EndGetResponse(asyncResult) as HttpWebResponse;
                    Debug.WriteLine("RES: " + (int)co.Response.StatusCode + "(" + co.Response.ContentLength + ") " + req.RequestUri.ToString());
                    cbk(true, co);
                }
                catch (WebException e)
                {
                    Debug.WriteLine("ERR: " + req.RequestUri.ToString());
                    Debug.WriteLine(e);
                    if (req.HaveResponse)
                        co.Response = e.Response as HttpWebResponse;
                    if (cbk != null)
                        cbk(false, co);
                }
            }, req);
        }
    }
}
