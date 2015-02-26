using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Sin.Data
{
    [DataContract]
    public class JsonModel : INotifyPropertyChanged
    {
        protected String _JsonString;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(params String[] propertyNames)
        {
            if (null != PropertyChanged)
            {
                foreach (String propertyName in propertyNames)
                {
                    Debug.WriteLine("NotifyPropertyChanged " + propertyName);
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        public void DataUpdated()
        {
            this._JsonString = null;
        }

        [DataMember]
        public String JsonString
        {
            get
            {
                if (_JsonString == null)
                {
                    _JsonString = _Json == null ? null : _Json.ToString();
                }
                return _JsonString;
            }
            set
            {
                _JsonString = value;
                _Json = null;
            }
        }

        private JObject _Json;
        public JObject Json
        {
            get
            {
                if (_Json == null)
                {
                    _Json = _JsonString == null ? null : JObject.Parse(_JsonString);
                }
                return _Json;
            }
            set
            {
                _Json = value;
                _JsonString = null;
            }
        }
        protected void SetValue<T>(String name, T v)where T:JToken
        {
            if (Json == null)
                Json = new JObject();
            Json[name] = v;
            NotifyPropertyChanged(name);
            DataUpdated();
        }
        protected T GetValue<T>(String name, T dval)
        {
            JToken jo = GetJToken(name);
            if (jo == null)
            {
                return dval;
            }
            Object obj = null;

            if (dval is String)
            {
                obj = (String)jo;
            }
            else if (dval is int)
            {
                obj = (int)jo;
            }
            else if (dval is long)
            {
                obj = (long)jo;
            }
            else if (dval is bool)
            {
                obj = (bool)jo;
            }

            if (obj == null)
            {
                throw new Exception("未支持类型 + " + dval.GetType().ToString());
            }

            return (T)obj;
        }
        protected JToken GetJToken(String name)
        {
            JToken jo = null;
            if (Json != null)
            {
                if (!Json.TryGetValue(name, out jo))
                {
                    jo = null;
                }
            }
            if (jo == null)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("-------JsonModel({0}).{1} is null", this.GetType().ToString(), name));
            }
            return jo;
        }

        /*
        protected String GetString(String name, String defVal)
        {
            return GetValue(name, defVal);
        }



        protected long GetLong(String name, long defVal)
        {
            return GetValue(name, defVal);
        }
        protected bool GetBool(String name, bool defVal)
        {
            return Json != null ? (bool)GetObject(name) : defVal;
        }
        */


        protected JArray GetJArray(String name)
        {
            return Json != null ? (JArray)GetJToken(name) : null;
        }
        protected JObject GetJson(String name, JObject defVal)
        {
            return Json != null ? (JObject)GetJToken(name) : defVal;
        }
        private Dictionary<String, Object> _ObjectCache;
        private Dictionary<String, Object> ObjectCache
        {
            get
            {
                if (_ObjectCache == null)
                    _ObjectCache = new Dictionary<string, object>();
                return _ObjectCache;
            }
        }
        protected T GetCachedObject<T>(String name) where T : JsonModel, new()
        {
            if (ObjectCache.ContainsKey(name))
            {
                return (T)ObjectCache[name];
            }
            else
            {
                JObject jo = GetJson(name, null);
                if (jo != null)
                {
                    T o = new T()
                    {
                        Json = GetJson(name, null)
                    };
                    ObjectCache[name] = o;
                    return o;
                }
                return null;
            }
        }
        protected List<T> GetCachedList<T>(String name) where T : JsonModel, new()
        {
            if (ObjectCache.ContainsKey(name))
            {
                return (List<T>)ObjectCache[name];
            }
            else
            {
                List<T> list = new List<T>();
                int i = 0;
                JArray ja = GetJArray(name);
                int count = ja.Count;
                foreach (JToken jo in ja)
                {
                    if (jo is JObject)
                    {
                        ++i;
                        T o = new T()
                        {
                            Json = (JObject) jo
                        };
                        o.Tag = String.Format("{0}/{1}", i, count);
                        list.Add(o);
                    }
                }
                ObjectCache[name] = list;
                return list;
            }
        }
        public Object Tag { get; set; }
    }
}
