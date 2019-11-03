using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.IO;
using System.Collections;
using System.Web;
using System.Collections.Specialized;
namespace Helper
{
    public class Http
    {
        string UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
        public DateTime LastUpdateDate = System.DateTime.Now;
        public CookieContainer mycook = new CookieContainer();
        string webCookie = "";
        string SESSID = "";
        //HttpCookieCollection cookies = new HttpCookieCollection();
        List<string[]> CookieList = new List<string[]>();
        System.Text.Encoding codeLange = System.Text.Encoding.Default;
        Uri mainurl;
        public Uri ResponseUri = null;//当前请求地址
        string Authorization = null;
        public Uri nowUri = null;
        public Http(Uri url)
        {
            init(url, Encoding.Default);
        }
        public Http(string url, Encoding c)
        {
            init(new Uri(url), c);
        }
        void init(Uri url, Encoding c)
        {
            mainurl = url;
            codeLange = c;
        }
        public static string postUrl(string url, NameValueCollection values)
        {
            return postUrl(url, values, Encoding.Default);
        }
        public static string postUrl(string url, NameValueCollection values, Encoding Encoding)
        {
            CookieAwareWebClient MyWebClient = new CookieAwareWebClient();
            MyWebClient.Encoding = Encoding;
            MyWebClient.Headers.Add("Accept", "text/html, application/xhtml+xml, */*");
            MyWebClient.Headers.Add("Accept-Language", "zh-CN");
            MyWebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            string p = "";
            for (int i = 0; i < values.Count; i++)
            {

                p += HttpUtility.UrlEncode(values.AllKeys[i], Encoding) + "=" + HttpUtility.UrlEncode(values[i], Encoding);
                p += "&";
            }
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            try
            {

                Byte[] pageData = MyWebClient.UploadData(url, "POST", Encoding.GetBytes(p));
                return (Encoding.GetString(pageData));
            }
            catch
            {

            }
            return ("");
        }
        public static string getUrl(string url)
        {
            return getUrl(url, Encoding.Default);
        }
        public static string getUrl(string url, Encoding Encoding)
        {

            WebClient MyWebClient = new WebClient();

            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            try
            {
                Byte[] pageData = MyWebClient.DownloadData(url);
                if (Encoding == null) return (Encoding.Default.GetString(pageData));
                else { return (Encoding.GetString(pageData)); }

            }
            catch
            {
                return ("");
            }
        }
        public static byte[] getByte(string url)
        {
            WebClient MyWebClient = new WebClient();
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            return MyWebClient.DownloadData(url);
        }

