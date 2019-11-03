<%@ WebHandler Language="C#" Class="frontEnd"%>
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
using System.Data.SqlClient;
public class frontEnd : IHttpHandler, System.Web.SessionState.IRequiresSessionState {

    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        if (context.Request["_m"] == null) context.Response.End();
        string m = context.Request["_m"].ToString();
        if (m == "clickTag") {
            ErrInfo info = new ErrInfo();
            string keyword=s_request.getString("tagName");
            Sql.ExecuteNonQuery("update IndexTable set c=c+1 where keyword=@keyword",new SqlParameter[] {
                        new SqlParameter("keyword",keyword)
                        });

            context.Response.Write(info.ToJson());
        }
else if (m == "buyP")
        {
            ErrInfo info = new ErrInfo();
if(s_request.getString("xieyi")==""){
info.errNo=-1;
info.errMsg="请先阅读购买协议，同意后打上勾再进行购买";
context.Response.Write(info.ToJson());
return;
}
            LoginInfo login = new LoginInfo();
            login.checkLogin();
            string a1= s_request.getString("a1");
            string a2= s_request.getString("a2");
            string a3= s_request.getString("a3");
            string pid= s_request.getString("pid");
            int count= s_request.getInt("count");
SqlDataReader rs = Sql.ExecuteReader("select * from carProducts  where id=@id",new SqlParameter[] {
                    new SqlParameter("id",pid)
                });
if(rs.Read()){
a1=rs["u_brand"].ToString();
a2=rs["u_sub_brand"].ToString();
a3=rs["u_version"].ToString();
}
rs.Close();
object d=Sql.ExecuteScalar("select u_expirationDate from u_buy_p where u_brand=@a1 and u_sub_brand=@a2 and u_version=@a3 and u_userId=@userId",new SqlParameter[] {
                    new SqlParameter("a1",a1),
                    new SqlParameter("a2",a2),
                    new SqlParameter("a3",a3),
                    new SqlParameter("userId",login.value.id)
                });
DateTime u_expirationDate=System.DateTime.Now;
if(d!=null)u_expirationDate=(DateTime)d;
if(u_expirationDate.Year>=2115){
info.errNo=-1;
info.errMsg="该项目您已购买了终身使用，无需重复购买";
            context.Response.Write(info.ToJson());
return;
}
int u_hour=0;
int obj=0;
if(a2==""){
obj=(int)Sql.ExecuteScalar("select count(1) from u_buy_p where   u_brand=@a1 and u_userid=@userid and u_expirationDate>getdate()",new SqlParameter[] {
                    new SqlParameter("a1",a1),
                    new SqlParameter("a2",a2),
                    new SqlParameter("a3",a3),
                    new SqlParameter("userid",login.value.id)
                });
}
else if(a3==""){
 obj =(int)Sql.ExecuteScalar("select count(1) from u_buy_p where u_brand=@a1 and  u_sub_brand=@a2 and u_userid=@userid and u_expirationDate>getdate()",new SqlParameter[] {
                    new SqlParameter("a1",a1),
                    new SqlParameter("a2",a2),
                    new SqlParameter("a3",a3),
                    new SqlParameter("userid",login.value.id)
                });
}
/*
if(u_hour ==0){
u_expirationDate=DateTime.Parse("2115-1-1");
}else{
u_expirationDate=u_expirationDate.AddHours(u_hour *count);
}
if(d==null){
                Sql.ExecuteNonQuery("insert into [u_buy_p] (id,u_brand,u_sub_brand,u_version,u_createDate,u_userId,u_expirationDate)values(@id,@u_brand,@u_sub_brand,@u_version,getdate(),@u_userId,@u_expirationDate)",new SqlParameter[] {
                        new SqlParameter("id",API.GetId()),
                        new SqlParameter("u_brand",a1),
                        new SqlParameter("u_sub_brand",a2),
                        new SqlParameter("u_version",a3),
                        new SqlParameter("u_expirationDate",u_expirationDate),
                        new SqlParameter("u_userId",login.value.id)
                        });
}else{
                Sql.ExecuteNonQuery("update [u_buy_p] set  u_expirationDate=@u_expirationDate where u_brand=@a1 and u_sub_brand=@a2 and u_version=@a3 and u_userId=@u_userId",new SqlParameter[] {
                        new SqlParameter("a1",a1),
                        new SqlParameter("a2",a2),
                        new SqlParameter("a3",a3),
                        new SqlParameter("u_expirationDate",u_expirationDate),
                        new SqlParameter("u_userId",login.value.id)
                        });
}
*/
info.userData=new object []{pid,count,obj};

            context.Response.Write(info.ToJson());
}
        else if(m=="vincar"){
            ErrInfo info = new ErrInfo();

            string vin = s_request.getString("vin");
            info.userData= Sql.ExecuteArray("select B.id value,B.C_车型名称 text from T_VIN对应表 A inner join T_车型表 B on A.C_车辆型号=B.C_车辆型号 where c_vin=@vin",new SqlParameter[] {
                        new SqlParameter("vin",vin)
                        });
            context.Response.Write(info.ToJson());
        }else if (m == "buyCar") {
            ErrInfo info = new ErrInfo();
            LoginInfo login = new LoginInfo();
            login.checkLogin();
            ErrInfo err= UserClass.addCash(login.value.id, -10, "购买绑定新车型", "系统");
            if (err.errNo >-1) {
                Sql.ExecuteNonQuery("update u_account set bind_count=bind_count+1 where id=@userId",new SqlParameter[] {
                        new SqlParameter("userId",login.value.id)
                        });
            }
            else
            {
                info = err;
            }
            context.Response.Write(info.ToJson());
        }else if (m == "editBind")
        {
            ErrInfo info = new ErrInfo();
            LoginInfo login = new LoginInfo();
            login.checkLogin();
            double id = s_request.getDouble("dataId");
            string u_carNumber = s_request.getString("u_carNumber");
            string u_maintainDate = s_request.getString("u_maintainDate");
            int u_carId = s_request.getInt("u_carId");
            int u_carId2 = s_request.getInt("u_carId2");
            int sel11= s_request.getInt("sel11");
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
                Sql.ExecuteNonQuery("update u_bind_car set u_maintainDate=@u_maintainDate where id=@id",new SqlParameter[] {
                        new SqlParameter("id",id),
                        new SqlParameter("u_maintainDate",maintainDate)
                        });
            }
            else {
                int count=(int)Sql.ExecuteScalar("select count(1) from u_bind_car where u_carId=@u_carId and u_userId=@userId",new SqlParameter[] {
                    new SqlParameter("u_carId",u_carId),
                    new SqlParameter("userId",login.value.id)
                });

                if (count > 0)
                {
                    info.errNo = -1;
                    info.errMsg = "您已添加该车型";
                    context.Response.Write(info.ToJson());
                    return;
                }
                count =(int)Sql.ExecuteScalar("select count(1) from u_bind_car where u_userId=@userId",new SqlParameter[] {
                    new SqlParameter("userId",login.value.id)
                });
                //int bind_count=(int)Sql.ExecuteScalar("select bind_count from u_account where id=@userId",new SqlParameter[] {
                //    new SqlParameter("userId",login.value.id)
                //});
                int bind_count=1;
                try{
                    bind_count=int.Parse( MWMS.Config.userConfig["account"].Item("bindCount"));
                }catch{
                }

                int bindCash=0;
                try{
                    bindCash=int.Parse( MWMS.Config.userConfig["account"].Item("bindCash"));
                }catch{
                }
                if (count >= bind_count)
                {
                    object _t=Sql.ExecuteScalar("select u_cash from carType where u_carId=@u_carId",new SqlParameter[] {
                    new SqlParameter("u_carId",u_carId)
                    });
                    if (_t != null) bindCash = int.Parse(_t.ToString());
                    info= UserClass.addCash(login.value.id, -bindCash, "购买绑定车型", "系统");
                    if (info.errNo < 0)
                    {
                        info.errMsg = "您的余额不足"+bindCash.ToString()+"，不能购买该车型";
                        context.Response.Write(info.ToJson());
                        return;
                    }
                    //info.errNo = -1;
                    //info.errMsg = "您最多可绑定"+bind_count.ToString()+"个车型";
                    //context.Response.Write(info.ToJson());
                    //return;
                }
                Sql.ExecuteNonQuery("insert into u_bind_car (id,u_carNumber,u_maintainDate,u_createdate,u_userId,u_carId,u_vin)values(@id,@u_carNumber,@u_maintainDate,getdate(),@u_userId,@u_carId,@u_vin)",new SqlParameter[] {
                        new SqlParameter("id",API.GetId()),
                        new SqlParameter("u_carNumber",u_carNumber),
                        new SqlParameter("u_maintainDate",maintainDate),
                        new SqlParameter("u_userId",login.value.id),
                        new SqlParameter("u_vin",u_vin),
                        new SqlParameter("u_carId",u_carId)
                        });
            }
            context.Response.Write(info.ToJson());
        }
        else if (m == "buy") {
            ErrInfo info = new ErrInfo();
            LoginInfo login = new LoginInfo();
            login.checkLogin();
            if(login.value.classId==0){
                info.errNo=-1000;
                info.errMsg="没有登录";
                context.Response.Write(info.ToJson());
                return;
            }
            double id = s_request.getDouble("id");
            /*
                        int count=(int)Sql.ExecuteScalar("select count(1) from u_buy where u_dataId=@id",new SqlParameter[] {
                                new SqlParameter("id",id)
                            });
                        if (count ==0) {
                            ErrInfo err= UserClass.addCash(login.value.id, -1, "购买文档,编号："+id.ToString(), "系统");
                            if (err.errNo >-1) {
                                Sql.ExecuteNonQuery("insert into u_buy (id,u_dataId,u_createdate,u_userId)values(@id,@u_dataId,getdate(),@u_userId)",new SqlParameter[] {
                                    new SqlParameter("id",API.GetId()),
                                    new SqlParameter("u_dataId",id),
                                    new SqlParameter("u_userId",login.value.id)
                                    });
                            }
                            else
                            {
                                info = err;
                            }
                        }
            */
            context.Response.Write(info.ToJson());
        }else  if (m == "delFavorite") {
            ErrInfo info = new ErrInfo();
            LoginInfo login = new LoginInfo();
            login.checkLogin();
            double id = s_request.getDouble("id");
            Sql.ExecuteNonQuery("delete from u_favorite where id=@id and u_userId=@userId",new SqlParameter[] {
                new SqlParameter("id",id),
                new SqlParameter("userId",login.value.id)
            });
            context.Response.Write(info.ToJson());
        }
        else if (m == "favorite"){
            ErrInfo info = new ErrInfo();
            LoginInfo login = new LoginInfo();
            login.checkLogin();
            if(login.value.classId==0){
                info.errNo=-1000;
                info.errMsg="没有登录";
                context.Response.Write(info.ToJson());
                return;
            }
            string url = s_request.getString("url");
double u_dataId=s_request.getDouble("id");
            string title = s_request.getString("title");
            int count=(int)Sql.ExecuteScalar("select count(1) from u_favorite where u_url=@u_url and u_userid=@u_userId",new SqlParameter[] {
                new SqlParameter("u_url",url),
                new SqlParameter("u_userId",login.value.id)
            });
            if (count == 0) {
                Sql.ExecuteNonQuery("insert into u_favorite (id,u_url,u_title,u_createdate,u_userId,u_dataId)values(@id,@u_url,@u_title,getdate(),@u_userId,@u_dataId)",new SqlParameter[] {
                new SqlParameter("id",API.GetId()),
                new SqlParameter("u_url",url),
                new SqlParameter("u_title",title),
                new SqlParameter("u_dataId",u_dataId),
                new SqlParameter("u_userId",login.value.id)
            });
            }else
            {
                info.errNo = -1;
                info.errMsg = "重复收藏";
            }
            context.Response.Write(info.ToJson());
        }
