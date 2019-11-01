using M5.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace M5.Base.Common
{

    public class SubDomains
    {

        #region 路径处理
        public void replaceUrl(ref string html)
        {
            html = Regex.Replace(html, "(?<=(href|src|action)=(\"|'| ))(?!(http))(.*?)(?=(\"|'| |>))", new MatchEvaluator(_replaceUrl), RegexOptions.IgnoreCase);

        }
        public bool isMobile = false;
        bool isMobileHost = BaseConfig.mobileUrl.IndexOf("http") == 0;//手机站点为独立域名
        string _replaceUrl(string url)
        {
            return _replaceUrl(url, false, false);
        }
        string _replaceUrl(string url, bool isMobile, bool isMobileHost)
        {
            bool flag = false;//是否存在虚拟站点
            if (Config.domainList != null)
            {
                for (int i = 0; i < Config.domainList.Count; i++)
                {

                    string virtualWebDir = Config.webPath + "/" + Config.domainList[i][1] + "/";
                    if (Regex.IsMatch(url, virtualWebDir, RegexOptions.IgnoreCase))
                    {
                        if (Config.domainList[i][0] == "")
                        {
                            //绑定目录
                            if (isMobile)
                            {
                                if (isMobileHost)
                                {
                                    url = Regex.Replace(url, "^" + virtualWebDir, BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                                }
                                else
                                {
                                    if (!Regex.IsMatch(url, "^" + virtualWebDir + BaseConfig.mobileUrl, RegexOptions.IgnoreCase)) url = Regex.Replace(url, "^" + virtualWebDir, virtualWebDir + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                                }
                            }
                        }
                        else
                        {
                            //绑定域名
                            if (isMobile)
                            {
                                if (Config.domainList[i][2] != "")//绑定有手机域名
                                {
                                    url = Regex.Replace(url, "^" + virtualWebDir, "http://" + Config.domainList[i][2] + "/", RegexOptions.IgnoreCase);
                                }
                                else if (isMobileHost)
                                {
                                    url = Regex.Replace(url, "^" + virtualWebDir, BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                                }
                                else
                                {
                                    url = Regex.Replace(url, "^" + virtualWebDir, "http://" + Config.domainList[i][0] + "/" + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                                }
                            }
                            else
                            {
                                url = Regex.Replace(url, "^" + virtualWebDir, "http://" + Config.domainList[i][0] + "/", RegexOptions.IgnoreCase);
                            }

                        }
                        flag = true;
                        break;
                    }
                }
            }
            #region 不存在虚拟站点的地址处理
            if (!flag)
            {
                if (isMobile)
                {
                    if (isMobileHost)
                    {
                        url = Regex.Replace(url, "^" + Config.webPath + "/", BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        if (!Regex.IsMatch(url, "^" + Config.webPath + "/" + BaseConfig.mobileUrl, RegexOptions.IgnoreCase)) url = Regex.Replace(url, "^" + Config.webPath + "/", Config.webPath + "/" + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                    }
                }
            }
            #endregion
            if (BaseConfig.urlConversion && BaseConfig.mainUrl != null)
            {
                try
                {
                    Uri u = new Uri(BaseConfig.mainUrl, url);
                    url = u.ToString();
                }
                catch
                {
                }
            }
            return url;
        }
        string _replaceUrl(Match m)
        {
            if (!Regex.IsMatch(m.Value, "(" + BaseConfig.extension + "|/)$", RegexOptions.IgnoreCase)) return m.Value;
            string url = _replaceUrl(m.Value, isMobile, isMobileHost);
            return url;
        }
        #endregion
    }
}
