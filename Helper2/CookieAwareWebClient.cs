using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Net;
using System.IO;

/// <summary>
/// 支持cookie的webclient 
/// <remarks> 
/// 请求完成后会自动将响应cookie填充至CookieContainer 
/// </remarks> 6: /// </summary> 
internal class CookieAwareWebClient : WebClient
{
    internal CookieContainer _cookieContainer = new CookieContainer();
    internal HttpCookieCollection cookies = new HttpCookieCollection();
    internal Uri ResponseUri = null;
    protected override WebRequest GetWebRequest(Uri address)
    {
        //_cookieContainer.Capacity = 100;
        var request = base.GetWebRequest(address);
        if (request is HttpWebRequest)
        {
            request.Timeout = 1000 * 60 * 10;
            string Cookie = request.Headers["Cookie"];
            if (Cookie != null)
            {
                string[] list = Cookie.Split(';');
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i] != "")
                    {
                        int index=list[i].IndexOf("=");
                        _cookieContainer.SetCookies(address, list[i]);
                        HttpCookie cookie = new HttpCookie(list[i].Substring(0, index).Trim(), list[i].Substring(index + 1).Trim());
                        cookie.Domain = address.Host;
                        //cookies.Set();
                    }
                }
            }
            (request as HttpWebRequest).AllowAutoRedirect = false;
            (request as HttpWebRequest).CookieContainer = this._cookieContainer;
        }
        return request;
    }
    protected override WebResponse GetWebResponse(WebRequest request)
    {
        try
        {
        var r = base.GetWebResponse(request);
        
            
   
            if (r is HttpWebResponse)
                ResponseUri = (r as HttpWebResponse).ResponseUri;
            for (int i = 0; i < (r as HttpWebResponse).Cookies.Count; i++)
            {

                Cookie c = (r as HttpWebResponse).Cookies[i];
                if (!c.Expired)
                {
                    cookies.Remove(c.Name);
                    HttpCookie c2 = new HttpCookie(c.Name, c.Value);
                    c2.Expires = c.Expires;
                    c2.Path = c.Path;
                    c2.Domain = c.Domain;
                    c2.HttpOnly = c.HttpOnly;
                    cookies.Set(c2);
                    this._cookieContainer.Add(c);
                }
                //this._cookieContainer.Add((r as HttpWebResponse).Cookies[i]);
            }

            //                 this._cookieContainer.Add((r as HttpWebResponse).Cookies);
            return r;
        }
        catch
        {
            return null;
        }
    }
}