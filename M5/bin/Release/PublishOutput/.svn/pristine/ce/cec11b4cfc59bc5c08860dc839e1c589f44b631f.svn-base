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
public class ajax : IHttpHandler {
    
    SafeReqeust s_request = new SafeReqeust(0, 0);
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        double moduleId = s_request.getDouble("moduleId");
        double classId = s_request.getDouble("classId");
        int type = s_request.getInt("type");
        string searchField = s_request.getString("searchField");
        string keyword = s_request.getString("keyword");
        string [] day = s_request.getString("day").Split(',');
        DateTime day0 = new DateTime(), day1 = new DateTime();
        try
        {
             day0 = DateTime.Parse(day[0]);
             day1 = DateTime.Parse(day[1]).AddDays(1);
        }
        catch
        {
            context.Response.Write("时间范围选定不合法");
            context.Response.End();
        }
        Permissions p = null;
        if (moduleId ==classId)
        {
            p = login.value.getModulePermissions(moduleId);
        }
        else
        {
            p = login.value.getColumnPermissions(classId);
        }
        if (!p.read)
        {
            context.Response.Write("无权访问");
            context.Response.End();
            return;
        }
        
        string sql = "select A.title,A.url from mainTable A ";
        string mainWhere="";
        if (type == 0) mainWhere += " orderid>-1 ";
        else if (type == 1) mainWhere += " orderid==-1 ";
        else if (type == 2) mainWhere += " orderid==-2 ";
        mainWhere += " and A.createdate>@day0 and A.createdate<@day1";
        if (moduleId==classId)
        {
            mainWhere += " and A.moduleId=@moduleId ";
        }
        else
        {
            string childId = ColumnClass.getChildId(classId);
            mainWhere += " and  A.classId in (" + childId + ") ";
        }
        if (keyword != "")
        {
            switch (searchField)
            {
                case "id":
                    mainWhere += " and A." + searchField + "=@keyword";
                    break;
                case "title":
                    mainWhere += " and A." + searchField + " like '%'+ @keyword + '%'";
                    break;
                case "userId":
                    object userId = Sql.ExecuteScalar("select id from m_admin where uname=@uname", new SqlParameter[]{
                        new SqlParameter("uname",keyword)
                    });
                    if (userId != null)
                    {
                        mainWhere += " and A." + searchField + "=" + userId.ToString();
                    }
                    break;
            }
        }
        if (!p.audit) mainWhere += " and A.userId=" + login.value.id.ToString();
        DataTable table= Sql.ExecuteDataset(sql +" where " +mainWhere, new SqlParameter[] { 
            new SqlParameter("moduleId", moduleId), 
            new SqlParameter("keyword", keyword),
            new SqlParameter("day0", day0),
            new SqlParameter("day1", day1),
            
        
        }).Tables[0];
        context.Response.Clear();
        context.Response.ClearContent();
        context.Response.ClearHeaders();
//        context.Response.AddHeader("Content-Length", buff.Length.ToString());
        context.Response.AddHeader("Content-Transfer-Encoding", "binary");
        context.Response.AddHeader("Content-Disposition", "attachment;filename="+System.DateTime.Now.ToString("yyyyMMddhhmmss")+".csv");
        context.Response.ContentType = "application/octet-stream";
        for (int i = 0; i < table.Rows.Count; i++)
        {
            string line = "";
            for (int x = 0; x < table.Columns.Count; x++)
            {
                if (table.Columns[x].ColumnName == "url")
                {
                    line += Config.webPath + table.Rows[i][x].ToString() + "." + BaseConfig.extension + ",";
                }
                else
                {
                    line += table.Rows[i][x].ToString() + ",";
                }
            }
            line += "\n";
            context.Response.Write(line);
        }
        context.Response.Flush();
        context.Response.End();
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}