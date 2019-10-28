<%@ WebHandler Language="C#" Class="upload"%>
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
public class upload : IHttpHandler
{
    LoginInfo login = new LoginInfo();
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();

        ErrInfo info = new ErrInfo();
        context.Response.ContentType = "text/plain";

        string[] fp = new string[context.Request.Files.Count];
        for(int i=0;i<context.Request.Files.Count;i++){
            ErrInfo e = API.saveImage(context.Request.Files[0], Config.tempPath);
            if (e.errNo >-1)fp[i] = e.userData.ToString();
        }
        info.userData = fp;
        context.Response.Write(info.ToJson());
        context.Response.End();
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}