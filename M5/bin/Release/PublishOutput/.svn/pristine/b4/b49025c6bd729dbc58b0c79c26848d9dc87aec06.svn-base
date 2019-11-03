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
public class ajax : IHttpHandler
{
    LoginInfo login = new LoginInfo();
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "cardPermissions")
        {
            context.Response.Write(login.value.isAdministrator);
            context.Response.End();
        }
        if (!login.value.isAdministrator) context.Response.End();
        if (m == "createDir")
        {
            ErrInfo err = new ErrInfo();
            string path = s_request.getString("path");
            string name = s_request.getString("name");
            string rootPath = context.Server.MapPath(@"\" + Config.webPath);
            path = context.Server.MapPath(@"~/" + s_request.getString("path")) + @"\";
            DirectoryInfo di = new DirectoryInfo(path);
            string newPath = di.FullName + @"/" + name;
            try
            {
                System.IO.Directory.CreateDirectory(newPath);
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary["text"] = name;
                dictionary["path"] = newPath.Replace(rootPath, "").Replace(@"\", "//");
                err.userData = dictionary;
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "editFileName")
        {
            ErrInfo err = new ErrInfo();
            string path = s_request.getString("path");
            string oldName = s_request.getString("oldName");
            string name = s_request.getString("name");
            System.IO.FileInfo f = new FileInfo( context.Server.MapPath(@"~\"+path+@"\"+oldName));
            try
            {
            f.MoveTo(context.Server.MapPath(@"~\"+path+@"\"+name));
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "editDir")
        {
            ErrInfo err = new ErrInfo();
            string path = s_request.getString("path");
            string name = s_request.getString("name");
            string rootPath = context.Server.MapPath(@"\" + Config.webPath);
            path = context.Server.MapPath("~/" + s_request.getString("path")) + @"\";
            DirectoryInfo di = new DirectoryInfo(path);
            string newPath = di.Parent.FullName + @"/" + name;
            try
            {
                di.MoveTo(newPath);
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary["text"] = name;
                dictionary["path"] = newPath.Replace(rootPath, "").Replace(@"\", "//");
                err.userData = dictionary;
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "delDir")
        {
            ErrInfo err = new ErrInfo();
            string path = "";
            path = context.Server.MapPath("~/" + s_request.getString("path")) + @"\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.GetDirectories().Length == 0)
            {
                di.Delete(true);
            }
            else
            {
                err.errNo = -1;
                err.errMsg = "删除文件夹失败，不允许删除包含子目录的文件夹";
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "delFile")
        {
            ErrInfo err = new ErrInfo();
            string[] files = s_request.getString("files").Split(',');
            string path = s_request.getString("path");
            path = context.Server.MapPath("~/" + s_request.getString("path")) + @"\";
            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo f = new FileInfo(path + files[i]);
                    if (f.Exists) f.Delete();
                }
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "readDir")
        {
            ErrInfo err = new ErrInfo();
            string path = s_request.getString("path");
            string rootPath = context.Server.MapPath(@"\" + Config.webPath);
            DirectoryInfo dir = new DirectoryInfo(rootPath + path);
            DirectoryInfo[] list = dir.GetDirectories();
            object[] dirlist = new object[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary["text"] = list[i].Name;
                dictionary["path"] = list[i].FullName.Replace(rootPath, "").Replace(@"\", "//");
                dictionary["ico"] = "fa-folder-o";
                dirlist[i] = dictionary;
            }
            err.userData = dirlist;
            context.Response.Write(err.ToJson());
        }
        else if (m == "readFiles")
        {
            ErrInfo err = new ErrInfo();
            int pageNo = s_request.getInt("pageNo");
            if (pageNo == 0) pageNo = 1;
            string path = s_request.getString("path");
            string rootPath = context.Server.MapPath(@"\" + Config.webPath);
            DirectoryInfo dir = new DirectoryInfo(rootPath + path);
            FileInfo[] list = dir.GetFiles();
            ReturnPageData page = new ReturnPageData();
            page.pageNo = pageNo;
            page.recordCount = list.Length;
            for (int i = (pageNo - 1) * page.pageSize; i < pageNo * page.pageSize; i++)
            {
                if (i < list.Length)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    dictionary["text"] = list[i].Name;
                    dictionary["path"] = list[i].FullName.Replace(rootPath, "").Replace(@"\", "//");
                    dictionary["updateTime"] = list[i].LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                    dictionary["size"] = list[i].Length.FormatFileSize();
                    page.data.Add(dictionary);
                }
            }
            err.userData = page;
            context.Response.Write(err.ToJson());
        }
        else if (m == "saveFile")
        {
            ErrInfo err = new ErrInfo();
            Dictionary<string, object> value = new Dictionary<string, object>();
            string path = @"\" + Config.webPath + s_request.getString("path")+@"\";
            string fileName = s_request.getString("fileName");
            string encoding = s_request.getString("encoding");
            string content = s_request.getString("content");
            try
            {
                FileInfo f = new FileInfo(context.Server.MapPath( path + fileName));
                System.IO.File.WriteAllText(f.FullName, content, System.Text.Encoding.GetEncoding(encoding));
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = "发生异常："+ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "getFile")
        {
            ErrInfo err = new ErrInfo();
            Dictionary<string, object> value = new Dictionary<string, object>();
            string path = @"\" + Config.webPath + s_request.getString("path") + @"\";
            string fileName = s_request.getString("fileName");
            FileInfo f = new FileInfo(context.Server.MapPath( path + fileName));
            Microshaoft.Text.IdentifyEncoding code = new Microshaoft.Text.IdentifyEncoding();
            string name = code.GetEncodingName(f);
            System.Text.Encoding e = code.GetEncoding(name);
            string text = System.IO.File.ReadAllText(f.FullName, e);
            value.Add("encoding", e.WebName);
            value.Add("content", text);
            err.userData = value;
            context.Response.Write(err.ToJson());
        }

    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}