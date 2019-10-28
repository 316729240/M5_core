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
        page.recordCount = (int)Sql.ExecuteScalar("select count(1) from alipay_orders where status=1", p);
        sql = "select A.id,money,A.createdate,B.uname,ROW_NUMBER() Over(order by A.createdate) as rowNum from alipay_orders A left join m_admin B on A.userId=B.id where A.status=1";
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