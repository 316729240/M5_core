using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace M5.Common
{

    public class SystemLink
    {
        static string Link = null, Color = null, Target = null, className = null;
        static int Count = 0, i = 0, CCount = 0;
        #region 添加系统内链
        public string Replace(string Str)
        {
            CCount = 0;//页面中全部内链个数
            string keyword = @"<(title|a|textarea|meta)[^>]*>.*?</(title|a|textarea|meta)>|<(div|table|td|img|a|input|meta)[^>]*>";
            try
            {
                //XmlDocument xmlDoc = new XmlDocument();
                //xmlDoc.Load(PageContext.Current.Server.MapPath("~/config/link.config"));
                //XmlNode xn = xmlDoc.SelectSingleNode("Link");
                XmlNodeList xnl = Config.userConfig["link"][0].ChildNodes;
                if (xnl != null && xnl.Count > 0)
                {
                    foreach (XmlNode xnf in xnl)
                    {
                        XmlNodeList xnf1 = xnf.ChildNodes;
                        if (xnf1.Item(0).InnerText != "")
                        {
                            Regex v1 = new Regex(keyword + "|" + xnf1.Item(0).InnerText, RegexOptions.IgnoreCase);
                            Link = xnf1.Item(1).InnerText;
                            Color = ((XmlElement)(xnf1.Item(0))).GetAttribute("Color");
                            Target = ((XmlElement)(xnf1.Item(1))).GetAttribute("Target");
                            className = ((XmlElement)(xnf1.Item(1))).GetAttribute("Class");
                            Count = 1;
                            try
                            {
                                Count = int.Parse(xnf1.Item(2).InnerText);
                            }
                            catch
                            {
                            }
                            if (Count > 0)
                            {

                                i = 0;
                                Str = v1.Replace(Str, new MatchEvaluator(ReplaceString));
                            }
                        }
                    }
                    return (Str);
                }
                else
                {
                    return (Str);
                }
            }
            catch
            {
                throw new NullReferenceException(@"读取内链设置时发生错误，您可以在后台文件管理中打开\config\link.config文件进行修改");
            }
        }
        static string ReplaceString(Match m)
        {
            if (Regex.IsMatch(m.ToString(), @"<(title|a|textarea|meta)[^>]*>.*?</(title|a|textarea|meta)>|<(div|table|td|img|a|input|meta)[^>]*>", RegexOptions.IgnoreCase))
            {
                return m.Value;
            }
            else
            {
                i++;
                if (i > Count) return m.Value;
                else
                {
                    if (Constant.maxLink != 0 && CCount >= Constant.maxLink) return (m.Value);
                    else
                    {
                        CCount++;
                        if (Color != "") Color = " style=\"color:" + Color + ";\" ";
                        if (className != "") className = " class=\"" + className + "\" ";
                        return ("<a href=\"" + Link + "\" target=\"" + Target + "\" " + Color + className + " Title=\"" + m.Value + "\" >" + m.Value + "</a>");
                    }
                }
            }
        }
        #endregion
    }
}
