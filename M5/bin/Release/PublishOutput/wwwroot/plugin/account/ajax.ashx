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
        else if (m == "edit") edit(context);
    }
    void edit(HttpContext context){
        ErrInfo info = new ErrInfo();
        login.value.id = s_request.getDouble("id");
        login.value.classId = s_request.getDouble("classId");
        login.value.name = s_request.getString("name");
        login.value.phone = s_request.getString("phone");
        login.value.mobile = s_request.getString("mobile");
        login.value.email= s_request.getString("email");
        login.value.sex = s_request.getInt("sex")==1;
        login.value.icon = s_request.getString("icon");
        if (login.value.id > 0) {
            info=UserClass.edit(login.value);
            Sql.ExecuteNonQuery("update u_account set u_zhicheng=@u_zhicheng,u_xueli=@u_xueli,u_shanchang=@u_shanchang,u_price=@u_price where id=@id", new SqlParameter[] {
                new SqlParameter("u_zhicheng",s_request.getString("u_zhicheng")),
                new SqlParameter("id",s_request.getDouble("id")),
                new SqlParameter("u_xueli",s_request.getString("u_xueli")),
                new SqlParameter("u_shanchang",s_request.getString("u_shanchang")),
                new SqlParameter("u_price",s_request.getDouble("u_price")),
            });
            if(s_request.getString("password") != "")
            {
                UserClass.editPassword(login.value.id, s_request.getString("password"),login.value);
            }
        }else {
            login.value.username = s_request.getString("uname");
            login.value.password = s_request.getString("password");
            info=UserClass.add(login.value,login.value);
            Sql.ExecuteNonQuery("insert into u_account (id,u_zhicheng,u_shanchang,u_xueli,u_price)values(@id,@u_zhicheng,@u_shanchang,@u_xueli,@u_price)", new SqlParameter[] {
                new SqlParameter("u_zhicheng",s_request.getString("u_zhicheng")),
                new SqlParameter("u_shanchang",s_request.getString("u_shanchang")),
                new SqlParameter("u_xueli",s_request.getString("u_xueli")),
                new SqlParameter("u_price",s_request.getDouble("u_price")),
                new SqlParameter("id",(double)info.userData),
            });
        }
        if (info.errNo > -1  && login.value.icon!="")
        {
            UserInfo userinfo = UserClass.get((double)info.userData);

            login.value.icon=MWMS.DAL.Datatype.FieldType.Files.Parse(login.value.icon).ToJson();
            UserClass.setIcon(login.value.icon, userinfo);
        }
        context.Response.Write(info.ToJson());
    }
    void shenhezizhi(HttpContext context){
        ErrInfo info = new ErrInfo();
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
        ErrInfo info = new ErrInfo();
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
        ErrInfo info = new ErrInfo();
        Dictionary<string,object> data=Helper.Sql.ExecuteDictionary("select A.uname,A.sex,A.name,email,phone,mobile,icon,B.* from m_admin A inner join  u_account B on A.id=B.id where A.id=@id", new SqlParameter[] { new SqlParameter("id", id) });
        info.userData = data;
        context.Response.Write(info.ToJson());
    }
    void list(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        int[] width = null;
        double classId=s_request.getDouble("classId");
        string keyword = s_request.getString("keyword");
        string sql = "";
        string keywordWhere = "";
        if (keyword != "") keywordWhere = " and uname like '%'+@keyword+'%'";
        width = new int[] { 120,100,100,200,220,180,100};
        sql = "select A.id,uname 用户名,A.name 姓名,B.u_zhicheng 职称,B.u_price 咨询费用,A.createDate 注册时间 from m_admin A inner join u_account B on A.id=b.id  where A.classId=@classId "+keywordWhere+" order by updatedate desc";

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