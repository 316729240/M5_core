using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Helper;
using M5;
using M5.Common;
using Microsoft.AspNetCore.Mvc;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace M5.Main.Manager
{
    [Route("manage/app/[controller]/[action]")]
    public class APIController : Controller
    {    public ReturnValue login(string uname,string pword)
        {

            ReturnValue err = new ReturnValue();
            try
            {
                err = UserClass.manageLogin(uname,pword,1);
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = "发生异常："+ex.Message;
            }
            return err;
        }
        public ReturnValue checkManagerLogin()
        {
            ReturnValue err = new ReturnValue();
            try {
                LoginInfo info = new LoginInfo(Request.Cookies["M5_SessionId"]);
                err.userData = info.value;
                err.errNo = (info.value != null && info.isManagerLogin()) ? 0 : -1;
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = "发生异常："+ex.Message;
            }
            return err;
        }
        public ReturnValue getDirName(string name)
        {
            
ReturnValue info = new ReturnValue();
string pinyin = name.GetPinYin();
            if (pinyin.Length > 15) pinyin = name.GetPinYin('2');
            pinyin = Regex.Replace(pinyin, "[ "+@"\-_"+"`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）——|{}【】‘；：”“'。，、？]", "");
            info.userData = pinyin;//.Replace(" ","").Trim();
    return info;

        }
        public ReturnValue readIcon()
        {
            ReturnValue info = new ReturnValue();
string path = Config.staticPath + "icon/";
System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Tools.MapPath("~" + path));
System.IO.FileInfo[] f = dir.GetFiles("*.jpg");
string[] file = new string[f.Length];
            for (int i = 0; i<f.Length; i++)
            {
                file[i] = path + f[i].Name;
            }
            info.userData = file;
    return info;
        }
        public ReturnValue getDataUrl(double id)
        {
ReturnValue info = new ReturnValue();
MySqlDataReader rs1 = Sql.ExecuteReader("select url from maintable where id=@id", new MySqlParameter[]{
                new MySqlParameter("id",id)
            });
            if (rs1.Read())
            {
                info.userData =TemplateEngine._replaceUrl(Config.webPath + rs1[0] + "." + BaseConfig.extension);
            }
            else
            {
                info.errNo = -1;
            }
            return info;
        }
    }
 
}
