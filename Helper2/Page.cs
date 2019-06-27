using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
    namespace MWMS.Helper
{ 
    public class Page
    {
        public static void ERR301(string url)
        {

            HttpContext.Current.Response.Status = "301 Moved Permanently";
            HttpContext.Current.Response.AddHeader("Location", url);
            HttpContext.Current.Response.End();
        }
        public static void ERR404()
        {
            HttpContext.Current.Response.Status = "404 Not Found";
            HttpContext.Current.Response.End();
        }
        public static void ERR404(string msg)
        {
            HttpContext.Current.Response.Status = "404 Not Found";
            if (HttpContext.Current.Request.Cookies["AdminClassID"] != null && HttpContext.Current.Request.Cookies["AdminClassID"].Value == "6") //管理员浏览网页时不使用缓存
            {
                HttpContext.Current.Response.Write(msg);
            }
            else
            {
                string file = HttpContext.Current.Server.MapPath("~/404.html");
                if (File.Exists(file))
                {
                    HttpContext.Current.Response.Write(File.ReadAllText(file));
                }
                else
                {
                    HttpContext.Current.Response.Write(msg);
                }
            }
            HttpContext.Current.Response.End();
        }
    }
}
