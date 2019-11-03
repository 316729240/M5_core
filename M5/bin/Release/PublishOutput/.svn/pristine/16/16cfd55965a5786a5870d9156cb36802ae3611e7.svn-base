<%@ WebHandler Language="C#" Class="ajax"%>
using System;
using System.Web;
using System.Collections.Generic;
using MWMS;
using Helper;
using System.Data.SqlClient;
using System.Collections;
using System.Text.RegularExpressions;
public class ajax : IHttpHandler {

    SafeReqeust s_request = new SafeReqeust(0, 0);
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        login.isManagerLogin();
        string [] script=Regex.Split(System.IO.File.ReadAllText(context.Server.MapPath("script.config")),"go");
        for(int i = 0; i < script.Length; i++)
        {
            try {
                Sql.ExecuteNonQuery(script[i]);
            }
            catch { }
        }
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}