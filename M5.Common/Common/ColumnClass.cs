using MWMS;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M5.Common
{
    public class ColumnClass
    {
        public static ReturnValue del(double id, UserInfo user)
        {
            ReturnValue err = new ReturnValue();
            string childId = null;
            double classId = 0;
            string parentId = null;
            string dirName = null;
            string className = null;
            double rootId = 0;
            double moduleId = 0;
            double userId = 0;
            MySqlDataReader rs = Sql.ExecuteReader("select childId,classId,parentId,dirname,className,rootid,moduleId,userId from class where id=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            if (rs.Read())
            {

                childId = rs[0].ToString();
                classId = rs.GetDouble(1);
                parentId = rs[2].ToString();
                dirName = rs[3].ToString();
                className = rs[4].ToString();
                rootId = rs.GetDouble(5);
                moduleId = rs.GetDouble(6);
                userId = rs.GetDouble(7);
            }
            rs.Close();
            if (childId == null)
            {
                err.errNo = -1;
                err.errMsg = "模块不存在";
                return err;
            }
            Permissions p = null;
            if (classId < 8)
            {
                p = user.getModulePermissions(moduleId);
            }
            else
            {
                p = user.getColumnPermissions(classId);
            }
            if ((p.write & p.delete && userId == user.id) | p.all)//有上级栏目决定权限
            {
                rs = Sql.ExecuteReader("select savedatatype,id from class where  id in (" + childId + ")");
                while (rs.Read())
                {
                    object tablename = Sql.ExecuteScalar("select tablename from datatype where id=@id", new MySqlParameter[] { new MySqlParameter("id", rs.GetDouble(0)) });
                    if (tablename != null)
                    {
                        Sql.ExecuteNonQuery("delete from " + (string)tablename + " where id in (select id from maintable where classid=@id )", new MySqlParameter[] { new MySqlParameter("id", rs.GetDouble(1)) });
                        Sql.ExecuteNonQuery("delete from maintable where classid=@id", new MySqlParameter[] { new MySqlParameter("id", rs.GetDouble(1)) });
                    }
                }
                rs.Close();
                int Count = Sql.ExecuteNonQuery("delete from class where id in (" + childId + ")");
                if (Count > 0)
                {
                    if (classId != 7)
                    {
                        reset(rootId);
                    }
                    else
                    {
                        Sql.ExecuteNonQuery("delete from htmltemplate where classid=@id",
                            new MySqlParameter[] { new MySqlParameter("id", id) });
                    }
                }
                err.errNo = 0;
                Tools.writeLog("1", "删除栏目" + className);
            }
            else
            {
                err.errNo = -1;
                err.errMsg = "越权操作，删除栏目失败";
                Tools.writeLog("2", "越权操作，删除栏目" + className + "失败");
            }
            return err;
        }
        public static ColumnInfo get(double id)
        {
            ColumnInfo value = null;
            MySqlDataReader rs = Sql.ExecuteReader("select id,classId,moduleId,rootId,className,pddir,dirName,dirPath,layer,parentId,keyword,info,maxico,saveDataType,skinId,contentSkinId,_skinId,_contentSkinId,custom,thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,inherit,domainName,titleRepeat,watermark,_domainName from class where  id=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            if (rs.Read())
            {
                value = new ColumnInfo();
                value.id = rs.GetDouble(0);
                value.classId = rs.GetDouble(1);
                value.rootId = rs.GetDouble(3);
                value.moduleId = rs.GetDouble(2);
                value.className = rs[4] + "";
                value.pddir = rs[5] + "";
                value.dirName = rs[6] + "";
                value.dirPath = rs[7] + "";
                value.layer = rs.GetInt32(8);
                value.parentId = rs[9] + "";
                value.keyword = rs[10] + "";
                value.info = rs[11] + "";
                value.maxIco = rs[12] + "";
                value.saveDataType = rs.GetDouble(13);

                value.skinId = rs.IsDBNull(14) ? 0 : rs.GetDouble(14);
                value.contentSkinId = rs.IsDBNull(15) ? 0 : rs.GetDouble(15);

                value._skinId = rs.IsDBNull(16) ? 0 : rs.GetDouble(16);
                value._contentSkinId = rs.IsDBNull(17) ? 0 : rs.GetDouble(17);

                value.custom = rs[18] + "";
                value.thumbnailWidth = rs.GetInt32(19);
                value.thumbnailHeight = rs.GetInt32(20);
                value.thumbnailForce = rs.GetInt32(21);
                value.saveRemoteImages = rs.GetInt32(22);
                value.watermark = (rs.IsDBNull(26) || rs.GetInt32(26) == 1) ? 1 : 0;
                value.inherit = rs.GetInt32(23);
                value.domainName = rs[24] + "";
                value.titleRepeat = (rs.IsDBNull(25) || rs.GetInt32(25) == 1) ? 1 : 0;

                value._domainName = rs[27] + "";
            }
            rs.Close();
            return value;
        }
        public static ReturnValue editDirName(double id, string dirName, UserInfo user)
        {
            ReturnValue err = new ReturnValue();
            ColumnInfo info = ColumnClass.get(id);
            int count = int.Parse( Sql.ExecuteScalar("select count(1) from class where id<>@id and classId=@classId and dirName=@dirName", new MySqlParameter[]{
                new MySqlParameter("id",info.id),
                new MySqlParameter("classId",info.classId),
                new MySqlParameter("dirName",dirName.ToLower())
            }).ToString());
            if (count > 0)
            {
                err.errNo = -1;
                err.errMsg = "所在栏目下目录名已存在";
                return err;
            }
            Sql.ExecuteNonQuery("update class set dirName=@dirName where id=@id", new MySqlParameter[]{
                new MySqlParameter("id",id),
                new MySqlParameter("dirName",dirName)});
            return err;
        }
        public static void reset(double dataId, double rootId)
        {
            if (dataId == 7) return;
            Sql.ExecuteNonQuery("update class set dirpath='',childid='',parentId='',layer=0 where id=" + dataId.ToString());
            MySqlDataReader rs = Sql.ExecuteReader("select classid,dirname,dirpath from class where id=" + dataId.ToString());
            if (rs.Read())
            {
                if (rs.GetDouble(0) == 7)
                {
                    rootId = dataId;
                    string url = "/" + rs[1].ToString() + "/";
                    Sql.ExecuteNonQuery("update class set rootid=" + dataId + ",dirpath='" + rs[1].ToString() + "',url='" + url + "',childid='" + dataId.ToString() + "',parentId='" + dataId.ToString() + "',layer=0 where id=" + dataId.ToString());
                }
                else
                {
                    MySqlDataReader rs1 = Sql.ExecuteReader("select classid,dirname,parentId,dirpath,layer,pddir,moduleId from class where id=" + rs[0].ToString());
                    if (rs1.Read())
                    {
                        string Path = rs1[3].ToString() + "/" + rs[1].ToString();
                        string url = "/" + Path + "/";

                        Sql.ExecuteNonQuery("update class set dirpath='" + Path + "',childid='" + dataId.ToString() + "',parentId='" + rs1[2].ToString() + "," + dataId.ToString() + "',layer=" + rs1[4].ToString() + "+1,pddir='" + rs1[5].ToString() + "',rootid=" + rootId.ToString() + ",url='" + url + "',moduleId=" + rs1[6].ToString() + " where id=" + dataId.ToString());

                        if (rs1[2].ToString() != "") Sql.ExecuteNonQuery("update class set childid=CONCAT(childid,'," + dataId.ToString() + "'),moduleId=" + rs1[6].ToString() + " where id in (" + rs1[2].ToString() + ")");
                    }
                    rs1.Close();


                }
            }
            rs.Close();
            rs = Sql.ExecuteReader("select id from class where classid=" + dataId.ToString());
            while (rs.Read())
            {
                reset(rs.GetDouble(0), rootId);
            }
            rs.Close();

        }
        public static void reset(double dataId)
        {
            double rootId = dataId;
            MySqlDataReader rs = Sql.ExecuteReader("select rootId,classId from class where id=@id", new MySqlParameter[] { new MySqlParameter("id", dataId) });
            if (rs.Read())
            {
                if (rs.GetDouble(1) > 7) rootId = rs.GetDouble(0);
            }
            rs.Close();
            reset(dataId, rootId);
        }
        public static ReturnValue resetContentUrl(double id)
        {
            ReturnValue err = new ReturnValue();
            ColumnInfo column = ColumnClass.get(id);
            ColumnInfo channel = column.classId == 7 ? column : ColumnClass.get(column.classId);
            StringBuilder url = new StringBuilder(BaseConfig.contentUrlTemplate);
            url.Replace(".$extension", "");
            //url.Replace("$id", "'+convert(varchar(20),convert(decimal(18,0),id))+'");
            //url.Replace("$create.year", "'+convert(varchar(4),year(createdate))+'");
            //url.Replace("$create.month", "'+right('00'+cast(month(createdate) as varchar),2)+'");
            //url.Replace("$create.day", "'+right('00'+cast(day(createdate) as varchar),2)+'");
            url.Replace("$id", "',id,'");
            url.Replace("$create.year", "',DATE_FORMAT(createdate,'%Y'),'");
            url.Replace("$create.month", "',DATE_FORMAT(createdate,'%m'),'");
            url.Replace("$create.day", "',DATE_FORMAT(createdate,'%d'),'");
            url.Replace("$column.dirPath", column.dirPath);
            url.Replace("$column.dirName", column.dirName);
            url.Replace("$channel.dirName", channel.dirName);
            string sql = "update maintable set url=CONCAT('" + url + "'),rootId=@rootId,moduleId=@moduleId where classId=@classId";
            Sql.ExecuteNonQuery(sql, new MySqlParameter[] {
                new MySqlParameter("classId", id),
                new MySqlParameter("rootId",column.rootId),
                new MySqlParameter("moduleId",column.moduleId) });
            return err;
        }

        static int downFiles(ref string Content)
        {
            int count = 0;
            if (Content == null) return count;
            Regex r = new Regex(Config.tempPath + @"(\d){4}-(\d){2}\/(\d){5,20}.(.){3,3}", RegexOptions.IgnoreCase); //定义一个Regex对象实例
            MatchCollection mc = r.Matches(Content);
            for (int n = 0; n < mc.Count; n++)
            {
                FileInfo f = new FileInfo(Tools.MapPath("~" + mc[n].Value));
                if (f.Exists)
                {
                    string newdir = Config.uploadPath + System.DateTime.Now.ToString("yyyy-MM") + "/";
                    DirectoryInfo d = new DirectoryInfo(Tools.MapPath("~" + newdir));
                    if (!d.Exists) d.Create();
                    f.MoveTo(d.FullName + f.Name);
                    string filepath = newdir + f.Name;

                    Content = Content.Replace(mc[n].Value, filepath);
                }
            }

            return (count);
        }
        public static void add(ColumnInfo info, UserInfo user)
        {
            ReturnValue err = new ReturnValue();
            #region 验证
            if (info.className.Trim() == "")
            {
                throw new Exception("栏目名不能为空");
            }
            if (info.dirName == "")
            {
                throw new Exception("目录名不能为空");
            }


            #endregion
            if (info.id < 1) info.id = double.Parse(Tools.GetId());
            int count = int.Parse(Sql.ExecuteScalar("select count(1) from class where classId=@classId and (dirName=@dirName or className=@className)", new MySqlParameter[]{
                new MySqlParameter("classId",info.classId),
                new MySqlParameter("dirName",info.dirName.ToLower()),
                new MySqlParameter("className",info.className)
            }).ToString());
            if (count > 0)
            {
                throw new Exception("所在栏目下栏目名或目录名已存在");
            }

            downFiles(ref info.maxIco);
            Sql.ExecuteNonQuery("insert into class " +
                "(id,className,classId,moduleId,rootId,keyword,maxIco,skinId,contentSkinId,info,custom,thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,inherit,domainName,updateDate,createDate,userId,childId,pddir,dirPath,parentId,saveDataType,dirName,layer,orderId,titleRepeat,watermark,_domainName)" +
                "values" +
                "(@id,@className,@classId,@moduleId,@rootId,@keyword,@maxIco,@skinId,@contentSkinId,@info,@custom,@thumbnailWidth,@thumbnailHeight,@thumbnailForce,@saveRemoteImages,@inherit,@domainName,@updateDate,@createDate,@userId,@childId,@pddir,@dirPath,@parentId,@saveDataType,@dirName,@layer,0,@titleRepeat,@watermark,@_domainName)", new MySqlParameter[]{
                new MySqlParameter("id",info.id),
                new MySqlParameter("className",info.className),
                new MySqlParameter("classId",info.classId),
                new MySqlParameter("moduleId",info.moduleId),
                new MySqlParameter("rootId",info.rootId),
                new MySqlParameter("keyword",info.keyword),
                new MySqlParameter("maxIco",info.maxIco),
                new MySqlParameter("skinId",info.skinId),
                new MySqlParameter("contentSkinId",info.contentSkinId),
                new MySqlParameter("info",info.info),
                new MySqlParameter("custom",info.custom),
                new MySqlParameter("thumbnailWidth",info.thumbnailWidth),
                new MySqlParameter("thumbnailHeight",info.thumbnailHeight),
                new MySqlParameter("thumbnailForce",info.thumbnailForce),
                new MySqlParameter("saveRemoteImages",info.saveRemoteImages),
                new MySqlParameter("inherit",info.inherit),
                new MySqlParameter("domainName",info.domainName),
                new MySqlParameter("_domainName",info._domainName),
                new MySqlParameter("createDate",info.createDate),
                new MySqlParameter("updateDate",info.updateDate),
                new MySqlParameter("userId",user.id),
                new MySqlParameter("childId",info.childId),
                new MySqlParameter("dirPath",info.dirPath),
                new MySqlParameter("pddir",info.pddir),
                new MySqlParameter("parentId",info.parentId),
                new MySqlParameter("saveDataType",info.saveDataType),
                new MySqlParameter("dirName",info.dirName),
                new MySqlParameter("layer",info.layer),
                new MySqlParameter("titleRepeat",info.titleRepeat),
                new MySqlParameter("watermark",info.watermark)
                });
            RecordClass.addKeyword(info.id, info.keyword,0);
            reset(info.id);
        }
        public static ReturnValue edit(ColumnInfo info, UserInfo user)
        {
            ReturnValue err = new ReturnValue();
            #region 验证
            if (info.className.Trim() == "")
            {
                err.errNo = -1;
                err.errMsg = "栏目名不能为空";
                return err;
            }
            #endregion
            if (info.id > 0)
            {
                downFiles(ref info.maxIco);
                Sql.ExecuteNonQuery("update class set watermark=@watermark,className=@className,keyword=@keyword,maxIco=@maxIco,skinId=@skinId,contentSkinId=@contentSkinId,info=@info,custom=@custom,thumbnailWidth=@thumbnailWidth,thumbnailHeight=@thumbnailHeight,thumbnailForce=@thumbnailForce,saveRemoteImages=@saveRemoteImages,inherit=@inherit,domainName=@domainName,updateDate=@updateDate,saveDataType=@saveDataType,titleRepeat=@titleRepeat,_skinId=@_skinId,_contentSkinId=@_contentSkinId,_domainName=@_domainName where id=@id", new MySqlParameter[]{
                new MySqlParameter("id",info.id),
                new MySqlParameter("className",info.className),
                new MySqlParameter("keyword",info.keyword),
                new MySqlParameter("maxIco",info.maxIco),
                new MySqlParameter("skinId",info.skinId),
                new MySqlParameter("contentSkinId",info.contentSkinId),
                new MySqlParameter("_skinId",info._skinId),
                new MySqlParameter("_contentSkinId",info._contentSkinId),
                new MySqlParameter("info",info.info),
                new MySqlParameter("custom",info.custom),
                new MySqlParameter("thumbnailWidth",info.thumbnailWidth),
                new MySqlParameter("thumbnailHeight",info.thumbnailHeight),
                new MySqlParameter("thumbnailForce",info.thumbnailForce),
                new MySqlParameter("saveRemoteImages",info.saveRemoteImages),
                new MySqlParameter("inherit",info.inherit),
                new MySqlParameter("saveDataType",info.saveDataType),
                new MySqlParameter("domainName",info.domainName),
                new MySqlParameter("_domainName",info._domainName),
                new MySqlParameter("updateDate",info.updateDate),
                new MySqlParameter("titleRepeat",info.titleRepeat),
                new MySqlParameter("watermark",info.watermark)

                });
                //reset(info.id);

                RecordClass.addKeyword(info.id, info.keyword,0);
                err.userData = info.id;
            }
            else
            {
                add(info, user);
            }
            return err;
        }

        public static string getChildId(double id)
        {
            string childId = "";
            MySqlDataReader rs = Sql.ExecuteReader("select childId from class where  id=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            if (rs.Read())
            {
                childId = rs[0].ToString();
            }
            rs.Close();
            return childId;
        }

        public static ReturnValue move(double classId, double moduleId1, double classId1, UserInfo user)
        {
            ReturnValue err = new ReturnValue();
            if (classId == classId1)
            {
                err.errNo = -1;
                err.errMsg = "移动栏目不能为父栏目";
                return (err);
            }
            bool mtag2 = false, tag = true;
            double dataTypeId2 = -1;
            MySqlDataReader rs;
            rs = Sql.ExecuteReader("select type,saveDataType from module where id=@id", new MySqlParameter[] { new MySqlParameter("id", moduleId1) });
            if (rs.Read())
            {
                dataTypeId2 = rs.GetDouble(1);
                mtag2 = rs.GetBoolean(0);
                if (moduleId1 == classId1 && !mtag2) classId1 = 7;
            }
            rs.Close();
            if (dataTypeId2 == -1)
            {
                err.errNo = -1;
                err.errMsg = "目标模块不存在";
                return (err);
            }
            ColumnInfo column = ColumnClass.get(classId);
            Permissions p = null;
            #region 获取要移动栏目的父栏目权限
            if (column.classId == 7)
            {
                p = user.getModulePermissions(column.moduleId);
            }
            else
            {
                p = user.getColumnPermissions(column.classId);
            }
            if (!p.all)
            {
                err.errNo = -1;
                err.errMsg = "越权操作，移动栏目失败";
                return err;
            }
            #endregion
            #region 获取目标位置权限
            if (classId1 == 7)
            {
                p = user.getModulePermissions(moduleId1);
            }
            else
            {
                p = user.getColumnPermissions(classId1);
            }
            if (!p.all)
            {
                err.errNo = -1;
                err.errMsg = "越权操作，无目标栏目权限，移动栏目失败";
                return err;
            }
            #endregion
            #region 栏目移动
            if (column.saveDataType == dataTypeId2)
            {
                string where = "";
                if (classId1 != 7)
                {
                    rs = Sql.ExecuteReader("select rootid from class where id=@id", new MySqlParameter[] { new MySqlParameter("id", classId1) });
                    if (rs.Read()) where = ",rootid=" + rs[0].ToString();
                    rs.Close();
                }
                Sql.ExecuteNonQuery("update class set moduleid=" + moduleId1 + ",classid=" + classId1 + where + " where id=" + classId);
                ColumnClass.reset(column.rootId);//重置旧栏目结构

            }
            else
            {
                err.errMsg = "数据类型不匹配";
                err.errNo = -1;
                tag = false;//移动失败，类型不匹配
            }
            #endregion
            return (err);
        }
        public static ColumnConfig getConfig(double id)
        {
            bool inherit = false;
            double classId = 0, moduleId = 0;
            string parentId = "";
            ColumnConfig config = new ColumnConfig();
            MySqlDataReader rs = Sql.ExecuteReader("select thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,inherit,classId,parentId,moduleId,titleRepeat,watermark,childId from class where id=@id", new MySqlParameter[]{
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
    public class ColumnInfo
    {
        double _id = -1;
        public double id
        {
            get { return _id; }
            set
            {
                _id = value;
                if (rootId == -1) rootId = value;
                childId = value.ToString();
            }
        }
        /// <summary>
        /// 父级栏目id
        /// </summary>
        public double classId = 0;
        /// <summary>
        /// 所属模块id
        /// </summary>
        public double moduleId = 0;
        /// <summary>
        /// 根极栏目id
        /// </summary>
        public double rootId = -1;
        /// <summary>
        /// 子栏目id
        /// </summary>
        public string childId = "";
        /// <summary>
        /// 栏目名称
        /// </summary>
        public string className = "";
        public double saveDataType = -1;
        public string pddir = "";
        string _dirName = "";
        /// <summary>
        /// 目录名称
        /// </summary>
        public string dirName
        {
            get { return _dirName; }
            set
            {
                _dirName = value;
                dirPath = value;
            }
        }
        /// <summary>
        /// 关键词
        /// </summary>
        public string keyword = "";
        /// <summary>
        /// 备注
        /// </summary>
        public string info = "";
        /// <summary>
        /// 目录对应的路径
        /// </summary>
        public string dirPath = "";
        /// <summary>
        /// 栏目图片
        /// </summary>
        public string maxIco = "";
        /// <summary>
        /// 栏目层级
        /// </summary>
        public int layer = 1;
        public string parentId = "";
        public DateTime createDate = System.DateTime.Now;
        public DateTime updateDate = System.DateTime.Now;
        /// <summary>
        /// 栏目模板
        /// </summary>
        public double skinId = 0;
        /// <summary>
        /// 内容模板
        /// </summary>
        public double contentSkinId = 0;
        /// <summary>
        /// 栏目模板
        /// </summary>
        public double _skinId = 0;
        /// <summary>
        /// 内容模板
        /// </summary>
        public double _contentSkinId = 0;
        /// <summary>
        /// 扩展字段
        /// </summary>
        public string custom = "";
        /// <summary>
        /// 栏目图片宽度
        /// </summary>
        public int thumbnailWidth = 0;
        /// <summary>
        /// 栏目图片高度
        /// </summary>
        public int thumbnailHeight = 0;
        /// <summary>
        /// 栏目图片是否剪裁（0 否 1是）
        /// </summary>
        public int thumbnailForce = 0;
        /// <summary>
        /// 是否保存远程图片（0 否 1是）
        /// </summary>
        public int saveRemoteImages = 1;
        /// <summary>
        /// 是否加水印
        /// </summary>
        public int watermark = 1;
        /// <summary>
        /// 是否继承（0 否 1是）
        /// </summary>
        public int inherit = 1;
        /// <summary>
        /// 栏目域名
        /// </summary>
        public string domainName = "";
        public string _domainName = "";
        public int titleRepeat = 1;//是否允许标题重复
        public bool isRoot
        {
            get { return classId == 7; }
        }
        public ArrayList getTemplateList(int type, bool isMobile)
        {
            ArrayList list = Sql.ExecuteArray("select id value,title text from template  where classid in (0,@moduleId,@rootId) and u_type=@type and u_defaultFlag=0 and u_webFAid=@webFAId", new MySqlParameter[]{
                new MySqlParameter("moduleId",moduleId),
                new MySqlParameter("rootId",rootId),
                new MySqlParameter("type",type),
                new MySqlParameter("webFAId",isMobile?1:0),
            });
            return list;
        }
    }
}
