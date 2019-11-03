<%@ WebHandler Language="C#" Class="loginInterface"%>
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
using System.Collections.Specialized;
public class loginInterface :  IHttpHandler,System.Web.SessionState.IRequiresSessionState {

    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        if (context.Request["_m"] == null) context.Response.End();
        string m = context.Request["_m"].ToString();
        if (m == "login_qq") login_qq(context);
        else if (m == "login_qq_redirect") login_qq_redirect(context);
        if (m == "login_sina") login_sina(context);
        else if (m == "login_sina_redirect") login_sina_redirect(context);

    }
    void login_sina(HttpContext context)
    {
        string sina_appId = Config.userConfig["account"].Item("sina_appId");
        string backUrl = "http://" + context.Request.Url.Authority  + context.Request.Url.AbsolutePath+"?_m=login_sina_redirect";
        context.Response.Redirect("https://api.weibo.com/oauth2/authorize?client_id="+sina_appId+"&response_type=code&redirect_uri="+context.Server.UrlEncode(backUrl));
    }
    void login_sina_redirect(HttpContext context)
    {
        string sina_id = context.Session["sina_id"] == null ? "" : context.Session["sina_id"].ToString();
        if (sina_id == "") {
            string code = s_request.getString("code");
            string sina_appId = Config.userConfig["account"].Item("sina_appId");
            string sina_appKey = Config.userConfig["account"].Item("sina_appKey");
            string host = context.Request.Url.Authority;
            string backUrl = "http://" + host + context.Request.Url.AbsolutePath + "?_m=login_sina_redirect";
            string html = API.readUrl("https://api.weibo.com/oauth2/access_token?client_id=" + sina_appId + "&client_secret=" + sina_appKey + "&grant_type=authorization_code&redirect_uri=" + context.Server.UrlEncode(backUrl) + "&code=" + code);
            NameValueCollection value = new NameValueCollection();
            value.Add("client_id", sina_appId);
            value.Add("client_secret", sina_appKey);
            html=Http.postUrl("https://api.weibo.com/oauth2/access_token?grant_type=authorization_code&redirect_uri=" + context.Server.UrlEncode(backUrl) + "&code=" + code,value);
            sina_id = html.SubString("\"uid\":\"", "\"");
            if (sina_id == "") context.Response.End();
            context.Session["sina_id"] = sina_id;
        }
        object obj= Sql.ExecuteScalar("select id from u_account where sina_id=@openId",new SqlParameter[] {
            new SqlParameter("openId",sina_id)
        });
        if (obj != null)
        {
            //帐号存在登录
            ErrInfo err= UserClass.login((double)obj);
            if (err.errNo < 0) { context.Response.Write(err.ToJson()); return; }
            context.Response.Redirect("/");
        }
        else
        {
            //帐号不存在
            PageContext.Current.Response.Redirect("connect.html");
        }
    }
    void login_qq(HttpContext context)
    {
        string qq_appId = Config.userConfig["account"].Item("qq_appId");
        string backUrl = "http://" + context.Request.Url.Authority  + context.Request.Url.AbsolutePath+"?_m=login_qq_redirect";
        context.Response.Redirect("https://graph.qq.com/oauth2.0/authorize?response_type=code&client_id="+qq_appId+"&redirect_uri="+context.Server.UrlEncode(backUrl));
    }
    void login_qq_redirect(HttpContext context)
    {
        string qq_id = context.Session["qq_id"] == null ? "" : context.Session["qq_id"].ToString();
        if (qq_id == "") {
            string code = s_request.getString("code");
            string qq_appId = Config.userConfig["account"].Item("qq_appId");
            string qq_appKey = Config.userConfig["account"].Item("qq_appKey");
            string host = context.Request.Url.Authority;
            //host = "www.mwms4.com";
            string backUrl = "http://" + host + context.Request.Url.AbsolutePath + "?_m=login_qq_redirect";
            string html = API.readUrl("https://graph.qq.com/oauth2.0/token?grant_type=authorization_code&client_id=" + qq_appId + "&client_secret=" + qq_appKey + "&code=" + code + "&redirect_uri=" + context.Server.UrlEncode(backUrl));
            string[] list = html.Split('&');
            string access_token = "", expires_in = "";
            for (int i = 0; i < list.Length; i++)
            {
                string[] item = list[i].Split('=');
                if (item[0] == "access_token") access_token = item[1];
                else if (item[0] == "expires_in") expires_in = item[1];
            }
            html = API.readUrl("https://graph.qq.com/oauth2.0/me?access_token=" + access_token);
            qq_id = html.SubString("openid\":\"", "\"");
            if (qq_id == "") context.Response.End();
            context.Session["qq_id"] = qq_id;
        }
        object obj= Sql.ExecuteScalar("select id from u_account where qq_id=@openId",new SqlParameter[] {
            new SqlParameter("openId",qq_id)
        });
        if (obj != null)
        {
            //帐号存在登录
            ErrInfo err= UserClass.login((double)obj);
            if (err.errNo < 0) { context.Response.Write(err.ToJson()); return; }
            context.Response.Redirect("/");
        }
        else
        {
            //帐号不存在
            PageContext.Current.Response.Redirect("connect.html");
        }
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}