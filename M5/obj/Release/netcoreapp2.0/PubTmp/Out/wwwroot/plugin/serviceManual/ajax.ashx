<%@ WebHandler Language="C#" Class="ajax" %>

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
        if (m == "dataList") { dataList(context); }
        else if (m == "delBuyCar")
        {
            string ids = s_request.getString("ids");
            ErrInfo info = new ErrInfo();
            Helper.Sql.ExecuteNonQuery("delete from alipay_orders where id in (" + ids + ")");
            context.Response.Write(info.ToJson());

        }
        else if (m == "buyList")
        {
            ErrInfo info = new ErrInfo();
            double userId = s_request.getDouble("userId");
            info.userData = Helper.Sql.ExecuteArray("select id,u_brand,u_sub_brand,u_version,createdate,u_hour,u_expirationDate,money,(case when u_expirationDate<getdate() then '已过期' else '未过期' end ) beizhu from alipay_orders  A  where A.userId=@userId and A.status=1 order by A.createdate desc", new SqlParameter[] {
                new SqlParameter("userId",userId)
            });
            context.Response.Write(info.ToJson());
        }
        else if (m == "buyP")
        {
            ErrInfo info = new ErrInfo();
            double userId = s_request.getDouble("userId");
            string a1= s_request.getString("a1");
            string a2= s_request.getString("a2");
            string a3= s_request.getString("a3");
            string pid= s_request.getString("pid");
            int count= s_request.getInt("count");
            if (pid == "")
            {

                info.errNo = -1;
                info.errMsg = "没有选择产品";
                context.Response.Write(info.ToJson());
                return;
            }
            if (count < 1)
            {
                info.errNo = -1;
                info.errMsg = "至少添加一份";
                context.Response.Write(info.ToJson());
                return;
            }
            int u_hour =-1;
            int u_cash=0;
            int bindCash=0;
            int u_type=0;
            string u_brand="",u_sub_brand="",u_version="",u_hourstr="";

            DateTime u_expirationDate = DateTime.Now;
            SqlDataReader rs = Sql.ExecuteReader("select u_hour,u_cash,u_brand,u_sub_brand,u_version,u_type from carProducts where id=@pid ",new SqlParameter[] {
                    new SqlParameter("pid",pid)
                });
            if (rs.Read())
            {
                u_brand=rs[2].ToString();u_sub_brand=rs[3].ToString();u_version=rs[4].ToString();
                u_hour =int.Parse(rs[0].ToString());
                u_type=int.Parse(rs[5].ToString());
                u_cash=int.Parse(rs[1].ToString());
                switch(u_type){
                    case 0:
                        u_expirationDate=u_expirationDate.AddHours(u_hour*count);
                        u_hourstr=(u_hour*count).ToString()+"小时";
                        break;
                    case 1:
                        u_expirationDate=u_expirationDate.AddDays(u_hour*count);
                        u_hourstr=(u_hour*count).ToString()+"天";
                        break;
                    case 2:
                        u_expirationDate=u_expirationDate.AddMonths(u_hour*count);
                        u_hourstr=(u_hour*count).ToString()+"月";
                        break;
                    case 3:
                        u_expirationDate=u_expirationDate.AddYears(u_hour*count);
                        u_hourstr=(u_hour*count).ToString()+"年";
                        break;
                }
                if(u_hour==0)u_hourstr="终身";
                bindCash=u_cash;
                if(u_hour >0)bindCash=u_cash*count;
                else{count=1;}
            }
            rs.Close();
            if(u_hour <0)context.Response.End();


            Sql.ExecuteNonQuery("insert into alipay_orders (id,[money],messge,createDate,userId,status,pid,count,u_brand,u_sub_brand,u_version,u_expirationDate,u_hour)values(@id,@money,@messge,getdate(),@userId,1,@pid,@count,@u_brand,@u_sub_brand,@u_version,@u_expirationDate,@u_hour)",new SqlParameter[] {
            new SqlParameter("id",API.GetId()),
            new SqlParameter("money",bindCash),
            new SqlParameter("messge",""),
            new SqlParameter("userId",userId),
            new SqlParameter("pid",pid),
            new SqlParameter("count",count),
            new SqlParameter("u_brand",u_brand),
            new SqlParameter("u_sub_brand",u_sub_brand),
            new SqlParameter("u_version",u_version),
            new SqlParameter("u_expirationDate",u_expirationDate),
            new SqlParameter("u_hour",u_hourstr)
        });

            context.Response.Write(info.ToJson());
        }
        else if (m == "readP1")
        {
            double userId = s_request.getDouble("userId");
            int type = s_request.getInt("t");
            string a1 = s_request.getString("a1");
            string a2 = s_request.getString("a2");
            ErrInfo info = new ErrInfo();
            if (type == 0)
            {
                info.userData = Helper.Sql.ExecuteArray("select distinct c_品牌 text from [T_车型表] where [c_品牌] in (select u_brand from carProducts A,maintable B where A.id=B.id and B.orderid>-1 and not u_brand in (select u_brand from u_buy_p where u_expirationDate>getdate() and u_userid=@userId))",
                    new SqlParameter[] { new SqlParameter("userId", userId) });
            }
            else if (type == 1)
            {
                info.userData = Sql.ExecuteArray("select distinct [c_车系] text from [T_车型表] where [c_品牌]=@a1 and [c_车系] in (select u_sub_brand from carProducts A,maintable B where A.id=B.id   and B.orderid>-1 and not u_brand in (select u_brand from u_buy_p where u_expirationDate>getdate() and u_userid=@userid)  and not u_sub_brand in (select u_sub_brand from u_buy_p where u_expirationDate>getdate() and u_userid=@userid))", new SqlParameter[] {
                new SqlParameter("a1",a1),
                new SqlParameter("userid",userId)
                });
            }
            else if (type == 2)
            {
                info.userData = Sql.ExecuteArray("select distinct [C_车型名称] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2  and [C_车型名称] in (select u_version from carProducts A,maintable B where A.id=B.id  and B.orderid>-1 and not u_sub_brand in (select u_sub_brand  from u_buy_p where  u_expirationDate>getdate() and u_userid=@userid)    and not u_version in (select u_version from u_buy_p where u_expirationDate>getdate() and u_userid=@userid))", new SqlParameter[] {
                new SqlParameter("a1",a1),
                new SqlParameter("a2",a2),
                new SqlParameter("userid",userId)
                });
            }
            context.Response.Write(info.ToJson());
        }

        else if (m == "readP2")
        {
            ErrInfo info = new ErrInfo();
            string a1 = s_request.getString("a1");
            string a2 = s_request.getString("a2");
            string a3 = s_request.getString("a3");
            string a4 = s_request.getString("a4");
            string a5 = s_request.getString("a5");
            string a6 = s_request.getString("a6");
            int t = s_request.getInt("t");
            SqlParameter[] op = new SqlParameter[] {
                new SqlParameter("a1",a1),
                new SqlParameter("a2",a2),
                new SqlParameter("a3",a3),
                new SqlParameter("a4",a4),
                new SqlParameter("a5",a5),
                new SqlParameter("a6",a6)
            };
            if (t == 1) info.userData = Sql.ExecuteArray("select u_cash,u_hour,u_type,A.id  from carProducts A,maintable B where A.id=B.id and B.orderid>-1 and u_brand=@a1 and u_sub_brand='' and u_version='' order by A.u_type desc,A.u_hour desc", op);
            if (t == 2) info.userData = Sql.ExecuteArray("select u_cash,u_hour,u_type,A.id  from carProducts A,maintable B where A.id=B.id and B.orderid>-1 and u_brand=@a1 and u_sub_brand=@a2 and u_version='' order by A.u_type desc,A.u_hour desc", op);
            if (t == 3) info.userData = Sql.ExecuteArray("select u_cash,u_hour,u_type,A.id  from carProducts A,maintable B where A.id=B.id and B.orderid>-1 and u_version=@a3 order by A.u_type desc,A.u_hour desc", op);


            context.Response.Write(info.ToJson());
        }
        else if (m == "readBind")
        {

            double id = s_request.getDouble("id");
            ErrInfo info = new ErrInfo();
            info.userData = Helper.Sql.ExecuteDictionary("select * from u_bind_car where id=@id", new SqlParameter[] { new SqlParameter("id", id) });

            context.Response.Write(info.ToJson());
        }
        else if (m == "editBind")
        {
            ErrInfo info = new ErrInfo();
            double id = s_request.getDouble("id");
            double userId = s_request.getDouble("userId");
            string u_carNumber = s_request.getString("u_carNumber");
            string u_maintainDate = s_request.getString("u_maintainDate");
            int u_carId = s_request.getInt("u_carId");
            int u_carId2 = s_request.getInt("u_carId2");
            int sel11 = s_request.getInt("sel11");
            string u_vin = s_request.getString("u_vin");
            if (sel11 == 1) u_carId = u_carId2;
            DateTime maintainDate = System.DateTime.Now;
            try
            {
                maintainDate = DateTime.Parse(u_maintainDate);
            }
            catch { }
            if (id > 0)
            {
                Sql.ExecuteNonQuery("update u_bind_car set u_maintainDate=@u_maintainDate where id=@id", new SqlParameter[] {
                        new SqlParameter("id",id),
                        new SqlParameter("u_maintainDate",maintainDate)
                        });
            }
            else
            {
                int count = (int)Sql.ExecuteScalar("select count(1) from u_bind_car where u_carId=@u_carId and u_userId=@userId", new SqlParameter[] {
                    new SqlParameter("u_carId",u_carId),
                    new SqlParameter("userId",userId),
                });
                if (count > 0)
                {
                    info.errNo = -1;
                    info.errMsg = "您已添加该车型";
                    context.Response.Write(info.ToJson());
                    return;
                }
                count = (int)Sql.ExecuteScalar("select count(1) from u_bind_car where u_userId=@userId", new SqlParameter[] {
                    new SqlParameter("userId",login.value.id)
                });

                Sql.ExecuteNonQuery("insert into u_bind_car (id,u_carNumber,u_maintainDate,u_createdate,u_userId,u_carId,u_vin)values(@id,@u_carNumber,@u_maintainDate,getdate(),@u_userId,@u_carId,@u_vin)", new SqlParameter[] {
                        new SqlParameter("id",API.GetId()),
                        new SqlParameter("u_carNumber",u_carNumber),
                        new SqlParameter("u_maintainDate",maintainDate),
                        new SqlParameter("u_userId",userId),
                        new SqlParameter("u_vin",u_vin),
                        new SqlParameter("u_carId",u_carId)
                        });
            }
            context.Response.Write(info.ToJson());
        }
        else if (m == "delCar")
        {
            string ids = s_request.getString("ids");
            ErrInfo info = new ErrInfo();
            Helper.Sql.ExecuteNonQuery("delete from u_bind_car where id in (" + ids + ")");
            context.Response.Write(info.ToJson());

        }
        else if (m == "carlist")
        {
            double dataId = s_request.getDouble("dataId");
            ErrInfo info = new ErrInfo();
            ArrayList data = Helper.Sql.ExecuteArray("select A.id,B.C_车型名称 title,A.u_vin,A.u_maintainDate  from u_bind_car A inner join T_车型表 B on A.u_carid=B.id where A.u_userid=@dataId", new SqlParameter[] { new SqlParameter("dataId", dataId) });
            info.userData = data;
            context.Response.Write(info.ToJson());

        }
        else if (m == "pinpai")
        {
            ErrInfo info = new ErrInfo();
            string a1 = s_request.getString("a1");
            string a2 = s_request.getString("a2");
            string a3 = s_request.getString("a3");
            string a4 = s_request.getString("a4");
            string a5 = s_request.getString("a5");
            string a6 = s_request.getString("a6");
            int t = s_request.getInt("t");
            SqlParameter[] op = new SqlParameter[] {
                new SqlParameter("a1",a1),
                new SqlParameter("a2",a2),
                new SqlParameter("a3",a3),
                new SqlParameter("a4",a4),
                new SqlParameter("a5",a5),
                new SqlParameter("a6",a6)
            };
            if (t == 0) info.userData = Sql.ExecuteArray("select distinct c_品牌 text from [T_车型表]");
            if (t == 1) info.userData = Sql.ExecuteArray("select distinct [c_车系] text from [T_车型表] where [c_品牌]=@a1", op);
            if (t == 2) info.userData = Sql.ExecuteArray("select distinct [c_排量(L)] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2", op);
            if (t == 3) info.userData = Sql.ExecuteArray("select distinct [c_变速箱] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2 and [c_排量(L)]=@a3", op);
            if (t == 4) info.userData = Sql.ExecuteArray("select distinct [c_年款] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2 and [c_排量(L)]=@a3 and c_变速箱=@a4", op);
            if (t == 5) info.userData = Sql.ExecuteArray("select distinct [c_版本] text,[id] value from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2 and [c_排量(L)]=@a3 and c_变速箱=@a4 and c_年款=@a5", op);
            //if(t==6)info.userData= Sql.ExecuteArray("select distinct [c_版本] text,[c_车辆型号] value from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2 and [c_排量(L)]=@a3 and c_变速箱=@a4 and c_年款=@a5 and c_版本=@a6",op);
            context.Response.Write(info.ToJson());
        }
        else if (m == "read")
        {
            double id = s_request.getDouble("id");
            ErrInfo info = new ErrInfo();
            Dictionary<string, object> data = Helper.Sql.ExecuteDictionary("select A.title,A.classId,A.skinId,A.url,B.* from mainTable A inner join  serviceManual B on A.id=B.id where A.id=@id", new SqlParameter[] { new SqlParameter("id", id) });
            data["url"] = Config.webPath + data["url"].ToString() + "." + BaseConfig.extension;
            object[] list = new object[6];
            SqlDataReader rs = Sql.ExecuteReader("select [c_品牌],[c_车系],[c_排量(L)],[c_变速箱],[c_年款],[c_版本] from T_车型表 where id=@u_carId", new SqlParameter[] { new SqlParameter("u_carId", data["u_carId"].ToString()) });
            if (rs.Read())
            {
                list[0] = rs[0];
                list[1] = rs[1];
                list[2] = rs[2];
                list[3] = rs[3];
                list[4] = rs[4];
                list[5] = rs[5];
            }
            rs.Close();
            data["carInfo"] = list;
            info.userData = data;
            context.Response.Write(info.ToJson());
        }
        else if (m == "edit")
        {
            ErrInfo info = new ErrInfo();
            RecordClass value = new RecordClass(login.value);
            string keyword = s_request.getString("u_keyword");
            string u_defaultPic = s_request.getString("u_defaultPic");
            value.tableName = "serviceManual";
            double id = s_request.getDouble("id");
            double classId = s_request.getDouble("classId");
            Permissions p = login.value.getColumnPermissions(classId);
            if (!p.write)
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
                context.Response.Write(info.ToJson());
                return;
            }
            string u_carName = Sql.ExecuteScalar("select C_车型名称 from T_车型表 where id=@u_carId", new SqlParameter[] {
                    new SqlParameter("u_carId",s_request.getString("u_carId"))
                }).ToString();
            value.addField("classId", classId);
            value.addField("skinId", s_request.getDouble("skinId"));
            value.addField("title", s_request.getString("title"));
            value.addField("u_keyword", keyword);
            value.addField("u_carId", s_request.getString("u_carId"));
            value.addField("u_carName", u_carName);
            value.addField("u_info", s_request.getString("u_info"));
            value.addField("u_from", s_request.getString("u_from"));
            value.addField("u_fromWeb", s_request.getString("u_fromWeb"));

            string u_content = s_request.getString("u_content");
            value.addField("u_content", u_content);
            SqlDataReader rs = Sql.ExecuteReader("select  C_车辆型号 from T_车型表 where id=@u_carId", new SqlParameter[] { new SqlParameter("u_carId", s_request.getString("u_carId")) });
            if (rs.Read())
            {
                value.addField("u_carNum", rs[0].ToString());
            }
            rs.Close();
            if (!p.delete && !p.audit) value.addField("orderId", -1);
            if (id > 0)
            {
                info = value.update(id);
                if (info.userData != null)
                {
                    Sql.ExecuteNonQuery("delete from indextable where dataId=@dataId", new SqlParameter[]{
                        new SqlParameter("dataId",info.userData)
                    });
                    RecordClass.addKeyword((double)info.userData, keyword);
                }
            }
            else
            {
                info = value.insert();
                if (info.userData != null) RecordClass.addKeyword((double)info.userData, keyword);
            }
            context.Response.Write(info.ToJson());

        }
    }
    void dataList(HttpContext context)
    {
        ErrInfo err = new ErrInfo();
        double moduleId = s_request.getDouble("moduleId");
        double classId = s_request.getDouble("classId");
        int pageNo =s_request.getInt("pageNo");
        string orderBy =s_request.getString("orderBy");
        int sortDirection = s_request.getInt("sortDirection");
        string type = s_request.getString("type");
        string searchField = s_request.getString("searchField");
        string keyword = s_request.getString("keyword").Replace("'","''");
        double dataTypeId = -1;
        SqlDataReader rs = null;
        Permissions p = null;
        if (moduleId == classId)
        {
            p = login.value.getModulePermissions(classId);
            rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new SqlParameter[] { new SqlParameter("moduleId", moduleId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
        }
        else
        {
            p = login.value.getColumnPermissions(classId);
            rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new SqlParameter[] { new SqlParameter("classId", classId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
        }
        if (!p.read)
        {
            err.errNo = -1;
            err.errMsg = "无权访问";
            context.Response.Write(err.ToJson());
            return;
        }
        TableInfo table = new TableInfo(dataTypeId);
        List<FieldInfo> fieldList = table.fields.FindAll(delegate(FieldInfo v)
        {
            return v.visible;
        });
        string where = "";// " and A.orderid>-3";
        if(type[0]=='0')where += " and A.orderid<0 ";
        if(type[1]=='0')where += " and A.orderid<>-1 ";
        if(type[2]=='0')where += " and A.orderid<>-2 ";
        if(type[3]=='0')where += " and A.orderid<>-3 ";
        //else if (type == 2) where = " and A.orderid=-3 ";
        object userId = null;
        if (keyword != "")
        {
            switch (searchField)
            {
                case "id":
                    where += " and A." + searchField + "=" + keyword + "";
                    break;
                case "title":
                    where += " and A." + searchField + " like '%" + keyword + "%'";
                    break;
                case "userId":
                    userId = Sql.ExecuteScalar("select id from m_admin where uname=@uname", new SqlParameter[]{
                        new SqlParameter("uname",keyword)
                    });
                    if (userId != null)
                    {
                        where += " and A." + searchField + "="+userId.ToString();
                    }
                    else
                    {
                        where += " and A.userId=-1 ";
                    }
                    break;
                case "auditorId":
                    userId = Sql.ExecuteScalar("select id from m_admin where uname=@uname", new SqlParameter[]{
                        new SqlParameter("uname",keyword)
                    });
                    if (userId != null)
                    {
                        where += " and A." + searchField + "="+userId.ToString();
                    }
                    else
                    {
                        where += " and A.userId=-1 ";
                    }
                    break;
                default:
                    where += " and ";
                    where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                    where += searchField + " like '%" + keyword + "%'";
                    break;
            }
        }
        if (!p.audit) where += " and A.userId="+login.value.id.ToString();
        ReturnPageData dataList = table.getDataList(moduleId,classId, pageNo, orderBy, sortDirection, where);
        object[] data = new object[] { fieldList, dataList };
        err.userData = data;
        context.Response.Write(err.ToJson());
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}