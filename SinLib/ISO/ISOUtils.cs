using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;

using System.Xml.Serialization;
using System.Xml.Linq;

namespace Sin.ISO
{
    /// <summary>
    /// ISO存储封装
    /// </summary>
    public class ISOUtils
    {
        static private IsolatedStorageFile Iso = IsolatedStorageFile.GetUserStoreForApplication();

        /// <summary>
        /// 显示所有的ISO文件，用于调试
        /// </summary>
        /// <param name="path"></param>
        static public void ListAllFiles(String path=null)
        {
            if (path == null)
                System.Diagnostics.Debug.WriteLine("-------LIST-------");
            System.Diagnostics.Debug.WriteLine(path == null ? "/" : "/" + path);
            foreach (String fl in (path == null ? Iso.GetFileNames() : Iso.GetFileNames(path)))
            {
                System.Diagnostics.Debug.WriteLine(" "+fl);
            }

            foreach (String dir in (path == null ? Iso.GetDirectoryNames() : Iso.GetDirectoryNames(path)))
            {
                ListAllFiles((path==null?"":path) + dir + "/");
            }
        }

        /// <summary>
        /// 获取ISO文件输入流
        /// </summary>
        /// <param name="filename">文件名（包括路径）</param>
        /// <returns>流</returns>
        static public IsolatedStorageFileStream GetIsoInputStream(String filename)
        {
            if (Iso.FileExists(filename))
            {
                return Iso.OpenFile(filename, FileMode.Open, FileAccess.Read);
            }
            else
            {
                throw new FileNotFoundException(filename + "not found");
            }
        }

        /// <summary>
        /// 获取ISO文件输出流
        /// </summary>
        /// <param name="filename">文件名（包括路径）</param>
        /// <returns>流</returns>
        static public IsolatedStorageFileStream GetIsoOutputStream(String filename)
        {
            return Iso.OpenFile(filename, FileMode.Create, FileAccess.Write);
        }

        /// <summary>
        /// 获取ISO文件图片
        /// </summary>
        /// <param name="filename">文件名（包括路径）</param>
        /// <returns>图片</returns>
        static public BitmapImage GetIsoImage(String filename)
        {
            BitmapImage bi = null;
            using (var stream = GetIsoInputStream(filename))
            {
                bi = new BitmapImage();
                bi.SetSource(stream);
            }
            return bi;
        }

        /// <summary>
        /// 获取ISO文件文本
        /// </summary>
        /// <param name="filename">文件名（包括路径）</param>
        /// /// <param name="encoding">编码方式，默认为UTF-8</param>
        /// <returns>文本</returns>
        static public String GetIsoString(String filename, Encoding encoding)
        {
            String txt = null;
            if (Iso.FileExists(filename))
            {
                using (var stream = Iso.OpenFile(filename, FileMode.Open, FileAccess.Read))
                {
                    StreamReader sr = new StreamReader(stream, encoding);
                    txt = sr.ReadToEnd();
                    sr.Close();
                }
            }
            return txt;
        }

        /// <summary>
        /// 获取ISO文件文本
        /// </summary>
        /// <param name="filename">文件名（包括路径）</param>
        /// <returns>文本</returns>
        static public String GetIsoString(String filename)
        {
            return GetIsoString(filename, Encoding.UTF8);
        }

        /// <summary>
        /// 将文本保存到ISO文件
        /// </summary>
        /// <param name="filename">文件名（包括路径）</param>
        /// /// /// <param name="encoding">编码方式，默认为UTF-8</param>
        /// <param name="text">文本</param>
        static public void SaveIsoString(String filename, String text, Encoding encoding)
        {
            using (var stream = Iso.OpenFile(filename, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(stream, encoding);
                sw.Write(text);
                sw.Close();
            }
        }

        /// <summary>
        /// 将文本保存到ISO文件
        /// </summary>
        /// <param name="filename">文件名（包括路径）</param>
        /// <param name="text">文本</param>
        static public void SaveIsoString(String filename, String text)
        {
            SaveIsoString(filename, text, Encoding.UTF8);
        }


        /// <summary>
        /// 删除ISO文件
        /// </summary>
        /// <param name="filename">文件名（包括路径）</param>
        static public void Delete(String filename)
        {
            if (Iso.FileExists(filename))
            {
                Iso.DeleteFile(filename);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static public Object DeSerializeFromIso(String filename, Type type)
        {
            XmlSerializer xs = new XmlSerializer(type);
            Object ret = null;
            System.IO.Stream stream = GetIsoInputStream(filename);
            ret = xs.Deserialize(stream);
            stream.Close();
            return ret;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="obj"></param>
        static public void SerializeToIso(String filename, Object obj)
        {
            XmlSerializer xs = new XmlSerializer(obj.GetType());
            System.IO.Stream stream = GetIsoOutputStream(filename);
            xs.Serialize(stream, obj);
            stream.Flush();
            stream.Close();
        }

        public static void SaveDictToIso(Dictionary<String, String> dict, String filename)
        {
            System.Diagnostics.Debug.WriteLine("Save: " + dict.Count);
            XDocument xd = new XDocument();
            XElement rt = new XElement("Dictionary");
            if (dict != null)
            {
                System.Diagnostics.Debug.WriteLine("Save " + dict.Count);
                Dictionary<String, String>.Enumerator em = dict.GetEnumerator();
                while (em.MoveNext())
                {
                    XElement el = new XElement("Item");
                    el.SetAttributeValue("Name", em.Current.Key);
                    el.SetAttributeValue("Value", em.Current.Value);
                    rt.Add(el);
                }
            }
            xd.Add(rt);
            Stream stm = Sin.ISO.ISOUtils.GetIsoOutputStream(filename);
            xd.Save(stm);
            stm.Flush();
            stm.Close();
        }

        public static Dictionary<String, String> GetDictFromIso(String filename)
        {
            Dictionary<String, String> coll = new Dictionary<string, string>();
            Stream stm = Sin.ISO.ISOUtils.GetIsoInputStream(filename);
            XDocument xd = XDocument.Load(stm);
            XElement rt = xd.Root;
            foreach (XElement el in rt.Nodes())
            {
                coll[(String)el.Attribute("Name").Value] = (String)el.Attribute("Value").Value;
            }

            stm.Close();
            System.Diagnostics.Debug.WriteLine("Load: " + coll.Count);
            return coll;
        }
    }
}