else if (m == "pinpai2") {

            LoginInfo login = new LoginInfo();

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
                new SqlParameter("a6",a6),
                new SqlParameter("userid",login.value.id)
            };
/*
            if(t==0)info.userData= Sql.ExecuteArray("select distinct c_品牌 text from [T_车型表] where [c_品牌] in (select u_brand from carProducts A,maintable B where A.id=B.id and B.orderid>-1)");
            if(t==1)info.userData= Sql.ExecuteArray("select distinct [c_车系] text from [T_车型表] where [c_品牌]=@a1 and [c_车系] in (select u_sub_brand from carProducts A,maintable B where A.id=B.id   and B.orderid>-1 and not u_brand in (select u_brand from u_buy_p where u_expirationDate>getdate() and u_userid=@userid))",op);
            if(t==2)info.userData= Sql.ExecuteArray("select distinct [C_车型名称] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2  and [C_车型名称] in (select u_version from carProducts A,maintable B where A.id=B.id  and B.orderid>-1 and not u_sub_brand in (select u_sub_brand  from u_buy_p where  u_expirationDate>getdate() and u_userid=@userid) )",op);
*/
            if(t==0)info.userData= Sql.ExecuteArray("select distinct c_品牌 text from [T_车型表] where [c_品牌] in (select u_brand from carProducts A,maintable B where A.id=B.id and B.orderid>-1 and not u_brand in (select u_brand from u_buy_p where u_expirationDate>getdate() and u_userid=@userid))",op);
            if(t==1)info.userData= Sql.ExecuteArray("select distinct [c_车系] text from [T_车型表] where [c_品牌]=@a1 and [c_车系] in (select u_sub_brand from carProducts A,maintable B where A.id=B.id   and B.orderid>-1 and not u_brand in (select u_brand from u_buy_p where u_expirationDate>getdate() and u_userid=@userid)  and not u_sub_brand in (select u_sub_brand from u_buy_p where u_expirationDate>getdate() and u_userid=@userid))",op);
            if(t==2)info.userData= Sql.ExecuteArray("select distinct [C_车型名称] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2  and [C_车型名称] in (select u_version from carProducts A,maintable B where A.id=B.id  and B.orderid>-1 and not u_sub_brand in (select u_sub_brand  from u_buy_p where  u_expirationDate>getdate() and u_userid=@userid)    and not u_version in (select u_version from u_buy_p where u_expirationDate>getdate() and u_userid=@userid))",op);

            context.Response.Write(info.ToJson());
        }