        public static void saveFile(string url, string path)
        {
            byte[] data = getByte(url);
            System.IO.File.WriteAllBytes(path, data);
        }
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="url">地址</param>
        /// <returns></returns>
        /// 
        public byte[] getVerificationCode(string url)
        {
            return (getVerificationCode(url, null));
        }
        public byte[] getVerificationCode(string url, string referer)
        {
            Uri u2 = new Uri(mainurl, url);
            CookieAwareWebClient MyWebClient = new CookieAwareWebClient();
            MyWebClient.Headers.Add("Accept", "text/html, application/xhtml+xml, */*");
            MyWebClient.Headers.Add("Accept-Language", "zh-CN");
            MyWebClient.Headers.Add("User-Agent", this.UserAgent);
            if (referer != null) MyWebClient.Headers.Add("Referer", referer);

            //MyWebClient.Headers.Add("Cookie", getCookieStr(u2));
            if (Authorization != null) MyWebClient.Headers.Add("Authorization", Authorization);
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            try
            {
                Byte[] pageData = MyWebClient.DownloadData(u2.AbsoluteUri);
                //setCookie(MyWebClient.ResponseHeaders["Set-Cookie"]);
                setCookie(MyWebClient._cookieContainer.GetCookieHeader(u2));

                //addCookie(MyWebClient.cookies);
                if (MyWebClient.ResponseHeaders["location"] != null)
                {
                    Uri u3 = new Uri(u2, MyWebClient.ResponseHeaders["location"].ToString());
                    return (getVerificationCode2(u3.AbsoluteUri, u2.AbsoluteUri));
                }
                else
                {
                    return (pageData);
                }
            }
            catch
            {
                return null;
            }
        }
        public byte[] getVerificationCode2(string url, string referer)
        {
            Uri u2 = new Uri(mainurl, url);
            CookieAwareWebClient MyWebClient = new CookieAwareWebClient();
            MyWebClient.Headers.Add("Accept", "text/html, application/xhtml+xml, */*");
            MyWebClient.Headers.Add("Accept-Language", "zh-CN");
            MyWebClient.Headers.Add("User-Agent", this.UserAgent);
            if (referer != null) MyWebClient.Headers.Add("Referer", referer);

            if (Authorization != null) MyWebClient.Headers.Add("Authorization", Authorization);
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            try
            {
                Byte[] pageData = MyWebClient.DownloadData(u2.AbsoluteUri);
                //setCookie(MyWebClient.ResponseHeaders["Set-Cookie"]);
                setCookie(MyWebClient._cookieContainer.GetCookieHeader(u2));
                if (MyWebClient.ResponseHeaders["location"] != null)
                {
                    Uri u3 = new Uri(u2, MyWebClient.ResponseHeaders["location"].ToString());
                    return (getVerificationCode2(u3.AbsoluteUri, u2.AbsoluteUri));
                }
                else
                {
                    return (pageData);
                }
            }
            catch
            {
                return null;
            }
        }
        public void setAuthorization(string u, string p)
        {
            byte[] bytes = codeLange.GetBytes(u + ":" + p);
            Authorization = "Basic " + Convert.ToBase64String(bytes);
        }
        public string post(string url, NameValueCollection values)
        {
            return post(url, values, null);
        }
        public string post(string url, NameValueCollection values, string referer)
        {
            Uri u2 = new Uri(mainurl, url);
            Console.WriteLine(u2);
            nowUri = u2;
            CookieAwareWebClient MyWebClient = new CookieAwareWebClient();
            MyWebClient.Encoding = this.codeLange;
            MyWebClient.Headers.Add("Accept", "text/html, application/xhtml+xml, */*");
            MyWebClient.Headers.Add("Accept-Language", "zh-CN");
            MyWebClient.Headers.Add("User-Agent", this.UserAgent);
            MyWebClient.Headers.Add("Cookie", getCookieStr(u2));
            //MyWebClient.Headers.Add("Cookie", "thw=cn; v=0; cna=n0hDDRyvfkwCATtsIIIdOTJi; _tb_token_=3US5SUg7IsSmcu8; lzstat_uv=38314302002160344654|1723936; lzstat_ss=1555326437_3_1421697244_1723936; CNZZDATA1000005116=1891360845-1421665997-%7C1421671399; uc3=nk2=CyMBDStsFw%3D%3D&id2=UoYY50bRzoW3&vt3=F8dATkKBF8jVvnm8sKY%3D&lg2=VFC%2FuZ9ayeYq2g%3D%3D; existShop=MTQyMTczMDcwMA%3D%3D; unt=hnylchf%26center; lgc=hnylchf; tracknick=hnylchf; sg=f78; cookie2=13c0d6181cd6197c2414ffdfe7d7b859; mt=np=; cookie1=UoDYQL4IWsRmy6qBNDZHcerPtX1qKYAxggxEZCZRBCw%3D; unb=176472297; t=1072a81fc0ae12b54650d89561b78729; publishItemObj=Ng%3D%3D; _cc_=Vq8l%2BKCLiw%3D%3D; tg=0; _l_g_=Ug%3D%3D; _nk_=hnylchf; cookie17=UoYY50bRzoW3; CNZZDATA1000005138=990648133-1421714366-http%253A%252F%252Fsubway.simba.taobao.com%252F%7C1421726058; CNZZDATA1000008471=1290382942-1421730760-http%253A%252F%252Fsubway.simba.taobao.com%252F%7C1421730760; CNZZDATA1000008461=911310030-1421665322-%7C1421730764; isg=4C7F7DA66868D425581F7558DBF290BF; CNZZDATA1000005141=1512813791-1421666780-%7C1421730347; uc1=lltime=1421730429&cookie14=UoW1FX38o7UOGw%3D%3D&existShop=true&cookie16=URm48syIJ1yk0MX2J7mAAEhTuw%3D%3D&cookie21=Vq8l%2BKCLiv0MyZ1zi973%2Bg%3D%3D&tag=3&cookie15=U%2BGCWk%2F75gdr5Q%3D%3D&pas=");
            MyWebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            if (referer != null) MyWebClient.Headers.Add("Referer", referer);
            if (Authorization != null) MyWebClient.Headers.Add("Authorization", Authorization);
            MyWebClient.Encoding = this.codeLange;
            string p = "";
            for (int i = 0; i < values.Count; i++)
            {

                p += HttpUtility.UrlEncode(values.AllKeys[i], this.codeLange) + "=" + HttpUtility.UrlEncode(values[i], this.codeLange);
                p += "&";
            }
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            try
            {

                Byte[] pageData = MyWebClient.UploadData(u2.AbsoluteUri, "POST", codeLange.GetBytes(p));
                //setCookie(MyWebClient.Headers["Cookie"].ToString());
                //this.mycook = MyWebClient._cookieContainer;
                setCookie(MyWebClient._cookieContainer.GetCookieHeader(u2));
                // setCookie(GetAllCookies(MyWebClient._cookieContainer));
                //addCookie(MyWebClient.cookies);
                //getCookieStr(u2);
                return (codeLange.GetString(pageData));
            }
            catch
            {
                webCookie = null;
            }
            return ("");
        }

