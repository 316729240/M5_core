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
        if (m == "readUserReport")
        {
            ErrInfo info = new ErrInfo();
            double roleId = s_request.getDouble("roleId");
            string name = s_request.getString("name");
            string day = s_request.getString("day");
            int sortDirection = s_request.getInt("sortDirection");
            string where = "";
            string orderBy = "";
            if (name != "")
            {
                if (name == "uname") name = "B.uname";
                if (name == "count") name = "A.count";
                orderBy = " order by " + name + " " + (sortDirection == 1 ? "desc" : "");
            }
            if(roleId>0)where=" inner join admin_role C on B.id=C.userId and C.roleId=" + roleId.ToString();
            string dayWhere = "";
            if (day != "")
            {
                DateTime.Parse(day);
                dayWhere = " createDate>'" + day + " 0:0:0' and createDate<'" + day + " 23:59:59' ";
            }
            if (!login.value.isAdministrator)
            {
                if (dayWhere != "") dayWhere += " and ";
                dayWhere = dayWhere+"  userId="+login.value.id.ToString();
            }
            if (dayWhere != "") dayWhere = " where " + dayWhere;
            info.userData = Sql.ExecuteArrayObj("select B.uname,A.count from (select userId,count(1) count from mainTable " + dayWhere + " group by userId) A left join  m_admin B on A.userId=B.id " + where + orderBy);
            context.Response.Write(info.ToJson());
        }else if (m == "getData")
        {
            ErrInfo info = new ErrInfo();
            int flag = s_request.getInt("flag");
            if (login.value.isAdministrator)
            {
                Cache cache = new Cache("statistical_getData", null, 720,Config.cachePath);
                if (cache.get() || flag==1)
                {
                    info.userData = Sql.ExecuteArrayObj("select CONVERT(varchar(100), createdate, 23),count(1) from mainTable where  createdate>DATEADD(day,-7,getDate()) group by CONVERT(varchar(100),createdate, 23)  order by CONVERT(varchar(100), createdate, 23) ");
                    cache.set(info.userData);
                }
                else
                {
                    info.userData = cache.data;
                }
            }
            else
            {
                Cache cache = new Cache("statistical_getData", new string[] { login.value.id.ToString() }, 720, Config.cachePath);
                if (cache.get() || flag == 1)
                {
                    info.userData = Sql.ExecuteArrayObj("select CONVERT(varchar(100), createdate, 23),count(1) from mainTable where userId=@id and createdate>DATEADD(day,-7,getDate()) group by CONVERT(varchar(100),createdate, 23)  order by CONVERT(varchar(100), createdate, 23) ", new SqlParameter[]{
                    new SqlParameter("id",login.value.id)
                    });
                    cache.set(info.userData);
                }
                else
                {
                    info.userData = cache.data;
                }
            }
            context.Response.Write(info.ToJson());
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}