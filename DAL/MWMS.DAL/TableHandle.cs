using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace MWMS.DAL
{
    /// <summary>
    /// 表操作类
    /// </summary>
    public class TableHandle
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string PrimaryKey = "id";
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        public TableHandle()
        {

        }
        public TableHandle(string tableName)
        {
            TableName = tableName;
        }
        public Dictionary<string,object> GetModel(double id,string fields)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add(PrimaryKey, id);
            return GetModel(fields, PrimaryKey+"=@"+ PrimaryKey, p, "");
        }
        public Dictionary<string, object> GetModel(string fields, string where, Dictionary<string, object> p)
        {
            return GetModel(fields, where, p, "");
        }
        MySqlParameter [] GetParameter(Dictionary<string, object> p)
        {
            MySqlParameter[] _p = new MySqlParameter[p.Count];
            int i1 = 0;
            foreach (var value in p)
            {
                _p[i1] = new MySqlParameter(value.Key, value.Value);
                i1++;
            }
            return _p;
        }
        public Dictionary<string, object> GetModel(double id)
        {
            return GetModel(id,"*");
        }
        public Dictionary<string, object> GetModel(string fields, string where, Dictionary<string, object> p, string desc="")
        {
            if (TableName == "") throw new Exception("表名不能为空");
            Dictionary<string, object> model = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            string[] _fields = fields.Split(',');
            MySqlParameter[] _p = GetParameter(p);
            if (desc != "") desc = "order by "+desc;
            MySqlDataReader rs = SqlHelper.Sql.ExecuteReader("select " + fields + " from " + TableName + " where "+ where+" "+desc, _p);
            bool flag = false;
            if (rs.Read())
            {
                for (int i = 0; i < rs.FieldCount; i++)
                {
                    if (rs.IsDBNull(i))
                    {
                        model[rs.GetName(i)] = "";
                    }
                    else { 
                    model[rs.GetName(i)] = rs[rs.GetName(i)];
                    }
                }
                flag = true;
            }
            rs.Close();
            if (!flag) return null;
            return model;
        }
        public int Count(string where, Dictionary<string, object> p)
        {
            MySqlParameter[] _p = GetParameter(p);
            return int.Parse(Sql.ExecuteScalar("select count(1) from " + TableName + " where " + where, _p).ToString());
        }
        /// <summary>
        /// 保存模型到表中
        /// </summary>
        /// <param name="model">模型</param>
        public double Save(Dictionary<string, object> model)
        {
            if (TableName == "") throw new Exception("表名不能为空");
            bool updateFlag = model.ContainsKey(PrimaryKey);
            if (updateFlag && (double)(model[PrimaryKey])>0) {
                return Update(model);
            }else
            {
                return Append(model);
            }
        }
        public double Append(Dictionary<string, object> model)
        {
            StringBuilder fieldstr = new StringBuilder();
            StringBuilder fieldstr2 = new StringBuilder();
            if(!model.ContainsKey(PrimaryKey) || (double)(model[PrimaryKey])==0) model[PrimaryKey] =double.Parse( Tools.GetId());
            foreach (var field in model)
            {
                if (fieldstr.Length > 0)
                {
                    fieldstr.Append(",");
                    fieldstr2.Append(",");
                }
                fieldstr.Append(field.Key);
                fieldstr2.Append("@" + field.Key);
            }
            Sql.ExecuteNonQuery("insert into  " + TableName + " (" + fieldstr.ToString() + ")values(" + fieldstr2.ToString()+ ")", model);
            return (double)model[PrimaryKey];
        }
        public double Update(Dictionary<string, object> model)
        {
            StringBuilder fieldstr = new StringBuilder();
            foreach (var field in model)
            {
                    if (fieldstr.Length > 0) fieldstr.Append(",");
                    fieldstr.Append(field.Key + "=@" + field.Key);
            }
            Sql.ExecuteNonQuery("update " + TableName + " set " + fieldstr.ToString() + " where "+ PrimaryKey + "=@"+ PrimaryKey, model);
            return (double)model[PrimaryKey];
        }
        public void Remove(double id)
        {
            Sql.ExecuteNonQuery("delete from "+TableName+" where "+PrimaryKey+"=@id",new MySqlParameter[] {
                new MySqlParameter("id",id)
            });
        }
    }
}
