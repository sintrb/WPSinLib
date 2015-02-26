using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Clz.Types
{
    public class SelectorArray
    {
        public List<Object> _Values = null;
        public List<Object> Values
        {
            get
            {
                if (_Values == null)
                    _Values = new List<object>();
                return _Values;
            }
            set
            {
                _Values = value;
            }
        }
    }
}
