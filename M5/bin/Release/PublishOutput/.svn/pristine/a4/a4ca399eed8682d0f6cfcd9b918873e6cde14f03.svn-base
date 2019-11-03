<%@ WebHandler Language="C#" Class="shortKeyword" %>
using System;
using System.Web;
using MWMS;
using System.Collections.Generic;
using System.Xml;
public class shortKeyword : IHttpHandler {

    public void ProcessRequest(HttpContext context) {
        context.Response.ContentType = "text/plain";
        LoginInfo login = new LoginInfo();
        login.checkLogin();
        string m = context.Request.Form["_m"].ToString();
        if (m == "loadKeyword")
        {
            ErrInfo info = new ErrInfo();
            List<key> list = new List<key>();
            loadDirict("sys",list,Config.systemVariables);
            loadDirict("config",list,Config.userConfig);
            info.userData = list;
            context.Response.Write(info.ToJson());
            context.Response.End();
        }
    }
    void loadDirict(string parentName,List<key> list,Dictionary<string, string> dir) {
        list.Add(new key() { value = parentName });
        foreach(var item in dir) {
            list.Add(new key() { value=item.Key,p=parentName});
        }
    }
    void loadDirict(string parentName,List<key> list,Dictionary<string, XmlNodeList> dir) {
        list.Add(new key() { value = parentName });
        foreach(var item in dir) {
            list.Add(new key() { value = item.Key ,p=parentName});
            loadDirict(item.Key, list, item.Value);
        }
    }
    void loadDirict(string parentName,List<key> list,XmlNodeList dir) {
        foreach(var item in dir) {
            XmlElement data = (XmlElement)item;
            list.Add(new key() { value=data.GetAttribute("name"),p=parentName,meta=data.GetAttribute("labelText")});
        }
    }
    public class key
    {
        public string value { get; set; }
        public string p { get; set; }
        public string meta { get; set; }
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}