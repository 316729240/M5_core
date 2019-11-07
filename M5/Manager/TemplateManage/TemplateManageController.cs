using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Helper;
using M5.Common;
using Microsoft.AspNetCore.Mvc;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;
using MWMS.SqlHelper;
using MWMS.Template;
using MySql.Data.MySqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace M5.Main.Manager
{
    [LoginAuthorzation]
    public class TemplateManageController : ManagerBase
    {
        public ReturnValue readViewForm(string viewName)
        {
            string[] _viewName = viewName.SubString("view.", @"\(").Split('.');
            double classId = (double)Sql.ExecuteScalar("select id from class where classId=12 and className=@className", new MySqlParameter[] { new MySqlParameter("className", _viewName[0]) });

            Dictionary<string, object> list = Sql.ExecuteDictionary("select B.u_p_form from template_view B where B.classId=@classId and B.title=@viewName",
                new MySqlParameter[]{
                new MySqlParameter("classId",classId),
                new MySqlParameter("viewName",_viewName[1])
                    });
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(list["u_p_form"].ToString());
            Dictionary<string, object> viewVariables = (Dictionary<string, object>)doc.ToJson().ParseJson<Dictionary<string, object>>();
            return ReturnValue.Success(viewVariables);
        }
        //读取模板
        //参数1:模板ID
        public ReturnValue readView(double id,string viewName)
        {
            ReturnValue info = new ReturnValue();
            string[] _viewName = viewName.SubString("view.", @"\(").Split('.');
            if (id > 0)
            {
                info.userData = Sql.ExecuteDictionary("select B.title,B.u_html,B.u_editboxStatus,B.u_parameterValue,B.u_viewType,B.u_datatypeId,B.classId,B.id from template_view B where B.id=@id",
                    new MySqlParameter[]{
                new MySqlParameter("id",id)
                        });
            }
            else
            {
                if (viewName.Length > 1)
                {
                    double classId = (double)Sql.ExecuteScalar("select id from class where classId=12 and className=@className", new MySqlParameter[] { new MySqlParameter("className", _viewName[0]) });
                    info.userData = Sql.ExecuteDictionary("select B.id,B.title,B.u_html,B.u_editboxStatus,B.u_parameterValue,B.u_viewType,B.u_datatypeId,B.classId,B.u_p_form from template_view B where B.classId=@classId and B.title=@viewName",
                    new MySqlParameter[]{
                new MySqlParameter("classId",classId),
                new MySqlParameter("viewName",viewName[1])
                        });
                }
            }
            return info;
        }
        public ReturnValue getTemplateType()
        {
            ReturnValue info = new ReturnValue();
            string value = "1";
            System.IO.FileInfo f = new System.IO.FileInfo(Tools.MapPath("~" + Config.tempPath + @"user\" + loginInfo.value.id.ToString() + @"\templateType.config"));
            if (f.Exists) value = System.IO.File.ReadAllText(f.FullName);
            info.userData = value;
            return info;
        }
        public ReturnValue setTemplateType(string value)
        {
            ReturnValue info = new ReturnValue();
            System.IO.FileInfo f = new System.IO.FileInfo(Tools.MapPath("~" + Config.tempPath + @"user\" + loginInfo.value.id.ToString() + @"\templateType.config"));
            if (!f.Directory.Exists) f.Directory.Create();
            System.IO.File.WriteAllText(f.FullName, value);
            return info;
        }
        string _replaceUrl(Match m)
        {
            if (Regex.IsMatch(m.Value, "^default_", RegexOptions.IgnoreCase))
            {
                return "";
            }
            else
            {
                return Regex.Replace(m.Value, @"_((\d){1,5})", "");
            }
        }
        public ReturnValue locateTemplate(string url,int u_webFAid)
        {
            ReturnValue info = new ReturnValue();
            Uri _url = new Uri(url);
            //Regex r = new Regex(@"(?<=/)((.[^/]*)_((\d){1,5}))(." + BaseConfig.extension + ")", RegexOptions.IgnoreCase);
            //string newUrl = r.Replace(url.AbsolutePath.ToLower(), new MatchEvaluator(_replaceUrl));
            bool isMobilePage = false;
            string virtualWebDir = "";
            string newUrl = WebService.urlZhuanyi(new Uri(url), ref isMobilePage, ref virtualWebDir);
            TemplateInfo v = TemplateClass.get(newUrl, isMobilePage);

            //ColumnInfo column = null;
            double moduleId = 0, classId = 0;
            if (v == null)
            {
                info.errNo = -1;
                info.errMsg = "定位失败";
            }
            else
            {
                if (v.classId > 0)
                {
                    object value = Sql.ExecuteScalar("select id from module where id=@id", new MySqlParameter[] { new MySqlParameter("id", v.classId) });
                    if (value == null)
                    {
                        IDataReader rs = Sql.ExecuteReader("select id,moduleId from class where id=@id", new MySqlParameter[] { new MySqlParameter("id", v.classId) });
                        if (rs.Read())
                        {
                            classId = v.classId;
                            moduleId = rs.GetDouble(1);
                        }
                        rs.Close();
                    }
                    else
                    {
                        moduleId = (double)value;
                        classId = 1;
                    }
                }
                object[] obj = new object[] { moduleId, classId, v.u_type, v.id, isMobilePage ? 1 : 0 };
                info.userData = obj;
            }
            return info;
        }
        public ReturnValue replace(double id,int type,int mbType,int u_webFAid,string keyword,string keyword2)
        {

            ReturnValue info = new ReturnValue();
            if (!loginInfo.value.isAdministrator)
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
                return info;
            }
            if (keyword == "")
            {
                info.errMsg = "查询内容不能为空";
                info.errNo = -1;
                return info;
            }
            string sql = "select id,classid,title,u_content,u_datatypeid,u_type,0 from template where  id=@id ", sql2 = "update template set u_content=@content where id=@id";
            if (mbType == 1)
            {
                sql = "select B.id,B.classid,A.className+'.'+B.title,B.u_html,B.u_datatypeid,0,1,B.title from template_view B inner join class A on B.classId=A.id where B.id=@id ";
                sql2 = "update template_view set u_html=@content where id=@id";
            }
            MySqlDataReader rs = Sql.ExecuteReader(sql, new MySqlParameter[]{
        new MySqlParameter("webFAid",u_webFAid),
        new MySqlParameter("id",id)
    });
            MatchCollection mc;
            Regex r = null;
            if (rs.Read())
            {
                string content = rs.GetString(3);
                bool flag = false;
                if (type == 0)
                {
                    flag = content.IndexOf(keyword) > -1;
                    if (flag) content = content.Replace(keyword, keyword2);
                }
                else
                {
                    r = new Regex(@keyword, RegexOptions.IgnoreCase); //定义一个Regex对象实例
                    mc = r.Matches(content);
                    flag = mc.Count > 0;
                    if (flag)
                    {
                        content = r.Replace(content, keyword2);
                    }
                }
                if (flag)
                {
                    Sql.ExecuteNonQuery(sql2, new MySqlParameter[]{
                new MySqlParameter("content",content),
                new MySqlParameter("id",rs[0])
            });
                }
                if (mbType == 1)//视 图时加载
                {
                    Config.viewVariables[rs["title"].ToString()] = content;
                }
            }
            rs.Close();
            return info;

        }
        public ReturnValue find(string keyword,int type=0,int u_webFAid=0)
        {
            ReturnValue info = new ReturnValue();
            if (!loginInfo.value.isAdministrator)
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
                return info;
            }
            if (keyword == "")
            {
                info.errMsg = "查询内容不能为空";
                info.errNo = -1;
                return info;;
            }
            List<object[]> data = new List<object[]>();
            MySqlDataReader rs = Sql.ExecuteReader("select id,classid,title,u_content,u_datatypeid,u_type,0 from template where   u_webFAid=@webFAid ", new MySqlParameter[]{
        new MySqlParameter("webFAid",u_webFAid)
    });
            MatchCollection mc;
            Regex r = null;
            while (rs.Read())
            {
                string content = rs.GetString(3);
                bool flag = false;
                if (type == 0)
                {
                    flag = content.IndexOf(keyword) > -1;
                }
                else
                {
                    r = new Regex(@keyword, RegexOptions.IgnoreCase); //定义一个Regex对象实例
                    mc = r.Matches(content);
                    flag = mc.Count > 0;
                }
                if (flag)
                {
                    object[] value = new object[]{
            rs[0],rs[1],rs[4],rs[5],rs[6],rs[2]
            };
                    data.Add(value);
                }
            }
            rs.Close();
            rs = Sql.ExecuteReader("select B.id,B.classid,A.className+'.'+B.title,B.u_html,B.u_datatypeid,0,1 from template_view B inner join class A on B.classId=A.id");
            while (rs.Read())
            {
                string content = rs.GetString(3);
                bool flag = false;
                if (type == 0)
                {
                    flag = content.IndexOf(keyword) > -1;
                }
                else
                {
                    mc = r.Matches(content);
                    flag = mc.Count > 0;
                }
                if (flag)
                {
                    object[] value = new object[]{
            rs[0],rs[1],rs[4],rs[5],rs[6],rs[2]
            };
                    data.Add(value);
                }
            }
            rs.Close();
            info.userData = data;
            return info;

        }
        public ReturnValue readBackup(double id)
        {
            ReturnValue info = new ReturnValue();
            info.userData = Sql.ExecuteScalar("select u_content from template_backup where id=@id", new MySqlParameter[]{
        new MySqlParameter("id",id )
    });
            return info;
        }
        public ReturnValue readBackupList(double dataId,double classId,int u_type,int u_defaultFlag,double u_datatypeId,string title,int u_webFAid)
        {
            ReturnValue info = new ReturnValue();
            string where = "";
            if (u_defaultFlag == 0) where = " and title=@title";
            if (dataId > 0)
            {
                info.userData = Sql.ExecuteArrayObj("select id,updateDate,userName from template_backup where dataId=@dataId  order by updatedate desc", new MySqlParameter[]{
            new MySqlParameter("dataId",dataId )
        });
            }
            else
            {
                info.userData = Sql.ExecuteArrayObj("select id,updateDate,userName from template_backup where classid=@classid and u_type=@u_type and u_webFAid=@u_webFAid and u_defaultFlag=@u_defaultFlag and u_datatypeId=@u_datatypeId " + where + "  order by updatedate desc", new MySqlParameter[]{
            new MySqlParameter("classid",classId ),
            new MySqlParameter("u_type",u_type),
            new MySqlParameter("u_webFAid",u_webFAid),
            new MySqlParameter("u_defaultFlag",u_defaultFlag),
            new MySqlParameter("u_datatypeId",u_datatypeId),
            new MySqlParameter("title",title)
        });
            }
            return info;
        }
        public ReturnValue readViewBackupList(double classId,double id,string title)
        {
            ReturnValue info = new ReturnValue();
            info.userData = Sql.ExecuteArrayObj("select id,updateDate,userName from template_backup where (classid=@classid and title=@title) or dataId=@dataId  order by updatedate desc", new MySqlParameter[]{
            new MySqlParameter("classid",classId ),
            new MySqlParameter("title",title),
            new MySqlParameter("dataId",id)
        });
            return info;
        }
        public ReturnValue delView(string ids)
        {
            ReturnValue info = new ReturnValue();
            Sql.ExecuteNonQuery("delete from template_view where id in (" + ids + ")");
            return info;
        }
        public ReturnValue addViewClass(string className)
        {
            ReturnValue info = new ReturnValue();
            if (!loginInfo.value.isAdministrator)
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
                return info;
            }
            double id = double.Parse(Tools.GetId());
            double classId = 12;
            object obj = Sql.ExecuteScalar("select id from class where classid=12 and className=@className", new MySqlParameter[] { new MySqlParameter("className", className) });
            if (obj != null)
            {
                info.errNo = -1;
                info.errMsg = "视图已存在";
            }
            else
            {
                Sql.ExecuteNonQuery("insert into class (id,classId,className)values(@id,@classId,@className)", new MySqlParameter[] {
        new MySqlParameter("id", id) ,
        new MySqlParameter("classId", classId) ,
        new MySqlParameter("className", className) }
                    );
            }
            return info;
        }
        public ReturnValue delViewClass(double id)
        {
            ReturnValue info = new ReturnValue();
            Sql.ExecuteNonQuery("delete from class where id=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            Sql.ExecuteNonQuery("delete from template_view where classid=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            return info;
        }

        public ReturnValue readSqlLable(string sql)
        {
            ReturnValue info = new ReturnValue();
            //if (sql.IndexOf(" top ") == -1) sql = sql.Replace("select ","select top 1 ");
            IDataReader rs = Sql.ExecuteReader(sql);
            object[] field = new object[rs.FieldCount];
            for (int i = 0; i < rs.FieldCount; i++)
            {
                string[] f = new string[] { rs.GetName(i), rs.GetName(i) };
                field[i] = f;
            }
            rs.Close();
            info.userData = field;
            return info;
        }

        public ReturnValue readDataTypeLable(double dataTypeId)
        {
            ReturnValue info = new ReturnValue();
            info.userData = new TableInfo(dataTypeId);
            return info;
        }
        //保存模板
        //参数1:模板ID
        public ReturnValue saveView(double id,double classId,int u_viewType,string title,string u_html,int u_editboxStatus, double u_datatypeId,string u_parameterValue)
        {
            try
            {

                ReturnValue info = new ReturnValue();
                if (!loginInfo.value.isAdministrator)
                {
                    info.errNo = -1;
                    info.errMsg = "没有权限";
                    return info;
                }

                ViewTemplate viewTemplate = new ViewTemplate()
                {
                    TemplateId = id,
                    ColumnId = classId,
                    ViewType = u_viewType,
                    TemplateName = title,
                    TemplateContent = u_html,
                    EditMode = (EditMode)u_editboxStatus,
                    DatatypeId = u_datatypeId,
                    ParameterValue = u_parameterValue
                };
                viewTemplate.Save(loginInfo.value.username);
                info.userData = viewTemplate.TemplateId;
                return info;
            }
            catch (Exception e) {
                return ReturnValue.Err(e.Message);
            }
        }
        public ReturnValue backupView(double id,string html)
        {
            ReturnValue info = new ReturnValue();
            Template.Backup(id, html,this.loginInfo.value.username);
            return info;
        }
        //备份模板
        public ReturnValue backupTemplate(double id, string html)
        {
            ReturnValue info = new ReturnValue();
            Template.Backup(id, html, this.loginInfo.value.username);
            return info;
        }
        //删除模板
        //参数1:模板ID
        public ReturnValue delTemplate(double id)
        {
            ReturnValue info = new ReturnValue();
            PageTemplate page = new PageTemplate(id);
            page.Remove();
            return info;
        }
        //读取模板
        //参数1:模板ID
        public ReturnValue readTemplate(double id,double classId,int typeId,double datatypeId,int defaultFlag,string title,int u_webFAid)
        {
            ReturnValue info = new ReturnValue();
            PageTemplate pageTemplate = null;
            if (id > 0)
            {
                pageTemplate = new PageTemplate(id);
                //info.userData = TemplateClass.get(id);
            }
            else
            {
                pageTemplate = new PageTemplate(classId, typeId, datatypeId, defaultFlag == 1, title, u_webFAid == 1);
                //info.userData = TemplateClass.get(classId, typeId, datatypeId, defaultFlag == 1, title, u_webFAid==1);

            }
            info.userData = pageTemplate.Get("id,classId,u_datatypeId,title,u_type,u_content,u_editboxStatus,u_parameterValue,u_defaultFlag,u_webFAid");
            return info;
        }
        /// <summary>
        /// 保存模板
        /// </summary>
        /// <param name="context"></param>
        public ReturnValue saveTemplate(double id,double classId,int u_typeId,double u_datatypeId,int u_defaultFlag,string title,int u_webFAid,string u_content,int u_editboxStatus,string u_parameterValue)
        {
            PageTemplate pageTemplate = null;
            try
            {
                pageTemplate = new PageTemplate(
                    classId,
                    u_typeId,
                    u_datatypeId,
                    u_defaultFlag == 1,
                    title,
                    u_webFAid==1
                );
                pageTemplate.TemplateName =title;
                pageTemplate.TemplateContent = u_content;
                pageTemplate.EditMode = (EditMode)u_editboxStatus;
                pageTemplate.ParameterValue = u_parameterValue;
            }
            catch
            {
                pageTemplate = new PageTemplate()
                {
                    TemplateId = id,
                    TemplateName = title,
                    TemplateContent = u_content,
                    TemplateType = (TemplateType)u_typeId,
                    IsDefault = u_defaultFlag == 1,
                    ColumnId = classId,
                    DatatypeId = u_datatypeId,
                    EditMode = (EditMode)u_editboxStatus,
                    ParameterValue = u_parameterValue,
                    IsMobile = u_webFAid == 1
                };
            }
            try
            {
                pageTemplate.Save(loginInfo.value.username);
                return new ReturnValue();

            }
            catch (Exception e)
            {
                return ReturnValue.Err(e.Message);
            }
        }
        public ReturnValue readTemplateView(double classId)
        {
            ReturnValue info = new ReturnValue();
            info.userData = Sql.ExecuteArray("select B.id,B.title text,6 type,B.u_pinyin from template_view B  where B.classid=@classId", new MySqlParameter[] { new MySqlParameter("classId", classId) });
            return info;
        }
        public ReturnValue readTemplateViewClass()
        {
            ReturnValue info = new ReturnValue();
            info.userData = Sql.ExecuteArray("select id,className text,5 type from class where classid=12");
            return info;
        }
        public ReturnValue readTemplateLable(double dataTypeId)
        {
            ReturnValue info = new ReturnValue();
            Dictionary<string, object> value = new Dictionary<string, object>();
            XmlNodeList list = Config.loadFile("systemVariables.config");
            Dictionary<string, string[]> systemVariables = new Dictionary<string, string[]>();
            for (int i = 0; i < list.Count; i++)
            {
                systemVariables.Add(list[i].Attributes["name"].Value, new string[] { list[i].Attributes["labelText"].Value, list[i].InnerText });
            }
            value.Add("systemVariables", systemVariables);
            if (dataTypeId > -1)
            {
                TableInfo table = new TableInfo(dataTypeId);
                value.Add("pageVariables", table.fields);
            }
            //Constant.systemVariables
            //"{\"systemVariables\": Constant.systemVariables.ToJson}
            info.userData = value;
            return info;
        }
        public ReturnValue templateList(double moduleId,double faId,int typeId,double dataTypeId,int u_webFAid)
        {
            ReturnValue info = new ReturnValue();
            MySqlDataReader rs = null;
            List<object[]> list = new List<object[]>();
            string[] templateRange = new string[] { "全站", "模块", "频道" };
            //ArrayList list = new ArrayList();
            /*
            rs = Sql.ExecuteReader("select type from module where id=" + ModuleID);
            if (rs.Read())
            {
                if (rs[0].ToString() == "False")//虚拟目录
                {
                    ModuleType = false;
                }
            }
            rs.Close();*/
            if (typeId == 0)
            {
                list.Add(new object[] { 0, "全站", "站点通用模板", "", 1, 0, 0 });
                if (moduleId > 0) list.Add(new object[] { 0, "模块", "模块通用模板", "", 1, moduleId, 0 });
                if (moduleId > 0 && faId != moduleId) list.Add(new object[] { 0, "频道", "频道页模板", "", 1, faId, 0 });
                rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid from  template B  where B.u_type=0 and B.u_datatypeid=0 and B.u_defaultFlag=1 and B.u_webFAId=@webFAId and B.classid in (0,@moduleId,@id)",
                    new MySqlParameter[] {
                    new MySqlParameter("id", faId),
                    new MySqlParameter("moduleId", moduleId),
                    new MySqlParameter("webFAId",u_webFAid)
                    });
                int index = 0;
                while (rs.Read())
                {
                    if (rs.GetDouble(2) == 0) index = 0;
                    else if (rs.GetDouble(2) == moduleId) index = 1;
                    else if (rs.GetDouble(2) == faId) index = 2;
                    list[index][0] = rs.GetDouble(0);
                    list[index][3] = rs.GetDateTime(1).ToString();
                    list[index][5] = rs.GetDouble(2);
                }
                rs.Close();
            }
            else if (typeId == 1)
            {
                list.Add(new object[] { 0, "全站", "站点通用模板", "", 1, 0, 0 });
                if (moduleId > 0) list.Add(new object[] { 0, "模块", "模块通用模板", "", 1, moduleId, 0 });
                if (faId != moduleId) list.Add(new object[] { 0, "频道", "频道页模板", "", 1, faId, 0 });
                rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid,B.u_defaultFlag,B.title from  template B  where B.u_type=1 and B.u_datatypeid=0 and B.u_webFAId=@webFAId and B.classid in (0,@moduleId,@id) order by B.classid ",
                    new MySqlParameter[] {
                    new MySqlParameter("id", faId),
                    new MySqlParameter("moduleId", moduleId),
                    new MySqlParameter("webFAId",u_webFAid)
                    });
                int index = 0;
                while (rs.Read())
                {
                    bool defaultFlag = rs.GetInt32(3) == 1;
                    if (rs.GetDouble(2) == 0) index = 0;
                    else if (rs.GetDouble(2) == moduleId) index = 1;
                    else if (rs.GetDouble(2) == faId) index = 2;

                    if (defaultFlag)
                    {
                        list[index][0] = rs.GetDouble(0);
                        list[index][3] = rs.GetDateTime(1).ToString();
                        list[index][5] = rs.GetDouble(2);
                    }
                    else
                    {
                        list.Add(new object[] { rs[0], templateRange[index], rs[4], rs.GetDateTime(1).ToString(), rs.GetDouble(2), 0 });
                    }
                }
                rs.Close();
            }
            else if (typeId == 2)
            {
                List<object[]> datatypeList = new List<object[]>();
                string sql = "select SaveDataType from class where moduleid=@moduleId group by SaveDataType";
                if (moduleId == 0) sql = "select SaveDataType from class where moduleId in (select id from module)";

                rs = Sql.ExecuteReader("select id,datatype from datatype where id in (" + sql + ")",
                new MySqlParameter[] {
                    new MySqlParameter("moduleId", moduleId)
                    });
                while (rs.Read())
                {
                    datatypeList.Add(new object[] { rs.GetDouble(0), rs.GetString(1) });
                }
                rs.Close();
                int templateRangeCount = templateRange.Length;
                if (moduleId == 0) templateRangeCount = 1;
                else if (faId == moduleId) templateRangeCount = 2;
                for (int i1 = 0; i1 < templateRangeCount; i1++)
                {
                    for (int i = 0; i < datatypeList.Count; i++)
                    {
                        double classId = 0;
                        if (i1 == 0) classId = 0;
                        else if (i1 == 1) classId = moduleId;
                        else if (i1 == 2) classId = faId;
                        list.Add(new object[] { 0, templateRange[i1], datatypeList[i][1].ToString() + "模板", "", 1, classId, datatypeList[i][0] });
                    }
                }
                #region 获取默认模板
                for (int i = 0; i < datatypeList.Count; i++)
                {
                    rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid,B.title,B.u_defaultFlag,B.u_datatypeId from  template B where B.u_type=2 and B.u_datatypeid=@datatypeId and B.u_webFAId=@webFAId and B.u_defaultFlag=1 and B.classid in (0,@moduleId,@id)",
                        new MySqlParameter[] {
                    new MySqlParameter("id", faId),
                    new MySqlParameter("moduleId", moduleId),
                    new MySqlParameter("datatypeId", datatypeList[i][0]),
                    new MySqlParameter("webFAId",u_webFAid)

                    });
                    int index = 0;
                    while (rs.Read())
                    {
                        if (rs.GetDouble(2) == 0) index = i;//全站
                        else if (rs.GetDouble(2) == moduleId) index = datatypeList.Count + i;//模块
                        else if (rs.GetDouble(2) == faId)
                        {
                            index = datatypeList.Count * 2 + i;//频道
                        }
                        list[index][0] = rs.GetDouble(0);
                        list[index][3] = rs.GetDateTime(1).ToString();
                        list[index][5] = rs.GetDouble(2);
                        list[index][6] = rs.GetDouble(5);

                    }
                    rs.Close();
                }
                #endregion
                rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid,B.title,B.u_defaultFlag,B.u_datatypeId from  template B where B.u_type=2 and B.u_webFAId=@webFAId and B.u_defaultFlag=0 and B.classid = @id",
                    new MySqlParameter[] {
                    new MySqlParameter("id", faId),
                    new MySqlParameter("webFAId",u_webFAid)
                    });
                while (rs.Read())
                {
                    int index = 0;
                    if (rs.GetDouble(2) == 0) index = 0;//全站
                    else if (rs.GetDouble(2) == moduleId) index = 1;//模块
                    else if (rs.GetDouble(2) == faId) index = 2;
                    list.Add(new object[] { rs.GetDouble(0), templateRange[index], rs[3], rs.GetDateTime(1).ToString(), 0, rs.GetDouble(2), 0, rs.GetDouble(5) });
                }
                rs.Close();
            }
            else if (typeId == 3)
            {
                string path = @"/";
                if (faId > 1)
                {
                    rs = Sql.ExecuteReader("select dirname from  class where id = @id", new MySqlParameter[] { new MySqlParameter("id", faId) });
                    if (rs.Read())
                    {
                        path = @"/" + rs.GetString(0) + @"/";
                    }
                    rs.Close();
                }
                rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid,B.title,B.u_defaultFlag from   template B  where B.u_type=3 and B.u_webFAId=@webFAId  and B.classid = @id",
                new MySqlParameter[] {
                new MySqlParameter("id", faId),
                new MySqlParameter("webFAId",u_webFAid)
                });
                while (rs.Read())
                {
                    list.Add(new object[] { rs.GetDouble(0), path, rs[3], rs.GetDateTime(1).ToString(), 0, 0 });
                }
                rs.Close();
            }

            info.userData = list;
            return info;
        }
    }
 
}
