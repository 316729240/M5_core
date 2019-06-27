using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
    namespace M5.Common
{ 
    public class Page
    {
        public static void ERR301(string url)
        {
            SystemData.HttpContext.Response.StatusCode = 301;
            SystemData.HttpContext.Response.Headers.Add("Location", url);
            SystemData.HttpContext.Response.Clear();
        }
        public static void ERR404()
        {
            SystemData.HttpContext.Response.StatusCode = 404;
            SystemData.HttpContext.Response.Clear();
        }
        public static void ERR404(string msg)
        {
            SystemData.HttpContext.Response.StatusCode = 404;
            SystemData.HttpContext.Response.Clear();
            SystemData.HttpContext.Response.WriteAsync(msg);
        }
    }
}
