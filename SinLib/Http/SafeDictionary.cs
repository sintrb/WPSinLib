using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Http
{
    public class SafeDictionary
    {
        private Dictionary<String, String> _dict = new Dictionary<string, string>();
        public Dictionary<String, String> Dict
        {
            get
            {
                return _dict;
            }
            set
            {
                _dict = value;
            }
        }
        public String this[String key]
        {
            get
            {
                String ret = null;
                lock (this)
                {
                    try
                    {
                        ret = _dict[key];
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("No key: " + key);
                    }
                }
                return ret;
            }
            set
            {
                lock (this)
                {
                    _dict[key] = value;
                }
            }
        }
        public int Count
        {
            get
            {
                int ret = 0;
                lock (this)
                {
                    ret = _dict.Count;
                }
                return ret;
            }
        }
        public void Clear()
        {
            lock (this)
            {
                _dict.Clear();
            }
        }
        public int Add(Dictionary<String, String> dict)
        {
            Dictionary<String, String>.Enumerator em = dict.GetEnumerator();
            lock (this)
            {
                while (em.MoveNext())
                {
                    _dict[em.Current.Key] = em.Current.Value;
                }
            }
            return this.Count;
        }
    }
}
