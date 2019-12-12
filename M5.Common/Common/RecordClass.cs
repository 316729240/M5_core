using System;
using System.Collections.Generic;
using System.Text;
using Helper;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using MWMS.SqlHelper;
using MWMS.Helper;
using MySql.Data.MySqlClient;
using MWMS.Helper.Extensions;

namespace M5.Common
{

    public class RecordClass
    {
        Hashtable fields = new Hashtable();
        //List <MySqlParameter> fields = new List<MySqlParameter>();
        DataTable Data = new DataTable(), Pics = new DataTable();
        ArrayList ThumbnailFields = new ArrayList(), OldPicList = new ArrayList();
        public string _Sql = null;
        public string tableName = null;
        public string keyword = "", wordHtml = "";
        public float orderId = 0;
        ColumnInfo parentColumn = null;//数据上级栏目
        public string OwnerId = "-1";//数据所有者
        public string title = "";
        string defaultPic = "";//默认图
        Picture.thumbnailSize size;
        UserInfo _user = null;
        public RecordClass(UserInfo user)
        {
            _user = user;
            //Data.Columns.Add("FieldName");
            //Data.Columns.Add("FieldValue");
            //Pics.Columns.Add("url");
            //Pics.Columns.Add("tag");
            //ThumbnailFields.Columns.Add("FieldName");
        }
        TableInfo table = null;
        public RecordClass(double datatypeId, UserInfo user)
        {
            table = new TableInfo(datatypeId);
            tableName = table.tableName;
            _user = user;
            //Data.Columns.Add("FieldName");
            //Data.Columns.Add("FieldValue");
            //Pics.Columns.Add("url");
            //Pics.Columns.Add("tag");
            //ThumbnailFields.Columns.Add("FieldName");
        }
        public static void resetUrl(double columnId, double id){
            ColumnInfo column = ColumnClass.get(columnId);
            ColumnInfo channel = ColumnClass.get(column.rootId);
            StringBuilder url = new StringBuilder(BaseConfig.contentUrlTemplate);
            /*
            url.Replace("$id", "'+convert(varchar(20),convert(decimal(18,0),id))+'");
            url.Replace("$create.year", "'+convert(varchar(4),year(createdate))+'");
            url.Replace("$create.month", "'+right('00'+cast(month(createdate) as varchar),2)+'");
            url.Replace("$create.day", "'+right('00'+cast(day(createdate) as varchar),2)+'");
            url.Replace("$column.dirPath", column.dirPath);
            url.Replace("$column.dirName", column.dirName);
            url.Replace("$channel.dirName", channel.dirName);
            url.Replace(".$extension", "");
            string sql = "update maintable set url='" + url + "' where id=@id";
            */

            url.Replace("$id", "',id,'");
            url.Replace("$create.year", "',DATE_FORMAT(createdate,'%Y'),'");
            url.Replace("$create.month", "',DATE_FORMAT(createdate,'%m'),'");
            url.Replace("$create.day", "',DATE_FORMAT(createdate,'%d'),'");
            url.Replace("$column.dirPath", column.dirPath);
            url.Replace("$column.dirName", column.dirName);
            url.Replace("$channel.dirName", channel.dirName);
            url.Replace(".$extension", "");
            string sql = "update maintable set url=CONCAT('" + url + "') where id=@id";
            Sql.ExecuteNonQuery(sql, new MySqlParameter[] { new MySqlParameter("id", id) });
        }
        public RecordClass(string _tableName)
        {
            this.tableName = _tableName;
        }
        List<string> pics = new List<string>();
        #region 取得正文中的文件地址并保存
        int downFiles(ref string Content)
        {
            
            Regex r = new Regex(Config.tempPath+@"(\d){4}-(\d){2}\/(\d){5,20}.(.){3,3}", RegexOptions.IgnoreCase); //定义一个Regex对象实例
            MatchCollection mc = r.Matches(Content);
            for (int n = 0; n < mc.Count; n++)
            {
                FileInfo f = new FileInfo(Tools.MapPath("~"+mc[n].Value));
                if (f.Exists)
                {
                    string newdir = Config.uploadPath + System.DateTime.Now.ToString("yyyy-MM/");
                    DirectoryInfo d = new DirectoryInfo(Tools.MapPath("~"+newdir));
                    if (!d.Exists) d.Create();
                    f.MoveTo(d.FullName+f.Name);
                    string filepath = newdir + f.Name;
                    #region 设置缩略图
                    if (defaultPic == "") defaultPic = setThumbnail(newdir + f.Name, "");
                    #endregion
                    if (columnConfig.watermarkFlag)
                    {
                        filepath= Lib.Watermark(newdir + f.Name);
                    }
                    pics.Add(filepath);
                    Content = Content.Replace(mc[n].Value, filepath);
                }
            }
            int count = 0;
            r = new Regex(@"((http|https|ftp|rtsp|mms):(\/\/|\\\\){1}((\w)+[.]){1,}(net|com|cn|org|cc|tv|[0-9]{1,3})(\S*\/)((\S)+[.]{1}(gif|jpg|png|bmp|jpeg)))", RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(Content);
            string uploadPath =Config.webPath+ Config.uploadPath+System.DateTime.Now.ToString("yyyy-MM/");
            string dir = Tools.MapPath(uploadPath);
            for (int n = 0; n < mc.Count; n++)
            {
                try
                {

                    if (columnConfig.picSave)
                    {
                        string kzm = "jpg";
                        if (mc[n].Value.LastIndexOf(".") > -1) kzm = mc[n].Value.Substring(mc[n].Value.LastIndexOf(".") + 1);
                        string newFile = uploadPath + Tools.GetId() + "." + kzm;
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        Http.saveFile(mc[n].Value, Tools.MapPath(newFile));
                        #region 设置缩略图
                        if (defaultPic == "") defaultPic = setThumbnail(newFile, "");
                        #endregion
                        if (columnConfig.watermarkFlag)
                        {
                            newFile = Lib.Watermark(newFile);
                        }
                        pics.Add(newFile);
                        Content = Content.Replace(mc[n].ToString(), newFile);
                        count++;
                    }
                    else
                    {
                        pics.Add(mc[n].ToString());
                    }
                }
                catch
                {
                }

            }
            return (count);
        }
        #endregion
        public void addField(string fieldName, object fieldValue)
        {

            FieldInfo f = table.fields.Find(delegate (FieldInfo o) {
                return o.name.ToLower() == fieldName.ToLower();
            });
            if (f != null)
            {
                switch (f.type)
                {
                    case "Number":
                        try
                        {
                            fieldValue = int.Parse(fieldValue.ToString());
                        }
                        catch
                        {
                            return;
                        }
                        break;
                    case "Double":
                        try
                        {
                            fieldValue = double.Parse(fieldValue.ToString());
                        }
                        catch
                        {
                            return;
                        }
                        break;
                    case "DateTime":
                        try
                        {
                            fieldValue = DateTime.Parse(fieldValue.ToString());
                        }catch
                        {
                            return;
                        }
                        break;
                }

            }
            else
            {
                throw new Exception(f.name + "字段不存在");
            }
            if (String.Compare(fieldName, "classid", true) == 0)
            {
                parentColumn = ColumnClass.get((double)fieldValue);
            }
            else if (String.Compare(fieldName, "title", true) == 0)
            {
                title = (string)fieldValue;
            }
            else if (String.Compare(fieldName, "pic", true) == 0)
            {
                defaultPic = (string)fieldValue;
            }

            fields[fieldName.ToLower()] = new MySqlParameter(fieldName, fieldValue);
            //            fields.Add(new MySqlParameter(fieldName,fieldValue));
        }
        ColumnConfig columnConfig = null;
        #region 更新数据
        public double update(double id)
        {
            if (parentColumn == null)
            {
                throw new Exception("未指定栏目或栏目不存在");
            }
            columnConfig = ColumnClass.getConfig(parentColumn.id);
            if (!columnConfig.titleRepeat)//标题不允许重复
            {
                string where = "id<>@id and title=@title";
                if (columnConfig.isModule)
                {
                    where += " and moduleId=@moduleId";
                }
                else if (columnConfig.isRoot)
                {
                    where += " and rootId=@rootId";
                }
                else
                {
                    where += " and classId in (" + columnConfig.childId + ")";
                }
                int c =int.Parse( Sql.ExecuteScalar("select count(1) from maintable where " + where, new MySqlParameter[] {
                    new MySqlParameter("title",title),
                    new MySqlParameter("moduleId",parentColumn.moduleId),
                    new MySqlParameter("rootId",parentColumn.rootId),
                    new MySqlParameter("id",id)
                }).ToString());
                if (c > 0)
                {
                    throw new Exception("标题已存在");
                }
            }
            addField("updatedate", System.DateTime.Now);
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            this.addField("id", id);
            this.addField("moduleId", parentColumn.moduleId);
            this.addField("rootId", parentColumn.rootId);
            string field1 = "",field2="";
            int picFieldIndex = -1;
            foreach (var item in fields)
            {
                MySqlParameter p = (MySqlParameter)((DictionaryEntry)item).Value;
                if (p.ParameterName.IndexOf("u_") > -1)
                {
                    if (field2 != "") field2 += ",";
                    field2 += p.ParameterName + "=" + "@" + p.ParameterName;
                }
                else
                {
                    if (field1 != "") field1 += ",";
                    field1 += p.ParameterName + "=" + "@" + p.ParameterName;
                }
                if (p.Value!=null && p.Value.GetType().Name == "String")
                {
                    string value=(string)p.Value;
                    verificationWord(ref value);
                    downFiles(ref value);
                    p.Value = value;
                }
            }
            if(picFieldIndex>-1) setThumbnail((string)((MySqlParameter)fields[picFieldIndex]).Value, (string)((MySqlParameter)fields[picFieldIndex]).Value);

            int count=0;
            count = Sql.ExecuteNonQuery("update maintable set " + field1 + " where id=@id", fields);
            count = Sql.ExecuteNonQuery("update " + tableName + " set " + field2 + " where id=@id", fields);
            return (id);
        }
        #endregion

        #region 添加数据
        public double insert()
        {
            double id =double.Parse(Tools.GetId());
            return (insert(id));
        }
        public double insert(double id)
        {
            if (parentColumn == null)
            {
                throw new Exception("未指定栏目或栏目不存在");
            }
            columnConfig = ColumnClass.getConfig(parentColumn.id);
            #region 标题重复处理
            if (!columnConfig.titleRepeat)//标题不允许重复
            {
                string where = "title=@title";
                if (columnConfig.isModule)
                {
                    where += " and moduleId=@moduleId";
                }
                else if (columnConfig.isRoot)
                {
                    where += " and rootId=@rootId";
                }
                else
                {
                    where += " and classId in (" + columnConfig.childId+ ")";
                }
                int c=int.Parse( Sql.ExecuteScalar("select count(1) from maintable where "+where,new MySqlParameter[] {
                    new MySqlParameter("title",title),
                    new MySqlParameter("moduleId",parentColumn.moduleId),
                    new MySqlParameter("rootId",parentColumn.rootId)
                }).ToString());
                if( c > 0)
                {
                    throw new Exception("标题已存在");
                }
            }
            #endregion
            //if (classId != null) size = API.getThumbnailSize(classId);
            string  datatypeclassid = null;
            double dataTypeId = 0;
            MySqlDataReader rsv = Sql.ExecuteReader("select id,classid,tablestructure from datatype where tablename='" + tableName + "'");
            if (rsv.Read()) { dataTypeId = rsv.GetDouble(0); datatypeclassid = rsv[1].ToString(); }
            rsv.Close();
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            System.Text.StringBuilder str1 = new System.Text.StringBuilder();
            this.addField("id", id);
            this.addField("createdate", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            this.addField("updatedate", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            this.addField("datatypeId", dataTypeId);
            this.addField("moduleId", parentColumn.moduleId);
            this.addField("rootId", parentColumn.rootId);
            this.addField("userId", _user.id);
            //this.addField("orderId",orderId);
            
            string field1_1 = "", field1_2 = "", field2_1 = "id", field2_2 = "@id";
            int picFieldIndex = -1;
            foreach(var item in fields)
            {
                MySqlParameter p = (MySqlParameter)((DictionaryEntry)item).Value;
                    if (p.ParameterName.IndexOf("u_") > -1)
                    {
                        if (field2_1 != "") { field2_1 += ","; field2_2 += ","; }
                        field2_1 += p.ParameterName;
                        field2_2 += "@" + p.ParameterName;
                    }
                    else
                    {
                        //if (p.ParameterName == "pic") picFieldIndex = n;
                        if (field1_1 != "") { field1_1 += ","; field1_2 += ","; }
                        field1_1 += p.ParameterName;
                        field1_2 += "@" + p.ParameterName;
                    }
                    if (p.Value!=null && p.Value.GetType().Name == "String")
                    {
                        string value = (string)p.Value;
                         verificationWord(ref value);
                        downFiles(ref value);
                        p.Value = value;
                    }
            }
            if (fields["pic"] == null )//没有添加缩略图字段时
            {
                field1_1 += ",pic"; field1_2 += ",@pic"; addField("pic", defaultPic);
            }
            else
            {
                if ((string)((MySqlParameter)fields["pic"]).Value == "") ((MySqlParameter)fields["pic"]).Value = defaultPic;
                setThumbnail((string)((MySqlParameter)fields["pic"]).Value, (string)((MySqlParameter)fields["pic"]).Value);
            }
            int count=Sql.ExecuteNonQuery("insert into maintable (" + field1_1 + ")values(" + field1_2 + ")", fields);
            count=Sql.ExecuteNonQuery("insert into " + tableName + " (" + field2_1 + ")values(" + field2_2 + ")", fields);
            if (count > 0)
            {
                resetUrl(parentColumn.id, id);
            }
            return (id);
        }
        #endregion
        string setThumbnail(string fileName, string newFile)
        {
            if (fileName==null || fileName == "") return "";
            if (fileName.IndexOf("http://") == -1) {
                if (newFile == "") newFile = Config.webPath + Config.uploadPath + System.DateTime.Now.ToString("yyyy-MM/") + Tools.GetId() + ".jpg";
                Picture pic = new Picture(fileName);
                FileInfo picfile = new FileInfo(newFile);
                return pic.PictureSize(picfile, columnConfig.picWidth, columnConfig.picHeight, 100, columnConfig.picForce).ToString();
            }
            return fileName;
        }
        public int Exec()
        {
            int n = 0;
            n = Sql.ExecuteNonQuery(_Sql, Data);
            Data.Clear();
            return (n);
        }
        /// <summary>
        /// 敏感词过滤
        /// </summary>
        /// <param name="SensitiveKeyword"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        void verificationWord(ref string html)
        {
            for (int i = 0; i < BaseConfig.SensitiveKeyword.Length; i++)
            {
                string k = BaseConfig.SensitiveKeyword[i].Trim();
                if (k != "")
                {
                    if (html.IndexOf(k) > -1)
                    {
                        if (BaseConfig.replaceNull) {
                            html = html.Replace(k,"");
                        }
                        else {
                            throw new Exception("不能包含关键词'" + k + "'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="DataTypeID">数据类型</param>
        /// <param name="Items">数据ID列表</param>
        /// <param name="Tag">是否放入回收站</param>
        /// <returns>操作成功的记录数</returns>
        public static int Del(string DataTypeID, string Items, bool Tag)
        {
            return (Del(DataTypeID, Items, Tag, "-1"));
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="DataTypeID">数据类型</param>
        /// <param name="Items">数据ID列表</param>
        /// <param name="Tag">是否放入回收站</param>
        /// <param name="OwnerId">操作指定所有用户</param>
        /// <returns>操作成功的记录数</returns>
        public static int Del(string DataTypeID, string Items, bool Tag, string OwnerId)
        {
            int n = Sql.ExecuteNonQuery("exec del " + DataTypeID + ",'" + Items + "'," + (Tag ? "1" : "0") + "," + OwnerId);
            Tools.writeLog("1", "删除数据[" + DataTypeID + "][" + Items + "]");
            return (n);
        }
        public static int DelPD(string Items)
        {
            string DirName = null;
            MySqlDataReader rs = Sql.ExecuteReader("select dirname from class where id=" + Items);
            if (rs.Read()) DirName = rs[0].ToString(); rs.Close();
            int n = Del("0", Items, true);
            if (n > 0)
            {
                DirectoryInfo dirpath = new DirectoryInfo(Tools.MapPath("~/" + DirName + "/"));
                if (dirpath.Exists) dirpath.Delete(true);
            }
            return (n);
        }
        public static int Move(string DataTypeID, string Items, string ParentID, string OldParentID)
        {
            int n = 0;
            if (DataTypeID != "0") Sql.ExecuteNonQuery("exec move " + DataTypeID + ",'" + Items + "','" + ParentID + "','" + OldParentID + "'");
            {
                MySqlDataReader rs = null;
                string DirPath = null, parentpath = null, RootID = null;
                string OldDirPath = null;
                string Oldparentpath = null;
                string OldLayer = null;
                int Layer = 0;
                rs = Sql.ExecuteReader("select dirpath,parentpath,Layer from class where id=" + OldParentID);
                if (rs.Read())
                {
                    OldDirPath = rs[0].ToString();
                    Oldparentpath = rs[1].ToString();
                    OldLayer = rs[2].ToString();
                }
                rs.Close();

                #region 取得新父栏目的数据
                rs = Sql.ExecuteReader("select dirpath,parentpath,Layer,RootID from class where id=" + ParentID);
                if (rs.Read())
                {
                    DirPath = rs[0].ToString();
                    Layer = rs.GetInt32(2);
                    parentpath = rs[1].ToString();
                    RootID = rs[3].ToString();
                    if (Layer > 0) DirPath = DirPath + "_";
                }
                rs.Close();
                #endregion
                string V = "";
                rs = Sql.ExecuteReader("select childid,dirpath,parentpath from class where id in(" + Items + ")");
                while (rs.Read())
                {
                    string[] v = rs[0].ToString().Split(',');
                    for (int n1 = 0; n1 < v.Length; n1++)
                    {
                        #region 去除原父目录下的Childid信息
                        Sql.ExecuteNonQuery("update class set childid=replace(childid,'," + v[n1] + "','') where id in (" + Oldparentpath + ")");
                        if (V.IndexOf(v[n1]) == -1) V = V + "," + v[n1];
                        #endregion
                    }


                    Sql.ExecuteNonQuery("update class set parentpath=replace(parentpath,'" + Oldparentpath + ",','" + parentpath + ",'),dirpath=replace(dirpath,'" + OldDirPath + "','" + DirPath + "')  where id in (" + rs[0] + ")");
                    //Sql.ExecuteNonQuery("update class set dirpath=replace('" + OldDirPath + "',''),parentpath=replace('" + Oldparentpath + "',''),layer=" + (Layer + 1).ToString() + ",RootID=" + RootID + " where id in(" + parentpath + ")");
                }
                rs.Close();
                #region 在新父目录下加入Childid信息
                //sqlserver
                //Sql.ExecuteNonQuery("update class set childid=childid+'" + V + "' where id in (" + parentpath + ")");
                Sql.ExecuteNonQuery("update class set childid=CONCAT(childid,'" + V + "') where id in (" + parentpath + ")");
                
                #endregion
                #region 设置所移目录的层次,根目录,父目录
                Sql.ExecuteNonQuery("update class set RootID=" + RootID + ",classid=" + ParentID + " where id in(" + Items + ")");
                #endregion

            }
            return (n);
        }
        public static int Reduction(string DataTypeID, string Items)
        {
            int n = 0;
            n = Sql.ExecuteNonQuery("exec ReductionData " + DataTypeID + ",'" + Items + "'");
            return (n);
        }
        public static int EmptyRecycle(string DataTypeID)
        {
            int n = 0;
            n = Sql.ExecuteNonQuery("exec EmptyRecycle '" + DataTypeID + "'");
            return (n);
        }

        public static void addKeyword(double dataId, string keyword,double datatypeId)
        {
            if (keyword == null || keyword.Trim() == "") return;
           // keyword = keyword.Replace(" ", ",");
            keyword = keyword.Replace("，", ",");
            string[] k = keyword.Split(',');
            for (int i = 0; i < k.Length; i++)
            {
                k[i] = k[i].Trim();
                if (k[i] != "" && k[i].Length < 50)
                {
                    string pinyin = k[i].GetPinYin();
                    if (pinyin.Length > 10) { pinyin = k[i].GetPinYin( 2); }
                    if (pinyin.Length < 50)
                    {
                        pinyin = Regex.Replace(pinyin, @"[ .~!@#$%\^\+\*&\\\/\?\|:\.{}()';=\[\]" + "\"]", "");
                        Sql.ExecuteNonQuery("insert into indextable (dataId,keyword,pinyin)values(@dataId,@keyword,@pinyin)", new MySqlParameter[]{
                    new MySqlParameter("dataId",dataId),
                    new MySqlParameter("keyword",k[i]),
                    new MySqlParameter("pinyin",pinyin),
                    new MySqlParameter("datatypeId",datatypeId),
                });
                    }
                }
            }
        }


        public static bool MoveClass(string moduleID, string classID, string moduleID1, string classID1)
        {
            MySqlDataReader rs;
            bool mtag2 = false, tag = true;
            string datatypeid2 = null;

            if (moduleID == classID)
            {
                #region 模块移动
                if (moduleID == moduleID1) return (false);//模块不能相同
                rs = Sql.ExecuteReader("select type,datatypeid from module where id=" + moduleID1);
                if (rs.Read()) { datatypeid2 = rs[1].ToString(); mtag2 = rs.GetBoolean(0); }
                rs.Close();
                bool mtag = false;
                rs = Sql.ExecuteReader("select type from module where id=" + moduleID);
                if (rs.Read()) mtag = rs.GetBoolean(0);
                rs.Close();
                if (mtag)
                {
                    #region 真实栏目移动
                    tag = MoveClass(classID, moduleID1, classID1);
                    #endregion
                }
                else
                {
                    #region 虚拟栏目移动
                    rs = Sql.ExecuteReader("select id from class where classid=7 and moduleid=" + moduleID);
                    while (rs.Read())
                    {
                        tag = MoveClass(rs[0].ToString(), moduleID1, classID1);
                    }
                    rs.Close();
                    #endregion
                }
                Sql.ExecuteNonQuery("delete from module where id=" + moduleID);
                #endregion
            }
            else
            {
                tag = MoveClass(classID, moduleID1, classID1);
            }
            return (tag);
        }
        public static bool MoveClass(string classID, string moduleID1, string classID1)
        {
            if (classID == classID1) return (false);//移动错误 移动栏目不能为父栏目
            bool mtag2 = false, tag = true;
            string TableName = "", datatypeid2 = "";
            MySqlDataReader rs;
            rs = Sql.ExecuteReader("select type,datatypeid from module where id=" + moduleID1);
            if (rs.Read())
            {
                datatypeid2 = rs[1].ToString();
                mtag2 = rs.GetBoolean(0);
                if (moduleID1 == classID1 && !mtag2) classID1 = "7";
            }
            rs.Close();
            if (datatypeid2 == "") { tag = false; return (tag); }//目标模块不存在
            rs = Sql.ExecuteReader("select tablename from datatype where id=" + datatypeid2);
            if (rs.Read()) { TableName = rs[0].ToString(); }
            rs.Close();

            #region 栏目移动
            string datatypeid = null, childid = null, rootid = null, dirpath = null, dirname = null, parentid = null;
            rs = Sql.ExecuteReader("select savedatatype,childid,rootid,dirpath,dirname,classid from class where id=" + classID);
            if (rs.Read())
            {
                datatypeid = rs[0].ToString();
                childid = rs[1].ToString();
                rootid = rs[2].ToString();
                dirpath = rs[3].ToString();
                dirname = rs[4].ToString();
                parentid = rs[5].ToString();
            }
            //throw new NullReferenceException(rootid);
            rs.Close();
            if (datatypeid == datatypeid2)
            {
                string where = "";
                if (classID1 != "7")
                {
                    rs = Sql.ExecuteReader("select rootid from class where id=" + classID1);
                    if (rs.Read()) where = ",rootid=" + rs[0].ToString();
                    rs.Close();
                }
                //throw new NullReferenceException("update class set classid=" + classID1 + where + " where id=" + classID);
                Sql.ExecuteNonQuery("update class set classid=" + classID1 + where + " where id=" + classID);
                Sql.ExecuteNonQuery("update class set moduleid=" + moduleID1 + " where id in (" + childid + ")");

                #region 重置内容页路径

                    rs = Sql.ExecuteReader("select dirpath from class where id=" + classID);
                    if (rs.Read())
                    {
                        Sql.ExecuteNonQuery("update [" + TableName + "] set pddir=B.dirpath from [" + TableName + "] as A,class as B where A.classid=B.id and  A.classid in (" + childid + ")");
                    }
                    rs.Close();
                #endregion
            }
            else
            {
                tag = false;//移动失败，类型不匹配
            }
            #endregion
            return (tag);
        }
    }
}