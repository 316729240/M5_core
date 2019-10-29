﻿<%@ WebHandler Language="C#" Class="ajax"%>
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
public class ajax :  IHttpHandler,System.Web.SessionState.IRequiresSessionState {

    LoginInfo login = new LoginInfo();
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        string m = context.Request.Form["_m"].ToString();
        if (m == "list") list(context);
        else if (m == "read") read(context);
        else if (m == "delData") delData(context);
        else if (m == "shenhezizhi") shenhezizhi(context);
    }
    void shenhezizhi(HttpContext context){
        ReturnValue info = new ReturnValue();
        double id = s_request.getDouble("id");
        int type = s_request.getInt("type");
        Sql.ExecuteNonQuery("update u_account set audit=@audit where id=@id",new SqlParameter[] {
            new SqlParameter("id",id),
            new SqlParameter("audit",type)
        });
        context.Response.Write(info.ToJson());
    }
    void delData(HttpContext context)
    {
        double dataTypeId = -1;
        string ids = context.Request.Form["ids"].ToString();
        double moduleId = s_request.getDouble("moduleId");
        double classId = s_request.getDouble("classId");
        int tag =int.Parse(context.Request.Form["tag"].ToString());
        Permissions p = null;
        if (classId < 8 || classId==moduleId)
        {
            SqlDataReader rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new SqlParameter[] { new SqlParameter("moduleId", moduleId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
            p = login.value.getModulePermissions(moduleId);

        }
        else
        {
            SqlDataReader rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new SqlParameter[] { new SqlParameter("classId", classId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
            p = login.value.getColumnPermissions(classId);
        }
        ReturnValue info = new ReturnValue();
        if (p.delete)
        {
            info = TableInfo.delData(dataTypeId, ids, true, login.value);
            Sql.ExecuteNonQuery("delete from u_account where id in ("+ids+")");
        }
        else{
            info.errNo = -1;
            info.errMsg = "权限不足";
        }
        context.Response.Write(info.ToJson());
    }
    void read(HttpContext context)
    {
        double id = s_request.getDouble("id");
        ReturnValue info = new ReturnValue();
        Dictionary<string,object> data=Helper.Sql.ExecuteDictionary("select A.uname,A.sex,A.name,email,phone,mobile,B.* from m_admin A inner join  u_account B on A.id=B.id where A.id=@id", new SqlParameter[] { new SqlParameter("id", id) });
        info.userData = data;
        context.Response.Write(info.ToJson());
    }
    void list(HttpContext context)
    {
        ReturnValue info = new ReturnValue();
        int[] width = null;
        double classId=s_request.getDouble("classId");
        string keyword = s_request.getString("keyword");
        string sql = "";
        string keywordWhere = "";
        if (keyword != "") keywordWhere = " and uname like '%'+@keyword+'%'";
        if (classId == 9896847028)
        {
            width = new int[] { 120,100,200,200,180,100}; 
            sql = "select id,uname 用户名,email 邮箱,mobile 手机,createDate 注册时间 from m_admin where classId=@classId "+keywordWhere+" order by updatedate desc";
        }
        else
        {
            width = new int[] { 120,100,200,200,120,180,100};
            sql = "select A.id,uname 用户名,B.companyName 公司名称,A.email 邮箱,A.mobile 手机,A.createDate 注册时间,B.audit 资质状态 from m_admin A inner join u_account B on A.id=b.id  where A.classId=@classId "+keywordWhere+" order by updatedate desc";
        }
        int pageNo = s_request.getInt("pageNo");
        List<FieldInfo> flist =new List<FieldInfo>();
        ReturnPageData r = new ReturnPageData();
        string orderBy = "";
        string[] temp = Regex.Split(sql, "order by", RegexOptions.IgnoreCase);
        if (temp.Length > 1)
        {
            sql = temp[0];
            orderBy = "order by " + temp[1];
        }
        string fieldList = sql.SubString("select", "from");
        r.recordCount = (int)(Sql.ExecuteScalar(sql.Replace(fieldList, " count(1) "),
            new SqlParameter[] {
            new SqlParameter("classId",classId),
            new SqlParameter("keyword",keyword)}));
        if (orderBy == "") orderBy = "order by (select 0)";
        sql = sql.Replace(fieldList, fieldList + ",row_number() OVER(" + orderBy + ") row_number ");
        ArrayList arrayList = new ArrayList();
        SqlDataReader rs = Sql.ExecuteReader("select * from ("+sql+") A where A.row_number> "+((pageNo-1)*r.pageSize).ToString()+" and A.row_number<"+(pageNo*r.pageSize+1).ToString(),new SqlParameter[] {
            new SqlParameter("classId",classId),
            new SqlParameter("keyword",keyword)
        });
        for (int i = 0; i < rs.FieldCount-1; i++)
        {
            FieldInfo f = new FieldInfo();
            f.name = rs.GetName(i);
            f.text = f.name;
            if(i==1)f.isTitle = true;
            f.visible = true;
            f.width = width[i];
            flist.Add(f);
        }
        while (rs.Read())
        {
            object[] dictionary = new object[rs.FieldCount];
            for (int i = 0; i < rs.FieldCount-1; i++)dictionary[i] = rs[i].ToString();
            arrayList.Add(dictionary);
        }
        rs.Close();
        r.pageNo = pageNo;
        r.data = arrayList;
        object[] data = new object[] { flist, r };
        info.userData = data;
        context.Response.Write(info.ToJson());
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}