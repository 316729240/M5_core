<%@ WebHandler Language="C#" Class="api" %>
using System;
using System.Web;
using System.Collections.Generic;
using MWMS;
using System.Xml;
using System.IO;
public class api : IHttpHandler {
    XmlElement root = null, interfaceRoot = null;
    XmlDocument doc = null, interfaceDoc = null;
    Uri u = null;
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.Write("$M.config.webPath=\"" + Config.webPath + "\";\r\n");
        context.Response.Write("$M.config.appPath=\"" + Config.webPath + Config.appPath + "\";\r\n");
        context.Response.Write("$M.config.sysAppPath=\"" + Config.webPath + Config.appPath + "system/\""+";\r\n");
        context.Response.Write("$M.config.backupDay=" + BaseConfig.backupDay.ToString() + ";\r\n");
        string appPath =  PageContext.Current.Server.MapPath("~" +Config.appPath);
        string[] f = Directory.GetDirectories(appPath);
        doc = new XmlDocument();
        interfaceDoc = new XmlDocument();
        interfaceRoot = interfaceDoc.CreateElement("interface");
        root = doc.CreateElement("reg");
        doc.AppendChild(root);
        interfaceDoc.AppendChild(interfaceRoot);
        u = new Uri(context.Request.Url, Config.webPath + Config.appPath);

        string path = context.Server.MapPath("~" + Config.tempPath + @"user\" + login.value.id.ToString() + @"\cardLayout.config");
        if (System.IO.File.Exists(path))
        {
            string  [] cardLayout = System.IO.File.ReadAllLines(path);
            for (int i = 0; i < cardLayout.Length; i++)
            {
                for (int i1 = 0; i1 < f.Length; i1++)
                {
                    if (f[i1] != null)
                    {
                        DirectoryInfo d = new DirectoryInfo(f[i1]);
                        if (cardLayout[i] == d.Name)
                        {
                            appendFileXml(f[i1], context);
                            f[i1] = null;
                            i1 = f.Length;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < f.Length; i++)
        {
            if (f[i] != null) appendFileXml(f[i], context);
        }

        context.Response.Write("var _appReg=" + doc.ToJson() + ";\n");
        context.Response.Write("var _interface=" + interfaceDoc.ToJson() + ";\n");
        context.Response.End();



    }
    void appendFileXml(string path, HttpContext context)
    {
        string xmlFile = path + @"\reg.xml";
        if (!System.IO.File.Exists(xmlFile)) return;
        XmlDocument reg = new XmlDocument();
        reg.Load(xmlFile);
        Helper.Http http = new Helper.Http(u);
        http.setCookie(context.Request.Headers["Cookie"]);
        string name = reg.ChildNodes[0].Attributes["name"].Value;
        System.Collections.Specialized.NameValueCollection value = new System.Collections.Specialized.NameValueCollection();
        value.Add("_m", "cardPermissions");
        string q = http.post(name + "/ajax.ashx", value).ToLower();
        XmlElement element = (XmlElement)reg.ChildNodes[0];
        if (q == "false")//检查是否加载插件
        {
            element.SetAttribute("load", "0");
        }
        else
        {
            element.SetAttribute("load", "1");
        }
        #region
        root.AppendChild(doc.ImportNode(reg.ChildNodes[0], true));
        XmlNodeList list = reg.SelectNodes("/app/interface");
        if (list.Count > 0)
        {
            for (int i1 = 0; i1 < list[0].ChildNodes.Count; i1++)
            {
                XmlElement node = (XmlElement)list[0].ChildNodes[i1];
                node.SetAttribute("name", "$M." + name + "." + node.Attributes["name"].Value);
                interfaceRoot.AppendChild(interfaceDoc.ImportNode(list[0].ChildNodes[i1], true));
            }
        }
        #endregion
    }
    public bool IsReusable
    {
        get {
            return false;
        }
    }

}