using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MWMS.Helper
{
    #region 字符串截取返回
    public struct String2
    {
        public bool V;//返回bool型
        public string String;//返回字符型
    }
    #endregion
    public class Tools
    {
        static int addDataCounter=0;
        #region 生成一个随机ID
        public static string GetId()
        {
            return (GetId(0));
        }
        public static string GetId(int n)
        {
            if (addDataCounter > int.MaxValue - 100) addDataCounter = 0;
            addDataCounter++;
            string id;
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            //id = ((long)((System.DateTime.Now.ToOADate() - 39781) * 1000000) - 432552).ToString() + rnd.Next(99).ToString("D2");
            //long webid = long.Parse(Config.webId.Substring(0, Config.webId.Length-2));
            id = ((System.DateTime.Now.Ticks - System.DateTime.Parse("2012-8-1").Ticks) / 10000000 + addDataCounter).ToString() + rnd.Next(99).ToString("D2");
            return (id);
        }
        #endregion

        #region 为字符串加省略
        public static String2 GetString(string str, int count)
        {
            String2 Value;
            if (count == 0)
            {
                Value.V = true;
                Value.String = str;
                return (Value);
            }
            char v;
            int n1 = 0;
            string str1 = "", str2 = "";
            for (int n = 0; n < str.Length; n++)
            {
                v = char.Parse(str.Substring(n, 1));
                if (v >= 0 && v <= 255)
                {
                    n1 = n1 + 1;
                }
                else { n1 = n1 + 2; }
                str1 = str1 + v;
                if (n1 >= count) { n = str.Length + 1; }
                if (n1 == count - 2 || n1 == count - 1) { str2 = str1; }
            }
            if (str1 == str) { Value.String = str; Value.V = true; }
            else { Value.String = str2 + "..."; Value.V = false; }
            return (Value);
        }
        #endregion
        public static string GetAppSettings(string name)
        {
            string value = "";
            if (ConfigurationManager.AppSettings[name] != null)
            {
                value = ConfigurationManager.AppSettings[name];
            }
            return (value);
        }

        /// <summary>
        /// 将变量写入文件
        /// </summary>
        /// <param name="file">文件名</param>
        /// <param name="data">变量名</param>
        public static void writeObjectFile(string file, object data)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            formatter.Serialize(stream, data);
            stream.Close();
        }
        /// <summary>
        /// 将变量从文件中读出
        /// </summary>
        /// <param name="file">文件名</param>
        /// <returns></returns>
        public static object readObjectFile(string file)
        {
            object data = null;
            if (System.IO.File.Exists(file))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream2 = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                data = formatter.Deserialize(stream2);
                stream2.Close();
            }
            return data;
        }
        public static string SaveImage(HttpPostedFile file, string filePath)
        {

                string path = filePath + System.DateTime.Now.ToString("yyyy-MM") + "/";
                if (!System.IO.Directory.Exists(HttpContext.Current.Server.MapPath(path))) System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~"+path));
                string kzm = "";
                if (file.FileName.LastIndexOf(".") > -1) kzm = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1).ToLower();
                if (!Regex.IsMatch(kzm, "(jpg|gif|png)"))
                {
                    throw new Exception("文件类型不合法，只能上传jpg,gif,png");
                }
                string fileName = GetId() + "." + kzm;
                file.SaveAs(HttpContext.Current.Server.MapPath(path + fileName));
                return path + fileName;
            
        }
    }
}
