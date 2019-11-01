using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace M5.Common
{


    /// <summary>
    ///LoginInfo 的摘要说明
    /// </summary>
    public class LoginInfo
    {
        public string sessionId = (M5.PageContext.Current.Request.Cookies ["M5_SessionId"] == null) ? "" : M5.PageContext.Current.Request.Cookies["M5_SessionId"];
        public UserInfo value = null;
        public LoginInfo()
        {
            this.Create();

        }
        public LoginInfo(string _sessionId)
        {
            this.Create(_sessionId);
        }
        void Create(string _sessionId="")
        {
            if(_sessionId!="") sessionId = _sessionId;
            if (sessionId == "")
            {
                string a1 = "";
                string a = M5.PageContext.Current.Request.Cookies["M5_SessionId"];
                M5.PageContext.Current.Request.Cookies.TryGetValue("M5_SessionId", out string value);
                sessionId = Tools.GetId();
                Microsoft.AspNetCore.Http.CookieOptions option = new Microsoft.AspNetCore.Http.CookieOptions();
                option.Expires = new DateTimeOffset(DateTime.Now.AddDays(1));
                M5.PageContext.Current.Response.Cookies.Append("M5_SessionId", sessionId, option);
            }
            /*
            MySqlDataReader rs2 = Sql.ExecuteReader("select A.userid,A.logindate from logininfo A where   A.sessionId=@sessionId and logindate>@loginDate)",
                new MySqlParameter[] {
                    new MySqlParameter("sessionId", sessionId),
                    new MySqlParameter("loginDate",DateTime.Now.AddHours(-1) )
             });*/
            MySqlDataReader rs2 = Sql.ExecuteReader("select A.userid,A.logindate from logininfo A where   A.sessionId=@sessionId and logindate>@loginDate",
                new MySqlParameter[] {
                    new MySqlParameter("sessionId", sessionId),
                    new MySqlParameter("loginDate",DateTime.Now.AddHours(-1) )
             });
            if (rs2.Read())
            {
                value = UserClass.get(rs2.GetDouble(0));
                DateTime loginDate = rs2.GetDateTime(1);
                if ((System.DateTime.Now - loginDate).TotalMinutes > 40)
                {//间隔40分钟更新一次
                    Sql.ExecuteNonQuery("update logininfo set logindate=@logindate where sessionId=@sessionId", new MySqlParameter[] {
                        new MySqlParameter("sessionId", sessionId),
                        new MySqlParameter("logindate", DateTime.Now)
                    });
                }
            }
            rs2.Close();
        }
        /// <summary>
        /// 是否是后台登陆
        /// </summary>
        /// <returns></returns>
        public bool isManagerLogin()
        {
            return value != null && value.classId == 0;
        }
        /// <summary>
        /// 是否登陆
        /// </summary>
        /// <returns></returns>
        public bool isLogin()
        {
            return (value != null);
        }
        public bool checkLogin()
        {
            return isLogin();
        }
        public bool checkManagerLogin()
        {
            return isManagerLogin();
        }
        public bool exit()
        {
            Sql.ExecuteNonQuery("delete from logininfo where sessionId=@sessionId", new MySqlParameter[] { new MySqlParameter("sessionId", sessionId) });
            string MyCo = M5.PageContext.Current.Request.Cookies["M5_Login"];
            if (MyCo != null)
            {
                //MyCo.Expires = DateTime.Now.AddDays(-1);
                M5.PageContext.Current.Response.Cookies.Append("M5_Login",MyCo);
            }
            MyCo = M5.PageContext.Current.Request.Cookies["u_name"];
            if (MyCo != null)
            {
                //MyCo.Expires = DateTime.Now.AddDays(-1);
                M5.PageContext.Current.Response.Cookies.Append("u_name", MyCo);
            }
            MyCo = M5.PageContext.Current.Request.Cookies["u_id"];
            if (MyCo != null)
            {
                //MyCo.Expires = DateTime.Now.AddDays(-1);
                M5.PageContext.Current.Response.Cookies.Append("u_id", MyCo);
            }
            return true;
        }
    }
}