        public string openUrl21(string url, NameValueCollection values, string referer)
        {
            Uri u2 = new Uri(mainurl, url);
            nowUri = u2;
            CookieAwareWebClient MyWebClient = new CookieAwareWebClient();
            MyWebClient.Encoding = this.codeLange;
            MyWebClient.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            MyWebClient.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            MyWebClient.Headers.Add("Accept-Encoding", "gzip, deflate");
            MyWebClient.Headers.Add("X-Requested-With", "XMLHttpRequest");
            //MyWebClient.Headers.Add("Connection", "keep-alive");
            MyWebClient.Headers.Add(HttpRequestHeader.KeepAlive, "true");
            MyWebClient.Headers.Add("Pragma", "no-cache");
            MyWebClient.Headers.Add("Cache-Control", "no-cache");

            MyWebClient.Headers.Add("User-Agent", this.UserAgent);
            //MyWebClient.Headers.Add("Cookie", getCookieStr(u2));
            //MyWebClient.Headers.Add("Cookie", "thw=cn; v=0; cna=n0hDDRyvfkwCATtsIIIdOTJi; _tb_token_=3US5SUg7IsSmcu8; lzstat_uv=38314302002160344654|1723936; lzstat_ss=1555326437_3_1421697244_1723936; CNZZDATA1000005116=1891360845-1421665997-%7C1421671399; uc3=nk2=CyMBDStsFw%3D%3D&id2=UoYY50bRzoW3&vt3=F8dATkKBF8jVvnm8sKY%3D&lg2=VFC%2FuZ9ayeYq2g%3D%3D; existShop=MTQyMTczMDcwMA%3D%3D; unt=hnylchf%26center; lgc=hnylchf; tracknick=hnylchf; sg=f78; cookie2=13c0d6181cd6197c2414ffdfe7d7b859; mt=np=; cookie1=UoDYQL4IWsRmy6qBNDZHcerPtX1qKYAxggxEZCZRBCw%3D; unb=176472297; t=1072a81fc0ae12b54650d89561b78729; publishItemObj=Ng%3D%3D; _cc_=Vq8l%2BKCLiw%3D%3D; tg=0; _l_g_=Ug%3D%3D; _nk_=hnylchf; cookie17=UoYY50bRzoW3; CNZZDATA1000005138=990648133-1421714366-http%253A%252F%252Fsubway.simba.taobao.com%252F%7C1421726058; CNZZDATA1000008471=1290382942-1421730760-http%253A%252F%252Fsubway.simba.taobao.com%252F%7C1421730760; CNZZDATA1000008461=911310030-1421665322-%7C1421730764; isg=4C7F7DA66868D425581F7558DBF290BF; CNZZDATA1000005141=1512813791-1421666780-%7C1421730347; uc1=lltime=1421730429&cookie14=UoW1FX38o7UOGw%3D%3D&existShop=true&cookie16=URm48syIJ1yk0MX2J7mAAEhTuw%3D%3D&cookie21=Vq8l%2BKCLiv0MyZ1zi973%2Bg%3D%3D&tag=3&cookie15=U%2BGCWk%2F75gdr5Q%3D%3D&pas=");
            MyWebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            if (referer != null) MyWebClient.Headers.Add("Referer", referer);
            if (Authorization != null) MyWebClient.Headers.Add("Authorization", Authorization);
            MyWebClient.Encoding = this.codeLange;
            string p = "";
            for (int i = 0; i < values.Count; i++)
            {

                p += HttpUtility.UrlEncode(values.AllKeys[i], this.codeLange) + "=" + HttpUtility.UrlEncode(values[i], this.codeLange);
                p += "&";
            }
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            try
            {

                Byte[] pageData = MyWebClient.UploadData(u2.AbsoluteUri, "POST", codeLange.GetBytes(p));
                //setCookie(MyWebClient.Headers["Cookie"].ToString());
                //this.mycook = MyWebClient._cookieContainer;
                setCookie(MyWebClient._cookieContainer.GetCookieHeader(u2));
                // setCookie(GetAllCookies(MyWebClient._cookieContainer));
                //addCookie(MyWebClient.cookies);
                //getCookieStr(u2);
                return (codeLange.GetString(pageData));
            }
            catch (Exception e)
            {
                webCookie = null;
                throw e;
            }
            return ("-1");
        }

