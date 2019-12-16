using M5.Base.Common;
using M5.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using MWMS.Helper;
using MWMS.Helper.Extensions;
using MWMS.SqlHelper;
using MWMS.Template;
using MySql.Data.MySqlClient;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace M5.Main
{
    public class WebService
    {

        public static HttpContext HttpContext = null;
        protected void Application_Start()
        {
            #region 防止多次执行
            //if (applicationFlag) return;
            //applicationFlag = true;
            Sql.connectionString = @"Data Source=" + Config.getAppSettings("ServerIP") + ";Initial Catalog=" + Config.getAppSettings("DataBaseName")+ ";Integrated Security=false;UID=" + Config.getAppSettings("Username")+ ";PWD=" + Config.getAppSettings("Password") + ";Pooling=true;MAX Pool Size=512;Min Pool Size=8;Connection Lifetime=5";
            #endregion
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Tools.MapPath("~" + Config.cachePath));
            if (!dir.Exists) dir.Create();
            if (Config.install) return;
        }
        /// <summary>
        /// 是否允许访问后台
        /// </summary>
        /// <returns></returns>
        bool allowAccessManagementIP(HttpRequest Request)
        {
            string ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            if (Config.allowAccessManagementIP != null)
            {
                if ((Regex.IsMatch(Request.Path, "^" + Config.webPath + Config.managePath, RegexOptions.IgnoreCase)))
                {
                    for (int i = 0; i < Config.allowAccessManagementIP.Length; i++)
                    {
                        if (Config.allowAccessManagementIP[i] != "")
                        {
                            string reg = Config.allowAccessManagementIP[i].Replace("*", @"\d{1,3}");
                            if (Regex.IsMatch(ip, reg)) return true;
                        }
                    }
                    return false;
                }
            }
            return true;
        }
        public void BeginRequest(HttpContext context)
        {
               HttpRequest Request = context.Request;
            HttpResponse Response = context.Response;
            if (!allowAccessManagementIP(Request)) Page.ERR404("非法访问");
            /*#region 非系统网页扩展名时处理方式(非文件跳转目录否则不再处理)

            if (!(Regex.IsMatch(Request.Path, "(.*)(/|" + BaseConfig.extension + ")$", RegexOptions.IgnoreCase)))
            {
                if (application.Request.Url.Segments[application.Request.Url.Segments.Length - 1].IndexOf(".") == -1)
                {

                    API.ERR301(application.Request.Url.ToString() + @"/");
                }
                return;
            }
            #endregion
            PageContext.Current.Response.ContentType = "text/html; charset=" + System.Text.Encoding.Default.HeaderName;
            //injection(Request);//注入过滤
            string acceptTypes = Request.Headers["Accept"];
            if (!string.IsNullOrEmpty(acceptTypes) && acceptTypes.ToLower().IndexOf("text/vnd.wap.wml") > -1)
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }
            */

            injection(Request);//注入过滤
                string html = RewriteUrl(context);//载入映射规则
                                                  //Response.ContentType = "html/plain; charset="+System.Text.Encoding.Default.HeaderName;
                if (html != null) Response.WriteAsync(html);




        }
        /// <summary>
        /// 手机访问
        /// </summary>
        /// <param name="Request"></param>
        //void mobileAccess(HttpResponse Response, HttpRequest Request)
        //{
        //    if (API.isMobileAccess() && !API.getWebFAId())//手机访问 但非手机域名 转向
        //    {
        //        string newUrl=Regex.Replace(Request.Url.AbsolutePath, "^" + Config.webPath+@"/", "", RegexOptions.IgnoreCase);
        //        string murl = (new Uri(BaseConfig.mobileUrl, newUrl)).ToString();
        //        Response.Redirect(murl);
        //    }
        //}

        /// <summary>
        /// 防止sql注入请求
        /// </summary>
        void injection(HttpRequest Request)
        {
            if (!Page.safetyVerification(Request.Path.Value))
            {
                Page.ERR301("/");
            }
            /* if (!Tools.safetyVerification(Request.QueryString[n]))
             {
                 string UName = "-1";
                 if (Request.Cookies["AdminUserID"] != null) UName = Request.Cookies["AdminUserID"].Value;
                 Tools.writeLog("3", "危险请求" + Request.Path.ToString());
                 Page.ERR301("/");
             }*/
            /*for (int n = 0; n < Request.QueryString.Count; n++)
            {
                
                if (!API.safetyVerification(Request.QueryString[n]))
                {
                    string UName = "-1";
                    if (Request.Cookies["AdminUserID"] != null) UName = Request.Cookies["AdminUserID"].Value;
                    Tools.writeLog("3", "危险请求" + Request.RawUrl);
                    API.ERR301("/");
                }
        }*/
        }

        static void mobileRedirect(string url, ref bool isMobilePage)
        {
            /*
            if (BaseConfig.mobileRedirectType == 1)
            {
                PageContext.Current.Response.Write("<script>location.href='" + url + "';</script>");
                PageContext.Current.Response.End();
            }
            else if (BaseConfig.mobileRedirectType == 2)
            {
                isMobilePage = true;
            }
            else
            {
                PageContext.Current.Response.Redirect(url);
            }*/
        }
        public static string urlZhuanyi(Uri rUrl, ref bool isMobilePage, ref string virtualWebDir)
        {
            string url = rUrl.AbsolutePath;
            url = Regex.Replace(url, "^" + Config.webPath, "", RegexOptions.IgnoreCase).ToLower();
            string newUrl = url;
            virtualWebDir = "/";//虚拟站点目录
            #region 自定义绑定域名处理
            if (Config.domainList != null)
            {
                for (int i = 0; i < Config.domainList.Count; i++)
                {
                    if (String.Compare(Config.domainList[i][0], rUrl.Host, true) == 0)//域名转换为目录
                    {
                        newUrl = "/" + Config.domainList[i][1] + url;
                        if (Config.domainList[i][2] != "" && Tools.isMobileAccess())
                        { //使用手机访问
                            mobileRedirect("http://" + Config.domainList[i][2] + url, ref isMobilePage);
                        }
                    }
                    else if (String.Compare(Config.domainList[i][2], rUrl.Host, true) == 0)//手机域名转换
                    {
                        newUrl = "/" + Config.domainList[i][1] + url;
                        isMobilePage = true;
                    }
                    else if (Regex.IsMatch(url, "^/" + Config.domainList[i][1] + "/", RegexOptions.IgnoreCase))//访问是原路径
                    {
                        virtualWebDir = "/" + Config.domainList[i][1] + "/";
                        string newurl = "/" + Regex.Replace(url, "^/" + Config.domainList[i][1] + "/", "", RegexOptions.IgnoreCase);
                        if (Config.domainList[i][2] != "" && Tools.isMobileAccess()) mobileRedirect("http://" + Config.domainList[i][2] + newurl, ref isMobilePage);
                        if (Config.domainList[i][0] != "") Page.ERR301("http://" + Config.domainList[i][0] + newurl);

                    }
                }
            }
            #endregion
            if (!isMobilePage)
            {
                if (BaseConfig.mobileUrl != "")
                {
                    if (BaseConfig.mobileUrl.IndexOf("http") > -1)
                    {
                        isMobilePage = Regex.IsMatch(rUrl.AbsoluteUri, "^" + BaseConfig.mobileUrl);
                    }
                    else
                    {
                        isMobilePage = Regex.IsMatch(newUrl, "^" + virtualWebDir + BaseConfig.mobileUrl);
                    }
                }
                if (isMobilePage)
                {

                    if (BaseConfig.mobileUrl.IndexOf("http") > -1)
                    {
                        //isMobilePage = Regex.IsMatch(newUrl, "^" + BaseConfig.mobileUrl);
                    }
                    else
                    {
                        newUrl = Regex.Replace(newUrl, "^" + virtualWebDir + BaseConfig.mobileUrl, virtualWebDir);
                    }
                }
            }
            return newUrl;
        }
        string  RewriteUrl(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            #region 已存在文件处理方式
            string file = request.Path;
            if (file.Substring(file.Length - 1) == "/") file += "index." + BaseConfig.extension;
            if (System.IO.File.Exists(Tools.MapPath(file))) return (null);
            #endregion

            bool isMobilePage = false;
            string virtualWebDir = "/";//虚拟站点目录
            string newUrl = urlZhuanyi(request.Url(), ref isMobilePage, ref virtualWebDir);
            bool isMobileAccess =Page.isMobileAccess();
  
            if (isMobileAccess && !isMobilePage)//手机访问 但非手机域名 转向
            {
                string murl = "";
                if (BaseConfig.mobileUrl.IndexOf("http") > -1)
                {
                    if (!Regex.IsMatch(newUrl, "^" + BaseConfig.mobileUrl))
                    {
                        murl = BaseConfig.mobileUrl + newUrl.Substring(1) + request.Url().Query;
                        if (BaseConfig.mobileRedirectType == 1)
                        {
                            context.Response.WriteAsync("<script>location.href='" + murl + "';</script>");
                            return "";
                        }
                        else if (BaseConfig.mobileRedirectType == 2)
                        {
                            //isMobilePage = true;
                        }
                        else
                        {
                            response.Redirect(murl);
                            return "";
                        }
                    }
                }
                else
                {
                    if (!Regex.IsMatch(newUrl, "^" + virtualWebDir + BaseConfig.mobileUrl))
                    {
                        string _murl = Regex.Replace(newUrl, "^" + virtualWebDir, virtualWebDir + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                        Uri nowurl = request.Url();
                        if (BaseConfig.mainUrl != null) nowurl = BaseConfig.mainUrl;
                        murl = new Uri(nowurl, Config.webPath + _murl).ToString() + request.Url().Query;
                        if (BaseConfig.mobileRedirectType == 1)
                        {
                            context.Response.WriteAsync("<script>location.href='" + Config.webPath + _murl + "';</script>");
                            //Response.End();
                        }
                        else if (BaseConfig.mobileRedirectType == 2)
                        {
                            //isMobilePage = true;
                        }
                        else
                        {
                            response.Redirect(murl);
                            return "";
                        }
                    }
                }
            }
            string M5_Login = (request.Cookies["M5_Login"] == null) ? "" : request.Cookies["M5_Login"];
            bool isLogin = M5_Login == "true";
            string cachePath=Tools.MapPath(Config.cachePath + (isMobilePage ? "mobile" : "default")+"/");
            PageCache pageCache = new PageCache(cachePath);
            string html = null;
            if(!isLogin) html=pageCache.Get(newUrl);
            if (html==null)
            {
                try
                {
                    html = getHtml(request, virtualWebDir, newUrl, isMobilePage);
                    pageCache.Set(newUrl, html);
                }
                catch (Exception ex)
                {
                    if (isLogin)
                    {
                        html = ex.Message;
                    }
                    else
                    {
                        html = pageCache.Get(newUrl);
                    }
                }
            }
            return html;
            /*
            PageCache pageCache = new PageCache();
            string html = pageCache.getCache(virtualWebDir, newUrl, isMobilePage);
            if (html != null)
            {
                Response.Write(html);
                sw.Stop();
                if (sw.ElapsedMilliseconds > 100)
                {
                    Tools.writeLog("time", sw.ElapsedMilliseconds.ToString() + "\t" + Request.Url.ToString());
                }
                //Response.Write("<!--页面执行时间：" + sw.ElapsedMilliseconds.ToString()+"-->");
                Response.End();
            }*/
        }

        int _pageNo = 1;
        string _fileName = "";
        string _replaceUrl(Match m)
        {
            _pageNo = int.Parse(Regex.Match(m.Value, @"(?<=_)((\d){1,5})(?=\.)").Value);
            if (Regex.IsMatch(m.Value, "^default_", RegexOptions.IgnoreCase))
            {
                return "";
            }
            else
            {
                return Regex.Replace(m.Value, @"_((\d){1,5})", "");
            }
        }
        public string getHtml(HttpRequest request, string virtualWebDir, string url, bool isMobile)
        {
            Config.systemVariables["webUrl"] = "http://" +request.Url();
            //            Config.systemVariables["webUrl"] = "http://" + PageContext.Current.Request.Url.Authority + Config.webPath;
            //Config.systemVariables["pageUrl"] = PageContext.Current.Request.Url.AbsoluteUri.ToString();// "http://" + PageContext.Current.Request.Url.Authority +""+ Config.webPath;
            _pageNo = 1;
            Regex r = new Regex(@"(?<=/)((.[^/]*)_((\d){1,5}))(." + BaseConfig.extension + ")", RegexOptions.IgnoreCase);
            string newUrl = r.Replace(url, new MatchEvaluator(_replaceUrl));
            PageTemplate pageTemplate = null;
            pageTemplate = new PageTemplate(newUrl, isMobile);
            pageTemplate.Build();
            //TemplateInfo info = TemplateClass.get(newUrl, isMobile);
            if (pageTemplate == null)
            {
                Page.ERR404("模板不存在");
            }
            else
            {
                /*
                if (info.u_type == 2)
                {
                    Helper.Sql.ExecuteNonQuery("update mainTable set clickCount=clickCount+1 where id=@id", new SqlParameter[]{
                    new SqlParameter("id",info.variable["id"])
                });
                }*/
                if (newUrl.IndexOf(".") > -1)
                {
                    string[] u = newUrl.Split('/');
                    _fileName = u[u.Length - 1].Replace("." + BaseConfig.extension, "");
                }
                else
                {
                    _fileName = "default";
                }

                pageTemplate.Variable.Add("_pageNo", _pageNo);
                pageTemplate.Variable.Add("_url", request.Url());
                pageTemplate.Variable.Add("_fileName", _fileName);
                string html = RazorEngine.Razor.Run(pageTemplate.TemplateId.ToString(), new object[] { Config.systemVariables, pageTemplate.Variable, pageTemplate.Parameter });

                SubDomains subDomains = new SubDomains();
                subDomains.isMobile = isMobile;
                subDomains.replaceUrl(ref html);
                return html;
            }
            return null;
        }

    }
}
