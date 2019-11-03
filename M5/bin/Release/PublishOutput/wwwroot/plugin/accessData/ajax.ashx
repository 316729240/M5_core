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
        if (m == "list") list(context);
    }

    void list(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        int pageNo = s_request.getInt("pageNo");
        string [] datefw=s_request.getString("datefw").Trim().Split(',');
        string userName = s_request.getString("userName").Trim();
        ReturnPageData page = new ReturnPageData();
        page.pageNo = pageNo;
        SqlParameter[] p = new SqlParameter[] {
            new SqlParameter("minDate",datefw[0]+" 0:0:0"),
            new SqlParameter("maxDate",datefw[1]+" 23:59:59"),
            new SqlParameter("userName",userName)
        };
        string sql = "";
        string where = "";
        if (userName != "") where = " and u_userName like '%'+@userName+'%'";
        page.recordCount = (int)Sql.ExecuteScalar("select count(1) from u_accessData where u_createDate>@minDate and u_createDate<@maxDate "+where,p);
        sql = "select A.id,A.u_title,A.u_url,A.u_userName,A.u_createDate,ROW_NUMBER() Over(order by A.u_createdate) as rowNum from u_accessData A  where u_createDate>@minDate and u_createDate<@maxDate  "+where;
        sql = "select * from (" + sql + ") M where M.rowNum> " + ((pageNo - 1) * page.pageSize).ToString() + " and M.rowNum<" + (pageNo * page.pageSize + 1).ToString();
        page.data = Sql.ExecuteArray(sql, p);
        info.userData = page;
        context.Response.Write(info.ToJson());
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}