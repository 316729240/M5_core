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
using System.IO;
using System.Web.Script.Serialization;
public class ajax : IHttpHandler
{
    LoginInfo login = new LoginInfo();
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "cardPermissions")
        {
            context.Response.Write(login.value.isAdministrator);
        }
        else if (m == "addColumn")
        {
            ErrInfo errinfo = new ErrInfo();
            double moduleId = s_request.getDouble("moduleId");
            double classId = s_request.getDouble("classId");
            double dataTypeId = s_request.getDouble("dataTypeId");
            Permissions p = null;
            if (classId<8)
            {
                classId = 7;
                p=login.value.getModulePermissions(moduleId);
            }
            else
            {
                p = login.value.getColumnPermissions(classId);
            }
            if (!p.all)
            {
                errinfo.errNo = -1;
                errinfo.errMsg = "没有编辑该栏目的权限";
                context.Response.Write(errinfo.ToJson());
                return;
            }
            string [] list = s_request.getString("list").Split('\n');
            string msg = "";
            double[] parentid = new double[] { classId, -1, -1, -1, -1, -1, -1 };
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Trim() != "")
                {
                    int layer = Regex.Split(list[i], "  ").Length-1;
                    classId=parentid[layer];
                    string[] temp = list[i].Trim().Split('\t');
                    string classname = temp[0].Trim(), dirname = "",keyword="";
                    if (temp.Length > 1) dirname = temp[1];
                    else
                    {
                        dirname = classname.GetPinYin();
                        if (dirname.Length > 15) dirname = classname.GetPinYin('2');
                        dirname = Regex.Replace(dirname, "[ " + @"\-_" + "`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）——|{}【】‘；：”“'。，、？]", "");
                    }
                    if (temp.Length > 2) keyword = temp[2];
                    ColumnInfo info = new ColumnInfo();
                    info.className = classname;
                    info.classId = classId;
                    info.keyword = keyword;
                    if(classId!=7){
                        ColumnInfo parent = ColumnClass.get(classId);
                        if (parent != null) info.rootId = parent.rootId;
                    }
                    info.moduleId =moduleId;
                    info.dirName = dirname;
                    info.saveDataType = dataTypeId;
                    ErrInfo err = new ErrInfo();
                    if (classId <1)
                    {
                        err.errNo = -1;
                        err.errMsg = "上级栏目不存在";
                    }
                    else
                    {
                        err = ColumnClass.edit(info, login.value);
                    }
                    if (err.errNo > -1)
                    {
                        double newId = Convert.ToDouble(err.userData);
                        if (newId > 0) parentid[layer+1] = newId;
                    }
                    else
                    {
                        msg += "["+classname+"]";
                        //errinfo.errNo = -1;
                        //errinfo.errMsg += classname+"添加失败 "+err.errMsg + "<br>";
                    }
                }
            }
            errinfo.userData = msg;
            context.Response.Write(errinfo.ToJson());
        }

    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}