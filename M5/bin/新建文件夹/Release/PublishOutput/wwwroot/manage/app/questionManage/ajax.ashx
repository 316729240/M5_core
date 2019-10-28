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
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "edit") edit(context);
        else if (m == "read") read(context);
        else if (m == "editAnswer") editAnswer(context);
        else if (m == "list") list(context);
        else if (m == "delAnswer") delAnswer(context);
        else if (m == "delData") delData(context);
        else if (m == "readAnswer") readAnswer(context);
        else if (m == "setAnswerId") setAnswerId(context);

    }
    void setAnswerId(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        double answerId = s_request.getDouble("answerId");
        Sql.ExecuteNonQuery("update u_question set u_answerId=@answerId where id=@id",new SqlParameter[] { new SqlParameter("answerId", answerId), new SqlParameter("id", id) });
        context.Response.Write(info.ToJson());
    }
    void readAnswer(HttpContext context)
    {
        double id = s_request.getDouble("id");
        ErrInfo info = new ErrInfo();
        Dictionary<string,object> data=Helper.Sql.ExecuteDictionary("select content,dataId,id from u_answer where id=@id", new SqlParameter[] { new SqlParameter("id", id) });
        info.userData = data;
        context.Response.Write(info.ToJson());
    }
    void delData(HttpContext context)
    {
        double dataTypeId = -1;
        string ids = context.Request.Form["ids"].ToString();
        double moduleId = s_request.getDouble("moduleId");
        double classId = s_request.getDouble("classId");
        int tag =int.Parse(context.Request.Form["tag"].ToString());
        Permissions p = null;
        if (classId < 8 || classId==moduleId)
        {
            SqlDataReader rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new SqlParameter[] { new SqlParameter("moduleId", moduleId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
            p = login.value.getModulePermissions(moduleId);

        }
        else
        {
            SqlDataReader rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new SqlParameter[] { new SqlParameter("classId", classId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
            p = login.value.getColumnPermissions(classId);
        }
        ErrInfo info = new ErrInfo();
        if (p.delete)
        {
            if (p.audit)
            {
                info = TableInfo.delData(dataTypeId, ids, tag == 1, login.value);
            }
            else
            {
                info = TableInfo.delData(dataTypeId, ids, tag == 1, login.value);
            }
            if (tag == 1)Sql.ExecuteNonQuery("delete from u_answer where dataId in ("+ids+")");

        }
        else{
            info.errNo = -1;
            info.errMsg = "权限不足";
        }
        context.Response.Write(info.ToJson());
    }
    void delAnswer(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double dataId = s_request.getDouble("dataId");
        string ids = s_request.getString("ids");
        string[] id = ids.Split(',');
        try
        {
            for (int i = 0; i < id.Length; i++)
            {
                double.Parse(id[i]);
            }
        }
        catch
        {
            info.errNo = -1;
            info.errMsg = "参数不合法";
            context.Response.Write(info.ToJson());
            context.Response.End();
        }
        Sql.ExecuteNonQuery("delete from u_answer where id in ("+ids+")");
        Sql.ExecuteNonQuery("update u_question set u_answerCount=(select count(1) from u_answer where u_question.id=dataId) where id=@dataId",new SqlParameter[] {
            new SqlParameter("dataId",dataId)
        });
        context.Response.Write(info.ToJson());
    }
    void list(HttpContext context)
    {
        double dataId = s_request.getDouble("dataId");
        ErrInfo info = new ErrInfo();
        ArrayList data=Helper.Sql.ExecuteArray("select id,content,createDate from u_answer where dataId=@dataId order by createDate desc", new SqlParameter[] { new SqlParameter("dataId", dataId) });
        info.userData = data;
        context.Response.Write(info.ToJson());
    }
    void read(HttpContext context)
    {
        double id = s_request.getDouble("id");
        ErrInfo info = new ErrInfo();
        Dictionary<string,object> data=Helper.Sql.ExecuteDictionary("select A.title,A.classId,A.skinId,A.url,B.* from mainTable A inner join  u_question B on A.id=B.id where A.id=@id", new SqlParameter[] { new SqlParameter("id", id) });
        info.userData = data;
        context.Response.Write(info.ToJson());
    }
    void editAnswer(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double dataId= s_request.getDouble("dataId");
        double id= s_request.getDouble("id");
        string content= s_request.getString("content");
        if (id > 0) {
            Sql.ExecuteNonQuery("update u_answer set content=@content where id=@id",new SqlParameter[] {
                new SqlParameter("id",id),
                new SqlParameter("dataId",dataId),
                new SqlParameter("content",content)
            });
        }
        else
        {
            Sql.ExecuteNonQuery("insert into  u_answer  (id,dataId,content,userId,createDate)values(@id,@dataId,@content,@userId,getDate()) ",new SqlParameter[] {
                new SqlParameter("id",double.Parse(API.GetId())),
                new SqlParameter("dataId",dataId),
                new SqlParameter("content",content),
                new SqlParameter("userId",login.value.id)
            });
        }
        
        Sql.ExecuteNonQuery("update u_question set u_answerCount=(select count(1) from u_answer where u_question.id=dataId) where id=@dataId",new SqlParameter[] {
            new SqlParameter("dataId",dataId)
        });
        context.Response.Write(info.ToJson());
    }
    void edit(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        RecordClass value = new RecordClass(login.value);
        value.tableName = "u_question";
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
        value.addField("classId", classId);
        value.addField("title", s_request.getString("title"));
        string u_content=s_request.getString("u_content");
        value.addField("u_content", u_content);
        value.addField("u_answerCount", 0);
        if (id > 0)
        {
            info = value.update(id);
        }
        else
        {
            if (!p.delete && !p.audit) value.addField("orderId", -1);
            info = value.insert();
        }
        if (info.errNo > -1)
        {

        }
        context.Response.Write(info.ToJson());
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}