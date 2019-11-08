using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using NETCore.Encrypt;
using System.Dynamic;
using Helper;
using PinyinForCore;

namespace MWMS.Helper.Extensions
{

    public static class ObjectExtensions
    {
        static string Md5(this string str)
        {
            var srcString = "Md5 hash";
            var hashed = EncryptProvider.Md5(str);
            return hashed;
        }
        static string GetString(string str, int count)
        {
            string value = "";
            if (count == 0)
            {
                return (str);
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
            if (str1 == str) { value = str; }
            else { value = str2 + "..."; }
            return (value);
        }
        public static string Left(this string text, int count)
        {
            return GetString(text.RemoveHtml(), count);
        }
        #region 取得HTML中字段项的值
        public static string GetHTMLValue(this string HTML, string FieldName)
        {
            string YH = @"\b";
            string YH2 = "\"";
            string headstr = FieldName + "=(\0| |\"|" + YH + ")";
            MatchCollection mc;
            Regex r = new Regex(@"(?<=" + headstr + @")(.*?)(?=( |}|" + YH2 + "|\r|\n))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            mc = r.Matches(HTML);
            if (mc.Count > 0) return (mc[0].Value.Replace("\"", ""));
            return ("");
        }
        #endregion
        public static string Item(this XmlNodeList list, string name)
        {
            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list.Item(i);
                if (node.Attributes["name"].Value == name)
                {
                    return node.InnerText;
                }
            }
            return "";
        }
        /// <summary>
        /// 转换字符串成拼音
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type">转换为全部小写 1首字母大写 2只取第一个字母</param>
        /// <returns></returns>
        public static string GetPinYin(this string str, int type=1)
        {
            if(type==1)return Pinyin.GetPinyin(str);
            else return Pinyin.GetInitials(str).ToLower();
        }
        public static string RemoveHtml(this string str)
        {
            str = Regex.Replace(str, @"<!--(.[^$]*?)-->", "");
            Regex r = new Regex(@"(\<.[^\<]*\>)"); //定义一个Regex对象实例
            str = r.Replace(str, "");
            Regex r1 = new Regex(@"(\<\/[^\<]*\>)"); //定义一个Regex对象实例
            str = r1.Replace(str, "");
            str = str.Replace("&nbsp;", "");
            str = str.Replace("<", "");
            str = str.Replace(">", "");
            //str=str.Replace("&","");
            return (str);
        }
        public static string FormatFileSize(this long Size)
        {

            string dw = "byte";
            double FileSize = Size;
            double ys = FileSize / 1024;
            if ((long)ys > 0) { FileSize = ys; dw = "KB"; }
            ys = FileSize / 1024;
            if ((long)ys > 0) { FileSize = ys; dw = "MB"; }
            ys = FileSize / 1024;
            if ((long)ys > 0) { FileSize = ys; dw = "GB"; }
            Size = (long)(FileSize * 100);
            FileSize = (double)Size / 100;
            return (FileSize.ToString() + " " + dw);
        }
        public static string FormatFileSize(this int Size)
        {

            string dw = "byte";
            double FileSize = Size;
            double ys = FileSize / 1024;
            if ((long)ys > 0) { FileSize = ys; dw = "KB"; }
            ys = FileSize / 1024;
            if ((long)ys > 0) { FileSize = ys; dw = "MB"; }
            ys = FileSize / 1024;
            if ((long)ys > 0) { FileSize = ys; dw = "GB"; }
            Size = (int)(FileSize * 100);
            FileSize = (double)Size / 100;
            return (FileSize.ToString() + " " + dw);
        }
        /// <summary>
        /// 截取两字符串间部分
        /// </summary>
        /// <param name="str"></param>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static string SubString(this string str, string str1, string str2)
        {
            if (str == null) return "";
            MatchCollection mc;
            Regex r = new Regex(@"(?<=" + str1 + ").*?(?=" + str2 + ")", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(str);
            if (mc.Count > 0) return (mc[0].Value); else { return (""); }

        }
        public static string MD5(this string Str)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(Str));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        //解密函数
        public static string Decryption(this string A_0, string A_1)
        {
            if (A_1.Length > 8) A_1 = A_1.Substring(0, 8);
            if (A_1.Length < 8) A_1 = A_1.PadRight(8);
            try
            {
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                byte[] buffer = new byte[A_0.Length / 2];
                for (int i = 0; i < (A_0.Length / 2); i++)
                {
                    int num2 = Convert.ToInt32(A_0.Substring(i * 2, 2), 0x10);
                    buffer[i] = (byte)num2;
                }
                provider.Key = Encoding.ASCII.GetBytes(A_1);
                provider.IV = Encoding.ASCII.GetBytes(A_1);
                MemoryStream stream = new MemoryStream();
                CryptoStream stream2 = new CryptoStream(stream, provider.CreateDecryptor(), CryptoStreamMode.Write);
                stream2.Write(buffer, 0, buffer.Length);
                stream2.FlushFinalBlock();
                StringBuilder builder = new StringBuilder();
                return Encoding.Default.GetString(stream.ToArray());
            }
            catch
            {
                return "";
            }
        }
        //加密函数
        public static string Encryption(this string A_0, string A_1)
        {
            if (A_1.Length > 8) A_1 = A_1.Substring(0, 8);
            if (A_1.Length < 8) A_1 = A_1.PadRight(8);
            try
            {
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                byte[] bytes = Encoding.Default.GetBytes(A_0);
                provider.Key = Encoding.ASCII.GetBytes(A_1);
                provider.IV = Encoding.ASCII.GetBytes(A_1);
                MemoryStream stream = new MemoryStream();
                CryptoStream stream2 = new CryptoStream(stream, provider.CreateEncryptor(), CryptoStreamMode.Write);
                stream2.Write(bytes, 0, bytes.Length);
                stream2.FlushFinalBlock();
                StringBuilder builder = new StringBuilder();
                foreach (byte num in stream.ToArray())
                {
                    builder.AppendFormat("{0:X2}", num);
                }
                builder.ToString();
                return builder.ToString();
            }
            catch
            {
                return "";
            }
        }
    }


    public class DynamicAttr : DynamicObject
    {
        //保存对象动态定义的属性值
        private Dictionary<string, object> _values;
        public DynamicAttr()
        {
            _values = new Dictionary<string, object>();
        }
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public void Clear()
        {
            _values.Clear();
        }
        public object Get(string propertyName)
        {
            // if (_values.ContainsKey(propertyName) == true)
            //{
            return _values[propertyName];
            //}
            return null;
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void Set(string propertyName, object value)
        {
            if (_values.ContainsKey(propertyName) == true)
            {
                _values[propertyName] = value;
            }
            else
            {
                _values.Add(propertyName, value);
            }
        }
        /// <summary>
        /// 实现动态对象属性成员访问的方法，得到返回指定属性的值
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = Get(binder.Name);
            return result == null ? false : true;
        }
        /// <summary>
        /// 实现动态对象属性值设置的方法。
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Set(binder.Name, value);
            return true;
        }
        /// <summary>
        /// 动态对象动态方法调用时执行的实际代码
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            return base.TryInvoke(binder, args, out result);
        }
    }
}
