using Helper;
using M5.Common;
using MWMS.DAL;
using MWMS.Helper.Extensions;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace MWMS.Template
{
    public class PageTemplate : Template
    {
        /// <summary>
        /// 是否手机模板
        /// </summary>
        public bool IsMobile { get; set; }
        /// <summary>
        /// 是否默认模板
        /// </summary>
        public bool IsDefault { get; set; }

        public Dictionary<string, object> Variable = new Dictionary<string, object>();
        public PageTemplate()
        {
            this.TableName = "template";
        }
        public PageTemplate(string url, bool isMobile)
        {
            this.TableName = "template";
            Get(url, isMobile);
        }
        public PageTemplate(double id)
        {
            this.TableName = "template";
            Get(id);
        }
        public PageTemplate(double classId, int typeId, double datatypeId, bool isDefault, string title, bool isMobile)
        {
            this.TableName = "template";
            Get(classId, typeId, datatypeId, isDefault, title, isMobile);
        }
        public PageTemplate(double classId, int typeId, double datatypeId, bool isDefault, bool isMobile)
        {
            this.TableName = "template";
            Get(classId, typeId, datatypeId, isDefault, "", isMobile);
        }
        void Get(double id)
        {
            this.TemplateId = id;
            Dictionary<string, object> model = this.Get("id,title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue");
            if (model == null) throw new Exception("指定模板不存在");
            SetAttr(model);
        }
        void Get(string url, bool isMobile)
        {
            if (url == "/") Get(0, 0, -1, true, "", isMobile);//获取首页模板
            else if (url.Substring(url.Length - 1) == "/")
            {
                GetColumnTemplate(url, isMobile);
            }
            else
            {
                url = Regex.Replace(url, @"\.(.*)$", "", RegexOptions.IgnoreCase);
                try
                {
                    GetCustomTemplate(url, isMobile);
                }
                catch
                {
                    GetContentTemplate(url, isMobile);
                }
            }
        }
        /// <summary>
        /// 跟据地址获取栏目模板信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isMobile"></param>
        void GetColumnTemplate(string url, bool isMobile)
        {
            double dataTypeId = 0, rootId = 0, moduleId = 0, skinId = 0;
            int typeId = 1;

            #region 栏目数据
            TableHandle table = new TableHandle("class");
            Dictionary<string, dynamic> p = new Dictionary<string, dynamic>();
            p.Add("url", url);
            Dictionary<string, object> variable = table.GetModel("rootId,moduleId,skinId,classId,id,className,info,orderId,createDate,saveDataType,maxIco,updatedate,dirName,keyword,dirPath,childId,parentId,layer,attribute,custom,url,_SkinID", "url=@url", p);
            if (variable == null) throw new Exception("栏目不存在");
            rootId = (double)variable["rootId"];
            moduleId = (double)variable["moduleId"];
            skinId = variable["skinId"] == null ? 0 : (double)variable["skinId"];
            if (isMobile) skinId = variable["_SkinID"] == null ? 0 : (double)variable["_SkinID"];
            if ((double)variable["classId"] == 7) typeId = 0;
            XmlDocument custom = new XmlDocument();
            custom.LoadXml(variable["custom"].ToString());
            Dictionary<string, string> customList = new Dictionary<string, string>();
            for (int i=0;i< custom.DocumentElement.ChildNodes.Count;i++)
            {
                customList.Add(custom.DocumentElement.ChildNodes[i].Attributes["name"].Value.Trim(), custom.DocumentElement.ChildNodes[i].InnerText);
            }
            variable["custom"] = customList;
            variable["maxIco"]= DAL.Datatype.FieldType.Pictures.Parse(variable["maxIco"].ToString());
            this.Variable = variable;
            #endregion
            if (skinId > 0)
            {
                Get(skinId);
                return;
            }
            Get(moduleId, rootId, typeId, dataTypeId, isMobile);
        }
        /// <summary>
        /// 获取内容模板信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isMobile"></param>
        void GetContentTemplate(string url, bool isMobile)
        {
            //url = url.Replace("." + BaseConfig.extension, "");
            double dataTypeId = 0, rootId = 0, moduleId = 0, skinId = 0, id = 0, classId = 0;
            TableHandle table = new TableHandle("maintable");
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("url", url);
            Dictionary<string, object> variable = table.GetModel("dataTypeId,classId,id,skinId,moduleId, rootId", "url=@url and orderId>-1", p);
            if (variable == null) throw new Exception("访问数据不存在");
            moduleId = (double)variable["moduleId"];
            rootId = (double)variable["rootId"];
            dataTypeId = (double)variable["dataTypeId"];
            skinId = (double)variable["skinId"];
            id = (double)variable["id"];
            classId = (double)variable["classId"];
            MySqlDataReader rs = null;
            if (skinId == 0)
            {
                rs = Sql.ExecuteReader("select " + (isMobile ? "_contentSkinID" : "contentSkinID") + " from class where id=@id", new MySqlParameter[] { new MySqlParameter("id", classId) });
                if (rs.Read())
                {
                    skinId = rs.IsDBNull(0) ? 0 : rs.GetDouble(0);
                }
                rs.Close();
            }
            DAL.Datatype.TableHandle T = new DAL.Datatype.TableHandle(dataTypeId);
            variable = T.GetModel(id);
            variable["attribute"] = "";
            /*
            string tableName = (string)Helper.Sql.ExecuteScalar("select tableName from datatype where id=" + dataTypeId.ToString());
            rs = Helper.Sql.ExecuteReader("select * from " + tableName + " where id=@id", new MySqlParameter[] {
                    new MySqlParameter("id",id)});
            if (rs.Read())
            {
                for (int i = 0; i < rs.FieldCount; i++)
                {
                    string fieldName = rs.GetName(i);

                    if (rs.IsDBNull(i))
                    {
                        variable[rs.GetName(i)] = "";
                    }
                    else
                    {
                        if (rs.GetDataTypeName(rs.GetOrdinal(fieldName)) == "ntext")
                        {
                            //SystemLink v1 = new SystemLink();
                            string FieldValue = rs[i] + "";
                            //FieldValue = v1.Replace(FieldValue);
                            variable[fieldName] = FieldValue;
                        }
                        else
                        {
                            variable[fieldName] = rs[i];
                        }
                    }
                }
            }
            rs.Close();*/
            this.Variable = variable;
            if (skinId > 0)
            {
                try
                {
                    Get(skinId);
                }
                catch
                {
                    Get(moduleId, rootId, 2, dataTypeId, isMobile);
                }
            }
            else
            {
                Get(moduleId, rootId, 2, dataTypeId, isMobile);
            }
        }
        /// <summary>
        /// 获取自定义模板信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isMobile"></param>
        void GetCustomTemplate(string url, bool isMobile)
        {
            //url = url.Replace("." + BaseConfig.extension, "");//无参数
            string url2 = "";
            Regex r = new Regex(@"/" + ".*(?=" + @"/" + ")", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            MatchCollection mc = r.Matches(url);
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("url", url);
            int count = this.Count("url=@url", p);
            if (count > 0)
            {
                url2 = url;
                this.Parameter = new object[] { "", "", "" };
            }
            else
            {
                if (mc.Count > 0) url2 = mc[0].Value;
                Dictionary<string, object> variable = new Dictionary<string, object>();
                if (url2 != "")
                {
                    string pstr = Regex.Replace(url, "^" + url2 + @"/", "", RegexOptions.IgnoreCase);
                    pstr = System.Web.HttpUtility.UrlDecode(pstr);
                    this.Parameter = pstr.Split('-');
                }
            }
            Get(url, url2, isMobile);
        }
        void Get(string url, string url2, bool isMobile)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("url", url);
            p.Add("url2", url2);
            p.Add("u_webFAid", isMobile ? 1 : 0);
            string where = "";
            if (url2 != "") where = " or url=@url2";
            Dictionary<string, object> model = null;
            model = this.GetModel("id,title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue",
                " u_webFAid = @u_webFAid and u_type = 3 and(url = @url" + where + ")", p);
            if (model == null) throw new Exception("指定模板不存在");
            SetAttr(model);
        }
        void Get(double classId, int typeId, double datatypeId, bool isDefault, string title, bool isMobile)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("classId", classId);
            p.Add("typeId", typeId);
            p.Add("datatypeId", datatypeId);
            p.Add("isDefault", isDefault ? 1 : 0);
            p.Add("title", title);
            p.Add("isMobile", isMobile ? 1 : 0);
            Dictionary<string, object> model = null;
            if (isDefault)
            {
                model = this.GetModel("id,title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue", "classId=@classId and u_type=@typeId and u_datatypeId=@datatypeId and u_defaultFlag=@isDefault  and u_webFAid=@isMobile", p);
            }
            else
            {
                model = this.GetModel("id,title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue", "classId=@classId and u_type=@typeId and u_datatypeId=@datatypeId and u_defaultFlag=@isDefault and title=@title and u_webFAid=@isMobile", p);
            }
            if (model == null) throw new Exception("指定模板不存在");
            SetAttr(model);
        }
        void Get(double moduleId, double rootId, int typeId, double datatypeId, bool isMobile)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("moduleId", moduleId);
            p.Add("rootId", rootId);
            p.Add("typeId", typeId);
            p.Add("datatypeId", datatypeId);
            p.Add("isMobile", isMobile ? 1 : 0);
            Dictionary<string, object> model = null;
            model = this.GetModel("id,title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue",
                "classId in (0,@moduleId,@rootId) and u_defaultFlag=1 and u_datatypeid=@datatypeId and u_type=@typeId and u_webFAid=@isMobile", p, "u_layer desc");

            if (model == null) throw new Exception("指定模板不存在");
            SetAttr(model);
        }
        void SetAttr(Dictionary<string, object> model)
        {
            this.TemplateId = (double)model["id"];
            this.TemplateContent = (string)model["u_content"];
            this.TemplateName = (string)model["title"];
            this.DatatypeId = (double)model["u_datatypeId"];
            this.TemplateType = (TemplateType)model["u_type"];
            this.EditMode = (EditMode)model["u_editboxStatus"];
            this.IsDefault = (int)model["u_defaultFlag"] == 1;
            this.IsMobile = (int)model["u_webFAid"] == 1 ? true : false;
            this.ColumnId = (double)model["classId"];
            this.ParameterValue = model["u_parameterValue"] + "";
        }

        /// <summary>
        /// 自定义模板是否重名
        /// </summary>
        /// <returns></returns>
        public bool CustomPageExist()
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();
            fields["classId"] = this.ColumnId;
            fields["id"] = this.TemplateId;
            fields["title"] = this.TemplateName;
            int count = this.Count("u_datatypeid=-3 and classId=@classId and id<>@id and title=@title", fields);
            return count > 0;
        }
        /// <summary>
        /// 保存模板
        /// </summary>
        public void Save()
        {
            if (TemplateType == TemplateType.自定义页 && this.CustomPageExist())
            {
                throw new Exception("页面“" + this.TemplateName + "”已存在请不要重复创建");
            }


            Dictionary<string, object> fields = new Dictionary<string, object>();
            fields["id"] = this.TemplateId;
            fields["title"] = this.TemplateName;
            fields["u_content"] = SetContent(this.TemplateContent);
            fields["u_type"] = (int)this.TemplateType;
            fields["u_defaultFlag"] = this.IsDefault ? 1 : 0;
            fields["classId"] = this.ColumnId;
            fields["u_datatypeId"] = this.DatatypeId;
            fields["u_editboxStatus"] = (int)this.EditMode;
            fields["u_parameterValue"] = this.ParameterValue;
            fields["u_webFAid"] = this.IsMobile ? 1 : 0;
            fields["createDate"] = System.DateTime.Now;
            fields["updateDate"] = System.DateTime.Now;
            int u_layer = 0;
            ModuleInfo moduleInfo = ModuleClass.get(ColumnId);
            if (moduleInfo == null)
            {
                ColumnInfo columnInfo = ColumnClass.get(ColumnId);
                if (columnInfo != null)
                {
                    u_layer = 2;
                    fields["url"] = @"/" + columnInfo.dirName + "/" + TemplateName;
                }
                else
                {
                    fields["url"] = @"/" + TemplateName;
                }
            }
            else
            {
                u_layer = 1;
                if (moduleInfo.type) fields["url"] = @"/" + moduleInfo.dirName + "/" + TemplateName;
            }
            fields["u_layer"] = u_layer;
            this.TemplateId = Save(fields);
            Build(true);
        }
        /// <summary>
        /// 保存并备份
        /// </summary>
        /// <param name="username">备份人</param>
        public void Save(string username)
        {
            Save();
            Dictionary<string, object> fields = new Dictionary<string, object>();
            fields["id"] = this.TemplateId;
            fields["title"] = this.TemplateName;
            fields["u_type"] = (int)this.TemplateType;
            fields["u_defaultFlag"] = this.IsDefault ? 1 : 0;
            fields["classId"] = this.ColumnId;
            fields["u_datatypeId"] = this.DatatypeId;
            fields["u_editboxStatus"] = (int)this.EditMode;
            fields["u_parameterValue"] = this.ParameterValue;
            fields["u_webFAid"] = this.IsMobile ? 1 : 0;
            TableHandle table = new TableHandle("template_backup");
            int count = table.Count("classid=@classid and u_type=@u_type and u_webFAid=@u_webFAid and u_defaultFlag=@u_defaultFlag and u_datatypeId=@u_datatypeId and title=@title and  '"+DateTime.Now.AddMinutes(-200)+ "'<updatedate", fields);
            if (count == 0) this.Backup(username);
        }
        /// <summary>
        /// 设置页面内容
        /// </summary>
        /// <param name="u_content">内容</param>
        /// <returns></returns>
        string SetContent(string u_content)
        {
            u_content = u_content + "";
            MatchCollection mc, mc2;
            Regex r = new Regex(@"(</title>).*?(</head>)", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(u_content);
            if (mc.Count > 0)//如果找到头部信息时
            {
                string H = mc[0].Value;
                Regex r2 = new Regex(@"<script(.*)</script>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                mc2 = r2.Matches(H);
                if (mc2.Count == 0)//如果没有js时
                {
                    string H2 = H.Replace("</head>", "<script type=\"text/javascript\" src=\"" + Config.webPath + "/static/m5_public.js\"></script>\n</head>");
                    u_content = u_content.Replace(H, H2);
                }
                else
                {
                    bool tag = false;
                    for (int i = 0; i < mc2.Count; i++)
                    {
                        if (mc2[i].Value.ToLower().IndexOf("m5_public.js") > -1)//如果包含系统js时
                        {
                            i = mc2.Count;
                            tag = true;
                        }
                    }
                    if (!tag)//如果所有的js都不是系统js时
                    {
                        string H2 = H.Replace(mc2[0].Value, "<script type=\"text/javascript\" src=\"" + Config.webPath + "/static/m5_public.js\"></script>\n" + mc2[0].Value);
                        u_content = u_content.Replace(H, H2);
                    }
                }
            }
            return u_content;
        }


    }

}
