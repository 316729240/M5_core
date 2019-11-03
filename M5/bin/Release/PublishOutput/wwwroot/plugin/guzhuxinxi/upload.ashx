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
            //string path = Config.webPath + filePath + System.DateTime.Now.ToString("yyyy-MM/");
            //if (!System.IO.Directory.Exists(PageContext.Current.Server.MapPath(path))) System.IO.Directory.CreateDirectory(PageContext.Current.Server.MapPath(path));
            string kzm = "";
            if (file.FileName.LastIndexOf(".") > -1) kzm = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1).ToLower();
            if (!Regex.IsMatch(kzm, "(csv)"))
            {
                err.errNo = -1;
                err.errMsg = "文件类型不合法";
                return err;
            }
            byte [] buff = new byte[file.ContentLength];
            file.InputStream.Read(buff,0,file.ContentLength);
            Microshaoft.Text.IdentifyEncoding w = new Microshaoft.Text.IdentifyEncoding();
            string name=w.GetEncodingName(buff);
            string u_content=System.Text.Encoding.GetEncoding(name).GetString(buff);

            string [] list=Regex.Split(u_content, "\n", RegexOptions.IgnoreCase);
            int infoLength = 600;
            string[] field = list[0].Split(',');
            TableInfo table = new TableInfo(22592528242);
            for(int i = 1; i < list.Length; i++)
            {
                string[] item = list[i].Split(',');
                RecordClass value = new RecordClass(22592528242,login.value);
                value.addField("classId", classId);
                for(int i1 = 0; i1 < item.Length; i1++)
                {
                    FieldInfo f=table.fields.Find(delegate(FieldInfo o){
                        return o.text==field[i1].Trim();
                    });
                    if(f!=null)value.addField(f.name, item[i1]);
                }
                value.insert();
            }

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