        /*

        public void addCookie(HttpCookieCollection c)
        {
            for (int i = 0; i < c.Count; i++)
            {
                cookies.Remove(c[i].Name);
                cookies.Set(c[i]);
            }
        }*/
        public string getCookieStr(Uri url)
        {
            return mycook.GetCookieHeader(url);
            /*
            string cookiestr = "";
            for (int i = 0; i < cookies.Count; i++)
            {

                if (url.Host.IndexOf(cookies[i].Domain) > -1)
                {
                    cookiestr += cookies[i].Name;
                    cookiestr += "=";

                    cookiestr += PageContext.Current.Server.UrlEncode(cookies[i].Value);
                    cookiestr += ";";
                }

            }
            return cookiestr;*/
        }
        public string get(string url)
        {
            return (get(null, url));
        }
        public string get(string referer, string url)
        {

            Uri u2 = new Uri(mainurl, url);
            nowUri = u2;

            // u2 = new Uri("http://www.baidu.com");
            CookieAwareWebClient MyWebClient = new CookieAwareWebClient();
            MyWebClient.Headers.Add("Accept", "text/html, application/xhtml+xml, */*");
            MyWebClient.Headers.Add("Accept-Language", "zh-CN");
            MyWebClient.Headers.Add("User-Agent", this.UserAgent);
            //MyWebClient.Headers.Add("Cookie", getCookieStr(u2));
            // MyWebClient.Headers.Add("Cookie", "cookie2=1c26acc028f9a5604e3210550e8d5cea;t=1;uc1=2;uc3=3;a1=D;unt=f;a2=s;f1=1;sg=2;mt=3;e1=U;e2=w;skt=d;s=c;e3=s;_tb_token_=s;tg=0;_l_g_=s;_nk_=c;cookie17=0;a=1;a2=2;");
            if (Authorization != null) MyWebClient.Headers.Add("Authorization", Authorization);
            //MyWebClient.Headers.Add("Cookie", "PHPSESSID=0cefe02fbabf45cf491cd30be8027039; phpcms_auth=VmcHM1YzVAsCPAI0A2EKNwdnVGUDNABjV2tQNlVuDjtXMgc4UWECMlA4BDAANQM%2FAWkBNwUxUzgEaQpsBG8PZFYzB2ZWZlRnAlACYwNhCjMHYVQzA2AAYFc8UGNVbA4yV2wHZlE%2FAjJQbQQzAGADbQFmAW4FYVMwBGcKaAQ9D2NWMwdmVmZUZAI8");

            if (referer != null) MyWebClient.Headers.Add("Referer", referer);


            //ecmsloginnum=3; ecmslastlogintime=1356325904; ecmsloginnum=3; kgereloginuserid=62; kgereloginusername=%B4%A8%B4%A8; kgereloginrnd=vXnnwvhbqYsHnXBS24zT; kgereloginlevel=4; kgereeloginlic=empirecmslic; kgereloginadminstyleid=1; kgereloginecmsckpass=ac806cd0151b82fb4fe5cf1f53945469; kgerelogintime=1356400760; kgeretruelogintime=1356400760; kgereecmsdodbdata=deleted; ecmscheckkey=deleted;kgereloginuserid=62
            //MyWebClient.Headers.Add("Cookie", "ecmsloginnum=3; ecmslastlogintime=1356325904; ecmsloginnum=3; kgereloginuserid=62; kgereloginusername=%B4%A8%B4%A8; kgereloginrnd=Kqj4qQcWbx4hpaBRBTFJ; kgereloginlevel=4; kgereeloginlic=empirecmslic; kgereloginadminstyleid=1; kgereloginecmsckpass=1140c99a496979aba6dc7d0c11cb30b7; kgerelogintime=1356400917; kgeretruelogintime=1356400917; kgereloginuserid=62");
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            try
            {
                Byte[] pageData = MyWebClient.DownloadData(u2.AbsoluteUri);
                this.ResponseUri = MyWebClient.ResponseUri;
                //setCookie(MyWebClient.ResponseHeaders["Set-Cookie"]);

                //addCookie(MyWebClient.cookies);
                setCookie(MyWebClient._cookieContainer.GetCookieHeader(mainurl));
                if (MyWebClient.ResponseHeaders["location"] != null)
                {
                    Uri u3 = new Uri(u2, MyWebClient.ResponseHeaders["location"].ToString());
                    return (get(u2.AbsoluteUri, u3.AbsoluteUri));
                }
                else
                {
                    return (codeLange.GetString(pageData));
                }
            }
            catch
            {
                webCookie = null;
                return ("");
            }
        }


