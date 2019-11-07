using MWMS;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace M5.Common
{
    public class ReturnPageData
    {
        public int pageSize = BaseConfig.managePageSize;
        public int recordCount = 0;
        public int pageNo = 0;
        public int pageCount = 0;
        public ArrayList data = new ArrayList();
    }
    public class TableInfo
    {
        public List<FieldInfo> fields = new List<FieldInfo>();
        public int width = 0;
        public string tableName = "";
        string titleField = "title";
        string[] displayField = null;
        string publicFieldStr = "id-数据ID-Double--15----否---------|classId-数据所属栏目-Double------否---------|createDate-创建时间-Date--20----否---------|updateDate-修改时间-Date--20----否---------|userId-用户ID-Double------否---------|orderId-排序至顶-Long------否---------|title-标题-String-0-30-TextBox---是---------|attribute-属性-String-0-10-TextBox---是---------|auditorId-审核者-Double------否---------|auditDate-审核时间-Date--20----否---------|auditMsg-审核信息-string--20----否---------|moduleId-模块id-Double------否---------|rootId-根id-Double------否---------|datatypeId-类型-Double------否---------|pic-图片-String-0-30-TextBox---是---------|skinId-皮肤-Double------否---------";
        public TableInfo(double id)
        {
            string tableStructure = "";
            MySqlDataReader rs = Sql.ExecuteReader("select TableName,TableStructure,displayField from datatype where id=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            if (rs.Read())
            {
                tableName = rs[0].ToString();
                tableStructure = rs[1].ToString();
                displayField = rs[2].ToString().Split(',');
                //fieldList_Sql = rs[2].ToString(); 
                //dataTypeId = rs.GetDouble(3);
            }
            rs.Close();
            if (id > 00) getFields(publicFieldStr, true);
            getFields(tableStructure, false);
        }
        public void getFields(string structure, bool flag)
        {
            string[] TS = structure.Split('|');
            for (int n = 0; n < TS.Length; n++)
            {
                if (TS[n] != "")
                {
                    string[] FL = TS[n].Split('-');
                    FieldInfo info = new FieldInfo();
                    info.name = FL[0];
                    info.text = FL[1];
                    info.type = FL[2];
                    if (FL[3] != "") info.minLenth = int.Parse(FL[3]);
                    if (FL[4] != "") info.maxLenth = int.Parse(FL[4]);
                    if (FL[6] != "") info.format = FL[6];
                    info.control = FL[5];
                    if (info.name == "id") info.isNecessary = true;
                    if (info.name == "orderId") info.isNecessary = true;
                    if (info.name == "auditMsg") info.isNecessary = true;
                    if (info.name == titleField)
                    {
                        info.visible = true;
                        info.isTitle = true;
                    }
                    info.isPublicField = flag;
                    for (int i = 0; i < displayField.Length; i++)
                    {
                        if (displayField[i].ToLower() == info.name.ToLower())
                        {
                            info.visible = true;
                            i = displayField.Length;
                        }
                    }
                    int findex = 0;
                    for (int i = 0; i < fields.Count; i++)
                    {
                        if (fields[i].name == info.name) findex = i;
                    }
                    if (findex > 0) fields[findex] = info;
                    else { fields.Add(info); }

                }
            }
        }
        public ReturnPageData getDataList(double moduleId=0, double classId=0, int pageNo=1, string orderBy="", int sortDirection=0, string where="")
        {
            orderBy = orderBy + "";
            ReturnPageData r = new ReturnPageData();
            string fieldStr = "";
            bool attachedFlag = false;//是否有附加字段
            bool showClassId = false, showUserId = false, showAuditorId = false;
            foreach (FieldInfo f in fields)
            {
                if (f.visible || f.isNecessary)
                {
                    if (fieldStr != "") fieldStr += ",";
                    if (f.name == "attribute")
                    {
                        //sqlserver
                        //fieldStr += "(convert(varchar(6),A.orderid)+','+convert(varchar(1),A.attr0)+','+convert(varchar(1),A.attr1)+','+convert(varchar(1),A.attr2)+','+convert(varchar(1),A.attr3)+','+convert(varchar(1),A.attr4)) attribute";
                        //mysql
                        fieldStr += "CONCAT(A.orderid,',',A.attr0,',',A.attr1,',',A.attr2,',',A.attr3) attribute";

                    }
                    else if (f.name == "classId")
                    {
                        fieldStr += "C.className";
                        showClassId = true;
                    }
                    else if (f.name == "userId")
                    {
                        fieldStr += "D.uname";
                        showUserId = true;
                    }
                    else if (f.name == "auditorId")
                    {
                        fieldStr += "E.uname auditor";
                        showAuditorId = true;
                    }
                    else
                    {
                        fieldStr += (f.name.IndexOf("u_") == -1 ? "A." : "B.") + f.name;
                    }
                    if (!f.isPublicField) attachedFlag = true;
                }
            }
            if (where.IndexOf("u_") > -1) attachedFlag = true;
            string orderStr = "order by A.orderid desc,A.createdate desc";
            if (orderBy != "")
            {
                orderStr = "order by " + ((orderBy.IndexOf("u_") == -1) ? "A." : "B.") + orderBy + " " + (sortDirection == 1 ? "desc" : "");
            }
            string mainWhere = "";
            if (moduleId == classId)
            {
                mainWhere = " A.moduleId=" + moduleId.ToString() + " ";
            }
            else
            {
                string childId = ColumnClass.getChildId(classId);
                mainWhere = " A.classId in (" + childId + ") ";
            }

            string countSql = "select count(1) from maintable A ";
            string sql2 = "";
            //sqlserver
            //sql2 = "select " + fieldStr + ",ROW_NUMBER() Over(" + orderStr + ") as rowNum from maintable A";
            //mysql
            sql2 = "select " + fieldStr + "  from maintable A";
            if (attachedFlag)
            {
                sql2 += " left join " + tableName + " B on A.id=B.id ";
                countSql += " left join " + tableName + " B on A.id=B.id ";

            }

            r.recordCount = int.Parse(Sql.ExecuteScalar(countSql + " where" + mainWhere + where).ToString());
            //if (attachedFlag) sql2 += " on A.id=B.id ";
            if (showClassId) sql2 += " left join class C on A.classId=C.id ";
            if (showUserId) sql2 += " left join m_admin D on A.userId=D.id ";
            if (showAuditorId) sql2 += " left join m_admin E on A.auditorId=E.id ";

            sql2 += " where " + mainWhere + " " + where;
            string sql = "";//
            //sqlserver
            sql ="SELECT * FROM (" +
                sql2 +
                " ) as A where rowNum> " + ((pageNo - 1) * r.pageSize).ToString() + " and rowNum<" + (pageNo * r.pageSize + 1).ToString();
            //mysql
            sql = sql2+" limit " + ((pageNo - 1) * r.pageSize).ToString() + "," + r.pageSize.ToString();
            r.data = Sql.ExecuteArrayObj(sql);
            r.pageNo = pageNo;
            return r;
        }

        public static ReturnValue moveData(string ids, double classId)
        {
            ReturnValue info = new ReturnValue();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            ColumnInfo column = ColumnClass.get(classId);
            if (column == null)
            {
                info.errNo = -1;
                info.errMsg = "栏目不存在";
                return info;
            }
            ColumnInfo channel = ColumnClass.get(column.rootId);
            StringBuilder url = new StringBuilder(BaseConfig.contentUrlTemplate);
            url.Replace("$id", "'+convert(varchar(20),convert(decimal(18,0),id))+'");
            url.Replace("$create.year", "'+convert(varchar(4),year(createdate))+'");
            url.Replace("$create.month", "'+right('00'+cast(month(createdate) as varchar),2)+'");
            url.Replace("$create.day", "'+right('00'+cast(day(createdate) as varchar),2)+'");
            url.Replace("$column.dirPath", column.dirPath);
            url.Replace("$column.dirName", column.dirName);
            url.Replace("$channel.dirName", channel.dirName);
            url.Replace(".$extension", "");
            Sql.ExecuteNonQuery("update maintable set  classId=@classId,rootId=@rootId,moduleId=@moduleId,url='" + url + "'  where id in (" + ids + ")", new MySqlParameter[] { new MySqlParameter("classId", classId), new MySqlParameter("rootId", column.rootId), new MySqlParameter("moduleId", column.moduleId) });

            return info;
        }
        public static ReturnValue setAttr(string ids="", int type=0, bool flag=false)
        {
            ReturnValue info = new ReturnValue();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            if (type < 0 || type > 4)
            {
                info.errNo = -1;
                info.errMsg = "属性字段不存在";
                return info;
            }
            if (flag)
            {
                Sql.ExecuteNonQuery("update maintable set attr" + type.ToString() + "=1 where id in (" + ids + ")");
            }
            else
            {
                Sql.ExecuteNonQuery("update maintable set attr" + type.ToString() + "=0 where id in (" + ids + ")");
            }
            return info;
        }

        public static ReturnValue auditData(string ids, bool flag, UserInfo value)
        {
            return auditData(ids, flag, "", value);
        }
        public static ReturnValue auditData(string ids, bool flag, string auditMsg, UserInfo value)
        {
            ReturnValue info = new ReturnValue();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            if (flag)
            {

                Sql.ExecuteNonQuery("update maintable set orderId=0,auditDate=getdate(),auditorId=@auditorId where id in (" + ids + ") ", new MySqlParameter[] { new MySqlParameter("auditorId", value.id) });
            }
            else
            {
                Sql.ExecuteNonQuery("update maintable set orderId=-2,auditMsg=@auditMsg,auditorId=@auditorId,auditDate=getdate() where id in (" + ids + ") ", new MySqlParameter[] { new MySqlParameter("auditMsg", auditMsg), new MySqlParameter("auditorId", value.id) });
            }
            return info;
        }
        public static ReturnValue setTop(string ids, bool flag)
        {
            ReturnValue info = new ReturnValue();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            if (flag)
            {
                int newOrderId = 0;
                try
                {
                    newOrderId = (int)(Sql.ExecuteScalar("select max(orderId) from maintable where id in (" + ids + ") and orderId>-1"));
                }
                catch
                {
                }
                newOrderId++;
                Sql.ExecuteNonQuery("update maintable set orderId=" + newOrderId.ToString() + " where id in (" + ids + ") and orderId>-1");
            }
            else
            {
                Sql.ExecuteNonQuery("update maintable set orderId=0 where id in (" + ids + ") and orderId>-1");
            }
            return info;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="dataTypeId"></param>
        /// <param name="ids"></param>
        /// <param name="deleteFlag">是否物理删除</param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static ReturnValue delData(double dataTypeId, string ids, bool deleteFlag, UserInfo user)
        {
            ReturnValue info = new ReturnValue();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            string tablename = "";
            object value = Sql.ExecuteScalar("select tablename from datatype where id=@id", new MySqlParameter[]{
                new MySqlParameter("id",dataTypeId)
            });
            if (value == null)
            {
                info.errMsg = "表类型不存在";
                info.errNo = -1;
                return info;
            }
            tablename = (string)value;
            if (deleteFlag)
            {
                Sql.ExecuteNonQuery("delete from " + tablename + " where id in (" + ids + ")");
                Sql.ExecuteNonQuery("delete from maintable where id in (" + ids + ")");
            }
            else
            {
                Sql.ExecuteNonQuery("update  maintable set orderId=-3 where id in (" + ids + ")");
            }
            Tools.writeLog("1", "删除数据[" + tablename + "][" + ids + "]");
            return (info);
        }
        public static ReturnValue reductionData(string ids, UserInfo user)
        {
            ReturnValue info = new ReturnValue();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            Sql.ExecuteNonQuery("update  maintable set orderId=0 where id in (" + ids + ")");
            return (info);
        }
    }
}
