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
    SafeReqeust s_request = new SafeReqeust(0, 0);
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
            Dictionary<string, object> data = Helper.Sql.ExecuteDictionary("select A.title,A.classId,A.skinId,A.url,A.pic,B.* from mainTable A inner join  u_paiqian B on A.id=B.id where A.id=@id", new SqlParameter[] { new SqlParameter("id", id) });
            data["url"] = TemplateEngine._replaceUrl(Config.webPath + data["url"].ToString() + "." + BaseConfig.extension);
            info.userData = data;
            context.Response.Write(info.ToJson());
        }
        else if (m == "edit")
        {
            ErrInfo info = new ErrInfo();
            RecordClass value = new RecordClass(22592528442,login.value);
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
            TableInfo table = new TableInfo(22592528442);
            for(int i = 0; i < table.fields.Count; i++) {
                if(context.Request.Form[table.fields[i].name]!=null)value.addField(table.fields[i].name,s_request.getString(table.fields[i].name));
            }
            if (id > 0)info = value.update(id);
            else info = value.insert();
            context.Response.Write(info.ToJson());
        }else if (m == "dataList")
        {
                dataList(context);
        }
    }
    void dataList(HttpContext context)
    {
        ErrInfo err = new ErrInfo();
        double moduleId = s_request.getDouble("moduleId");
        double classId = s_request.getDouble("classId");
        int pageNo =s_request.getInt("pageNo");
        string orderBy =s_request.getString("orderBy");
        int sortDirection = s_request.getInt("sortDirection");
        string type = s_request.getString("type");
        string searchField = s_request.getString("searchField");
        string keyword = s_request.getString("keyword").Replace("'","''");
        double dataTypeId = -1;
        SqlDataReader rs = null;
        Permissions p = null;
        if (moduleId == classId)
        {
            p = login.value.getModulePermissions(classId);
            rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new SqlParameter[] { new SqlParameter("moduleId", moduleId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
        }
        else
        {
            p = login.value.getColumnPermissions(classId);
            rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new SqlParameter[] { new SqlParameter("classId", classId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
        }
        if (!p.read)
        {
            err.errNo = -1;
            err.errMsg = "无权访问";
            context.Response.Write(err.ToJson());
            return;
        }
        TableInfo table = new TableInfo(dataTypeId);

        List<FieldInfo> fieldList = table.fields.FindAll(delegate(FieldInfo v)
        {
            return v.visible || v.isNecessary;
        });
        string where = "";// " and A.orderid>-3";
        if(type[0]=='0')where += " and A.orderid<0 ";
        if(type[1]=='0')where += " and A.orderid<>-1 ";
        if(type[2]=='0')where += " and A.orderid<>-2 ";
        if(type[3]=='0')where += " and A.orderid<>-3 ";
        //else if (type == 2) where = " and A.orderid=-3 ";
        if (keyword != "")
        {
            switch (searchField)
            {
                case "id":
                    where += " and A." + searchField + "=" + keyword + "";
                    break;
                case "title":
                    where += " and A." + searchField + " like '%" + keyword + "%'";
                    break;
                case "userId":
                    object userId = Sql.ExecuteScalar("select id from m_admin where uname=@uname", new SqlParameter[]{
                        new SqlParameter("uname",keyword)
                    });
                    if (userId != null)
                    {
                        where += " and A." + searchField + "="+userId.ToString();
                    }
                    else
                    {
                        where += " and A.userId=-1 ";
                    }
                    break;
                default:
                    where += " and ";
                    FieldInfo list = table.fields.Find(delegate(FieldInfo v)
                    {
                        if (v.name == searchField) return true;
                        else return false;
                    });
                    if (list.type == "String") {
                        where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                        where += searchField + " like '%" + keyword + "%'";
                    }
                    else if (list.type == "DateTime")
                    {
                        string[] item = keyword.Split(',');
                        where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                        where += searchField + ">='"+item[0].ToString()+"' and "+searchField + "<='"+item[1].ToString()+"'";
                    }
                    else
                    {
                        string[] item = keyword.Split(',');
                        where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                        where += searchField + ">="+item[0].ToString()+" and "+searchField + "<="+item[1].ToString();
                    }
                    break;
            }
        }
        if (!p.audit) where += " and A.userId="+login.value.id.ToString();
        ReturnPageData dataList = table.getDataList(moduleId,classId, pageNo, orderBy, sortDirection, where);
        object[] data = new object[] { fieldList, dataList };
        err.userData = data;
        context.Response.Write(err.ToJson());
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}