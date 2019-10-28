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
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "editDirName")
        {
            ErrInfo info = new ErrInfo();
            double id = s_request.getDouble("id");
            string dirName = s_request.getString("dirName");
            info = ColumnClass.editDirName(id, dirName, login.value);
            context.Response.Write(info.ToJson());
        }
        else if (m == "editModuleDirName")
        {
            ErrInfo info = new ErrInfo();
            double id = s_request.getDouble("id");
            string dirName = s_request.getString("dirName");
            info = ModuleClass.editDirName(id, dirName, login.value);
            context.Response.Write(info.ToJson());
        }
        else if (m == "moduleEdit")
        {
            moduleEdit(context);
        }
        else if (m == "columnEdit")
        {
            columnEdit(context);
        }
        else if (m == "moduleInfo")
        {
            ErrInfo err = new ErrInfo();
            double moduleId = s_request.getDouble("id");
            Permissions p = login.value.getModulePermissions(moduleId);
            if (!p.read)
            {
                err.errNo = -1;
                err.errMsg = "没有查看该栏目的权限";
                context.Response.Write(err.ToJson());
                return;
            }
            ModuleInfo info = ModuleClass.get(moduleId);
            if (info == null)
            {
                err.errNo = -1;
                err.errMsg = "没有找到指定id数据";
            }
            else
            {
                err.userData = info;
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "columnInfo")
        {
            ErrInfo err = new ErrInfo();
            ColumnInfo info = ColumnClass.get(s_request.getDouble("id"));
            if (info == null)
            {
                err.errNo = -1;
                err.errMsg = "没有找到指定id数据";
            }
            else
            {
                Permissions p = login.value.getColumnPermissions( info.id);
                if (p.read)
                {
                    err.userData = info;
                }
                else
                {
                    err.errNo = -1;
                    err.errMsg = "没有查看该栏目的权限";
                }
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "setColumnStatus") setColumnStatus(context);
    }

    void setColumnStatus(HttpContext context)
    {
        ErrInfo err = new ErrInfo();
        double classId = s_request.getDouble("columnId");
        int status=s_request.getInt("status");
        Permissions p = login.value.getColumnPermissions(classId);
        if (!p.write)
        {
            err.errNo = -1;
            err.errMsg = "没有编辑该栏目的权限";
            context.Response.Write(err.ToJson());
            return;
        }
        string childId = "";
        IDataReader rs = Sql.ExecuteReader("select childId from class where id=@id",new SqlParameter[] {
                new SqlParameter ("id",classId )
        });
        if (rs.Read())childId = rs[0].ToString();
        rs.Close();
        if (childId == "")
        {
            err.errNo = -1;
            err.errMsg = "指定栏目无效";
            context.Response.Write(err.ToJson());
            return;
        }
        Sql.ExecuteNonQuery("update class set orderId="+(status==1?"0":"-1")+" where id in (" + childId + ")");
        context.Response.Write(err.ToJson());
    }
    void moduleEdit(HttpContext context)
    {

        ModuleInfo info = new ModuleInfo();
        info.id = s_request.getDouble("id");
        info.moduleName = s_request.getString("moduleName");
        info.dirName = s_request.getString("dirName");
        info.type = s_request.getInt("type") == 1;
        info.saveDataType = s_request.getDouble("saveDataType");

        info.custom = s_request.getString("custom");
        info.watermark= s_request.getInt("watermark");
        info.thumbnailWidth = s_request.getInt("thumbnailWidth");
        info.thumbnailHeight = s_request.getInt("thumbnailHeight");
        info.thumbnailForce = s_request.getInt("thumbnailForce");
        info.saveRemoteImages = s_request.getInt("saveRemoteImages");
        info.inherit = s_request.getInt("inherit");
        info._domainName=s_request.getString("_domainName");
        info.domainName = s_request.getString("domainName");
        info.keyword = s_request.getString("keyword");
        info.info = s_request.getString("info");
        info.titleRepeat = s_request.getInt("titleRepeat");

        ErrInfo err = new ErrInfo();
        Permissions p = login.value.getModulePermissions(info.id);
        if (!p.all)
        {
            err.errNo = -1;
            err.errMsg = "没有编辑该模块的权限";
            context.Response.Write(err.ToJson());
            return;
        }

        err = ModuleClass.edit(info, login.value);
        Config.loadDomain();
        context.Response.Write(err.ToJson());
    }
    void columnEdit(HttpContext context)
    {

        ColumnInfo info = new ColumnInfo();
        info.id = s_request.getDouble("id");
        info.className = s_request.getString("className");
        info.classId = s_request.getDouble("classId");
        info.moduleId = s_request.getDouble("moduleId");
        info.dirName = s_request.getString("dirName");
        info.keyword = s_request.getString("keyword");
        info.maxIco = s_request.getString("maxIco");
        info.saveDataType = s_request.getDouble("saveDataType");
        info.skinId = s_request.getDouble("skinId");
        info.contentSkinId = s_request.getDouble("contentSkinId");
        info._skinId = s_request.getDouble("_skinId");
        info._contentSkinId = s_request.getDouble("_contentSkinId");
        info.info = s_request.getString("info");
        info.watermark= s_request.getInt("watermark");

        info.custom = s_request.getString("custom");

        info.thumbnailWidth = s_request.getInt("thumbnailWidth");
        info.thumbnailHeight = s_request.getInt("thumbnailHeight");
        info.thumbnailForce = s_request.getInt("thumbnailForce");
        info.saveRemoteImages = s_request.getInt("saveRemoteImages");
        info.inherit = s_request.getInt("inherit");
        info.domainName = s_request.getString("domainName");
        info._domainName = s_request.getString("_domainName");

        info.titleRepeat = s_request.getInt("titleRepeat");
        ErrInfo err = new ErrInfo();
        Permissions p = null;
        if (info.classId == 7 )
        {
            p = login.value.getModulePermissions(info.moduleId);//获取上线栏目权限
        }
        else
        {
            ColumnInfo parentColumn = ColumnClass.get(info.classId);
            info.rootId = parentColumn.rootId;
            p = login.value.getColumnPermissions(parentColumn);//获取上线栏目权限
        }
        if (!p.all)
        {
            err.errNo = -1;
            err.errMsg = "没有编辑该栏目的权限";
            context.Response.Write(err.ToJson());
            return;
        }
        err = ColumnClass.edit(info, login.value);
        Config.loadDomain();
        context.Response.Write(err.ToJson());
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}