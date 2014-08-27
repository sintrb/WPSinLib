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
    public class CachedRequest: Request
    {
        private IsolatedStorageFile Iso = IsolatedStorageFile.GetUserStoreForApplication();
        private String CacheDir;
        private const String LOCK = ".lock";
        private const int LOCKTIMEOUT = 10000;
        private const int LOCKSLEEP = 500;
        public CachedRequest(String dir)
        {
            this.CacheDir = (dir.StartsWith("/") ? "" : "/") + dir + (dir.EndsWith("/") ? "" : "/");
            String CacheDriectory = CacheDir.TrimEnd('/');
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.DirectoryExists(CacheDriectory) == false)
            {
                iso.CreateDirectory(CacheDriectory);
            }
            DebugInfo(true);
        }

        public void Clear()
        {
            DebugInfo(true, true);
        }

        public void DebugInfo(bool dellock=false, bool delall=false)
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
        }

        public long CacheSize
        {
            get
            {
                long size = 0;
                foreach (String s in CacheFiles)
                {
                    try
                    {
                        IsolatedStorageFileStream isfs = Iso.OpenFile(s, FileMode.Open);
                        size += isfs.Length;
                        isfs.Close();
                    }
                    catch
                    {
                    }
                }
                return size;
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


        override public void ReadyRequest(String url, String method, Object data, RequestCallback cbk)
        {
            String md5 = Sin.Utils.StringUtils.MD5(method + " " + url);
            String fln = CacheDir + md5;
            String flnlock = fln + LOCK;

            lock (this)
            {
                if (Iso.FileExists(fln))
                {
                    Debug.WriteLine("from cache of " + fln + " " + url);
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
                        Debug.WriteLine("lock file " + flnlock+ " " + url);
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

                            Debug.WriteLine("exit lock file " + fln + " " + url);
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
