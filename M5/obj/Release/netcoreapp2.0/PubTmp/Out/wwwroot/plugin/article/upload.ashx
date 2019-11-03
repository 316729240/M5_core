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
            ErrInfo e =saveFile(context.Request.Files[0], "/enclosure/");
            if (e.errNo >-1)fp[i] = e.userData.ToString();
        }
        info.userData = fp;
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    
          ErrInfo saveFile(HttpPostedFile file,string filePath)
        {
            ErrInfo err = new ErrInfo();

            try
            {

                string path = Config.webPath + filePath + System.DateTime.Now.ToString("yyyy-MM")+"/";
                if (!System.IO.Directory.Exists(PageContext.Current.Server.MapPath(path))) System.IO.Directory.CreateDirectory(PageContext.Current.Server.MapPath(path));
                string kzm = "";
                if (file.FileName.LastIndexOf(".") > -1) kzm = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1).ToLower();
                if (Regex.IsMatch(kzm, "(exe|asp|php)"))
                {
                    err.errNo = -1;
                    err.errMsg = "文件类型不合法";
                    return err;
                }
                string fileName = API.GetId() + "." + kzm;
                file.SaveAs(PageContext.Current.Server.MapPath(path + fileName));
                err.userData = path + fileName;
                return err;
            }catch(Exception ex)
            {
                err.errNo=-1;
                err.errMsg = ex.Message;
                return err;
            }
        }
    public bool IsReusable {
        get {
            return false;
        }
    }

}