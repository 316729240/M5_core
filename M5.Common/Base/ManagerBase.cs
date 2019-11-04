using Helper;
using M5.Base;
using M5.Common;
using Microsoft.AspNetCore.Mvc;
using MWMS;
using MWMS.DAL.Datatype;
using MWMS.DAL.Datatype.Table;
using MWMS.Helper.Extensions;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace M5.Main.Manager
{
    [Route("manage/app/[controller]/[action]")]
    public class ManagerBase : Controller
    {
        protected SafeReqeust s_request = new SafeReqeust(0, 0);
        TableHandle table = null;
        public LoginInfo loginInfo = null;
        void init()
        {

            string sessionId = PageContext.Current.Request.Cookies["M5_SessionId"];
            loginInfo = new LoginInfo(sessionId);
        }
        public  ManagerBase()
        {
            init();
        }
        public ManagerBase(double datatypeId)
        {
            table = new TableHandle(datatypeId);
            init();
        }
        public ManagerBase(string tableName)
        {
            table = new TableHandle(tableName);
            init();

        }
        public ReturnValue cardPermissions()
        {
            return new ReturnValue(loginInfo.value.isAdministrator);
        }

        /// <summary>
        /// 数据信息读取
        /// </summary>
        /// <returns></returns>
        public virtual ReturnValue read(double id)
        {
            ReturnValue returnValue = new ReturnValue();
            Dictionary<string, object> data = table.GetModel(id);
            data["url"] = TemplateEngine._replaceUrl(Config.webPath + data["url"].ToString() + "." + BaseConfig.extension);
            returnValue.userData = data;
            return returnValue;
        }
        /// <summary>
        /// 数据列表
        /// </summary>
        public virtual ReturnValue dataList(double moduleId = 0, double classId = 0, int pageNo = 0, string orderBy = "", int sortDirection = 0, string type = "", string searchField = "", string keyword = "")
        {
            LoginInfo login = new LoginInfo();
            ReturnValue returnValue = new ReturnValue();

            double dataTypeId = -1;
            MySqlDataReader rs = null;
            Permissions p = null;
            if (moduleId == classId)
            {
                p = login.value.getModulePermissions(classId);
                rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new MySqlParameter[] { new MySqlParameter("moduleId", moduleId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
            }
            else
            {
                p = login.value.getColumnPermissions(classId);
                rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new MySqlParameter[] { new MySqlParameter("classId", classId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
            }
            if (!p.read)
            {
                returnValue.errNo = -1;
                returnValue.errMsg = "无权访问";
                return returnValue;
            }
            TableInfo table = new TableInfo(dataTypeId);

            List<FieldInfo> fieldList = table.fields.FindAll(delegate (FieldInfo v)
            {
                return v.visible || v.isNecessary;
            });
            string where = "";// " and A.orderid>-3";
            if (type[0] == '0') where += " and A.orderid<0 ";
            if (type[1] == '0') where += " and A.orderid<>-1 ";
            if (type[2] == '0') where += " and A.orderid<>-2 ";
            if (type[3] == '0') where += " and A.orderid<>-3 ";
            //else if (type == 2) where = " and A.orderid=-3 ";
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
                        object userId = Sql.ExecuteScalar("select id from m_admin where uname=@uname", new MySqlParameter[]{
                        new MySqlParameter("uname",keyword)
                    });
                        if (userId != null)
                        {
                            where += " and A." + searchField + "=" + userId.ToString();
                        }
                        else
                        {
                            where += " and A.userId=-1 ";
                        }
                        break;
                    default:
                        where += " and ";
                        FieldInfo list = table.fields.Find(delegate (FieldInfo v)
                        {
                            if (v.name == searchField) return true;
                            else return false;
                        });
                        if (list.type == "String")
                        {
                            where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                            where += searchField + " like '%" + keyword + "%'";
                        }
                        else if (list.type == "DateTime")
                        {
                            string[] item = keyword.Split(',');
                            where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                            where += searchField + ">='" + item[0].ToString() + "' and " + searchField + "<='" + item[1].ToString() + "'";
                        }
                        else
                        {
                            string[] item = keyword.Split(',');
                            where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                            where += searchField + ">=" + item[0].ToString() + " and " + searchField + "<=" + item[1].ToString();
                        }
                        break;
                }
            }
            if (!p.audit) where += " and A.userId=" + login.value.id.ToString();
            ReturnPageData dataList = table.getDataList(moduleId, classId, pageNo, orderBy, sortDirection, where);
            object[] data = new object[] { fieldList, dataList };
            returnValue.userData = data;
            return returnValue;
        }

        /// <summary>
        /// 数据编辑
        /// </summary>
        /// <returns></returns>
        public virtual ReturnValue edit(double  classId)
        {
            ReturnValue returnValue = new ReturnValue();
            LoginInfo login = new LoginInfo();
            Permissions p = login.value.getColumnPermissions(classId);
            if (!p.write)
            {
                returnValue.errNo = -1;
                returnValue.errMsg = "没有权限";
                return returnValue;
            }
            Dictionary<string, object> model = new Dictionary<string, object>();
            foreach (var field in table.Fields)
            {
                if (PageContext.Current.Request.Form.ContainsKey(field.Key))
                {
                    model[field.Key] = s_request.getString(field.Key);
                }
            }
            Column column = new Column(classId);
            MWMS.DAL.Datatype.Table.ColumnConfig config = column.GetConfig();
            if (!config.titleRepeat)
            {
                int count = 0;
                MySqlDataReader rs = Sql.ExecuteReader("select count(1) from maintable where id<>@id and title=@title", new MySqlParameter[]{
                    new MySqlParameter("id",model["id"]),
                    new MySqlParameter("title",model["title"])
                });
                if (rs.Read()) count = rs.GetInt32(0);
                rs.Close();
                if (count > 0)
                {
                    returnValue.errNo = -1;
                    returnValue.errMsg = "标题已存在";
                    return returnValue;
                }
            }
            if (!(model.ContainsKey("id") && model["id"].ToDouble() > 0)) model["userId"] =  this.loginInfo.value.id;
            returnValue.userData = table.Save(model);
            return returnValue;
        }
     }
 
}
