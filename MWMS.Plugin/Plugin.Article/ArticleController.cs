using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using M5.Common;
using M5.Main.Manager;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MWMS.Plugin
{
    [LoginAuthorzation]
    public class ArticleController : ManagerBase
    {
        public override ReturnValue read(double id = 0)
        {
            ReturnValue info = new ReturnValue();
            Dictionary<string, object> data = Sql.ExecuteDictionary("select A.title,A.classId,A.skinId,A.url,A.pic,B.* from maintable A inner join  article B on A.id=B.id where A.id=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            if(data.ContainsKey("url"))data["url"] = TemplateEngine._replaceUrl(Config.webPath + data["url"].ToString() + "." + BaseConfig.extension);
            info.userData = data;
            return info;
        }
        public override ReturnValue edit(double classId)
        {
            double skinId = s_request.getDouble("skinId");
            double id = s_request.getDouble("id");
            string u_custom = s_request.getString("u_custom");
            string u_content = s_request.getString("u_content");
            string u_info = s_request.getString("u_info");
            string u_fromWeb = s_request.getString("u_fromWeb");
            string pic = s_request.getString("pic");
            string title = s_request.getString("title");
            string u_keyword = s_request.getString("u_keyword");
            //string u_keyword,string u_defaultPic,double id,double classId,string u_custom,string u_content,string u_info, string u_fromWeb,string pic,string title,double skinId
            ReturnValue info = new ReturnValue();
    RecordClass value = new RecordClass(22192428132, loginInfo.value);
    string keyword = u_keyword;
    value.tableName = "article";
    Permissions p = loginInfo.value.getColumnPermissions(classId);
            if (!p.write)
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
                return info;
            }
value.addField("classId", classId);
            value.addField("skinId", skinId);
            value.addField("title", title);
            value.addField("u_keyword", keyword);
            value.addField("pic", pic);
            value.addField("u_fromWeb", u_fromWeb);
            if (u_info == "") {
                string[] list = Regex.Split(u_content, "(</div>|</p>)", RegexOptions.IgnoreCase);
int infoLength = 600;
                for(int i = 0; i<list.Length; i++)
                {
                    string html = Tools.nohtml(list[i]).Trim();
infoLength-=Tools.GetStringLength(html);
                    if (i>0 && infoLength< 0) break;
                    if(html!="")u_info += "<p>"+html+"</p>";
                }
            }
            value.addField("u_info", u_info);
            value.addField("u_custom", u_custom);
            value.addField("u_content", u_content);
            if (id > 0)
            {
                info.userData = value.update(id);
                if (info.userData != null)
                {
                    Sql.ExecuteNonQuery("delete from indextable where dataId=@dataId",new MySqlParameter[]{
                        new MySqlParameter("dataId", info.userData)
                    });
                    RecordClass.addKeyword((double) info.userData, keyword, 22192428132);
                }
            }
            else
            {

                if (!p.delete && !p.audit) value.addField("orderId", -1);

                info.userData = value.insert();
                if (info.userData != null) RecordClass.addKeyword((double) info.userData, keyword, 22192428132);
            }
            return info;

        }
    }
 
}
