using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;

namespace Sin.Http
{
    /// <summary>
    /// 缓存方式请求，主要用于图片的获取
    /// </summary>
    public class CachedRequest : Request
    {
        private IsolatedStorageFile Iso = IsolatedStorageFile.GetUserStoreForApplication();
        private String CacheDir;
        private const String LOCK = ".lock";
        private const int LOCKTIMEOUT = 10000;
        private const int LOCKSLEEP = 500;

        private bool _UseCache = true;
        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool UseCache
        {
            get
            {
                return _UseCache;
            }
            set
            {
                _UseCache = value;
            }
        }

        public CachedRequest(String dir)
        {
            this.CacheDir = (dir.StartsWith("/") ? "" : "/") + dir + (dir.EndsWith("/") ? "" : "/");
            String CacheDriectory = CacheDir.TrimEnd('/');
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.DirectoryExists(CacheDriectory) == false)
            {
                iso.CreateDirectory(CacheDriectory);
            }
            // DebugInfo(true);
        }

        public void Clear()
        {
            DebugInfo(true, true);
        }

        public void DebugInfo(bool dellock = false, bool delall = false)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                long size = 0;
                Debug.WriteLine("Cache " + CacheDir);
                foreach (String s in CacheFiles)
                {
                    IsolatedStorageFileStream isfs = Iso.OpenFile(s, FileMode.Open);
                    size += isfs.Length;
                    Debug.WriteLine(" {0} {1}", s, Utils.StringUtils.HumanSize(isfs.Length));
                    isfs.Close();

                    if (delall || (s.EndsWith(LOCK) && dellock))
                        Iso.DeleteFile(s);
                }
                Debug.WriteLine(" Totle Size: {0}", Utils.StringUtils.HumanSize(size));
            }
            else if (dellock || delall)
            {
                foreach (String s in CacheFiles)
                {
                    if (delall || s.EndsWith(LOCK))
                        Iso.DeleteFile(s);
                }
            }
            if (delall)
                _CacheSize = 0;
        }

        private long _CacheSize = -1;
        private long _TempAdd = -1;
        public long CacheSize
        {
            get
            {
                if (_CacheSize < 0)
                {
                    _CacheSize = 0;
                    System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                    {
                        foreach (String s in CacheFiles)
                        {
                            try
                            {
                                IsolatedStorageFileStream isfs = Iso.OpenFile(s, FileMode.Open);
                                _CacheSize += isfs.Length;
                                isfs.Close();
                            }
                            catch
                            {
                            }
                        }
                        if (_TempAdd > 0)
                            _CacheSize += _TempAdd;
                        _TempAdd = -1;
                    });
                }
                return _CacheSize;
            }
        }

        public String[] CacheFiles
        {
            get
            {
                String[] names = Iso.GetFileNames(this.CacheDir);
                String[] paths = new String[names.Length];
                for (int i = 0; i < paths.Length; ++i)
                {
                    paths[i] = CacheDir + names[i];
                }
                return paths;
            }
        }

        public Stream StreamOf(String fln)
        {
            return Iso.OpenFile(fln, FileMode.Open);
        }

        public String FileOfUrl(String url, String method = "GET")
        {
            String md5 = Sin.Utils.StringUtils.MD5(method + " " + url);
            String fln = CacheDir + md5;
            return fln;
        }
        public bool IsCachedUrl(String url, String method = "GET")
        {
            String fln = FileOfUrl(url, method);
            return Iso.FileExists(fln);
        }
        public Uri UriOfUrl(String url, String method = "GET")
        {
            String fln = FileOfUrl(url, method);
            return new Uri("isostore:" + fln);
            /*
            if (Iso.FileExists(fln))
            {
                return new Uri("isostore:" + fln);
            }
            else
            {
                return new Uri(url, UriKind.Absolute);
            }
             * */
        }

        override public void ReadyRequest(String url, String method, Object data, RequestCallback cbk)
        {
            if (!UseCache)
            {
                // 不启用缓存
                base.ReadyRequest(url, method, data, cbk);
                return;
            }


            // String md5 = Sin.Utils.StringUtils.MD5(method + " " + url);
            String fln = FileOfUrl(url, method);
            String flnlock = fln + LOCK;

            lock (this)
            {
                if (Iso.FileExists(fln))
                {
                    //Debug.WriteLine("from cache of " + fln + " " + url);
                    using (Stream stream = Iso.OpenFile(fln, FileMode.Open))
                    {
                        CallbackObject co = new CallbackObject();
                        co.ResponseStream = stream;
                        if (cbk != null)
                            cbk(true, co);
                    }
                }
                else
                {
                    RequestCallback mrcbk = (ok, co) =>
                    {
                        if (ok)
                        {
                            if (co.Response.ContentLength > 0)
                            {
                                if (_TempAdd < 0)
                                    _CacheSize += co.Response.ContentLength;
                                else
                                    _TempAdd = co.Response.ContentLength;

                                Stream sr = null;
                                Stream sw = null;
                                try
                                {
                                    sr = co.ResponseStream;
                                    sw = Iso.CreateFile(fln);
                                    sr.CopyTo(sw);
                                }
                                catch
                                {
                                    ok = false;
                                }
                                finally
                                {
                                    sw.Close();
                                    sr.Close();
                                }
                                if (cbk != null)
                                {
                                    lock (this)
                                    {
                                        using (Stream stream = Iso.OpenFile(fln, FileMode.Open))
                                        {
                                            co.ResponseStream = stream;
                                            cbk(ok, co);
                                        }
                                    }
                                }
                            }
                        }
                        lock (this)
                        {
                            Iso.DeleteFile(flnlock);
                        }
                        return cbk(ok, co);
                    };

                    if (Iso.FileExists(flnlock))
                    {
                        //Debug.WriteLine("lock file " + flnlock+ " " + url);
                        System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                        {
                            int timeout = 0;
                            do
                            {
                                lock (this)
                                {
                                    if (!Iso.FileExists(flnlock))
                                        break;
                                }
                                System.Threading.Thread.Sleep(LOCKSLEEP);
                                timeout += LOCKSLEEP;
                                if (timeout >= LOCKTIMEOUT)
                                    break;
                            } while (true);

                            //Debug.WriteLine("exit lock file " + fln + " " + url);
                            if (Iso.FileExists(fln))
                            {
                                lock (this)
                                {
                                    using (Stream stream = Iso.OpenFile(fln, FileMode.Open))
                                    {
                                        CallbackObject co = new CallbackObject();
                                        co.ResponseStream = stream;
                                        if (cbk != null)
                                            cbk(true, co);
                                    }
                                }
                            }
                            else
                            {
                                Debug.WriteLine("no file " + flnlock + " " + url);
                            }
                        });
                    }
                    else
                    {
                        lock (this)
                        {
                            Iso.CreateFile(flnlock).Close();
                        }
                        base.ReadyRequest(url, method, data, mrcbk);
                    }
                }
            }
        }
    }
}
