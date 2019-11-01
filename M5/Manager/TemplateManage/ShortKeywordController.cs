using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Helper;
using M5.Common;
using Microsoft.AspNetCore.Mvc;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace M5.Main.Manager
{
    [Route("manage/app/TemplateManage/[controller]/[action]")]
    [LoginAuthorzation]
    public class ShortKeywordController : ManagerBase
    {
        public ReturnValue loadKeyword()
        {
            ReturnValue info = new ReturnValue();
            List<key> list = new List<key>();
            loadDirict("sys", list, Config.systemVariables);
            loadDirict("config", list, Config.userConfig);
            info.userData = list;
            return info;
        }
        void loadDirict(string parentName, List<key> list, Dictionary<string, string> dir)
        {
            list.Add(new key() { value = parentName });
            foreach (var item in dir)
            {
                list.Add(new key() { value = item.Key, p = parentName });
            }
        }
        void loadDirict(string parentName, List<key> list, Dictionary<string, XmlNodeList> dir)
        {
            list.Add(new key() { value = parentName });
            foreach (var item in dir)
            {
                list.Add(new key() { value = item.Key, p = parentName });
                loadDirict(item.Key, list, item.Value);
            }
        }
        void loadDirict(string parentName, List<key> list, XmlNodeList dir)
        {
            foreach (var item in dir)
            {
                XmlElement data = (XmlElement)item;
                list.Add(new key() { value = data.GetAttribute("name"), p = parentName, meta = data.GetAttribute("labelText") });
            }
        }
    }

    public class key
    {
        public string value { get; set; }
        public string p { get; set; }
        public string meta { get; set; }
    }
}
