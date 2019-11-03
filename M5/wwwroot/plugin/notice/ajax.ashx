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
        if (m == "read") read(context);
        if (m == "messageList") messageList(context);
        if (m == "setMessageStatus") setMessageStatus(context);
        if (m == "delMessage") delMessage(context);
        if (m == "messageCount")messageCount(context);

    }void messageCount(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string  ids = s_request.getString("ids");
        info.userData=Sql.ExecuteScalar("select count(1) from notice_read A inner join notice B on A.noticeId=B.id where userId=@userId and isRead=0",new SqlParameter[] { new SqlParameter("userId",login.value.id)});
        context.Response.Write(info.ToJson());

    }
    void setMessageStatus(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id=s_request.getDouble("id");
        int status= s_request.getInt("status");
        Sql.ExecuteNonQuery("update notice_read set isRead=@status where noticeId=@id and userId=@userId",new SqlParameter[] {
            new SqlParameter("id",id),
            new SqlParameter("status",status),
            new SqlParameter("userId",login.value.id)
        });
        context.Response.Write(info.ToJson());
    }
    void delMessage(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string  ids = s_request.getString("ids");
        Sql.ExecuteNonQuery("delete from notice_read where noticeId in ("+ids+")");
        context.Response.Write(info.ToJson());

    }
    void messageList(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double roleId = s_request.getDouble("roleId");
        int pageNo = s_request.getInt("pageNo");
        int status = s_request.getInt("status");
        string keyword = s_request.getString("keyword").Trim();
        ReturnPageData page = new ReturnPageData();
        page.pageNo = pageNo;
        string sql = "";
        string findName = "";
        if (keyword != "")
        {
            findName = " and uname like '%'+@keyword+'%'";
        }
        SqlParameter[] p = new SqlParameter[] { new SqlParameter("roleId", roleId), new SqlParameter("status", status), new SqlParameter("keyword", keyword), new SqlParameter("userId",login.value.id) };
        page.recordCount = (int)Sql.ExecuteScalar("select count(1) from notice_read A,notice B,maintable C,m_admin D where  B.id=A.noticeId and B.id=C.id and A.userid=D.id and A.isread=@status and A.userId=@userId", p);
        sql = "select B.id,C.title,C.createDate,D.uname,ROW_NUMBER() Over(order by C.createdate desc) rowNum from notice_read A,notice B,maintable C,m_admin D where  B.id=A.noticeId and B.id=C.id and A.userid=D.id  and A.isread=@status and A.userId=@userId";
        sql = "select * from (" + sql + ") M where M.rowNum> " + ((pageNo - 1) * page.pageSize).ToString() + " and M.rowNum<" + (pageNo * page.pageSize + 1).ToString();
        page.data = Sql.ExecuteArray(sql, p);
        info.userData = page;
        context.Response.Write(info.ToJson());
    }
    void read(HttpContext context)
    {
        double id = s_request.getDouble("id");
        ErrInfo info = new ErrInfo();
        Dictionary<string,object> data=Helper.Sql.ExecuteDictionary("select A.title,A.classId,A.skinId,A.url,C.uname,B.* from mainTable A inner join  notice B  on A.id=B.id  inner join m_admin C on A.userid=C.id where A.id=@id", new SqlParameter[] { new SqlParameter("id", id) });
        info.userData = data;
        context.Response.Write(info.ToJson());
    }
    void edit(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        RecordClass value = new RecordClass(login.value);
        value.tableName = "notice";
        double id=s_request.getDouble("id");
        double classId = s_request.getDouble("classId");
        /*Permissions p= login.value.getColumnPermissions(classId);
        if (!p.write)
        {
            info.errNo = -1;
            info.errMsg = "没有权限";
            context.Response.Write(info.ToJson());
            return;
        }*/
        value.addField("classId", classId);
        value.addField("title", s_request.getString("title"));
        string u_content=s_request.getString("u_content");
        value.addField("u_content", u_content);
        string userList = s_request.getString("u_userList");
        string[] _uList = userList.Split(',');
        string groupList = s_request.getString("u_groupList");
        value.addField("u_userList", userList);
        value.addField("u_groupList",groupList );
        if (id > 0)
        {
            info = value.update(id);
        }
        else
        {
            //if (!p.delete && !p.audit) value.addField("orderId", -1);
            info = value.insert();
        }
        if (info.errNo > -1)
        {

            Sql.ExecuteNonQuery("delete from notice_read where noticeid=@noticeid",new SqlParameter[] {
                        new SqlParameter("noticeId",info.userData)
                    });
            for(int i = 0; i < _uList.Length; i++)
            {
                SqlDataReader rs=Sql.ExecuteReader("select id from m_admin where uname=@uname",new SqlParameter[] { new SqlParameter("uname",_uList[i])});
                if (rs.Read())
                {
                    Sql.ExecuteNonQuery("insert into notice_read (noticeId,userId,isRead)values(@noticeId,@userId,0)",new SqlParameter[] {
                        new SqlParameter("noticeId",info.userData),
                        new SqlParameter("userId",rs[0])
                    });
                }
                rs.Close();
            }
            if (groupList.IndexOf("9896847028") > -1)
            {
                Sql.ExecuteNonQuery("insert into notice_read (noticeId,userId,isRead) select "+info.userData.ToString()+",id,0 from m_admin where classid=9896847028 ");
            }
            if (groupList.IndexOf("9896848409") > -1)
            {
                Sql.ExecuteNonQuery("insert into notice_read (noticeId,userId,isRead) select "+info.userData.ToString()+",id,0 from m_admin where classid=9896848409 ");
            }
        }
        context.Response.Write(info.ToJson());
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}