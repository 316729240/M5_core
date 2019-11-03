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
using ManagerFramework;
public class upload : IHttpHandler
{
    //LoginInfo login = new LoginInfo();
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public class File
    {
        public string title = "";
        public int size = 0;
        public string path = "";
    }
    public void ProcessRequest(HttpContext context)
    {
        //login.checkLogin();

        ReturnValue returnValue = new ReturnValue();
        context.Response.ContentType = "text/plain";
        string[] fp = new string[context.Request.Files.Count];
        List<File> list = new List<File>();
        for(int i=0;i<context.Request.Files.Count;i++){
            try {
                string newfile = SaveFile(context.Request.Files[i], Config.tempPath);
                list.Add(new File { title=context.Request.Files[i].FileName,size=context.Request.Files[i].ContentLength,path=newfile});
            }catch
            {

            }
        }
        returnValue.userData = list;
        context.Response.Write(returnValue.ToJson());
        context.Response.End();
    }
    public static string SaveFile(HttpPostedFile file, string filePath)
    {

        string path = filePath + System.DateTime.Now.ToString("yyyy-MM")+"/";
        if (!System.IO.Directory.Exists(PageContext.Current.Server.MapPath(path))) System.IO.Directory.CreateDirectory(PageContext.Current.Server.MapPath("~"+path));
        string kzm = "";
        if (file.FileName.LastIndexOf(".") > -1) kzm = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1).ToLower();
        string fkzm = "jpg,png,gif,doc,rar,zip,mp4,wmv,pdf";
            /*
        for (int i = 0; i < Config.userConfig["uploadSet"].Count; i++)
        {
            XmlNode node = Config.userConfig["uploadSet"][i];
            if (i>0) fkzm += "|";
            fkzm += node.InnerText;
        }*/

        if (!Regex.IsMatch(kzm, "("+fkzm.Replace(",","|")+")"))
        {
            throw new Exception("文件类型不合法，只能上传"+fkzm);
        }
        string fileName = Helper.Tools.GetId() + "." + kzm;
        file.SaveAs(PageContext.Current.Server.MapPath(path + fileName));
        return path + fileName;

    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}