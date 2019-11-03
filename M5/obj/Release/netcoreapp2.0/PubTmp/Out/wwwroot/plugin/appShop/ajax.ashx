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
using Ionic.Zip;
public class ajax : IHttpHandler {
    SafeReqeust s_request = new SafeReqeust(0, 0);
    LoginInfo login = new LoginInfo();
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
        if (!login.value.isAdministrator)
        {
            ErrInfo err = new ErrInfo();
            err.errNo = -1;
            err.errMsg = "没有权限";
            context.Response.Write(err.ToJson());
        }
        if (m == "updateCount")
        {

            List<string[]> list = new List<string[]>();
            string appPath = PageContext.Current.Server.MapPath("~" + Config.appPath);
            string[] f = Directory.GetDirectories(appPath);
            for (int i = 0; i < f.Length; i++)
            {
                if (System.IO.File.Exists(f[i] + @"\reg.xml")) {
                    XmlDocument reg = new XmlDocument();
                    reg.Load(f[i] + @"\reg.xml");
                    string name = reg.DocumentElement.GetAttribute("name").ToLower();
                    string datetime = reg.DocumentElement.GetAttribute("datetime");
                    list.Add(new string[] { name, datetime });
                }
            }
            ErrInfo err = new ErrInfo();
            string http = Http.getUrl("http://"+ConfigurationManager.AppSettings["OfficialWeb"]+"app/", System.Text.Encoding.UTF8);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            object[] arr = (object[])ser.DeserializeObject(http);
            int count = 0;
            if (arr != null) {
            for (int i = 0; i < arr.Length; i++)
            {
                Dictionary<string, object> item = (Dictionary<string, object>)arr[i];
                string name = item["name"].ToString().ToLower();
                DateTime d = DateTime.Parse(item["datetime"].ToString());
                string oldDateTime = "";
                bool installFlag = false;
                for (int i2 = 0; i2 < list.Count; i2++)
                {
                    if (name == list[i2][0])
                    {
                        oldDateTime = list[i2][1];
                        installFlag = true;
                        break;
                    }
                }
                if (installFlag && (oldDateTime == "" || DateTime.Parse(oldDateTime) < d)) count++;
            }
            }
        err.userData = count;
        context.Response.Write(err.ToJson());
    }
        else if (m == "readAppList")
        {

            List<string[]> list = new List<string[]>();
    string appPath = PageContext.Current.Server.MapPath("~" + Config.appPath);
    string[] f = Directory.GetDirectories(appPath);
            for (int i = 0; i < f.Length; i++)
            {
                if (System.IO.File.Exists(f[i] + @"\reg.xml"))
                {
                    XmlDocument reg = new XmlDocument();
    reg.Load(f[i] + @"\reg.xml");
                    string name = reg.DocumentElement.GetAttribute("name").ToLower();
    string datetime = reg.DocumentElement.GetAttribute("datetime");
    list.Add(new string[] { name, datetime });
                    //if (f[i] != null) appendFileXml(f[i], context);
                }
            }
            ErrInfo err = new ErrInfo();
string http = Http.getUrl("http://"+ConfigurationManager.AppSettings["OfficialWeb"]+"/app/", System.Text.Encoding.UTF8);
JavaScriptSerializer ser = new JavaScriptSerializer();
object[] arr = (object[])ser.DeserializeObject(http);
            for (int i = 0; i < arr.Length; i++)
            {
                Dictionary<string, object> item = (Dictionary<string, object>)arr[i];
string name = item["name"].ToString().ToLower();
DateTime d = DateTime.Parse(item["datetime"].ToString());
string oldDateTime = "";
bool installFlag = false;
                for (int i2 = 0; i2 < list.Count; i2++)
                {
                    if (name == list[i2][0])
                    {
                        oldDateTime = list[i2][1];
                        installFlag = true;
                        break;
                    }
                }
                item["installFlag"] = installFlag;
                item["updateFlag"] = installFlag && (oldDateTime == "" || DateTime.Parse(oldDateTime) < d);
            }
            err.userData = arr;
            context.Response.Write(err.ToJson());
        }
        else if (m == "uninstall")
        {
            ErrInfo err = new ErrInfo();
string appName = s_request.getString("appName");
string appPath = PageContext.Current.Server.MapPath("~" + Config.appPath+appName+@"/");
            if (System.IO.Directory.Exists(appPath)) System.IO.Directory.Delete(appPath,true);
            context.Response.Write(err.ToJson());
        }
        else if (m == "setup")
        {
            ErrInfo err = new ErrInfo();
string appName = s_request.getString("appName");
string datetime= s_request.getString("datetime");
            try
            {
                byte[] data = Http.getByte("http://"+ConfigurationManager.AppSettings["OfficialWeb"]+"/app/" + appName + ".zip");
string path = context.Server.MapPath("~" + Config.tempPath);
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                System.IO.File.WriteAllBytes(path + appName + ".zip", data);
                using (ZipFile zip = new ZipFile(path + appName + ".zip"))
                {
                    zip.ExtractAll(context.Server.MapPath("~" + Config.appPath + appName + @"/"), ExtractExistingFileAction.OverwriteSilently);
                }
                System.IO.File.Delete(path + appName + ".zip");
                XmlDocument reg = new XmlDocument();
string xmlpath = context.Server.MapPath("~" + Config.appPath + appName + @"\reg.xml");
reg.Load(xmlpath);
                string name = reg.DocumentElement.GetAttribute("name");
reg.DocumentElement.SetAttribute("datetime", datetime);
                reg.DocumentElement.SetAttribute("sn", name.Encryption(Config.webId).MD5());
                reg.Save(xmlpath);
                Uri u = new Uri(context.Request.Url, Config.webPath + Config.appPath+name+"/");
Helper.Http http = new Helper.Http(u);
http.setCookie(context.Request.Headers["Cookie"]);
                http.get(u.ToString()+"setup.ashx");
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
    }

    public bool IsReusable {
    get {
        return false;
    }
}

}