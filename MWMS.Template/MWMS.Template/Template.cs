using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MWMS.DAL;
using System.Data.SqlClient;
using Helper;
using MWMS.SqlHelper;
using MWMS.Helper;
using MySql.Data.MySqlClient;

namespace MWMS.Template
{
    public enum TemplateType
    {
        //主页=0,
        频道页 = 0,
        栏目页 = 1,
        内容页 = 2,
        自定义页 = 3,
        视图 = 10
    }
    public enum EditMode
    {
        代码模式 = 1,
        设计模式 = 0
    }
    public class Template : TableHandle
    {
        /// <summary>
        /// 模板所属数据类型
        /// </summary>
        public double DatatypeId { get; set; }
        /// <summary>
        /// 参数表
        /// </summary>
        public string ParameterValue { get; set; }
        /// <summary>
        /// 编辑模型
        /// </summary>
        public EditMode EditMode { get; set; }
        /// <summary>
        /// 模板id
        /// </summary>
        public double TemplateId { get; set; }
        private TemplateType _templateType;
        /// <summary>
        /// 模板类型
        /// </summary>
        public TemplateType TemplateType
        {
            get { return _templateType; }
            set { _templateType = value; this.TableName = value == TemplateType.视图 ? "template_view" : "template"; }
        }
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; }
        /// <summary>
        /// 模板内容
        /// </summary>
        public string TemplateContent { get; set; }
        /// <summary>
        /// 栏目id
        /// </summary>
        public double ColumnId { get; set; }
        public object[] Parameter = null;
        /// <summary>
        /// 模板备份
        /// </summary>
        //public abstract void Backup();
        public void Remove()
        {
            if (TemplateType == TemplateType.视图)
            {
                Sql.ExecuteNonQuery("delete  from template_view where id=@id", new MySqlParameter[] {
                    new MySqlParameter("id",TemplateId)
                });
            }
            else
            {
                Sql.ExecuteNonQuery("delete  from template where id=@id", new MySqlParameter[] {
                    new MySqlParameter("id",TemplateId)
                });
            }
        }
        /// <summary>
        /// 备份模板
        /// </summary>
        /// <param name="username">操作人</param>
        public void Backup(string username)
        {

            if (TemplateType == TemplateType.视图)
            {
                Sql.ExecuteNonQuery("insert into template_backup (id,dataId,u_content,updatedate,username,title,classid)" +
                    "select @id,@dataId,u_html,@updatedate,@username,title,classid from template_view where id=@dataId", new MySqlParameter[]{
                 new MySqlParameter("id",double.Parse(Tools.GetId())),
                 new MySqlParameter("dataId",this.TemplateId),
                 new MySqlParameter("username",username),
                 new MySqlParameter("updatedate",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                });
            }
            else
            {
                Sql.ExecuteNonQuery("insert into template_backup (id,dataId,classId,u_type,title,u_content,updateDate,userName,u_webFAid,u_defaultflag,u_datatypeId)" +
"select @id,B.id,B.classid,B.u_type,B.title,B.u_content,@updatedate,@username,B.u_webFAid,B.u_defaultflag,u_datatypeId from template B  " +
" where B.id=@dataId", new MySqlParameter[]{
                 new MySqlParameter("id",double.Parse(Tools.GetId())),
                 new MySqlParameter("dataId",this.TemplateId),
                 new MySqlParameter("username",username),
                 new MySqlParameter("updatedate",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
});
            }
        }
        public static void Backup(double templateId, string html, string username)
        {
            Sql.ExecuteNonQuery("insert into template_backup (id,dataId,u_content,updateDate,userName)" +
"values(@id,@dataId,@u_content,@updatedate,@username)  ", new MySqlParameter[]{
                 new MySqlParameter("id",double.Parse(Tools.GetId())),
                 new MySqlParameter("dataId",templateId),
                 new MySqlParameter("u_content",html),
                 new MySqlParameter("username",username),
                 new MySqlParameter("updatedate",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
            });
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public Dictionary<string, object> Get(string fields)
        {
            return GetModel(this.TemplateId, fields);
        }
        /// <summary>
        /// 编译模板
        /// </summary>
        public void Build(bool flag)
        {
            string code = this.TemplateContent;
            StringBuilder fieldCode = new StringBuilder();
            /*
            if (this.TemplateType == TemplateType.频道页 ||  this.TemplateType==TemplateType.栏目页 || this.TemplateType == TemplateType.内容页)
            {
                DAL.Datatype.TableStructure table = new DAL.Datatype.TableStructure(this.DatatypeId);
                foreach (var item in table.Fields)
                {

                    if (item.Key == "url")
                    {
                        fieldCode.Append("" + item.Key + "=Config.webPath + _page[\"" + item.Key + "\"] + \".\" + BaseConfig.extension,\r\n");
                    }else { 
                        fieldCode.Append("" + item.Key + "=_1page[\"" + item.Key + "\"],\r\n");
                    }
                }
            }*/
            //BuildCode build = new BuildCode(this.TemplateId.ToString(), "@{\r\nvar page=new {" + fieldCode.ToString() + "};\r\n}" + code);
            BuildCode build = new BuildCode(this.TemplateId.ToString(), code);
            build.compile(flag);
        }
        public void Build()
        {
            Build(false);
        }
    }
}