        public void clearCookie()
        {
            mycook = new CookieContainer();
        }
        public void addCookie(string cookiestr)
        {
            mycook.SetCookies(mainurl, cookiestr);
        }
        public string GetAllCookies(CookieContainer cc)
        {
            List<Cookie> lstCookies = new List<Cookie>();

            Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.Instance, null, cc, new object[] { });
            string cooklist = "";
            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                    | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies)
                    {
                        lstCookies.Add(c);
                        if (cooklist != "") cooklist += ";";
                        cooklist += c.ToString();
                    }
            }

            return cooklist;
        }
        public void setCookie(string Cookie)
        {

            if (Cookie != null && Cookie != "")
            {
                string[] cooklist = Cookie.Split(';');
                for (int i1 = 0; i1 < cooklist.Length; i1++)
                {
                    try
                    {
                        addCookie(cooklist[i1].Replace("$", ""));
                        int index = cooklist[i1].IndexOf('=');
                        string name = cooklist[i1].Substring(0, index);
                        string value = cooklist[i1].Substring(index + 1);

                        string[] item = cooklist[i1].Split('=');
                        //HttpCookie cookie = new HttpCookie(name);
                        //cookie.Value = value;
                        //cookie.Domain = mainurl.Host;
                        //cookies.Remove(cookie.Name);
                        //cookies.Set(cookie);
                    }
                    catch
                    {
                    }
                }
            }
        }
        public string getCookie()
        {

            return (mycook.GetCookieHeader(mainurl));

        }
    }
}
