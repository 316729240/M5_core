using Helper;
using MWMS.DAL.Table;
using MWMS.Helper.Extensions;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
namespace MWMS.DAL.Datatype.Table
{
    public class Column: BaseColumn
    {
        /// <summary>
        /// 栏目目录
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 所属模块id
        /// </summary>
        public double ModuleId { get; set; }
        public Column(double columnId): base(columnId)
        {

        }
        public override void SetAttrubite(double id)
        {
            Dictionary<string, object> model = this.GetModel(id);
            if (model == null) throw new Exception("栏目不存在");
            Id = id;
            ParentId = model["classId"].ToDouble();
            Name = model["className"].ToStr();
            MnemonicName = model["dirName"].ToStr();
            Picture = model["maxico"].ToStr();
            RootId = model["rootId"].ToDouble();
            OrderID = model["orderID"].ToInt();
            Layer = model["Layer"].ToInt();
            Info = model["info"].ToStr();
        }
        public ColumnConfig GetConfig()
        {
            bool inherit = false;
            double classId = 0, moduleId = 0;
            string parentId = "";
            ColumnConfig config = new ColumnConfig();
            MySqlDataReader rs = Sql.ExecuteReader("select thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,inherit,classId,parentId,moduleId,titleRepeat,watermark,childId from class where id=@id", new MySqlParameter[]{
                new MySqlParameter("id",Id)
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
                config.pId = Id;
                config.watermarkFlag = rs.IsDBNull(9) || rs.GetInt32(9) == 1;
                config.childId = rs.GetString(10);
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

        public static Column Get(double columnId)
        {
            try
            {
                return new Column(columnId);
            }
            catch
            {
                return null;
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
