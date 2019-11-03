using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

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

        public static int GetStringLength(string oString)
        {
            byte[] strArray = System.Text.Encoding.Default.GetBytes(oString);
            int res = strArray.Length;
            return res;

        }
        public static string MapPath(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = System.IO.Directory.GetCurrentDirectory() + @"\wwwroot" + path.Replace("~", "");
            }
            else
            {
                path = System.IO.Directory.GetCurrentDirectory() + @"/wwwroot" + path.Replace("~", "");
                path = path.Replace("\\", "/");
            }
           
           return path;
        }
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
        public static bool isMobileAccess()
        {
            return false;
        }
        public static string GetHTMLValue(string HTML, string FieldName)
        {
            string YH = @"\b";
            string YH2 = "\"";
            string headstr = FieldName + "=(\0| |\"|" + YH + ")";
            MatchCollection mc;
            Regex r = new Regex(@"(?<=" + headstr + @")(.*?)(?=( |}|" + YH2 + "))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            mc = r.Matches(HTML);
            if (mc.Count > 0) return (mc[0].Value.Replace("\"", ""));
            return ("");
        }
        public static void writeLog(string type,string msg)
        {

        }

        #region 取得两个符串中间的部分
        public static string GetStrFG(string str1, string headstr, string endstr)
        {
            MatchCollection mc;
            Regex r = new Regex(@"(?<=" + headstr + ").*?(?=" + endstr + ")", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(str1);
            if (mc.Count > 0) return (mc[0].Value); else { return (""); }
        }
        #endregion

        #region 格式化字符串
        public static string FormatStr(string DateTime, string Type)
        {
            System.DateTime Date;
            try
            {
                switch (Type.ToUpper())
                {
                    case "HTMLENCODE":
                        return (System.Web.HttpUtility.HtmlEncode(DateTime));
                    case "ICO":
                        /*
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(PageContext.Current.Server.MapPath("~/config/DataAttribute.xml"));
                        string icostr = "";
                        XmlNode xn = xmlDoc.SelectSingleNode("Type");
                        XmlNodeList xnl = xn.ChildNodes;
                        if (xnl.Count > 0)
                        {
                            foreach (XmlNode xnf in xnl)
                            {
                                XmlNodeList xnf1 = xnf.ChildNodes;
                                if (DateTime.IndexOf(xnf1[2].InnerText) > -1)
                                {
                                    icostr += "<img src='" + xnf1[1].InnerText + "' class='ico'/>";
                                }
                            }
                        }
                        return (icostr);*/
                        return "";
                    case "MAXPIC":
                        DateTime = DateTime.ToUpper();
                        if (DateTime.IndexOf("MIN") > -1) { DateTime = DateTime.Replace("MIN", ""); }
                        return (DateTime);
                    case "YY.MM.DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString().Substring(2, 2) + "." + Date.Month.ToString().PadLeft(2, '0') + "." + Date.Day.ToString().PadLeft(2, '0'));
                    case "YY-MM-DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString().Substring(2, 2) + "-" + Date.Month.ToString().PadLeft(2, '0') + "-" + Date.Day.ToString().PadLeft(2, '0'));
                    case "YYYY.MM.DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "." + Date.Month.ToString().PadLeft(2, '0') + "." + Date.Day.ToString().PadLeft(2, '0'));
                    case "YYYY-MM-DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "-" + Date.Month.ToString().PadLeft(2, '0') + "-" + Date.Day.ToString().PadLeft(2, '0'));
                    case "YY-MM":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "-" + Date.Month.ToString().PadLeft(2, '0'));
                    case "MM-DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Month.ToString().PadLeft(2, '0') + "-" + Date.Day.ToString().PadLeft(2, '0'));
                    case "YYYY年MM月DD日":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "年" + Date.Month.ToString().PadLeft(2, '0') + "月" + Date.Day.ToString().PadLeft(2, '0') + "日");
                    case "YYYY年MM月":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "年" + Date.Month.ToString().PadLeft(2, '0') + "月");
                    case "999,999":
                        return (long.Parse(DateTime).ToString("###,###"));
                    case "999.99":
                        return (double.Parse(DateTime).ToString("###.##"));
                    case "PAGE":
                        string[] sArray = Regex.Split(DateTime, "<!-- PageSpacer -->");
                        int pageno = 1;
                        //if (M5.PageContext.Current.Request.QueryString["pageno"] != null && M5.PageContext.Current.Request.QueryString["pageno"].ToString() != "") pageno = int.Parse(M5.PageContext.Current.Request.QueryString["pageno"].ToString());
                        return (sArray[pageno - 1]);
                    case "PAGEBAR":
                        StringBuilder O = new StringBuilder("页码：");
                        int pn = 1;
                        //if (M5.PageContext.Current.Request.QueryString["pageno"] != null && M5.PageContext.Current.Request.QueryString["pageno"].ToString() != "") pn = int.Parse(M5.PageContext.Current.Request.QueryString["pageno"].ToString());
                        string[] s = Regex.Split(DateTime, "<!-- PageSpacer -->", RegexOptions.IgnoreCase);
                        string filename = "";
                        /*
                        filename = M5.PageContext.Current.Request.QueryString["dataid"];
                        if (filename == null) filename = M5.PageContext.Current.Request.QueryString["S_FileName"];

                        if (s.Length > 1)
                        {
                            for (int i = 1; i <= s.Length; i++)
                            {
                                if (i == pn) O.Append("<font>" + i.ToString() + "</font>");
                                else
                                {
                                    if (i == 1) O.Append("<a href='" + filename + "." + BaseConfig.extension + "'>" + i.ToString() + "</a>");
                                    else
                                    {
                                        O.Append("<a href='" + filename + "_" + i.ToString() + "." + BaseConfig.extension + "'>" + i.ToString() + "</a>");
                                    }
                                }
                            }
                            return (O.ToString());
                        }
                        else
                        {
                            return ("");
                        }*/
                        return "";
                    default:
                        return (DateTime);
                }
            }
            catch { return (DateTime); }
        }
        #endregion
        #region 取得url中参数项的值
        public static string GetUrlValue(string HTML, string FieldName)
        {
            string YH = @"\b";
            string YH2 = "\"";
            string headstr = YH + FieldName + "=";
            MatchCollection mc;
            Regex r = new Regex(@"(?<=" + headstr + @")(.*?)(?=(\&|$))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            mc = r.Matches(HTML);
            if (mc.Count > 0) return (mc[0].Value.Replace("\"", ""));
            return ("");
        }
        #endregion

        #region 去掉html中的html格式字符
        public static string nohtml(string str)
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
        #endregion

        #region 读取文本文件内容
        public static string GetFileText(string path)
        {
            string Content = null;
            if (System.IO.File.Exists(path))
            {
                FileInfo f = new FileInfo(path);
                Microshaoft.Text.IdentifyEncoding code = new Microshaoft.Text.IdentifyEncoding();
                string name = code.GetEncodingName(f);
                System.Text.Encoding e = code.GetEncoding(name);
                Content = File.ReadAllText(path, e);
            }
            else
            {
                Content = "";
            }
            return (Content);
        }
        #endregion
        public static long IPToNumber(string strIP)
        {
            if (strIP == "::1") strIP = "127.0.0.1";
            long Ip = 0;
            string[] addressIP = strIP.Split('.');
            Ip = Convert.ToUInt32(addressIP[3]) + Convert.ToUInt32(addressIP[2]) * 256 + Convert.ToUInt32(addressIP[1]) * 256 * 256 + Convert.ToUInt32(addressIP[0]) * 256 * 256 * 256;
            return (Ip);
        }

    }
}
