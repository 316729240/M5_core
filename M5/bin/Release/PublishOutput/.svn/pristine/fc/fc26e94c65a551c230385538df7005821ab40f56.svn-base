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
    public void ProcessRequest(HttpContext context)
    {
        if (context.Request["_m"] == null) context.Response.End();
        string m = context.Request["_m"].ToString();
        if (m == "reg") reg(context);
        if (m == "editApply") editApply(context);
        else if (m == "passwordRecovery") passwordRecovery(context);
        else if (m == "setPword") setPword(context);
        else if (m == "editBase") editBase(context);
        else if (m == "editPassword") editPassword(context);
        else if (m == "editIcon") editIcon(context);
        else if (m == "editPic") editPic(context);
        else if (m == "getMobileCode") getMobileCode(context);
        else if (m == "checkName") checkName(context);
        else if (m == "connect_create") connect_create(context);
        else if (m == "connect_login") connect_login(context);
        else if (m == "login") login(context);
        else if (m == "activation_account") activation_account(context);
        else if (m == "exit")
        {
            LoginInfo l = new LoginInfo();
            l.exit();
            context.Response.Redirect("/");
        }

    }
    public static string getCity(string ip){
            
        string text=API.readUrl("http://int.dpool.sina.com.cn/iplookup/iplookup.php?format=txt&ip="+ip);
        string [] item=text.Split('\n');
        string cid = "";
        SqlDataReader rs = Sql.ExecuteReader("select top 1 cid from region where name like '"+item[4]+"%' or name like '"+item[5]+"%' order by cid desc");
        if (rs.Read())
        {
            cid = rs[0].ToStr();
        }
        rs.Close();
        return cid;
    }
    void editApply(HttpContext context)
    {
        ErrInfo err = new ErrInfo();
        LoginInfo login = new LoginInfo();
        if (login.value == null)
        {
            err.errNo = -1;
            err.errMsg = "请先登陆";
            context.Response.Write(err.ToJson());
            return;
        }
        RecordClass value = new RecordClass(22592528442,login.value);
        double id=s_request.getDouble("id");
        double classId = s_request.getDouble("u_bzqy");
        value.addField("classid",classId);
        TableInfo table = new TableInfo(22592528442);
        for(int i = 0; i < table.fields.Count; i++) {
            if(context.Request.Form[table.fields[i].name]!=null)value.addField(table.fields[i].name,s_request.getString(table.fields[i].name));
        }
        if (id > 0)err = value.update(id);
        else err = value.insert();
        context.Response.Write(err.ToJson());
    }
    void passwordRecovery(HttpContext context)
    {
        ErrInfo err = new ErrInfo();
        string verification = s_request.getString("verification");
        if (context.Session["CheckCode"] == null || context.Session["CheckCode"].ToString().ToLower() != verification.ToLower()) {
            err.errNo = -1;
            err.errMsg = "验证码不正确";
            context.Response.Write(err.ToJson());
            return;
        }
        string sId = s_request.getString("sid").ToUpper();
        string userName=s_request.getString("userName");
        string email=s_request.getString("email");
        string backUrl = "http://" +context.Request.Url.Authority+ "/setpword/$sId.html";
        err= UserClass.passwordRecovery(userName, email, backUrl);
        if (err.errNo > -1)
        {
            err.userData = "mail."+email.Substring(email.IndexOf("@")+1);
        }
        context.Response.Write(err.ToJson());
    }
    void setPword(HttpContext context)
    {
        string sId = s_request.getString("sid").ToUpper();
        string password=s_request.getString("password");
        context.Response.Write(UserClass.editPassword(sId, password).ToJson());
    }
    void activation_account(HttpContext context)
    {
        string sId = s_request.getString("sid").ToUpper();

        int count=(int)Sql.ExecuteScalar("select count(1) from m_admin where sId=@sId and status=-1",new SqlParameter[] {
                new SqlParameter("sId",sId)
            });
        if (count > 0)
        {
            Sql.ExecuteNonQuery("update   m_admin set status=1 where sId=@sId",new SqlParameter[] {
                new SqlParameter("sId",sId)
            });
            context.Response.Write("<script>alert('帐号激活成功，请登录');location.href='/';</script>");
            context.Response.End();
        }
        else
        {
            context.Response.Write("<script>alert('链接已失效');location.href='/';</script>");
            context.Response.End();
        }
    }
    void getMobileCode(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string mobile= s_request.getString("mobile");
        int count=(int)Sql.ExecuteScalar("select count(1) from m_admin where mobile=@mobile",new SqlParameter[] {
                new SqlParameter("mobile",mobile)
            });
        if (count > 0)
        {
            info.errNo = -1;
            info.errMsg = "该手机号已被绑定";
            context.Response.Write(info.ToJson());
            return;
        }
        if (context.Request.Cookies["getM"] != null)
        {
            info.errNo = -1;
            info.errMsg = "请不要频繁获取";
            context.Response.Write(info.ToJson());
            return;
        }
        Random rnd = new Random(System.DateTime.Now.Millisecond);
        string code = rnd.Next(999999).ToString("D6");
        context.Session["CheckCode"] =code ;
        HttpCookie getM = new HttpCookie("getM");
        getM.Expires = System.DateTime.Now.AddMinutes(1);
        getM.Value = "1";
        context.Response.Cookies.Add(getM);
        string username = "kadake";
        string password = "kdk123456";//.MD5();
        //string mobile = "13810991891";
        Random r = new Random();
        string content = "您正在注册中国机动车维修技术信息网用户，验证码是："+ code + "【北京卡达克】";
        System.Collections.Specialized.NameValueCollection value = new System.Collections.Specialized.NameValueCollection();
        value.Add("account", username);
        value.Add("password", password);
        value.Add("mobile", mobile);
        value.Add("content", content);
        value.Add("action", "send");
        value.Add("userid", "");
        value.Add("sendTime", "");
        value.Add("extno", "");
        string html =Helper.Http.postUrl("http://111.206.219.21/sms.aspx",value,System.Text.Encoding.UTF8);
        if (html.IndexOf("操作成功") == -1)
        {
            info.errNo = -1;
            info.errMsg = "发送短信失败，请联系管理员";
        }
        context.Response.Write(info.ToJson());
    }
    void editIcon(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        LoginInfo login = new LoginInfo();
        if (login.value == null)
        {
            info.errNo = -1;
            info.errMsg = "请先登陆";
            context.Response.Write(info.ToJson());
            return;
        }
        if (context.Request.Files.Count > 0)
        {
            int index=context.Request.Files[0].FileName.IndexOf(".");
            if (index == -1)
            {
                context.Response.Write("<script>alert(\"请选择一个文件\");history.go(-1);</script>");
                context.Response.End();
            }
            string kzm = context.Request.Files[0].FileName.Substring(index).ToLower();
            if (".jpg,.png,.gif".IndexOf(kzm)==-1)
            {
                info.errNo = -1;
                info.errMsg = "只能上传jpg,png,gif文件";

                context.Response.Write("<script>alert(\""+info.errMsg+"\");history.go(-1);</script>");
                context.Response.End();
                return;
            }
            string newdir = Config.uploadPath +"icon/";
            DirectoryInfo d = new DirectoryInfo(PageContext.Current.Server.MapPath("~"+newdir));
            if (!d.Exists) d.Create();
            string file =newdir+ login.value.id.ToString() + context.Request.Files[0].FileName.Substring(index);
            context.Request.Files[0].SaveAs(PageContext.Current.Server.MapPath("~"+file));
            info=UserClass.setIcon(file, login.value);
            if (info.errNo <0)
            {
                context.Response.Write("<script>alert(\""+info.errMsg+"\");history.go(-1);</script>");
                context.Response.End();
            }
            string url = context.Request.ServerVariables["http_referer"];
            context.Response.Redirect(url);
        }
    }void editPic(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        LoginInfo login = new LoginInfo();
        if (login.value == null)
        {
            info.errNo = -1;
            info.errMsg = "请先登陆";
            context.Response.Write(info.ToJson());
            return;
        }
        if (context.Request.Files.Count > 0)
        {
            int index=context.Request.Files[0].FileName.IndexOf(".");
            if (index == -1)
            {
                context.Response.Write("<script>alert(\"请选择一个文件\");history.go(-1);</script>");
                context.Response.End();
            }
            string kzm = context.Request.Files[0].FileName.Substring(index).ToLower();
            if (".jpg,.png,.gif".IndexOf(kzm)==-1)
            {
                info.errNo = -1;
                info.errMsg = "只能上传jpg,png,gif文件";

                context.Response.Write("<script>alert(\""+info.errMsg+"\");history.go(-1);</script>");
                context.Response.End();
                return;
            }
            string newdir = Config.uploadPath +"avatar/";
            DirectoryInfo d = new DirectoryInfo(PageContext.Current.Server.MapPath("~"+newdir));
            if (!d.Exists) d.Create();
            string file =newdir+ login.value.id.ToString() + context.Request.Files[0].FileName.Substring(index);
            context.Request.Files[0].SaveAs(PageContext.Current.Server.MapPath("~"+file));
            Sql.ExecuteNonQuery("update u_account set pic=@pic,audit=0 where id=@id",new SqlParameter[] { new SqlParameter("id",login.value.id),new SqlParameter("pic",file)});

            string url = context.Request.ServerVariables["http_referer"];
            context.Response.Redirect(url);
        }
    }
    void editPassword(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        LoginInfo login = new LoginInfo();
        if (login.value == null)
        {
            info.errNo = -1;
            info.errMsg = "请先登陆";
            context.Response.Write(info.ToJson());
            return;
        }
        string oldPassword = s_request.getString("oldPassword");
        string password = s_request.getString("password");
        info=login.value.editPassword(oldPassword,password);
        context.Response.Write(info.ToJson());
    }
    void editBase(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        LoginInfo login = new LoginInfo();
        if (login.value == null)
        {
            info.errNo = -1;
            info.errMsg = "请先登陆";
            context.Response.Write(info.ToJson());
            return;
        }
        login.value.name = s_request.getString("name");
        login.value.phone = s_request.getString("phone");
        login.value.mobile = s_request.getString("mobile");
        login.value.email= s_request.getString("email");
        login.value.sex = s_request.getInt("sex")==1;
        UserClass.edit(login.value);
        Sql.ExecuteNonQuery("update u_account set companyName=@companyName,birthday=@birthday,city=@city,area=@area,marriage=@marriage,education=@education,occupation=@occupation,character=@character,habit=@habit,like_domain=@like_domain,like_brand=@like_brand,vin=@vin,address=@address where id=@id", new SqlParameter[] {
            new SqlParameter("companyName",s_request.getString("companyName")),
            new SqlParameter("birthday",s_request.getString("birthday")),
            new SqlParameter("city",s_request.getString("city")),
            new SqlParameter("area",s_request.getString("area")),
            new SqlParameter("marriage",s_request.getString("marriage")),
            new SqlParameter("education",s_request.getString("education")),
            new SqlParameter("occupation",s_request.getString("occupation")),
            new SqlParameter("character",s_request.getString("character")),
            new SqlParameter("habit",s_request.getString("habit")),
            new SqlParameter("like_domain",s_request.getString("like_domain")),
            new SqlParameter("like_brand",s_request.getString("like_brand")),
            new SqlParameter("vin",s_request.getString("vin")),
            new SqlParameter("address",s_request.getString("address")),
            new SqlParameter("id",login.value.id),
        });

        context.Response.Write(info.ToJson());
    }
    void login(HttpContext context)
    {

        ErrInfo info = new ErrInfo();
        string username = s_request.getString("username");
        string password = s_request.getString("password");
        string remember= s_request.getString("remember");
        string type= s_request.getString("type");
        object classId=Sql.ExecuteScalar("select classId from m_admin where uname=@uname",new SqlParameter[] {
                new SqlParameter("uname",username )
            });
        if(classId!=null){
            if(classId.ToString()=="9896847028" && type=="1"){
                info.errNo=-1;
                info.errMsg="请选择个人用户登录";
                context.Response.Write(info.ToJson());
                return;
            }
            if(classId.ToString()=="9896848409" && type=="0"){
                info.errNo=-1;
                info.errMsg="请选择企业用户登录";
                context.Response.Write(info.ToJson());
                return;
            }
        }
        if(remember=="1"){
            info= UserClass.login(username, password,0);
        }else{
            info= UserClass.login(username, password);
        }
        PageContext.Current.Response.Cookies["up1"].Value="";
        PageContext.Current.Response.Cookies["up2"].Value="";
        if(info.errNo>-1 && remember=="1"){
            PageContext.Current.Response.Cookies["up1"].Value =PageContext.Current.Server.UrlEncode(username);
            PageContext.Current.Response.Cookies["up1"].Expires = System.DateTime.Now.AddYears(1);
            PageContext.Current.Response.Cookies["up2"].Value =PageContext.Current.Server.UrlEncode(password);
            PageContext.Current.Response.Cookies["up2"].Expires = System.DateTime.Now.AddYears(1);
        }
        context.Response.Write(info.ToJson());
        return;
    }
    void connect_login(HttpContext context) {
        ErrInfo info = new ErrInfo();
        string qq_id=context.Session["qq_id"]+"",sina_id= context.Session["sina_id"]+"";
        if (qq_id == "" && sina_id=="") {
            info.errNo = -1;
            info.errMsg = "无有效的绑定帐号";
            context.Response.Write(info.ToJson());
            return;
        }
        string username = s_request.getString("username");
        string password = s_request.getString("password");
        info= UserClass.login(username, password);
        if (info.errNo > -1)
        {
            double id = ((UserInfo)info.userData).id;
            if(qq_id!="")info=bindQQ(id,qq_id);
            if (sina_id!="")info= bindSina(id,sina_id);
        }
        context.Response.Write(info.ToJson());
    }
    ErrInfo bindSina(double id, string sina_id)
    {
        ErrInfo info = new ErrInfo();
        int count=(int)Sql.ExecuteScalar("select count(1) from u_account where sina_id=@sina_id",new SqlParameter[] {
                new SqlParameter("sina_id",sina_id)
            });
        if (count > 0) {
            info.errNo = -1;
            info.errMsg = "与当前新浪微博已绑定";
            return info;
        }
        Sql.ExecuteNonQuery("update u_account set sina_id=@sina_id where id=@id",new SqlParameter[] {
                new SqlParameter("id",id),
                new SqlParameter("sina_id",sina_id)
            });
        return info;
    }
    ErrInfo bindQQ(double id,string qq_id)
    {
        ErrInfo info = new ErrInfo();
        int count=(int)Sql.ExecuteScalar("select count(1) from u_account where qq_id=@qq_id",new SqlParameter[] {
                new SqlParameter("qq_id",qq_id)
            });
        if (count > 0) {
            info.errNo = -1;
            info.errMsg = "与当前QQ已绑定";
            return info;
        }
        Sql.ExecuteNonQuery("update u_account set qq_id=@qq_id where id=@id",new SqlParameter[] {
                new SqlParameter("id",id),
                new SqlParameter("qq_id",qq_id)
            });
        return info;
    }
    void connect_create(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string username = s_request.getString("username");
        string email = s_request.getString("email");
        if (context.Session["qq_id"] == null) {
            info.errNo = -1;
            info.errMsg = "无有效的绑定帐号";
            context.Response.Write(info.ToJson());
            return;
        }
        string qq_id = context.Session["qq_id"].ToString();
        int count=(int)Sql.ExecuteScalar("select count(1) from u_account where qq_id=@qq_id",new SqlParameter[] {
                new SqlParameter("qq_id",qq_id)
            });
        if (count > 0) {
            info.errNo = -1;
            info.errMsg = "与当前QQ已绑定";
            context.Response.Write(info.ToJson());
            return;
        }
        string verification = s_request.getString("verification");
        UserInfo user = new UserInfo();
        user.classId = 9896847028;
        user.username =username;
        user.password =API.GetId();
        user.email =email;
        info=UserClass.add(user,null);
        if (info.errNo > -1)
        {
            Sql.ExecuteNonQuery("insert into u_account (id,qq_id)values(@id,@qq_id)",new SqlParameter[] {
                new SqlParameter("id",info.userData),
                new SqlParameter("qq_id",qq_id)
            });
        }
        context.Response.Write(info.ToJson());
    }
    void checkName(HttpContext context)
    {
        string username = s_request.getString("username");
        int count = (int)Sql.ExecuteScalar("select count(1) from m_admin where uname=@username", new SqlParameter[] {
            new SqlParameter("username",username)
        });
        context.Response.Write((count==0)?"true":"false");
        context.Response.End();
    }
    void reg(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string verification = s_request.getString("verification");
        if (context.Session["CheckCode"] == null || context.Session["CheckCode"].ToString().ToLower() != verification.ToLower()) {
            info.errNo = -1;
            info.errMsg = "验证码不正确";
            context.Response.Write(info.ToJson());
            return;
        }
        UserInfo user = new UserInfo();
        user.classId = 14063315977;
        user.username = s_request.getString("username");
        user.password = s_request.getString("password");
        string mobile = s_request.getString("mobile");



        //手机注册
        user.mobile = s_request.getString("mobile");
        info=UserClass.add(user,null);
        if (info.errNo >-1) {
            double u_quyuid= s_request.getDouble("u_quyuid");
            object quyu = Sql.ExecuteScalar("select classname from class where id=@id", new SqlParameter[] { new SqlParameter("id", u_quyuid) });

            Sql.ExecuteNonQuery("insert into u_account (id,u_bzb,u_danwei,u_quyu,u_jxdh,u_quyuid)values(@id,@u_bzb,@u_danwei,@u_quyu,@u_jxdh,@u_quyuid)", new SqlParameter[] {
                new SqlParameter("id",info.userData),
                new SqlParameter("u_bzb",s_request.getString("u_bzb")),
                new SqlParameter("u_danwei",s_request.getString("u_danwei")),
                new SqlParameter("u_quyu",quyu),
                new SqlParameter("u_jxdh",s_request.getString("u_jxdh")),
                new SqlParameter("u_quyuid",u_quyuid)
        }
           );
            /*
        if (isEmail)
        {
            string sId=info.userData.ToString().Encryption(Config.webId).MD5();
            string backUrl = "http://" + context.Request.Url.Authority  + context.Request.Url.AbsolutePath+"?_m=activation_account&sid="+sId;
            string html = System.IO.File.ReadAllText(PageContext.Current.Server.MapPath("mail.html"));
            TemplateEngine page = new TemplateEngine();
            page.TE_statistical = new TE_statistical();
            page.addVariable("sys", Config.systemVariables);
            Dictionary<string, object> _public = new Dictionary<string, object>();
            _public.Add("url", backUrl);
            _public.Add("dateTime", System.DateTime.Now);
            _public.Add("accountName", user.username);
            page.addVariable("public", _public);
            page.render(ref html);

            MWMS.API.sendMail(user.email,Config.systemVariables["webName"]+"-帐号激活", html);


        }*/
        }
        context.Response.Write(info.ToJson());

    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}