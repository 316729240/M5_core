<%@ WebHandler Language="C#" Class="upload"%>
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
public class upload : IHttpHandler
{
    LoginInfo login = new LoginInfo();
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();

        ErrInfo info = new ErrInfo();
        context.Response.ContentType = "text/plain";

        string[] fp = new string[context.Request.Files.Count];
        ErrInfo e = null;
        for(int i=0;i<context.Request.Files.Count;i++){
            e =saveFile(context,context.Request.Files[0], "/enclosure/");
            //if (e.errNo >-1)fp[i] = e.userData.ToString();
        }
        //info.userData = fp;
        context.Response.Write(e.ToJson());
        context.Response.End();
    }

    ErrInfo saveFile(HttpContext context,HttpPostedFile file,string filePath)
    {
        ErrInfo err = new ErrInfo();

        try
        {
            double classId = double.Parse(context.Request.Form["classId"]);
            Permissions p=login.value.getColumnPermissions(classId);
            if (!p.write)
            {
                err.errNo = -1;
                err.errMsg = "没有权限";
                return err;
            }
            RecordClass value = new RecordClass(login.value);
            value.tableName = "article";
            value.addField("classId", classId);
            value.addField("skinId", 0);
            value.addField("u_keyword", "");
            value.addField("u_fromWeb", "");
            if (!p.delete && !p.audit) value.addField("orderId", -1);
            string u_info ="";
            string u_content="";
            //string path = Config.webPath + filePath + System.DateTime.Now.ToString("yyyy-MM/");
            //if (!System.IO.Directory.Exists(PageContext.Current.Server.MapPath(path))) System.IO.Directory.CreateDirectory(PageContext.Current.Server.MapPath(path));
            string kzm = "";
            if (file.FileName.LastIndexOf(".") > -1) kzm = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1).ToLower();
            if (!Regex.IsMatch(kzm, "(txt)"))
            {
                err.errNo = -1;
                err.errMsg = "文件类型不合法";
                return err;
            }
            byte [] buff = new byte[file.ContentLength];
            file.InputStream.Read(buff,0,file.ContentLength);
            Microshaoft.Text.IdentifyEncoding w = new Microshaoft.Text.IdentifyEncoding();
            string name=w.GetEncodingName(buff);
            u_content=System.Text.Encoding.GetEncoding(name).GetString(buff);

            string [] list=Regex.Split(u_content, "(</div>|</p>)", RegexOptions.IgnoreCase);
            int infoLength = 600;
            for(int i = 0; i < list.Length; i++)
            {
                string html = API.nohtml(list[i]).Trim();
                infoLength-=API.GetStringLength(html);
                if (i>0 && infoLength < 0) break;
                if(html!="")u_info += "<p>"+html+"</p>";
            }

            value.addField("title",file.FileName.Replace(".txt",""));
            value.addField("u_info", u_info);
            value.addField("u_custom","");
            value.addField("u_content", u_content);
            err = value.insert();

            //string fileName = API.GetId() + "." + kzm;
            //file.SaveAs(PageContext.Current.Server.MapPath(path + fileName));
            //err.userData = path + fileName;
            return err;
        }catch(Exception ex)
        {
            err.errNo=-1;
            err.errMsg = ex.Message;
            return err;
        }
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}