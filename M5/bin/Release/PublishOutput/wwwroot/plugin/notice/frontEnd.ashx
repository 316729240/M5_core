<%@ WebHandler Language="C#" Class="frontEnd"%>
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
public class frontEnd : IHttpHandler, System.Web.SessionState.IRequiresSessionState {

    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        if (context.Request["_m"] == null) context.Response.End();
        string m = context.Request["_m"].ToString();
        if(m == "read")
        {
            LoginInfo l = new LoginInfo();
            double noticeId= s_request.getDouble("id");
            Sql.ExecuteNonQuery("update notice_read set isRead=1 where noticeId=@noticeId and userId=@userId",new SqlParameter[] {
                new SqlParameter("noticeId",noticeId),
                new SqlParameter("userId",l.value.id)
            });
            context.Response.Redirect("/spacecp/notice_read-"+noticeId.ToString()+".html");
        }

    }


    public bool IsReusable {
        get {
            return false;
        }
    }

}