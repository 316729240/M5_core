using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using M5.Common;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace M5.Main.Manager
{
    [LoginAuthorzation]
    public class ConfigController : ManagerBase
    {
        XmlElement root = null, interfaceRoot = null;
        XmlDocument doc = null, interfaceDoc = null;
        Uri u = null;
        public ReturnValue Init()
        {
            Console.WriteLine(Request.Host);
            ReturnValue info = new ReturnValue();
            Dictionary<string, object> data = new Dictionary<string, object>();
            loginInfo.checkLogin();
            data.Add("webPath", Config.webPath);
            data.Add("appPath", Config.appPath);
            data.Add("sysAppPath", Config.webPath + Config.appPath + "system/");
            data.Add("backupDay", BaseConfig.backupDay.ToString());
            //Response.Write("$M.config.webPath=\"" + Config.webPath + "\";\r\n");
            //Response.Write("$M.config.appPath=\"" + Config.webPath + Config.appPath + "\";\r\n");
            //context.Response.Write("$M.config.sysAppPath=\"" + Config.webPath + Config.appPath + "system/\"" + ";\r\n");
            //context.Response.Write("$M.config.backupDay=" + BaseConfig.backupDay.ToString() + ";\r\n");
            string appPath = Tools.MapPath(Config.pluginPath);// Path.Combine(Directory.GetCurrentDirectory(), Config.pluginPath);
            string[] f = Directory.GetDirectories(appPath);
            doc = new XmlDocument();
            interfaceDoc = new XmlDocument();
            interfaceRoot = interfaceDoc.CreateElement("interface");
            root = doc.CreateElement("reg");
            doc.AppendChild(root);
            interfaceDoc.AppendChild(interfaceRoot);
            u = new Uri(Request.Url(), Config.webPath + Config.appPath);
            
            string path = Tools.MapPath("~" + Config.tempPath + @"user\" + loginInfo.value.id.ToString() + @"\cardLayout.config");
            if (System.IO.File.Exists(path))
            {
                string[] cardLayout = System.IO.File.ReadAllLines(path);
                for (int i = 0; i < cardLayout.Length; i++)
                {
                    for (int i1 = 0; i1 < f.Length; i1++)
                    {
                        if (f[i1] != null)
                        {
                            DirectoryInfo d = new DirectoryInfo(f[i1]);
                            if (cardLayout[i] == d.Name)
                            {
                                appendFileXml(f[i1]);
                                f[i1] = null;
                                i1 = f.Length;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < f.Length; i++)
            {
                if (f[i] != null) appendFileXml(f[i]);
            }

            data.Add("_appReg",doc.ToJson());
            data.Add("_interface", interfaceDoc.ToJson());
            //context.Response.Write("var _appReg=" + doc.ToJson() + ";\n");
            //context.Response.Write("var _interface=" + interfaceDoc.ToJson() + ";\n");
            //context.Response.End();
            info.userData = data;
            return info;


        }
        void appendFileXml(string path)
        {
            string xmlFile = path + @"/reg.xml";
            //Console.WriteLine(xmlFile);
            //Console.WriteLine(System.IO.File.Exists(xmlFile));
            if (!System.IO.File.Exists(xmlFile)) return;
            XmlDocument reg = new XmlDocument();
            reg.Load(xmlFile);
            Helper.Http http = new Helper.Http(u);
            http.setCookie(Request.Headers["Cookie"]);
            string name = reg.ChildNodes[0].Attributes["name"].Value;
            System.Collections.Specialized.NameValueCollection value = new System.Collections.Specialized.NameValueCollection();
            value.Add("_m", "cardPermissions");
            string q = http.post(name + "/cardPermissions", value).ToLower();
            XmlElement element = (XmlElement)reg.ChildNodes[0];
            try
            {
                Dictionary<string, string> json = q.ParseJson<Dictionary<string,string>>();
                if (json["userdata"] == "true")
                {
                    element.SetAttribute("load", "1");

                }
                else
                {
                    element.SetAttribute("load", "0");
                }
            }
            catch
            {
                element.SetAttribute("load", "0");
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
    }
 
}
