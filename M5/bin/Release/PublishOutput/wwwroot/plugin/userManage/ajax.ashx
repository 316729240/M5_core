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
public class ajax : IHttpHandler {
    SafeReqeust s_request = new SafeReqeust(0, 0);
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "readRole") readRole(context);
        if (m == "editRole") editRole(context);
        if (m == "delRole") delRole(context);
        else if (m == "readUserList") readUserList(context);
        else if (m == "edit") edit(context);
        else if (m == "delUser") delUser(context);
        else if (m == "getUser") getUser(context);
        else if (m == "resetPassword") resetPassword(context);
        else if (m == "setStatus") setStatus(context);
        else if (m == "editPassword") editPassword(context);
        else if (m == "cardPermissions")
        {
            context.Response.Write(login.value.isAdministrator);
        }
    }
    void editRole(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        string name = s_request.getString("name");
        if(id>-1 && id < 10)
        {
            info.errNo = -1;
            info.errMsg = "不能修改系统角色";
            context.Response.Write(info.ToJson());
            return;
        }
        if (id == -1) {
            id =double.Parse( API.GetId());
            Sql.ExecuteNonQuery("insert into role (id,name)values(@id,@name)",new SqlParameter[] {
            new SqlParameter("id",id),
            new SqlParameter("name",name)
            });
        }
        else
        {
            Sql.ExecuteNonQuery("update  role set name=@name where id=@id",new SqlParameter[] {
            new SqlParameter("id",id),
            new SqlParameter("name",name)
            });
        }
        context.Response.Write(info.ToJson());
    }
    void delRole(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        if (id < 10) {
            info.errNo = -1;
            info.errMsg = "不能删除系统角色";
        } else {
            Sql.ExecuteNonQuery("delete from  role where id=@id",new SqlParameter[] {
            new SqlParameter("id",id)
            });
            Sql.ExecuteNonQuery("delete from  permissions where dataId=@id",new SqlParameter[] {
            new SqlParameter("id",id)
            });
            Sql.ExecuteNonQuery("delete from  admin_role where roleId=@id",new SqlParameter[] {
            new SqlParameter("id",id)
            });
        }
        context.Response.Write(info.ToJson());
    }
    void editPassword(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string oldPassword = s_request.getString("oldPassword");
        string newPassword = s_request.getString("newPassword");
        info=UserClass.editPassword(login.value.id, oldPassword, newPassword, login.value);
        context.Response.Write(info.ToJson());
    }
    void setStatus(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        int status = s_request.getInt("status");
        info = UserClass.setState(s_request.getString("ids"), status==1);
        context.Response.Write(info.ToJson());
    }
    void resetPassword(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        string password = System.Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        info.userData = UserClass.editPassword(id,password,login.value);
        if (info.errNo > -1) info.userData = password;
        context.Response.Write(info.ToJson());
    }
    void getUser(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        info.userData = UserClass.get(id);
        context.Response.Write(info.ToJson());

    }
    void delUser(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        info=UserClass.del(s_request.getString("ids"));
        context.Response.Write(info.ToJson());
    }
    void edit(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        UserInfo user = new UserInfo();
        user.id = s_request.getDouble("id");
        user.username = s_request.getString("uname");
        user.password = s_request.getString("pword");
        user.mobile= s_request.getString("mobile");
        user.email = s_request.getString("email");
        user.phone = s_request.getString("phone");
        user.role = s_request.getString("role");
        user.filteringIP = s_request.getString("filteringIP");
        if (user.id > 0)
        {
            info = UserClass.edit(user, login.value);
        }
        else
        {
            info = UserClass.add(user, login.value);
        }
        context.Response.Write(info.ToJson());
    }
    void readUserList(HttpContext context)
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
        SqlParameter[] p = new SqlParameter[] { new SqlParameter("roleId", roleId), new SqlParameter("status", status), new SqlParameter("keyword", keyword) };
        if (roleId > 0)
        {
            page.recordCount = (int)Sql.ExecuteScalar("select count(1) from m_admin A  inner join admin_role B on A.id=B.userId where B.roleId=@roleId and A.status=@status" + findName, p);
            sql = "select A.id,A.uname,A.loginDateTime,B.roleId,ROW_NUMBER() Over(order by A.id) as rowNum from m_admin A  inner join admin_role B on A.id=B.userId where B.roleId=@roleId and A.classId=0  and A.status=@status" + findName;
        }
        else
        {
            page.recordCount = (int)Sql.ExecuteScalar("select count(1) from m_admin where  status=@status" + findName, p);
            sql = "select id,uname,loginDateTime,ROW_NUMBER() Over(order by id) as rowNum from m_admin  where  classId=0 and status=@status" + findName;
        }
        sql = "select * from (" + sql + ") M where M.rowNum> " + ((pageNo - 1) * page.pageSize).ToString() + " and M.rowNum<" + (pageNo * page.pageSize + 1).ToString();
        page.data = Sql.ExecuteArray(sql, p);
        info.userData = page;
        context.Response.Write(info.ToJson());
    }
    void readRole(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        info.userData = Sql.ExecuteArray("select id,name text from role ", null);
        context.Response.Write(info.ToJson());
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}