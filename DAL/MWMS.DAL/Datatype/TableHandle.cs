
using Helper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using System.IO;

namespace MWMS.DAL.Datatype
{
    public class TableHandle : TableStructure
    {
        public TableHandle(string tableName) : base(tableName)
        {

        }
        public TableHandle(double datatypeId) : base(datatypeId)
        {

        }
        public Dictionary<string, object> GetModel(object id)
        {
            return GetModel(id.ToDouble(), "*");
        }
        public Dictionary<string, object> GetModel(double id)
        {
            return GetModel(id, "*");
        }
        public Dictionary<string, object> GetModel(double id, string fields)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("id", id);
            return GetModel(fields, "A.id=@id", p, "");
        }
        public Dictionary<string, object> GetModel(string fields, string where, Dictionary<string, object> p, string desc)
        {
            if (TableName == "") throw new Exception("表名不能为空");
            Dictionary<string, object> model = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            string[] _fields = fields.Split(',');
            string fieldList = "";
            for (int i = 0; i < _fields.Length; i++)
            {
                int count = Fields.Where(p1 => p1.Value.isPublicField && p1.Value.name == _fields[i]).Count();
                fieldList += ((count > 0) ? "A." : "B.") + _fields[i];
            }
            MySqlParameter[] _p = GetParameter(p);
            MySqlDataReader rs = Sql.ExecuteReader("select " + fields + " from maintable A inner join " + TableName + " B on A.id=B.id where " + where + " " + desc, _p);
            bool flag = false;
            if (rs.Read())
            {
                for (int i = 0; i < rs.FieldCount; i++)
                {
                    string name = rs.GetName(i);
                    Field field = null;
                    try
                    {
                        field = Fields[name];
                    }
                    catch { }
                    if (field == null)
                    {
                        model[name] = rs[i];
                    }
                    else
                    {
                        model[name] = field.Convert(rs[i], Field.ConvertType.UserData);
                    }
                }
                flag = true;
            }
            rs.Close();
            if (!flag) return null;
            return model;
        }
        MySqlParameter[] GetParameter(Dictionary<string, object> p)
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
        /// <summary>
        /// 获取数据地址
        /// </summary>
        /// <param name="columnId">数据所属栏目</param>
        /// <returns></returns>
        void ReplaceUrl(double columnId, double dataId)
        {
            MWMS.DAL.TableHandle column = new MWMS.DAL.TableHandle("class");
            Dictionary<string, object> columnModel = column.GetModel(columnId, "dirPath,dirName,rootId");
            Dictionary<string, object> channelModel = column.GetModel(columnModel["rootId"].ToDouble(), "dirName");
            //StringBuilder url = new StringBuilder(BaseConfig.contentUrlTemplate);
            StringBuilder url = new StringBuilder("/$column.dirPath/$id.$extension");
            //url.Replace("$id", "'+convert(varchar(20),convert(decimal(18,0),id))+'");
            url.Replace("$id", "',id,'");
            url.Replace("$create.year", "'+convert(varchar(4),year(createdate))+'");
            url.Replace("$create.month", "'+right('00'+cast(month(createdate) as varchar),2)+'");
            url.Replace("$create.day", "'+right('00'+cast(day(createdate) as varchar),2)+'");
            url.Replace("$column.dirPath", columnModel["dirPath"].ToStr());
            url.Replace("$column.dirName", columnModel["dirName"].ToStr());
            url.Replace("$channel.dirName", channelModel["dirName"].ToStr());
            url.Replace(".$extension", "");
            string sql = "update maintable set url=CONCAT('" + url + "') where id=@id";
            Sql.ExecuteNonQuery(sql, new MySqlParameter[] { new MySqlParameter("id", dataId) });
        }
        /*
        public Dictionary<string, object> RequestToModel(HttpRequest request)
        {
            Dictionary<string, object> model = new Dictionary<string, object>();
            foreach (var field in Fields)
            {
                if (request.Form[field.Key] != null)
                {
                    model[field.Key] = request.Form[field.Key];
                }
            }
            return model;
        }*/
        public double Save(Field[] list)
        {

            Dictionary<string, object> fields = new Dictionary<string, object>();
            for (int i = 0; i < list.Length; i++)
            {
                fields[list[i].name] = list[i].value;
            }
            return Save(fields);
        }
        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="model">数据模型</param>
        /// <returns></returns>
        public double Save(Dictionary<string, object> model)
        {
            if (TableName == "") throw new Exception("表名不能为空");
            Dictionary<string, object> mainFields = new Dictionary<string, object>();
            Dictionary<string, object> dataFields = new Dictionary<string, object>();
            ColumnConfig config = null;
            if (model.ContainsKey("classId"))
            {
                config = GetConfig(model["classId"].ToDouble());
            }
            foreach (var field in model)
            {
                try
                {
                    Field f = Fields[field.Key];
                    object value = null;
                    if (f.type == "Pictures")
                    {
                        FieldType.Pictures files = FieldType.Pictures.Parse(field.Value.ToStr());
                        for (int i = 0; i < files.Count; i++)
                        {
                            string kzm = "";
                            if (files[i].path.LastIndexOf(".") > -1) kzm = files[i].path.Substring(files[i].path.LastIndexOf(".") + 1);
                            MWMS.Helper.Picture pic = new Helper.Picture(files[i].path);
                            FileInfo _newfile=pic.PictureSize(new FileInfo(files[i].path.Replace("." + kzm, "_min." + kzm)), config.picWidth, config.picHeight, 100, config.picForce);
                            string newfile = _newfile.FullName;
                            if (newfile == "")
                            {
                                files[i].minPath = files[i].path;
                            }
                            else
                            {
                                files[i].minPath = newfile;
                            }
                        }
                        value = files.ToJson();
                    }
                    else
                    {
                        value = f.Convert(field.Value, Field.ConvertType.SqlData);
                    }
                    if (value != null)
                    {
                        if (f.isPublicField)
                        {
                            mainFields[field.Key] = value;
                        }
                        else
                        {
                            dataFields[field.Key] = value;
                        }
                    }
                }
                catch
                {

                }
            }
            if (mainFields.ContainsKey("id")) dataFields["id"] = mainFields["id"];

            StringBuilder fieldstr = new StringBuilder();
            MWMS.DAL.TableHandle t = new MWMS.DAL.TableHandle("maintable");
            MWMS.DAL.TableHandle t1 = new MWMS.DAL.TableHandle(TableName);
            double id = 0;
            MWMS.DAL.TableHandle column = new MWMS.DAL.TableHandle("class");
            if (mainFields.ContainsKey("classId"))
            {
                Dictionary<string, object> columnModel = column.GetModel((double)mainFields["classId"], "rootId,moduleId");
                mainFields["rootId"] = columnModel["rootId"];
                mainFields["moduleId"] = columnModel["moduleId"];
            }
            mainFields["datatypeId"] = DatatypeId;
            if (mainFields.ContainsKey("id") && mainFields["id"].ToDouble() > 0)
            {
                mainFields["updateDate"] = DateTime.Now;
                t.Update(mainFields);
                id = t1.Update(dataFields);
            }
            else
            {
                id = double.Parse(Helper.Tools.GetId());
                mainFields["id"] = id;
                mainFields["auditorid"] = 0;
                mainFields["updateDate"] = DateTime.Now;
                mainFields["createDate"] = DateTime.Now;
                dataFields["id"] = id;
                t.Append(mainFields);
                id = t1.Append(dataFields);
            }
            if (mainFields.ContainsKey("classId")) ReplaceUrl(mainFields["classId"].ToDouble(), mainFields["id"].ToDouble());
            return id;
        }

