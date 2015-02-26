using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;

namespace Sin.ISO
{
    public class SettingHelper
    {
        private IsolatedStorageSettings _Settings;
        public IsolatedStorageSettings Settings
        {
            get { return _Settings; }
        }
        public SettingHelper(IsolatedStorageSettings settings)
        {
            this._Settings = settings;
        }
        public Object this[String k]
        {
            get
            {
                try
                {
                    return Settings[k];
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                Settings[k] = value;
            }
        }
        public Object GetOrCreate(String key, Object def)
        {
            Object obj = _Settings.Contains(key) ? _Settings[key] : null;
            if (obj == null)
            {
                _Settings[key] = def;
                return def;
            }
            else
            {
                return obj;
            }
        }

        public bool GetBool(String key, bool dft)
        {
            return (bool)GetOrCreate(key, dft);
        }
        public int GetInt(String key, int dft)
        {
            return (int)GetOrCreate(key, dft);
        }
        public String GetString(String key, String dft)
        {
            return (String)GetOrCreate(key, dft);
        }
        public void Set(String key, Object val)
        {
            _Settings[key] = val;
        }

        public void Save()
        {
            _Settings.Save();
        }
    }
}
