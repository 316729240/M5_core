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
        ErrInfo err = new ErrInfo();
        //支付类型
        string payment_type = "1";
        //必填，不能修改
        //服务器异步通知页面路径
        string notify_url = context.Request.Url.ToString().Replace("frontend.ashx","")+"alipayNotifyEnd.ashx";
        //需http://格式的完整路径，不能加?id=123这类自定义参数

        //页面跳转同步通知页面路径
        string return_url = context.Request.Url.ToString().Replace("frontend.ashx","")+"alipayReturnEnd.ashx";
        //需http://格式的完整路径，不能加?id=123这类自定义参数，不能写成http://localhost/

        //商户订单号
        string out_trade_no = API.GetId();
        //商户网站订单系统中唯一订单号，必填

        //订单名称
        string subject ="会员充值";
        //必填

        //付款金额
        string total_fee = s_request.getString("total_fee");
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
System.Data.SqlClient.SqlDataReader rs = Helper.Sql.ExecuteReader("select A.money,A.userId,A.count*B.u_hour,B.u_brand+' '+B.u_sub_brand+' '+B.u_version,B.u_brand,B.u_sub_brand,B.u_version from alipay_orders A,carProducts B where A.pid=B.id and  A.id=@id and status=0", new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("id",out_trade_no)
                    });
        if (rs.Read())
        {
        Sql.ExecuteNonQuery("insert into alipay_orders (id,[money],messge,createDate,userId,status,u_brand,u_sub_brand,u_version)values(@id,@money,@messge,getdate(),@userId,0,@u_brand,@u_sub_brand,@u_version)",new SqlParameter[] {
            new SqlParameter("id",out_trade_no),
            new SqlParameter("money",s_request.getDouble("total_fee")),
            new SqlParameter("messge",""),
            new SqlParameter("userId",login.value.id),
            new SqlParameter("u_brand",rs[4]),
            new SqlParameter("u_sub_brand",rs[5]),
            new SqlParameter("u_version",rs[6]+"aa"),
        });

}
rs.Close();
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