        public void Remove(double id)
        {
            MWMS.DAL.TableHandle maintable = new MWMS.DAL.TableHandle("maintable");
            maintable.Remove(id);
            MWMS.DAL.TableHandle datatable = new MWMS.DAL.TableHandle(TableName);
            datatable.Remove(id);
        }
        public void Recycle(double id)
        {
            Sql.ExecuteNonQuery("update maintable set orderId=-3 where id=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
        }
        ColumnConfig GetConfig(double id)
        {
            bool inherit = false;
            double classId = 0, moduleId = 0;
            string parentId = "";
            ColumnConfig config = new ColumnConfig();
            MySqlDataReader rs = Sql.ExecuteReader("select thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,inherit,classId,parentId,moduleId,titleRepeat,watermark from class where id=@id", new MySqlParameter[]{
                new MySqlParameter("id",id)
            });
            if (rs.Read())
            {
                inherit = rs.GetInt32(4) == 1;
                config.picForce = rs.GetInt32(2) == 1;
                config.picSave = rs.GetInt32(3) == 1;
                config.picWidth = rs.GetInt32(0);
                config.picHeight = rs.GetInt32(1);
                classId = rs.GetDouble(5);
                parentId = rs.GetString(6);
                moduleId = rs.GetDouble(7);
                config.titleRepeat = (rs.IsDBNull(8) || rs.GetInt32(8) == 1);
                config.isRoot = rs.GetDouble(5) == 7;
                config.isColumn = rs.GetDouble(5) != 7;
                config.isModule = false;
                config.pId = id;
                config.watermarkFlag = rs.IsDBNull(9) || rs.GetInt32(9) == 1;
            }
            rs.Close();
            if (inherit)
            {
                string sql = "";
                if (classId == 7)
                {

                    rs = Sql.ExecuteReader("select thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,titleRepeat,watermark from module where id=@moduleId", new MySqlParameter[] { new MySqlParameter("moduleId", moduleId) });
                    if (rs.Read())
                    {
                        config.picForce = rs.GetInt32(2) == 1;
                        config.picSave = rs.GetInt32(3) == 1;
                        config.picWidth = rs.GetInt32(0);
                        config.picHeight = rs.GetInt32(1);
                        config.titleRepeat = (rs.IsDBNull(4) || rs.GetInt32(4) == 1);
                        config.isModule = true;
                        config.isRoot = false;
                        config.isColumn = false;
                        config.pId = moduleId;
                        config.watermarkFlag = rs.IsDBNull(5) || rs.GetInt32(5) == 1;

                    }
                    rs.Close();
                }
                else
                {
                    sql = "select thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,titleRepeat,classId,childId,id,watermark from class where id in (" + parentId + ")  and inherit=0  order by layer desc ";
                    bool flag = false;
                    rs = Sql.ExecuteReader(sql);
                    if (rs.Read())
                    {
                        flag = true;
                        config.picForce = rs.GetInt32(2) == 1;
                        config.picSave = rs.GetInt32(3) == 1;
                        config.picWidth = rs.GetInt32(0);
                        config.picHeight = rs.GetInt32(1);
                        config.titleRepeat = (rs.IsDBNull(4) || rs.GetInt32(4) == 1);
                        config.isRoot = rs.GetDouble(5) == 7;
                        config.isColumn = rs.GetDouble(5) != 7;
                        config.isModule = false;
                        config.childId = rs.GetString(6);
                        config.pId = rs.GetDouble(7);
                        config.watermarkFlag = rs.IsDBNull(8) || rs.GetInt32(8) == 1;

                    }
                    rs.Close();
                    if (!flag)//从模块中查找配制
                    {

                        rs = Sql.ExecuteReader("select thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,titleRepeat,watermark from module where id=@moduleId", new MySqlParameter[] { new MySqlParameter("moduleId", moduleId) });
                        if (rs.Read())
                        {
                            config.picForce = rs.GetInt32(2) == 1;
                            config.picSave = rs.GetInt32(3) == 1;
                            config.picWidth = rs.GetInt32(0);
                            config.picHeight = rs.GetInt32(1);
                            config.titleRepeat = (rs.IsDBNull(4) || rs.GetInt32(4) == 1);
                            config.isModule = true;
                            config.isRoot = false;
                            config.isColumn = false;
                            config.pId = moduleId;
                            config.watermarkFlag = rs.IsDBNull(5) || rs.GetInt32(5) == 1;
                        }
                        rs.Close();
                    }
                }
                return config;
            }
            else
            {
                return config;
            }
        }
    }
    public class ColumnConfig
    {
        public int picWidth = 0;
        public int picHeight = 0;
        public bool picForce = false;//图片剪裁
        public bool picSave = true;//是否保存远程图片
        public bool watermarkFlag = true;//是否加水印
        public bool titleRepeat = true;//标题是否可以重复
        public bool isModule = false;
        public bool isRoot = false;
        public bool isColumn = false;
        public double pId = -1;
        public string childId = "";
    }
}
