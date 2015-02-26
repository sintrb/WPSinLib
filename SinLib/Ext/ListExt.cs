using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Ext
{
    public static class ListExt
    {
        public static void Push<T>(this List<T> lst, T o)
        {
            lst.Add(o);
        }
        public static T Pop<T>(this List<T> lst)
        {
            if (lst.Count > 0)
            {
                T o = lst[lst.Count - 1];
                lst.RemoveAt(lst.Count - 1);
                return o;
            }
            else
                return default(T);
        }
        public static T Peek<T>(this List<T> lst)
        {
            return lst.Count > 0 ? lst[lst.Count - 1] : default(T);
        }
    }
}
