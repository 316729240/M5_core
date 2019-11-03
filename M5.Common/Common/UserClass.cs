using Microsoft.AspNetCore.Http;
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
    ///UserClass 的摘要说明
    /// </summary>
    public class UserClass
    {
        public UserClass()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }
        public static ReturnValue addCash(double userId, int cash, string message, string Operator)
        {
            ReturnValue v = new ReturnValue();

            MySqlParameter[] p = new MySqlParameter[]{
            new MySqlParameter("amount",cash),
            new MySqlParameter("message",message),
            new MySqlParameter("userid",userId),
            new MySqlParameter("Operator",Operator)
        };
            if (cash > 0) Sql.ExecuteNonQuery("update m_admin set cash=cash+@cash where id=@userId", new MySqlParameter[] {
                new MySqlParameter("cash", cash),
                new MySqlParameter("userId", userId)
            });
            if (cash < 0)
            {
                int c = Sql.ExecuteNonQuery("update m_admin set cash=cash+@cash where id=@userId and cash>@cash", new MySqlParameter[] {
                new MySqlParameter("cash", -cash),
                new MySqlParameter("userId", userId)
                });
                if (c == 0)
                {

                    v.errNo = -1;
                    v.errMsg = "剩余网站币不足";
                    return (v);
                }
            }
            Sql.ExecuteNonQuery("insert into  cashChange (id,cash,message,userId,operator,createDate)values(@id,@cash,@message,@userId,@operator,getDate())", new MySqlParameter[] {
                new MySqlParameter("id", Tools.GetId()),
                new MySqlParameter("cash", cash),
                new MySqlParameter("message", message),
                new MySqlParameter("userId", userId),
                new MySqlParameter("operator", Operator)
                });
            v.errNo = 0;
            v.errMsg = "操作成功";
            return (v);
        }
        /// <summary>
        /// 密码找回
        /// </summary>
        /// <param name="email">用户名</param>
        /// <param name="email">邮箱</param>
        /// <returns></returns>
        public static ReturnValue passwordRecovery(string username, string email, string backUrl)
        {
            ReturnValue err = new ReturnValue();
            /*

            object id = Sql.ExecuteScalar("select id from m_admin where status=1 and uname=@username and email=@email", new MySqlParameter[] {
                new MySqlParameter("username",username),
                new MySqlParameter("email",email)
            });
            if (id == null)
            {
                err.errNo = -1;
                err.errMsg = "用户名或邮箱不正确";
                return err;
            }
            string pId = Tools.GetId();
            string sId = pId.Encryption(Config.webId).MD5();
            backUrl = backUrl.Replace("$sId", sId);
            string html = "<p>亲爱的用户：wangxu 您好</strong></p>" +
            "<p> 您于 " + System.DateTime.Now.ToString("yyyy年MM月dd日 hh: mm") + " 提交了密码重置请求：<br>请在24小时内点击下面链接设置密码</p>" +
            "<p><a href = \"" + backUrl + "\" target=\"_blank\">" + backUrl + "</a></p>" +
            "<p>(如果您无法点击此链接，请将它复制到浏览器地址栏后访问)</p>";
            err = Message.sendMail(email, Config.systemVariables["webName"] + "-密码找回", html);
            if (err.errNo < 0) return err;
            Sql.ExecuteScalar("insert into passwordRecovery (id,sId,userId,createDate)values(@id,@sId,@userId,@createDate)", new MySqlParameter[] {
                new MySqlParameter("id",pId),
                new MySqlParameter("sId",sId),
                new MySqlParameter("userId",id),
                new MySqlParameter("createDate",DateTime.Now),
                
            });
            */
            return err;
        }
        public static ReturnValue editPassword(string sId, string password)
        {
            ReturnValue v = new ReturnValue();

            object id = Sql.ExecuteScalar("select userId from passwordRecovery where sId=@sId and createDate>@createDate", new MySqlParameter[] {
                new MySqlParameter("sId",sId),
                new MySqlParameter("createDate",DateTime.Now.AddDays(-1)),
            });
            if (id == null)
            {
                v.errNo = -1;
                v.errMsg = "无效id或已过期";
                return v;
            }
            MySqlParameter[] p = new MySqlParameter[]{
                new MySqlParameter("pword",password.Encryption(Config.webId).MD5())
            };
            int c = Sql.ExecuteNonQuery("update  m_admin set pword=@pword where id=" + id.ToString(), p);
            if (c > 0)
            {
                v.errNo = 0; v.errMsg = "设置成功";
            }
            else
            {
                v.errNo = -1; v.errMsg = "设置失败";
            }
            Sql.ExecuteNonQuery("delete  from passwordRecovery where sId=@sId", new MySqlParameter[] {
                new MySqlParameter("sId",sId)
            });
            return (v);
        }
        public static ReturnValue login(double userId)
        {
            return login(userId, 1);
        }
        public static ReturnValue login(double userId, int hour)
        {
            string ip = Tools.IPToNumber("127.0.0.1").ToString();//  Tools.IPToNumber(M5.PageContext.Current.Request.UserHostAddress).ToString();
            ReturnValue err = new ReturnValue();
            UserInfo info = UserClass.get(userId);
            /*if (!info.ipAccess(M5.PageContext.Current.Request.UserHostAddress))
            {
                err.errNo = -1;
                err.errMsg = "您的ip不能登陆";
                return err;
            }*/
            err.errNo = 0;
            //添加登陆信息
            //添加登陆日志
            string sessionId = (M5.PageContext.Current.Request.Cookies["M5_SessionId"] == null) ? "" : M5.PageContext.Current.Request.Cookies["M5_SessionId"];
            if (sessionId == "")
            {
                sessionId = Tools.GetId();
                M5.PageContext.Current.Response.Cookies.Append("M5_SessionId", sessionId);
            }
            Sql.ExecuteNonQuery("update m_admin set loginDateTime=loginDateTime where id=@id", new MySqlParameter[]{
                        new MySqlParameter("id",userId),
                        new MySqlParameter("loginDateTime",DateTime.Now),
                    });
            Sql.ExecuteNonQuery("delete from logininfo where logindate<@logindate or sessionId=@sessionId", new MySqlParameter[]{
                        new MySqlParameter("sessionId",sessionId),
                        new MySqlParameter("logindate",DateTime.Now.AddHours(-1)),
                    });//删除超时用户及重登陆用户
            Sql.ExecuteNonQuery("insert logininfo (sessionId,ip,logindate,userid)values(@sessionId,@ip,@logindate,@userId)",
            new MySqlParameter[]{
                        new MySqlParameter("sessionId",sessionId),
                        new MySqlParameter("ip",ip),
                        new MySqlParameter("userId",userId),
                        new MySqlParameter("logindate",DateTime.Now),
                });
            err.userData = info;
            if (info.classId == 0) M5.PageContext.Current.Response.Cookies.Append("M5_Login","true");
            else { M5.PageContext.Current.Response.Cookies.Append("M5_Login", ""); }
            M5.PageContext.Current.Response.Cookies.Append("u_name", HttpUtility.UrlEncode(info.username));
            M5.PageContext.Current.Response.Cookies.Append("u_id", info.id.ToString());
            /*HttpCookie cook = new HttpCookie("u_name");
            if (hour > 0) cook.Expires = System.DateTime.Now.AddHours(hour);
            cook.Value = HttpUtility.UrlEncode(info.username);
            PageContext.Current.Response.Cookies.Add(cook);
            cook = new HttpCookie("u_id");
            if (hour > 0) cook.Expires = System.DateTime.Now.AddHours(hour);
            cook.Value = info.id.ToString();
            PageContext.Current.Response.Cookies.Add(cook);*/

            //PageContext.Current.Response.Cookies["u_name"].Value = HttpUtility.UrlEncode(info.username);
            //PageContext.Current.Response.Cookies["u_id"].Value = info.id.ToString();
            Tools.writeLog("login", info.username + "登陆成功");
            return err;
        }

        public static ReturnValue login(string uname, string pword)
        {
            return login(uname, pword, 1);
        }
        public static ReturnValue login(string uname, string pword, int hour)
        {
            string ip = "0";// PageContextc.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];// API.IPToNumber(M5.PageContext.Current.Request.UserHostAddress).ToString();
            ReturnValue err = new ReturnValue();
            string pword2 = "";
            double userId = 0;
            MySqlDataReader rs = Sql.ExecuteReader("select id,pword from m_admin where status=1 and uname=@uname", new MySqlParameter[] { new MySqlParameter("uname", uname) });
            if (rs.Read())
            {
                pword2 = rs[1].ToString();
                userId = rs.GetDouble(0);
            }
            rs.Close();
            #region 用户不存在
            if (userId == 0)
            {
                Tools.writeLog("login", uname + "用户名密码错误");
                err.errNo = -1;
                err.errMsg = "用户名密码错误";
                return err;
            }
            #endregion
            #region 出错次数过多ip封锁
            Sql.ExecuteNonQuery("delete from invalid_login where createdate<DATEADD(hh,-3,GETDATE()) ");//清空3天前的登陆错误日志
            rs = Sql.ExecuteReader("select ip from invalid_login where createdate>DATEADD(hh,-2,GETDATE()) and uname=@uname and ip=@ip and count>10", new MySqlParameter[]{
                new MySqlParameter("uname",uname),
                new MySqlParameter("ip",ip)
            });
            if (rs.Read()) { err.errNo = -1; err.errMsg = "由于您登录后台错误次数过多，系统自动屏蔽您的登录！"; }
            rs.Close();
            if (err.errNo < 0) return err;
            #endregion

            if (pword.Encryption(Config.webId).MD5() == pword2)
            {
                return login(userId, hour);
            }
            else
            {
                #region 密码错误
                MySqlParameter[] p = new MySqlParameter[] {
                            new MySqlParameter ( "ip", ip ),
                            new MySqlParameter ( "uname", uname ),
                            new MySqlParameter ( "createdate", DateTime.Now ),
                            
                };
                rs = Sql.ExecuteReader("select ip from invalid_login where uname=@uname and ip=@ip", p);
                if (rs.Read())
                {
                    Sql.ExecuteNonQuery("update invalid_login set count=count+1 where uname=@uname and ip=@ip", p);
                }
                else
                {
                    Sql.ExecuteNonQuery("insert into invalid_login (ip,uname,count,createdate)values(@ip,@uname,1,@createdate)", p);
                }
                rs.Close();
                Tools.writeLog("login", uname + "用户名密码错误");
                err.errNo = -1;
                err.errMsg = "用户名密码错误";
                #endregion
            }
            return err;
        }

        public static ReturnValue manageLogin(string uname, string pword, int hour)
        {
            string ip = "0";//API.IPToNumber(M5.PageContext.Current.Request.UserHostAddress).ToString();
            ReturnValue err = new ReturnValue();
            string pword2 = "";
            double userId = 0;
            MySqlDataReader rs = Sql.ExecuteReader("select id,pword from m_admin where status=1 and classId=0 and uname=@uname", new MySqlParameter[] { new MySqlParameter("uname", uname) });
            if (rs.Read())
            {
                pword2 = rs[1].ToString();
                userId = rs.GetDouble(0);
            }
            rs.Close();
            #region 用户不存在
            if (userId == 0)
            {
                Tools.writeLog("login", uname + "用户名密码错误");
                err.errNo = -1;
                err.errMsg = "用户名密码错误";
                return err;
            }
            #endregion
            #region 出错次数过多ip封锁
            Sql.ExecuteNonQuery("delete from invalid_login where createdate<@createdate ",new MySqlParameter[] {
                new MySqlParameter("createdate",DateTime.Now.AddHours(-3))
            });//清空3天前的登陆错误日志
            rs = Sql.ExecuteReader("select ip from invalid_login where createdate>@createdate and uname=@uname and ip=@ip and count>10", new MySqlParameter[]{
                new MySqlParameter("uname",uname),
                new MySqlParameter("ip",ip),
                new MySqlParameter("createdate",DateTime.Now.AddHours(-2))
            });
            if (rs.Read()) { err.errNo = -1; err.errMsg = "由于您登录后台错误次数过多，系统自动屏蔽您的登录！"; }
            rs.Close();
            if (err.errNo < 0) return err;
            #endregion

            if (pword.Encryption(Config.webId).MD5() == pword2)
            {
                return login(userId, hour);
            }
            else
            {
                #region 密码错误
                MySqlParameter[] p = new MySqlParameter[] {
                            new MySqlParameter ( "ip", ip ),
                            new MySqlParameter ( "uname", uname ),
                            new MySqlParameter("createdate",DateTime.Now)
                };
                rs = Sql.ExecuteReader("select ip from invalid_login where uname=@uname and ip=@ip", p);
                if (rs.Read())
                {
                    Sql.ExecuteNonQuery("update invalid_login set count=count+1 where uname=@uname and ip=@ip", p);
                }
                else
                {
                    Sql.ExecuteNonQuery("insert into invalid_login (ip,uname,count,createdate)values(@ip,@uname,1,@createdate)", p);
                }
                rs.Close();
                Tools.writeLog("login", uname + "用户名密码错误");
                err.errNo = -1;
                err.errMsg = "用户名密码错误";
                #endregion
            }
            return err;
        }
        public static UserInfo get(double id)
        {
            UserInfo value = new UserInfo();
            MySqlDataReader rs = Sql.ExecuteReader("select id,uname,status,createdate,updatedate,email,mobile,phone,icon,filteringIP,sex,classId,cash from m_admin where  id=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            if (rs.Read())
            {
                value.id = rs.GetDouble(0);
                value.username = rs[1].ToString();
                value.classId = rs.GetDouble(11);
                value.status = rs.GetInt32(2);
                value.createdate = rs.GetDateTime(3);
                value.updatedate = rs.GetDateTime(4);
                value.email = rs[5] + "";
                value.mobile = rs[6] + "";
                value.phone = rs[7] + "";
                value.icon = rs[8] + "";
                value.filteringIP = rs[9] + "";
                value.sex = rs.IsDBNull(10) ? true : rs.GetBoolean(10);
                value.cash = rs.IsDBNull(12) ? 0 : rs.GetInt32(12);

            }
            rs.Close();
            if (value.id < 1) return null;
            rs = Sql.ExecuteReader("select roleId from admin_role where  userId=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            string roleId = "";
            while (rs.Read())
            {
                if (roleId != "") roleId += ",";
                roleId += rs.GetDouble(0);
                value.role = roleId;
            }
            rs.Close();
            return value;
        }
        public static ReturnValue edit(UserInfo value, UserInfo loginInfo)
        {
            ReturnValue v = null;
            v = edit(value);
            return (v);
        }
        public static ReturnValue setIcon(string icon, UserInfo loginInfo)
        {

            ReturnValue v = new ReturnValue();
            Sql.ExecuteNonQuery("update  m_admin set icon=@icon where id=@id", new MySqlParameter[]{
            new MySqlParameter("icon",icon),
            new MySqlParameter("id",loginInfo.id)
            });
            return v;
        }
        public static ReturnValue edit(UserInfo value)
        {
            ReturnValue v = new ReturnValue();
            MySqlParameter[] p = new MySqlParameter[]{
            new MySqlParameter("id",value.id),
            new MySqlParameter("updateDate",System.DateTime.Now),
            new MySqlParameter("email",value.email),
            new MySqlParameter("phone",value.phone),
            new MySqlParameter("mobile",value.mobile),
            new MySqlParameter("filteringIP",value.filteringIP),
            new MySqlParameter("sex",value.sex),
            new MySqlParameter("name",value.name)
        };
            Sql.ExecuteNonQuery("update  m_admin set name=@name,sex=@sex,updateDate=@updateDate,email=@email,phone=@phone,mobile=@mobile,filteringIP=@filteringIP where id=@id", p);
            Sql.ExecuteNonQuery("delete from admin_role where userId=@id", new MySqlParameter[]{
                        new MySqlParameter("id",value.id)
                    });
            for (int i = 0; i < value.roleList.Count; i++)
            {
                Sql.ExecuteNonQuery("insert into  admin_role (id,userId,roleId)values(@id,@userId,@roleId) ", new MySqlParameter[]{
                        new MySqlParameter("id",Tools.GetId()),
                        new MySqlParameter("userId",value.id),
                        new MySqlParameter("roleId",value.roleList[i])
                    });

            }
            Tools.writeLog("user", "修改帐号 username:" + value.username);
            v.userData = value.id;
            v.errNo = 0;
            v.errMsg = "修改成功";
            return (v);
        }
        public static ReturnValue add(UserInfo value, UserInfo loginInfo)
        {
            if (value.classId == 0 && loginInfo != null && !loginInfo.isAdministrator)
            {
                //只有管理员才能建立后台帐号
                ReturnValue err = new ReturnValue();
                err.errMsg = "无权创建管理帐号";
                err.errNo = -1;
            }
            return add(value);
        }
        static ReturnValue add(UserInfo value)
        {
            ReturnValue v = new ReturnValue();
            if (int.Parse((Sql.ExecuteScalar("select count(1) from m_admin where uname=@uname", new MySqlParameter[]{
                    new MySqlParameter("uname",value.username)
            })).ToString()) > 0)
            {
                v.errNo = -1;
                v.errMsg = "用户名被占用请换一个用户名重试";
                return (v);
            }
            if (value.mobile != "" && int.Parse((Sql.ExecuteScalar("select count(1) from m_admin where mobile=@mobile", new MySqlParameter[]{
                    new MySqlParameter("mobile",value.mobile)
            })).ToString()) > 0)
            {
                v.errNo = -1;
                v.errMsg = "手机已被注册";
                return (v);
            }
            if (value.email != "" && (int)(Sql.ExecuteScalar("select count(1) from m_admin where email=@email", new MySqlParameter[]{
                    new MySqlParameter("email",value.email)
            })) > 0)
            {
                v.errNo = -1;
                v.errMsg = "邮箱已被注册";
                return (v);
            }
            //int subcount = (int)(Sql.ExecuteScalar("select count(1) from cms_admin where userid=" + loginInfo.id.ToString()));

            try
            {
                value.id = double.Parse(Tools.GetId());
                MySqlParameter[] p = new MySqlParameter[]{
                    new MySqlParameter("id",value.id),
                    new MySqlParameter("sId",value.id.ToString().Encryption(Config.webId).MD5()),
                    new MySqlParameter("uname",value.username),
                    new MySqlParameter("pword",value.password.Encryption(Config.webId).MD5()),
                    new MySqlParameter("createDate",System.DateTime.Now),
                    new MySqlParameter("updateDate",System.DateTime.Now),
                    new MySqlParameter("loginDateTime",System.DateTime.Now),
                    new MySqlParameter("status",value.status),
                    new MySqlParameter("integral",value.integral),
            new MySqlParameter("email",value.email),
            new MySqlParameter("phone",value.phone),
            new MySqlParameter("mobile",value.mobile),
            new MySqlParameter("classId",value.classId),
            new MySqlParameter("filteringIP",value.filteringIP),
            new MySqlParameter("sex",value.sex)
                };
                Sql.ExecuteNonQuery("insert into  m_admin (id,uname,pword,createDate,updateDate,loginDateTime,status,integral,email,phone,mobile,filteringIP,classId,sex,sId,cash)values(@id,@uname,@pword,@createDate,@updateDate,@loginDateTime,@status,@integral,@email,@phone,@mobile,@filteringIP,@classId,@sex,@sId,0) ", p);
                for (int i = 0; i < value.roleList.Count; i++)
                {
                    Sql.ExecuteNonQuery("insert into  admin_role (id,userId,roleId)values(@id,@userId,@roleId) ", new MySqlParameter[]{
                        new MySqlParameter("id",Tools.GetId()),
                        new MySqlParameter("userId",value.id),
                        new MySqlParameter("roleId",value.roleList[i])
                    });

                }

                if (value.id > 0)
                {
                    Tools.writeLog("user", "新增帐号 username:" + value.username);
                    v.errNo = 0;
                    v.errMsg = "添加成功";
                    v.userData = value.id;
                    return (v);
                }
                else
                {
                    v.errNo = -1;
                    v.errMsg = "添加失败";
                    return (v);
                }
            }
            catch (Exception ex)
            {

                v.errNo = -1;
                v.errMsg = ex.Message;
                return (v);
            }
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        public static ReturnValue del(string ids)
        {
            ReturnValue info = new ReturnValue();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            ReturnValue v = new ReturnValue();
            Sql.ExecuteNonQuery("delete  m_admin where id in (" + ids + ")");
            Sql.ExecuteNonQuery("delete  admin_role where userId  in (" + ids + ")");
            v.errNo = 0;
            return v;
        }
        /// <summary>
        /// 设置用户状态
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static ReturnValue setState(string ids, bool status)
        {
            ReturnValue info = new ReturnValue();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            ReturnValue v = new ReturnValue();
            Sql.ExecuteNonQuery("update m_admin set status=" + (status ? 1 : 0).ToString() + " where id in (" + ids + ")");
            v.errNo = 0;
            return v;
        }
        public static ReturnValue editPassword(double id, string oldPassword, string password, UserInfo loginInfo)
        {
            ReturnValue v = new ReturnValue();
            int c = (int)(Sql.ExecuteScalar("select count(1) from  m_admin where id=" + id + " and pword='" + oldPassword.Encryption(Config.webId).MD5() + "'"));
            if (c > 0)
            {
                v = editPassword(id, password, loginInfo);
            }
            else
            {
                v.errNo = -1; v.errMsg = "原密码不正确";
            }
            return (v);
        }
        public static ReturnValue editPassword(double id, string password, UserInfo loginInfo)
        {
            ReturnValue v = new ReturnValue();

            bool qx = false;
            if (loginInfo.id == id || loginInfo.isAdministrator)
            {
                qx = true;
            }
            if (!qx)
            {
                v.errNo = -1;
                v.errMsg = "无权修改";
                return (v);
            }
            MySqlParameter[] p = new MySqlParameter[]{
                new MySqlParameter("pword",password.Encryption(Config.webId).MD5())
            };
            int c = Sql.ExecuteNonQuery("update  m_admin set pword=@pword where id=" + id.ToString(), p);
            if (c > 0)
            {
                v.errNo = 0; v.errMsg = "设置成功";
            }
            else
            {
                v.errNo = -1; v.errMsg = "设置失败";
            }
            return (v);
        }
    }
    public class UserInfo
    {
        public double id = -1;
        public string username = "";
        public string password = "";
        public double classId = 0;
        public DateTime lastLoginTime;
        public string lastLoginIp = "";
        public string email = "";//邮箱
        public string phone = "";//电话
        public string mobile = "";//手机
        public int integral = 0;
        public DateTime createdate = System.DateTime.Now;//开户时间
        public DateTime updatedate = System.DateTime.Now;
        public string question = "";//密码问题
        public string answer = "";//密码答案
        public string icon = "img/default_icon.png";//头像
        public string name = "";//名称
        public bool sex = false;//性别
        public int status = 1;
        public bool isAccess = false;//浏览权限
        public bool isAdministrator = false;//管理权限
        public int cash = 0;
        string[] _ipList = null;
        string _filteringIP = "";
        public string filteringIP//ip访问过滤
        {
            get { return _filteringIP; }
            set { _filteringIP = value; if (_filteringIP != "") { _ipList = _filteringIP.Split('\n'); } else { _ipList = null; } }
        }
        /// <summary>
        /// 是否允许访问
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool ipAccess(string ip)
        {
            if (_ipList != null)
            {
                for (int i = 0; i < _ipList.Length; i++)
                {
                    if (_ipList[i] != "")
                    {
                        string reg = _ipList[i].Replace("*", @"\d{1,3}");
                        if (Regex.IsMatch(ip, reg)) return true;
                    }
                }
                return false;

            }
            return true;
        }

        string _role = "";
        public List<double> roleList = new List<double>();
        public string role
        {
            get
            {
                return _role;
            }
            set
            {
                _role = value;
                string[] list = _role.Split(',');
                roleList.Clear();
                for (int i = 0; i < list.Length; i++)
                {

                    if (list[i] != "")
                    {

                        double roleId = double.Parse(list[i]);
                        if (roleId > 0 && roleId < 6) isAccess = true;
                        roleList.Add(roleId);
                    }
                }
                isAdministrator = roleList.IndexOf(1) > -1;
            }
        }
        public Permissions getModulePermissions(double moduleId, double classId)
        {
            double dataTypeId = -1;
            Permissions p = null;
            if (classId < 8)
            {
                MySqlDataReader rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new MySqlParameter[] { new MySqlParameter("moduleId", moduleId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
                p = getModulePermissions(moduleId);

            }
            else
            {
                MySqlDataReader rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new MySqlParameter[] { new MySqlParameter("classId", classId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
                p = getColumnPermissions(classId);
            }
            return p;
        }

        /// <summary>
        /// 获取模块权限
        /// </summary>
        /// <param name="moduleId">模块Id</param>
        /// <returns></returns>
        public Permissions getModulePermissions(double moduleId)
        {
            Permissions value = new Permissions(this);
            string role = this.id.ToString();
            if (this.role != "") role += "," + this.role;
            MySqlDataReader rs = Sql.ExecuteReader("select count(1),sum(p0),sum(p1),sum(p2),sum(p3) from permissions where classid=" + moduleId.ToString() + " and dataId in (" + role + ") ");
            if (rs.Read())
            {
                if (rs.GetInt32(0) > 0)
                {
                    value.read = true;
                    value.write = value.write | (rs.GetInt32(1) > 0);
                    value.delete = value.delete | (rs.GetInt32(2) > 0);
                    value.audit = value.audit | (rs.GetInt32(3) > 0);
                    value.all = value.all | (rs.GetInt32(4) > 0);
                }
            }
            if (value.all) value.read = value.write = value.audit = true;
            return value;
        }
        /// <summary>
        /// 获取栏目权限
        /// </summary>
        /// <param name="moduleId">模块id</param>
        /// <param name="columnId">栏目id</param>
        /// <returns></returns>
        public Permissions getColumnPermissions(double columnId)
        {
            ColumnInfo column = ColumnClass.get(columnId);
            return getColumnPermissions(column);
        }
        public Permissions getColumnPermissions(ColumnInfo column)
        {
            Permissions value = new Permissions(this);
            string role = this.id.ToString();
            if (this.role != "") role += "," + this.role;
            string parentId = column.parentId;
            if (parentId != "") parentId += ",";
            parentId += column.moduleId.ToString();
            int count = 0;
            MySqlDataReader rs = Sql.ExecuteReader("select count(1),sum(p0),sum(p1),sum(p2),sum(p3) from permissions where classid in (" + parentId + ") and dataId in (" + role + ") ");
            if (rs.Read())
            {
                if (rs.GetInt32(0) > 0)
                {
                    value.read = true;
                    value.write = value.write | (rs.GetInt32(1) > 0);
                    value.delete = value.delete | (rs.GetInt32(2) > 0);
                    value.audit = value.audit | (rs.GetInt32(3) > 0);
                    value.all = value.all | (rs.GetInt32(4) > 0);
                }
            }
            rs.Close();
            if (value.all) value.read = value.write = value.audit = true;
            return value;
        }
        public ReturnValue editPassword(string oldPassword, string password)
        {
            return UserClass.editPassword(id, oldPassword, password, this);
        }
    }
}
