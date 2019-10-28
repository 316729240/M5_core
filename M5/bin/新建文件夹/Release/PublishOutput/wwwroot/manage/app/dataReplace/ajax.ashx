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
        }
        else if (m == "fieldList")
        {
            ErrInfo info = new ErrInfo();
            double id = s_request.getDouble("id");
            TableInfo t = new TableInfo(id);
            info.userData = t.fields;
            context.Response.Write(info.ToJson());
        }
        else if (m == "findReplace") findReplace(context);
        else if (m == "replace") replace(context);

    }
    void replace(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double dataTypeId = s_request.getDouble("dataTypeId");
        string keyword1 = s_request.getString("keyword1");
        string keyword2 = s_request.getString("keyword2");
        string ids = s_request.getString("ids");


        string[] fields = s_request.getString("fieldList").Split(',');
        string tableName = (string)Sql.ExecuteScalar("select tablename from datatype where id=@id", new SqlParameter[]{
            new SqlParameter("id",dataTypeId)
        });
        bool isTitle=false;
        string flist = "",uflist="";
        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i] == "title") isTitle = true;
            if (fields[i].IndexOf("u_") == 0)
            {
                if (flist != "") { flist += ","; uflist += ","; }
                flist += fields[i];
                uflist += fields[i] + "=@" + fields[i];
            }
        }

        string [] id = ids.Split(',');
        for (int i = 0; i < id.Length; i++)
        {
            if (uflist != "") {
                
                SqlParameter[] p = null;
                SqlDataReader rs = Sql.ExecuteReader("select " + flist + " from [" + tableName + "] where id=" + id[i]);
                if (rs.Read())
                {
                    p=new SqlParameter[rs.FieldCount];
                    for (int i1 = 0; i1 < rs.FieldCount; i1++)
                    {
                        p[i1] = new SqlParameter(rs.GetName(i1), rs[i1].ToString().Replace(keyword1,keyword2));
                    }
                }
                rs.Close();
                if (p != null)
                {
                    Sql.ExecuteNonQuery("update [" + tableName + "] set " + uflist+" where id="+id[i],p);
                }
            }
        }
        if (isTitle) Sql.ExecuteNonQuery("update mainTable set title=replace(title,@keyword1,@keyword2) where id in (" + ids + ")", new SqlParameter[]{
            new SqlParameter("keyword1",keyword1),
            new SqlParameter("keyword2",keyword2)
        });
        context.Response.Write(info.ToJson());
    }
    void findReplace(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double dataTypeId = s_request.getDouble("dataTypeId");
        double moduleId=s_request.getDouble("moduleId");
        double classId = s_request.getDouble("classId");
        string keyword = s_request.getString("keyword");
        string userWhere = "";
        if (moduleId == 0 && classId == 0 && !login.value.isAdministrator)
        {
            info.errNo = -1;
            info.errMsg = "没有权限";
            context.Response.Write(info.ToJson());
            return;
        }
        else if (moduleId > 0 && classId<8)
        {
            Permissions p=login.value.getModulePermissions(moduleId);
            if (p.read && p.write)
            {
                if (!p.audit) userWhere = "  A.userId=@userId";
            }
            else
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
                context.Response.Write(info.ToJson());
                return;
            }

        }
        else if (classId > 0)
        {
            Permissions p = login.value.getColumnPermissions(classId);
            if (p.read && p.write)
            {
                if (!p.audit) userWhere = " and A.userId=@userId";
            }
            else
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
                context.Response.Write(info.ToJson());
                return;
            }
        }
        if (keyword == "")
        {
            info.errNo = -1;
            info.errMsg = "关键词不能为空";
            context.Response.Write(info.ToJson());
            return;
        }
        if (s_request.getString("fieldList") == "")
        {
            info.errNo = -1;
            info.errMsg = "没有选择要替换的字段";
            context.Response.Write(info.ToJson());
            return;
        }
        string[] fields = s_request.getString("fieldList").Split(',');
        string tableName=(string)Sql.ExecuteScalar("select tablename from datatype where id=@id",new SqlParameter[]{
            new SqlParameter("id",dataTypeId)
        });
        string flist = "";
        for (int i = 0; i < fields.Length; i++)
        {
            if (flist != "") flist += " or ";
            if (fields[i].IndexOf("u_") == 0)
            {
                flist += "B."+fields[i]+" like '%'+@keyword+'%'";
            }
            else
            {
                flist += "A." + fields[i] + " like '%'+@keyword+'%'";
            }
        }
        string where = "";
        if (classId > 7)
        {
            ColumnInfo column = ColumnClass.get(classId);
            if (column.classId == 7)
            {
                where = "  A.rootId=" + classId.ToString();
            }
            else
            {
                where = "  A.classId in (" + column.childId + ")";
            }
        }
        else if (moduleId > 0)
        {
            where = "  A.moduleId=" + moduleId.ToString();
        }
        if (where != "" && userWhere!="") where += " and ";
        where += userWhere;
        if (where != "") where += " and ";
        where += " (" + flist + ") ";
        string sql = "select top 100000 A.id from mainTable A left join "+tableName+" B on A.id=B.id where "+where;
        ArrayList list = Sql.ExecuteArrayObj(sql, new SqlParameter[]{
            new SqlParameter("keyword",keyword),
            new SqlParameter("userId",login.value.id)
        });
        info.userData = list;
        context.Response.Write(info.ToJson());
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}