else if (m == "pinpai3") {
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
            if(t==0)info.userData= Sql.ExecuteArray("select u_cash,u_hour,u_type,A.id  from carProducts A,maintable B where A.id=B.id and B.orderid>-1 and u_brand=@a1 and u_sub_brand=''  and u_version='' order by A.u_type desc,A.u_hour desc",op);
            if(t==1)info.userData= Sql.ExecuteArray("select u_cash,u_hour,u_type,A.id  from carProducts A,maintable B where A.id=B.id and B.orderid>-1  and   u_brand=@a1 and u_sub_brand='' and u_version='' order by A.u_type desc,A.u_hour desc",op);
            if(t==2)info.userData= Sql.ExecuteArray("select u_cash,u_hour,u_type,A.id  from carProducts A,maintable B where A.id=B.id and B.orderid>-1  and  u_brand=@a1 and u_sub_brand=@a2 and u_version='' order by A.u_type desc,A.u_hour desc",op);
            if(t==3)info.userData= Sql.ExecuteArray("select u_cash,u_hour,u_type,A.id  from carProducts A,maintable B where A.id=B.id and B.orderid>-1  and  u_version=@a3 order by A.u_type desc,A.u_hour desc",op);
            context.Response.Write(info.ToJson());
        }
        else if (m == "pinpai") {
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
            if(t==0)info.userData= Sql.ExecuteArray("select distinct c_品牌 text from [T_车型表]");
            if(t==1)info.userData= Sql.ExecuteArray("select distinct [c_车系] text from [T_车型表] where [c_品牌]=@a1",op);
            if(t==2)info.userData= Sql.ExecuteArray("select distinct [c_排量(L)] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2",op);
            if(t==3)info.userData= Sql.ExecuteArray("select distinct [c_变速箱] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2 and [c_排量(L)]=@a3",op);
            if(t==4)info.userData= Sql.ExecuteArray("select distinct [c_年款] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2 and [c_排量(L)]=@a3 and c_变速箱=@a4",op);
            if(t==5)info.userData= Sql.ExecuteArray("select distinct [c_版本] text from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2 and [c_排量(L)]=@a3 and c_变速箱=@a4 and c_年款=@a5",op);
            if(t==6)info.userData= Sql.ExecuteArray("select distinct [c_车辆型号] text,id value from [T_车型表] where [c_品牌]=@a1 and [c_车系]=@a2 and [c_排量(L)]=@a3 and c_变速箱=@a4 and c_年款=@a5 and c_版本=@a6",op);
            context.Response.Write(info.ToJson());
        }
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}