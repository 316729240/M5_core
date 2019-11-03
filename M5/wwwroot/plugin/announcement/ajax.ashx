<%@ WebHandler Language="C#" Class="ajax"%>
using System;
using System.Web;
using System.Collections.Generic;
using MWMS;
using Helper;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Script.Serialization;
public class ajax : IHttpHandler {
    
    SafeReqeust s_request = new SafeReqeust(0, 0);
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "save")
        {
            ErrInfo info = new ErrInfo();
            if (login.value.isAdministrator)
            {
                string text = s_request.getString("text");
                string path = PageContext.Current.Server.MapPath("~" + Config.appPath + "announcement/content.d");
                System.IO.File.WriteAllText(path, text);
            }
            else
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
            }
            context.Response.Write(info.ToJson());
        }
        else if (m == "read")
        {
            ErrInfo info = new ErrInfo();
            string path = PageContext.Current.Server.MapPath("~" + Config.appPath + "announcement/content.d");
            string text ="";
            if (System.IO.File.Exists(path)) text = System.IO.File.ReadAllText(path);
            info.userData = text;
            context.Response.Write(info.ToJson());

        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}