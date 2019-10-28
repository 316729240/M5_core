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
public class frontEnd : IHttpHandler, System.Web.SessionState.IRequiresSessionState {

    SafeReqeust s_request = new SafeReqeust(0, 0);

    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        if (context.Request["_m"] == null) context.Response.End();
        if (login.value == null)
        {
            ErrInfo info = new ErrInfo();
            info.errNo = -1;
            info.errMsg = "请先登陆";
            context.Response.Write(info.ToJson());
        }
        string m = context.Request["_m"].ToString();
        if (m == "alipay") alipay(context);

    }
    void alipay(HttpContext context)
    {
        //商户订单号
        string out_trade_no = API.GetId();
int count= s_request.getInt("count");
double pid= s_request.getDouble("pid");
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


        Sql.ExecuteNonQuery("insert into alipay_orders (id,[money],messge,createDate,userId,status,pid,count,u_brand,u_sub_brand,u_version,u_expirationDate,u_hour)values(@id,@money,@messge,getdate(),@userId,0,@pid,@count,@u_brand,@u_sub_brand,@u_version,@u_expirationDate,@u_hour)",new SqlParameter[] {
            new SqlParameter("id",out_trade_no),
            new SqlParameter("money",bindCash),
            new SqlParameter("messge",""),
            new SqlParameter("userId",login.value.id),
            new SqlParameter("pid",pid),
            new SqlParameter("count",count),
            new SqlParameter("u_brand",u_brand),
            new SqlParameter("u_sub_brand",u_sub_brand),
            new SqlParameter("u_version",u_version),
            new SqlParameter("u_expirationDate",u_expirationDate),
            new SqlParameter("u_hour",u_hourstr)
        });

//context.Response.Write(u_hour.ToString()+"<br>"+bindCash.ToString());
//context.Response.End();
        ErrInfo err = new ErrInfo();
        //支付类型
        string payment_type = "1";
        //必填，不能修改
        //服务器异步通知页面路径
        string notify_url = "http://"+context.Request.Url.Host+"/manage/app/onlinepayment/"+"alipayNotifyEnd.ashx";
        //需http://格式的完整路径，不能加?id=123这类自定义参数

        //页面跳转同步通知页面路径
        string return_url ="http://"+context.Request.Url.Host+"/manage/app/onlinepayment/"+"alipayReturnEnd.ashx";
        //需http://格式的完整路径，不能加?id=123这类自定义参数，不能写成http://localhost/

        //商户网站订单系统中唯一订单号，必填

        //订单名称
        string subject ="会员充值";
        //必填

        //付款金额
        string total_fee = bindCash.ToString();
        //必填

        //订单描述

        string body = "";
        //商品展示地址
        string show_url = "";
        //需以http://开头的完整路径，例如：http://www.商户网址.com/myorder.html

        //防钓鱼时间戳
        string anti_phishing_key = "";
        //若要使用请调用类文件submit中的query_timestamp函数

        //客户端的IP地址
        string exter_invoke_ip = "";
        //非局域网的外网IP地址，如：221.0.0.1


        ////////////////////////////////////////////////////////////////////////////////////////////////
        Com.Alipay.Config.Partner =Config.userConfig["onlinePayment"].Item("alipay_partner");
        Com.Alipay.Config.Seller_email =Config.userConfig["onlinePayment"].Item("alipay_email");
        Com.Alipay.Config.Key =Config.userConfig["onlinePayment"].Item("alipay_key");
        //context.Response.Write(Com.Alipay.Config.Partner+"<br>"+Com.Alipay.Config.Seller_email+"<br>"+Com.Alipay.Config.Key+"<br>");
        //context.Response.End();
        //把请求参数打包成数组
        SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
        sParaTemp.Add("partner", Com.Alipay.Config.Partner);
        sParaTemp.Add("seller_email", Com.Alipay.Config.Seller_email);
        sParaTemp.Add("_input_charset", Com.Alipay.Config.Input_charset.ToLower());
        sParaTemp.Add("service", "create_direct_pay_by_user");
        sParaTemp.Add("payment_type", payment_type);
        sParaTemp.Add("notify_url", notify_url.ToLower());
        sParaTemp.Add("return_url", return_url.ToLower());
        sParaTemp.Add("out_trade_no", out_trade_no);
        sParaTemp.Add("subject", subject);
        sParaTemp.Add("total_fee", total_fee);
        sParaTemp.Add("body", body);
        sParaTemp.Add("show_url", show_url);
        sParaTemp.Add("anti_phishing_key", anti_phishing_key);
        sParaTemp.Add("exter_invoke_ip", exter_invoke_ip);
        //建立请求
        string sHtmlText = Com.Alipay.Submit.BuildRequest(sParaTemp, "get", "确认");
        context.Response.Write(sHtmlText);
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}