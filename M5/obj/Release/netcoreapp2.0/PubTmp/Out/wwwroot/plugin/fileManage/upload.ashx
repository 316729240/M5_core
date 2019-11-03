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
        if (login.value == null || !login.value.isAdministrator)
        {
            info.errNo = -1;
            info.errMsg = "非管理员权限";
            context.Response.Write(info.ToJson());
            context.Response.End();
        }
        context.Response.ContentType = "text/plain";

        string path = context.Server.MapPath(@"~\" + s_request.getString("path")) + @"\";
        //string filePath = s_request.getString("filePath");
        int covered = s_request.getInt("covered");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        for(int i=0;i<context.Request.Files.Count;i++){
            if (context.Request.Files[i].FileName.IndexOf(@"/.") > -1)//为目录时
            {
                context.Response.Write(info.ToJson());
                context.Response.End();
            }
            FileInfo f = new FileInfo(path + context.Request.Files[i].FileName);
            if (covered !=1)
            {
                if (f.Exists)
                {
                    info.errNo = -2;
                    info.errMsg = context.Request.Files[i].FileName + "文件已存在";
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    //context.Request.Files[i].
                    dictionary["oldFile"] = new object[] { f.Name, f.Length.FormatFileSize(), f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") };
                    dictionary["newFile"] = new object[] { f.Name, context.Request.Files[i].ContentLength.FormatFileSize(), context.Request.Form["editDate"].ToString() };
                    info.userData = dictionary;
                    context.Response.Write(info.ToJson());
                    context.Response.End();
                }
            }
            if (!f.Directory.Exists) f.Directory.Create();
            context.Request.Files[i].SaveAs(f.FullName);
        }
        context.Response.Write(info.ToJson());
        context.Response.End();
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}