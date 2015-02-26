using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
namespace Sin.Data
{
    [DataContract]
    public class SerialJObject
    {
        public JObject JObj;

        [DataMember]
        public String json
        {
            get
            {
                return JObj==null?"{}":JObj.ToString();
            }
            set
            {
                this.JObj = JObject.Parse(value);
            }
        }
        public SerialJObject(JObject jo)
        {
            this.JObj = jo;
        }
    }
}
