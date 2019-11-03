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
public class ajax : IHttpHandler
{
    LoginInfo login = new LoginInfo();
    int webFAId = 0;//默认网站模板方案
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if(m=="renderView")renderView(context);
        else if (m == "getContentData") getContentData(context);
        else if (m == "save")
        {
            ErrInfo info = new ErrInfo();
            string url = s_request.getString("url");
            string html=s_request.getString("html");
            Uri u = new Uri(url);
            bool isMobilePage = false;
            string virtualWebDir = "";
            string newUrl=Rewrite.urlZhuanyi(u, ref isMobilePage, ref virtualWebDir);
            TemplateInfo v = TemplateClass.get(newUrl, false);
            string head = "", foot = "";
            MatchCollection mc;
            Regex r = new Regex(@"(.*)<body([^>]*)>", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(v.u_content);
            if (mc.Count > 0) head = mc[0].Value;
            r = new Regex(@"</body>(.*)$", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(v.u_content);
            if (mc.Count > 0) foot = mc[0].Value;

            r = new Regex("<div class=\"m5_template m5_view\"(.*?)</div>", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(html);
            for(int i = 0; i < mc.Count; i++)
            {
                string value =PageContext.Current.Server.HtmlDecode(  mc[i].Value.SubString("viewvalue=\"", "\""));
                html = html.Replace(mc[i].Value,"${"+value+"}");
            }
            html = head+ html+foot;
            v.u_content = html;
            info=TemplateClass.edit(v,login.value);
            context.Response.Write(info.ToJson());
        }
    }
    void getContentData(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string url = s_request.getString("url");
        TemplateInfo v = TemplateClass.get(url, false);
        info.userData = v.variable;
        context.Response.Write(info.ToJson());
    }
    void renderView(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string html ="${"+ s_request.getString("viewName")+"}";
        TE_statistical TE_statistical = new TE_statistical();
        TemplateEngine page = new TemplateEngine();
        page.isEdit = true;
        page.addVariable("sys", Config.systemVariables);
        page.addVariable("view", Config.viewVariables);
        Dictionary<string, object> _public = new Dictionary<string, object>();
        _public.Add("_pageNo", 1);
        page.addVariable("public", _public);
        page.TE_statistical = TE_statistical;
        page.render(ref html);
        info.userData = html;
        context.Response.Write(info.ToJson());
    }
    
    public bool IsReusable {
        get {
            return false;
        }
    }

}