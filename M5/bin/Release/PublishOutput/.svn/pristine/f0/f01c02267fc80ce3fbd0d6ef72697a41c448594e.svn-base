<%@ WebHandler Language="C#" Class="ajax" %>

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
public class ajax : IHttpHandler
{
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        SafeReqeust s_request = new SafeReqeust(0, 0);
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "read")
        {
            double id = s_request.getDouble("id");
            ErrInfo info = new ErrInfo();
            Dictionary<string, object> data = Helper.Sql.ExecuteDictionary("select A.title,A.classId,A.skinId,A.url,A.pic,B.* from mainTable A inner join  u_guzhuxinxi B on A.id=B.id where A.id=@id", new SqlParameter[] { new SqlParameter("id", id) });
            data["url"] = TemplateEngine._replaceUrl(Config.webPath + data["url"].ToString() + "." + BaseConfig.extension);
            info.userData = data;
            context.Response.Write(info.ToJson());
        }
        else if (m == "edit")
        {
            ErrInfo info = new ErrInfo();
            RecordClass value = new RecordClass(22592528242,login.value);
            double id=s_request.getDouble("id");
            double classId = s_request.getDouble("classId");
            Permissions p= login.value.getColumnPermissions(classId);
            if (!p.write)
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
                context.Response.Write(info.ToJson());
                return;
            }
            TableInfo table = new TableInfo(22592528242);
            for(int i = 0; i < table.fields.Count; i++) {
                    if(context.Request.Form[table.fields[i].name]!=null)value.addField(table.fields[i].name,s_request.getString(table.fields[i].name));
            }
            if (id > 0)info = value.update(id);
            else info = value.insert();
            context.Response.Write(info.ToJson());
        }
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}