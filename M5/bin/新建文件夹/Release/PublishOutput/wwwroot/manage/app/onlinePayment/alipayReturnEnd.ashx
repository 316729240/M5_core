<%@ WebHandler Language="C#" Class="alipayReturnEnd"%>
using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using Com.Alipay;
using MWMS;
using System.Data.SqlClient;
public class alipayReturnEnd : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{

    public void ProcessRequest(HttpContext context)
    {
        HttpRequest Request = context.Request;
        HttpResponse Response = context.Response;
        SortedDictionary<string, string> sPara = GetRequestGet(context);

        if (sPara.Count > 0)//判断是否有带返回参数
        {
            Notify aliNotify = new Notify();
            bool verifyResult = aliNotify.Verify(sPara, Request.QueryString["notify_id"], Request.QueryString["sign"]);
            string out_trade_no = Request.QueryString["out_trade_no"];
            API.writeLog("onlinePayment", "验证状态:" + verifyResult + "\t" + sPara.Count.ToString() + "\t" + Request.QueryString["notify_id"] + "\t" + Request.QueryString["sign"] + "\t" + out_trade_no);
//verifyResult=true;
            if (verifyResult)//验证成功
            {
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //请在这里加上商户的业务逻辑程序代码


                //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                //获取支付宝的通知返回参数，可参考技术文档中页面跳转同步通知参数列表

                //商户订单号


                //支付宝交易号

                string trade_no = Request.QueryString["trade_no"];

                //交易状态
                string trade_status = Request.QueryString["trade_status"];

                if (Request.QueryString["trade_status"] == "TRADE_FINISHED" || Request.QueryString["trade_status"] == "TRADE_SUCCESS")
                {
                    double total_fee = double.Parse(Request.QueryString["total_fee"]);
                chuli(total_fee, out_trade_no);
                    
                }
                else
                {
                    Response.Write("trade_status=" + Request.QueryString["trade_status"]);
                }

                //打印页面
                Response.Write("验证成功<br />");

                //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            }
            else//验证失败
            {
                Response.Write("验证失败");
            }
        }
        else
        {
            Response.Write("无返回参数");
        }

    }
    void chuli(double total_fee,string out_trade_no)
    {
        System.Data.SqlClient.SqlDataReader rs = Helper.Sql.ExecuteReader("select A.money,A.userId,A.count*B.u_hour,B.u_brand+' '+B.u_sub_brand+' '+B.u_version,B.u_brand,B.u_sub_brand,B.u_version,B.u_type from alipay_orders A,carProducts B where A.pid=B.id and  A.id=@id and status=0", new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("id",out_trade_no)
                    });
        if (rs.Read())
        {
            double cash = double.Parse(rs[0].ToString());
            if (total_fee != cash) return;
            double userId = double.Parse(rs[1].ToString());
            int hour = int.Parse(rs[2].ToString());
            string name = rs[3].ToString();
            DateTime u_expirationDate = DateTime.Now;
            int u_type= int.Parse(rs["u_type"].ToString());
            if (hour == 0)
            {
                u_expirationDate = DateTime.Parse("2116-1-1");
            }
            else { 
switch(u_type){
case 0:
u_expirationDate=u_expirationDate.AddHours(hour); 
break;
case 1:
u_expirationDate=u_expirationDate.AddDays(hour); 
break;
case 2:
u_expirationDate=u_expirationDate.AddMonths(hour); 
break;
case 3:
u_expirationDate=u_expirationDate.AddYears(hour); 
break;
}
}
            //ErrInfo err= UserClass.addCash(userId, -(int)cash, "购买"+name, "系统");
            //if (err.errNo > -1) { 
            Helper.Sql.ExecuteNonQuery("update alipay_orders set status=1 where id=@id", new System.Data.SqlClient.SqlParameter[] {
                                                new System.Data.SqlClient.SqlParameter("id",out_trade_no)
                                            });
            //}
            object d = Helper.Sql.ExecuteScalar("select u_expirationDate from u_buy_p where u_brand=@a1 and u_sub_brand=@a2 and u_version=@a3 and u_userId=@userId", new SqlParameter[] {
                            new SqlParameter("a1",rs[4]),
                            new SqlParameter("a2",rs[5]),
                            new SqlParameter("a3",rs[6]),
                            new SqlParameter("userId",userId )
                        });
            if (d == null)
            {
                Helper.Sql.ExecuteNonQuery("insert into [u_buy_p] (id,u_brand,u_sub_brand,u_version,u_createDate,u_userId,u_expirationDate)values(@id,@u_brand,@u_sub_brand,@u_version,getdate(),@userId,@u_expirationDate)", new SqlParameter[] {
                            new SqlParameter("id",API.GetId()),
                            new SqlParameter("u_brand",rs[4]),
                            new SqlParameter("u_sub_brand",rs[5]),
                            new SqlParameter("u_version",rs[6]),
                            new SqlParameter("u_expirationDate",u_expirationDate),
                            new SqlParameter("userId",userId )
                            });
            }
            else
            {
                Helper.Sql.ExecuteNonQuery("update [u_buy_p] set  u_expirationDate=@u_expirationDate where u_brand=@u_brand and u_sub_brand=@u_sub_brand and u_version=@u_version and u_userId=@userId", new SqlParameter[] {
                            new SqlParameter("u_brand",rs[4]),
                            new SqlParameter("u_sub_brand",rs[5]),
                            new SqlParameter("u_version",rs[6]),
                                new SqlParameter("u_expirationDate",u_expirationDate),
                                new SqlParameter("userId",userId )
                                });
            }
            HttpContext.Current.Response.Write("<script>alert('操作成功!');location.href='/';</script>");
        }
        else
        {
            HttpContext.Current.Response.Write("<script>alert('未找到该订单!');location.href='/';</script>");
        }
        rs.Close();
    }
    /// <summary>
    /// 获取支付宝GET过来通知消息，并以“参数名=参数值”的形式组成数组
    /// </summary>
    /// <returns>request回来的信息组成的数组</returns>
    public SortedDictionary<string, string> GetRequestGet(HttpContext context)
    {
        HttpRequest Request = context.Request;
        int i = 0;
        SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
        NameValueCollection coll;
        //Load Form variables into NameValueCollection variable.
        coll = Request.QueryString;

        // Get names of all forms into a string array.
        String[] requestItem = coll.AllKeys;

        for (i = 0; i < requestItem.Length; i++)
        {
            sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
        }

        return sArray;
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}