﻿<%@ WebHandler Language="C#" Class="api" %>

using System;
using System.Web;
using System.Text.RegularExpressions;

using System.Collections.Generic;
using MWMS;
public class api : IHttpHandler {

    public void ProcessRequest(HttpContext context)
    {
        SafeReqeust s_request = new SafeReqeust(0, 0);
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "login")
        {

            ReturnValue err =new ReturnValue();
            try
            {
                err = UserClass.manageLogin(context.Request.Form["uname"].ToString(), context.Request.Form["pword"].ToString(),1);
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = "发生异常："+ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "checkManagerLogin")
        {
            ReturnValue err = new ReturnValue();
            try {
                LoginInfo info = new LoginInfo();
                err.userData = info.value;
                err.errNo = (info.value != null && info.isManagerLogin()) ? 0 : -1;
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = "发生异常："+ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "getDirName")
        {
            
            string name = s_request.getString("name");
            ReturnValue info = new ReturnValue();
            string pinyin = name.GetPinYin();
            if (pinyin.Length > 15) pinyin = name.GetPinYin('2');
            pinyin = Regex.Replace(pinyin, "[ "+@"\-_"+"`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）——|{}【】‘；：”“'。，、？]", "");
            info.userData = pinyin;//.Replace(" ","").Trim();
            context.Response.Write(info.ToJson());

        }
        else if (m == "readIcon")
        {
            ReturnValue info = new ReturnValue();
            string path = Config.staticPath + "icon/";
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(HttpContext.Current.Server.MapPath("~" + path));
            System.IO.FileInfo[] f = dir.GetFiles("*.jpg");
            string[] file = new string[f.Length];
            for (int i = 0; i < f.Length; i++)
            {
                file[i] = path + f[i].Name;
            }
            info.userData = file;
            context.Response.Write(info.ToJson());
        }
        else if (m == "getDataUrl")
        {
            double id = s_request.getDouble("id");
            ReturnValue info = new ReturnValue();
            System.Data.SqlClient.SqlDataReader rs1 = Helper.Sql.ExecuteReader("select url from maintable where id=@id", new System.Data.SqlClient.SqlParameter[]{
                new System.Data.SqlClient.SqlParameter("id",id)
            });
            if (rs1.Read())
            {
                info.userData =TemplateEngine._replaceUrl(Config.webPath + rs1[0] + "." + BaseConfig.extension);
            }
            else
            {
                info.errNo = -1;
            }
            context.Response.Write(info.ToJson());
        }
        context.Response.End();
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}