using Helper;
using M5.Common;
using MWMS.DAL;
using MWMS.Helper.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace MWMS.Template
{
    public class ViewTemplate : Template
    {
        public ViewTemplate()
        {

            this.TableName = "DataView";
            this.TemplateType = TemplateType.视图;
        }
        /// <summary>
        /// 试图类型
        /// </summary>
        public int ViewType { get; set; }
        /// <summary>
        /// 模板所属数据类型
        /// </summary>
        public double DatatypeId { get; set; }
        /// <summary>
        /// 保存模板
        /// </summary>
        public void Save(string username)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();
            fields["id"] = this.TemplateId;
            fields["classId"] = this.ColumnId;
            fields["u_viewType"] = this.ViewType;
            fields["title"] = this.TemplateName;
            fields["u_pinyin"] = this.TemplateName.GetPinYin('0');
            fields["u_html"] = this.TemplateContent;
            fields["u_editboxStatus"] = (int)this.EditMode;
            fields["u_datatypeId"] = this.DatatypeId;
            fields["u_parameterValue"] = this.ParameterValue;
            fields["createDate"] = System.DateTime.Now;
            fields["updateDate"] = System.DateTime.Now;
            this.TemplateId = Save(fields);
            DAL.TableHandle table = new DAL.TableHandle("class");
            string className = (string)(table.GetModel(ColumnId, "classname")["classname"]);
            Dictionary<string, object> list = null;
            if (Config.viewVariables.ContainsKey(className))
            {
                list = (Dictionary<string, object>)Config.viewVariables[className];
            }
            else
            {
                list = new Dictionary<string, object>();
                Config.viewVariables[className] = list;
            }
            Build(true);
            list[TemplateName] = new object[] { TemplateId, TemplateContent };
            TableHandle table2 = new TableHandle("template_backup");
            int count = table2.Count("classid=@classid and title=@title and '"+DateTime.Now.AddMinutes(-200).ToString("yyyy-MM-dd HH:mm:ss")+"'<updatedate", fields);
            if (count == 0) this.Backup(username);
        }
    }
}
