<%@ WebHandler Language="C#" Class="verification"%>
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
public class verification :  IHttpHandler,System.Web.SessionState.IRequiresSessionState {

    public void ProcessRequest(HttpContext context)
    {
        validatedCode v = new validatedCode();
        string code = v.CreateVerifyCode();            //取随机码 

        v.CreateImageOnPage(code, context);       // 输出图片 
       context.Session["CheckCode"] = code;
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}