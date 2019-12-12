using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Helper;
using M5;
using M5.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using MWMS.DAL.Datatype.FieldType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace M5.Main.Manager
{
    [LoginAuthorzation]
    public class SystemController : ManagerBase
    {
        public ReturnValue test()
        {
            //      MySqlDataReader rs= Sql.ExecuteReader("select * from class");
          
            //    rs.Close();
            MySqlDataReader rs = Sql.ExecuteReader("select * from class limit 0,1");

            rs.Close();
            Sql.ExecuteArray("select id,keyword from class");
            /*
            ArrayList rs = Sql.ExecuteArray("select id,keyword from class");
            for(int i = 0; i < rs.Count; i++)
            {
                Dictionary<string, object> value = (Dictionary<string, object>)rs[i];

                RecordClass.addKeyword(double.Parse( value["id"]+""),value["keyword"] +"",0);
                //rs[i]
            }
;*/
            Sql.ExecuteDataset("select id from class");
            return new ReturnValue();
        }
        //loginInfo.checkLogin();
        public ReturnValue exit()
        {
            
            loginInfo.exit();
            return new ReturnValue();
        }
        public ReturnValue saveCardLayout(string layout)
        {
            string path =Tools.MapPath("~" + Config.tempPath + @"user\" + loginInfo.value.id.ToString() + @"\");
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
            System.IO.File.WriteAllText(path + "cardLayout.config", layout);
            return new ReturnValue();
        }
        public ReturnValue saveDisplayField(double id,string fields)
        {
            Sql.ExecuteNonQuery("update datatype set displayField=@fields where id=@id", new MySqlParameter[]{
                new MySqlParameter("id",id),
                new MySqlParameter("fields",fields)
            });
            return new ReturnValue();
        }
        public ReturnValue fieldInfo(double id)
        {
            TableInfo t = new TableInfo(id);
            return new ReturnValue(t.fields);
        }
        public ReturnValue getCustomField(double classId)
        {
            object moduleId = Sql.ExecuteScalar("select moduleId from class where id=@id", new MySqlParameter[] { new MySqlParameter("id", classId) });
            if (moduleId != null)
            {
                object custom = Sql.ExecuteScalar("select custom from module where id=@id", new MySqlParameter[] { new MySqlParameter("id", moduleId) });
                string xml = "";
                if (custom != null) xml = custom.ToString();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<?xml version=\"1.0\"?>" + xml);
                return new ReturnValue(doc.ToJson());
            }
            return new ReturnValue();
        }
        public ReturnValue getUpdateLog(string dateTime)
        {
            string xml = Http.getUrl("http://" + ConfigurationManager.AppSettings["OfficialWeb"] + "/Updatelog.aspx?datetime=" + dateTime, Encoding.UTF8);
            XmlDocument reg = new XmlDocument();
            reg.LoadXml(xml);
            return new ReturnValue(reg);
        }
        public ReturnValue getUpdateFile(string dateTime)
        {
            string xml = Http.getUrl("http://" + ConfigurationManager.AppSettings["OfficialWeb"] + "/UpdateXML.aspx?datetime=" + dateTime, Encoding.UTF8);
            XmlDocument reg = new XmlDocument();
            reg.LoadXml(xml);
            return new ReturnValue(reg);
        }
        public ReturnValue downUpdateFile(string fileName)
        {
            ReturnValue err = new ReturnValue();
            string tempPath =Tools.MapPath("~" + Config.tempPath);
            if (!System.IO.Directory.Exists(tempPath)) System.IO.Directory.CreateDirectory(tempPath);
            try
            {
                Http.saveFile("http://" + ConfigurationManager.AppSettings["OfficialWeb"] + "/manage/files/" + fileName + ".zip?" + Tools.GetId(), tempPath + fileName + ".zip");
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            return err;
        }
        public ReturnValue updateSystem(string files,string fileIds,string types,string dateTime)
        {
            ReturnValue err = new ReturnValue();
            string tempPath = Tools.MapPath("~" + Config.tempPath);
            string[] _files = files.Split(',');
            string[] _fileIds = fileIds.Split(',');
            string[] _types = types.Split(',');
            try
            {
                for (int i = 0; i < fileIds.Length; i++)
                {
                    if (_fileIds[i].Trim() != "")
                    {
                        if (_types[i] == "0")
                        {
                            _files[i] = _files[i].Replace("[manage]", Config.managePath);
                            System.IO.FileInfo f = new System.IO.FileInfo(Tools.MapPath("~" + files[i]));
                            if (!f.Directory.Exists) f.Directory.Create();
                            System.IO.File.Copy(tempPath + fileIds[i] + ".zip", f.FullName, true);
                            System.IO.File.Delete(tempPath + fileIds[i] + ".zip");
                        }
                        else
                        {
                            string text = System.IO.File.ReadAllText(tempPath + fileIds[i] + ".zip", System.Text.Encoding.GetEncoding("gb2312"));
                            try
                            {
                                Sql.ExecuteNonQuery(text);
                            }
                            catch
                            {
                            }
                            System.IO.File.Delete(tempPath + fileIds[i] + ".zip");
                        }
                    }
                }
                #region 更新系统版本
                if (dateTime != "")
                {
                    string fileName = Tools.MapPath("~" + Config.configPath + "Version.xml");
                    XmlDocument doc2 = new XmlDocument();
                    doc2.Load(fileName);
                    doc2.DocumentElement.ChildNodes.Item(1).InnerText = dateTime;
                    doc2.DocumentElement.ChildNodes.Item(2).InnerText = "false";
                    doc2.Save(fileName);
                }
                #endregion
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            return err;
        }
        public ReturnValue checkUpdate()
        {

            ReturnValue err = new ReturnValue();
            string fileName = Tools.MapPath("~" + Config.configPath + "Version.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            string dateTime = doc.DocumentElement.ChildNodes.Item(1).InnerText;
            string flag = doc.DocumentElement.ChildNodes.Item(2).InnerText;
            if (flag == "true")
            {
                err.userData = dateTime.Trim();
                return err;
            }
            try
            {
                XmlDocument doc2 = new XmlDocument();
                doc2.Load("http://" + ConfigurationManager.AppSettings["OfficialWeb"] + "/updatexml.aspx?datetime=" + dateTime);
                XmlNodeList root2 = doc2.DocumentElement.ChildNodes;
                if (root2.Count > 0)
                {
                    doc.DocumentElement.ChildNodes.Item(2).InnerText = "true";
                    doc.Save(fileName);
                    err.userData = dateTime.Trim();
                }
                else
                {
                    err.userData = "";
                }
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = "检查升级失败，" + ex.Message;
            }
            return err;
        }
        public ReturnValue getUserList(int type)
        {
            ReturnValue info = new ReturnValue();
            if (type == 0)
            {
                info.userData = Sql.ExecuteArray("select id,1 type,name from role UNION " +
                    "select id,2 type,uname name from m_admin where status=1");
            }
            else if (type == 1)
            {
                info.userData = Sql.ExecuteArray("select id,1 type,name from role");
            }
            else if (type == 2)
            {
                info.userData = Sql.ExecuteArray("select id,2 type,uname name from m_admin where status=1");
            }
            return info;
        }
        public ReturnValue getPermissions(double classId)
        {

            ReturnValue info = new ReturnValue();
            info.userData = Sql.ExecuteArrayObj("select A.dataId,A.type,B.name,p0,p1,p2,p3,p4 from permissions A inner join role B  on A.dataId=B.id where A.classId=@classId " +
            " union " +
            " select A.dataId,A.type,B.uname name,p0,p1,p2,p3,p4 from permissions A inner join m_admin B  on A.dataId=B.id where A.classId=@classId",
            new MySqlParameter[]{
                new MySqlParameter("classId", classId)
            });
            return info;
        }
        public ReturnValue setPermissions(double classId,string permissions)
        {
            ReturnValue info = new ReturnValue();
            if (!loginInfo.value.isAdministrator)
            {
                info.errMsg = "没有权限";
                info.errNo = -1;
                return info;
            }
    string[] _permissions = permissions.Split('\n');
    MySqlParameter[] p = new MySqlParameter[]{
                new MySqlParameter("classId",classId)
            };
    Sql.ExecuteNonQuery("delete from permissions where classId=@classId", p);
            for (int i = 0; i<permissions.Length; i++)
            {
                if (_permissions[i] != "")
                {
                    string[] item = _permissions[i].Split(',');
    Sql.ExecuteNonQuery("insert into permissions (classId,type,dataId,p0,p1,p2,p3,p4)values(@classId,@type,@dataId,@p0,@p1,@p2,@p3,@p4)",
                            new MySqlParameter[]{
                    new MySqlParameter("classId", classId),
                    new MySqlParameter("type", item[0]),
                    new MySqlParameter("dataId", item[1]),
                    new MySqlParameter("p0", item[2]),
                    new MySqlParameter("p1", item[3]),
                    new MySqlParameter("p2", item[4]),
                    new MySqlParameter("p3", item[5]),
                    new MySqlParameter("p4", item[6])
                    }
                        );
                }

            }
            return info;
        }
        public ReturnValue setIcon(string icon)
        {
            ReturnValue info = new ReturnValue();
            info = UserClass.setIcon(icon, loginInfo.value);
            return info;
        }
        public ReturnValue columnSorting(double classId, string ids,int flag)
        {
            ReturnValue info = new ReturnValue();
string[] id = ids.Split(',');
            for (int i = 0; i<id.Length; i++)
            {
                Sql.ExecuteNonQuery("update class set orderid=@orderid,classId=@classId   where id=@id",
                new MySqlParameter[]{
                    new MySqlParameter("orderid", i),
                    new MySqlParameter("id", double.Parse(id[i])),
                    new MySqlParameter("classId", classId)
                });
            }
            if(flag==1 && classId>7)ColumnClass.reset(classId);
            return info;
        }

        public ReturnValue moduleDel(double moduleId,double classId,int type,double saveDataType)
        {
            ReturnValue info = new ReturnValue();
Permissions p = loginInfo.value.getModulePermissions(moduleId);
            if (!p.all)
            {
                info.errNo = -1;
                info.errMsg = "没有删除该模块的权限";
                return info;
            }
            if (type == 0)
            {
                Sql.ExecuteNonQuery("delete from module where id=@moduleId", new MySqlParameter[]{
                new MySqlParameter("moduleId", moduleId)
                });
                //if (moduleId == classId)
                //{
                //    Sql.ExecuteNonQuery("delete from class where id=@classId", new MySqlParameter[]{
                //new MySqlParameter("classId",classId)
                //});
                //}
                info.userData = Sql.ExecuteArrayObj("select id,savedatatype from class where moduleId=@moduleId order by layer desc", new MySqlParameter[]{
                new MySqlParameter("moduleId", moduleId)
                });
            }
            else
            {
Sql.ExecuteNonQuery("delete from class where id=@classId", new MySqlParameter[]{
                new MySqlParameter("classId", classId)
                });
                object tableName = Sql.ExecuteScalar("select tablename from datatype where id=" + saveDataType.ToString());
                if (tableName != null)
                {
                    try
                    {
                        Sql.ExecuteNonQuery("delete from maintable where classId=@classId", new MySqlParameter[]{
                            new MySqlParameter("classId", classId)
                        });
                        Sql.ExecuteNonQuery("delete from " + tableName.ToString() + " where classId=@classId", new MySqlParameter[]{
                            new MySqlParameter("classId", classId)
                        });
                    }
                    catch { }
                }
            }
            return info;
        }
        public ReturnValue resetContent(double id)
        {
            ReturnValue info = new ReturnValue();
info = ColumnClass.resetContentUrl(id);
            return info;
        }
        public ReturnValue resetColumn(double id)
        {
            ReturnValue info = new ReturnValue();
            ColumnClass.reset(id);
            string childId = ColumnClass.getChildId(id);
            info.userData = childId;
            return info;
        }
        public ReturnValue moduleList()
        {
            ReturnValue info = new ReturnValue();
string sql = "";
            if (loginInfo.value.isAccess)
            {
                sql = "select A.id value,A.moduleName text,A.saveDataType ,A.type,sum(P0) p0,sum(P1)  p1,sum(P2) p2,sum(P3) p3 from module A left join permissions B on A.id=B.classId group by A.id,A.moduleName,A.saveDataType,A.type";
            }
            else
            {
                sql = "select A.id value,A.moduleName text,A.saveDataType ,A.type,sum(P0) p0,sum(P1)  p1,sum(P2) p2,sum(P3) p3 from module A inner join "+
                    "(select p0,p1,p2,p3,p4,classId from permissions where dataId in (select roleId from admin_role where userId="+loginInfo.value.id.ToString()+") or dataId="+loginInfo.value.id.ToString()+") B"+
                    " on A.id=B.classId group by A.id,A.moduleName,A.saveDataType,A.type";
            }
            ArrayList arrayList = new ArrayList();
MySqlDataReader rs = Sql.ExecuteReader(sql);
            while (rs.Read())
            {
                Permissions p = new Permissions(loginInfo.value);

bool p0 = (rs[4] != System.DBNull.Value && rs.GetInt32(4) > 0);
bool p1 = (rs[5] != System.DBNull.Value && rs.GetInt32(5) > 0);
bool p2 = (rs[6] != System.DBNull.Value && rs.GetInt32(6) > 0);
bool p3 = (rs[7] != System.DBNull.Value && rs.GetInt32(7) > 0);
p.read = true;
                p.write |= p0;
                p.delete |= p0;
                p.audit |= p0;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                for (int i = 0; i< 4; i++)
                {
                    dictionary.Add(rs.GetName(i), rs[i]);
                }
                dictionary.Add("permissions", p);
                arrayList.Add(dictionary);
            }
            rs.Close();
            info.userData = arrayList;
            return info;
        }
        public ReturnValue columnList(double moduleId,double classId)
        {
            ReturnValue info = new ReturnValue();
            if (moduleId == -1 && classId == 7)
            {
                //获取共享栏目
                ArrayList arrayList = new ArrayList();
string role = loginInfo.value.id.ToString();
                if (loginInfo.value.role != "") role += "," + loginInfo.value.role;
                MySqlDataReader rs = Sql.ExecuteReader("select distinct B.id,B.classId,B.className text,moduleId,case when B.orderid<0 then 'M5_Del' end  class,B.saveDataType dataTypeId,A.p0,A.p1,A.p2,A.p3 from permissions A inner join class B on A.classId=B.id where  A.dataId in (" + role + ") ");
                while (rs.Read())
                {
                    Permissions p = new Permissions(loginInfo.value);

bool p0 = (rs[6] != System.DBNull.Value && rs.GetInt32(6) > 0);
bool p1 = (rs[7] != System.DBNull.Value && rs.GetInt32(7) > 0);
bool p2 = (rs[8] != System.DBNull.Value && rs.GetInt32(8) > 0);
bool p3 = (rs[9] != System.DBNull.Value && rs.GetInt32(9) > 0);
p.read = true;
                    p.write |= p0;
                    p.delete |= p0;
                    p.audit |= p0;
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    for (int i = 0; i< 6; i++)
                    {
                        dictionary.Add(rs.GetName(i), rs[i]);
                    }
                    dictionary.Add("permissions", p);
                    arrayList.Add(dictionary);

                }
                rs.Close();
                info.userData = arrayList;
            }
            else
            {
                Permissions p = null;
                //获取所选栏目或模块的权限
                if (classId == 7 || classId==moduleId) p = loginInfo.value.getModulePermissions(moduleId);
                else
                {
                    p = loginInfo.value.getColumnPermissions(classId);
                }
                //如果有该栏目的读取权限时列出子栏目
                if (p.read)
                {
                    ArrayList list = Sql.ExecuteArray("select id,classId,className text,moduleId,case when orderid<0 then 'M5_Del' end  class,saveDataType dataTypeId from class where  moduleId=@moduleId and classId=@classId order by orderid",
                        new MySqlParameter[]{
                        new MySqlParameter("moduleId",moduleId),
                        new MySqlParameter("classId",classId)
                });
                    for (int i = 0; i<list.Count; i++)
                    {
                        Dictionary<string, object> obj = (Dictionary<string, object>)list[i];
obj.Add("permissions", p);
                    }
                    info.userData = list;
                }
            }
            return info;

        }

        public ReturnValue columnDel(double classId)
        {
            try
            {
                ColumnClass.del(classId, loginInfo.value);
                return ReturnValue.Success(null);

            } catch (Exception e)
            {
                return ReturnValue.Err(e.Message);
            };
        }
        public ReturnValue templateList(double classId,int type,int isMobile)
        {
        ColumnInfo info = ColumnClass.get(classId);
        ReturnValue err = new ReturnValue();
            if (info != null)
            {
                err.userData = info.getTemplateList(type, isMobile==1);
            }
            else
            {
                err.errNo = -1;
                err.errMsg = "栏目不存在";
            }
return err;
        }
        public ReturnValue dataTypeList()
        {
            ReturnValue err = new ReturnValue();
err.userData = Sql.ExecuteArray("select id value,datatype text from datatype where id>100 and classid=16");
            return err;
        }
        public ReturnValue columnMove(double id ,double moduleId,double classId)
        {
            ReturnValue err = ColumnClass.move(id,moduleId,classId, loginInfo.value);
return err;
        }

    public ReturnValue auditData(double classId,double moduleId,string ids,int flag,string msg)
{
    ReturnValue err = new ReturnValue();
    Permissions p = null;
    if (classId < 8 || classId == moduleId)
    {
        p = loginInfo.value.getModulePermissions(moduleId);

    }
    else
    {
        p = loginInfo.value.getColumnPermissions(classId);
    }
    if (p.audit)
    {
        err = TableInfo.auditData(ids, flag == 0, msg, loginInfo.value);
    }
    else { err.errNo = -1; err.errMsg = "没有权限"; }
    return err;
}
public ReturnValue saveCaptureScreen(string byteStr)
{
    ReturnValue err = new ReturnValue();
    byte[] byteData = Convert.FromBase64String(byteStr);
    if (byteData.Length > 0)
    {
        string fileName = Tools.GetId() + ".jpg";
        string path = Config.webPath + Config.tempPath + System.DateTime.Now.ToString("yyyy-MM/");
        if (!System.IO.Directory.Exists(Tools.MapPath(path))) System.IO.Directory.CreateDirectory(Tools.MapPath(path));
        System.IO.File.WriteAllBytes(Tools.MapPath(path) + fileName, byteData);
        err.userData = path + fileName;

    }
    else
    {
        err.errNo = -1;
        err.errMsg = "截图失败";
    }
    return err;

}
public ReturnValue appConfigList()
{
    ReturnValue err = new ReturnValue();
    List<object> list = new List<object>();
    string path = Tools.MapPath("~" + Config.appPath);
    string[] appDir = System.IO.Directory.GetDirectories(path);
    for (int i = 0; i < appDir.Length; i++)
    {
        string reg_xml = appDir[i] + @"\reg.xml";
        string app_config = appDir[i] + @"\app.config";
        if (System.IO.File.Exists(app_config) && System.IO.File.Exists(reg_xml))
        {
            XmlDocument reg = new XmlDocument();
            reg.Load(reg_xml);
            string title = reg.ChildNodes[0].Attributes["title"].Value;
            string name = reg.ChildNodes[0].Attributes["name"].Value;
            Dictionary<string, string> item = new Dictionary<string, string>();
            item.Add("text", title);
            item.Add("file", name);
            list.Add(item);
        }
    }
    err.userData = list;
    return err;
}
public ReturnValue getSystemColor()
{
    ReturnValue err = new ReturnValue();
    string path = Tools.MapPath("~" + Config.tempPath + @"user\" + loginInfo.value.id.ToString() + @"\");
    Dictionary<string, string> colors = (Dictionary<string, string>)Tools.readObjectFile(path + "systemColor.config");
    err.userData = colors;
    return err;
}
public ReturnValue setSystemColor(string background_color,string active_background_color,string link_color)
{
    ReturnValue err = new ReturnValue();
    Dictionary<string, string> colors = new Dictionary<string, string>();
    colors["background_color"] = background_color;
    colors["active_background_color"] = active_background_color;
    colors["link_color"] = link_color;
    string path = Tools.MapPath("~" + Config.tempPath + @"user\" + loginInfo.value.id.ToString() + @"\");
    if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
    Tools.writeObjectFile(path + "systemColor.config", colors);
    return err;
}
public ReturnValue saveConfig(string _configFile)
{
    ReturnValue err = new ReturnValue();
    if (!loginInfo.value.isAdministrator)
    {
        err.errNo = -1;
        err.errMsg = "无权访问";
        return err;
    }
    try
    {
        string file = _configFile;
        string configFile = file.IndexOf(".") > -1 ? Tools.MapPath("~" + Config.configPath + file) : Tools.MapPath("~" + Config.appPath + file + @"/app.config");

        string xml = Tools.GetFileText(configFile);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);
        XmlNodeList list = doc.GetElementsByTagName("item");
        for (int i = 0; i < list.Count; i++)
        {
            string name = list[i].Attributes["name"] == null ? "" : list[i].Attributes["name"].Value;
            string xtype = list[i].Attributes["xtype"] == null ? "" : list[i].Attributes["xtype"].Value;
            if (name != "")
            {
                if (xtype != "")
                {
                    if (xtype == "GridView")
                    {
                        list[i].InnerXml = Request.Form[name].ToString();
                    }
                    else
                    {
                        list[i].InnerText = Request.Form[name].ToString();
                    }
                }
                else
                {
                    list[i].InnerText = Request.Form[name].ToString();
                }
            }
        }
        doc.Save(configFile);
        Config.loadUserConfig(file);
    }
    catch (Exception ex)
    {
        err.errNo = -1;
        err.errMsg = ex.Message;
    }
    return err;
}
public ReturnValue getConfig(string file)
{
    ReturnValue err = new ReturnValue();
    string configFile = file.IndexOf(".") > -1 ? Tools.MapPath("~" + Config.configPath + file) : Tools.MapPath("~" + Config.appPath + file + @"/app.config");
    if (!System.IO.File.Exists(configFile))
    {
        err.errNo = -1;
        err.errMsg = "文件不存在";
        return err;
    }
    string xml = Tools.GetFileText(configFile);
    XmlDocument doc = new XmlDocument();
    doc.LoadXml(xml);
    Dictionary<string, object> viewVariables = (Dictionary<string, object>)doc.ToJson().ParseJson<Dictionary<string,object>>();
    return new ReturnValue(viewVariables);
    //context.Response.Write("{\"errNo\":0,\"errMsg\":\"\",\"userData\":" + Tools.XmlToJSON(doc) + "}");
}
public ReturnValue setOrderId(string ids,int orderId)
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
        info.errMsg = "参数不合法"; return info;
    }
    Sql.ExecuteNonQuery("update maintable set orderId=@orderId,auditorId=@auditorId where id in (" + ids + ")", new MySqlParameter[] {
            new MySqlParameter("orderId",orderId),
            new MySqlParameter("auditorId",loginInfo.value.id)
        });
    return info;
}
public ReturnValue setAttr(string ids,int type,int flag)
{
    ReturnValue info = new ReturnValue();
    info = TableInfo.setAttr(ids, type, flag == 1);
    return info;
}
public ReturnValue moveData(string ids,double classId)
{
    ReturnValue info = new ReturnValue();
    info = TableInfo.moveData(ids, classId);
    return info;
}
public ReturnValue setTop(string ids,int flag)
{
    ReturnValue info = new ReturnValue();
    info = TableInfo.setTop(ids, flag == 1);
    return info;
}
public ReturnValue reductionData(string ids,double moduleId,double classId,int tag)
{
    Permissions p = loginInfo.value.getModulePermissions(moduleId, classId);

    ReturnValue info = new ReturnValue();
    if (p.delete)
    {
        info = TableInfo.reductionData(ids, loginInfo.value);
    }
    return info;
}
public ReturnValue delData(string ids,double moduleId,double classId,int tag)
{
    double dataTypeId = -1;
    Permissions p = null;
    if (classId < 8 || classId == moduleId)
    {
        MySqlDataReader rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new MySqlParameter[] { new MySqlParameter("moduleId", moduleId) });
        if (rs.Read()) dataTypeId = rs.GetDouble(0);
        rs.Close();
        p = loginInfo.value.getModulePermissions(moduleId);

    }
    else
    {
        MySqlDataReader rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new MySqlParameter[] { new MySqlParameter("classId", classId) });
        if (rs.Read()) dataTypeId = rs.GetDouble(0);
        rs.Close();
        p = loginInfo.value.getColumnPermissions(classId);
    }
    ReturnValue info = new ReturnValue();
    if (p.delete)
    {
        if (p.audit)
        {
            info = TableInfo.delData(dataTypeId, ids, tag == 1, loginInfo.value);
        }
        else
        {
            info = TableInfo.delData(dataTypeId, ids, tag == 1, loginInfo.value);
        }
    }
    else
    {
        info.errNo = -1;
        info.errMsg = "权限不足";
    }
    return info;
}
/*
public ReturnValue dataList(double moduleId,double classId=-1,int pageNo=1,string orderBy="",int sortDirection=0,string type="", string searchField="",string keyword="")
{
    ReturnValue err = new ReturnValue();
    double dataTypeId = -1;
    MySqlDataReader rs = null;
    Permissions p = null;
    if (moduleId == classId)
    {
        p = loginInfo.value.getModulePermissions(classId);
        rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new MySqlParameter[] { new MySqlParameter("moduleId", moduleId) });
        if (rs.Read()) dataTypeId = rs.GetDouble(0);
        rs.Close();
    }
    else
    {
        p = loginInfo.value.getColumnPermissions(classId);
        rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new MySqlParameter[] { new MySqlParameter("classId", classId) });
        if (rs.Read()) dataTypeId = rs.GetDouble(0);
        rs.Close();
    }
    if (!p.read)
    {
        err.errNo = -1;
        err.errMsg = "无权访问";
        return err;
    }
    TableInfo table = new TableInfo(dataTypeId);

    List<FieldInfo> fieldList = table.fields.FindAll(delegate (FieldInfo v)
    {
        return v.visible || v.isNecessary;
    });
    string where = "";// " and A.orderid>-3";
    if (type[0] == '0') where += " and A.orderid<0 ";
    if (type[1] == '0') where += " and A.orderid<>-1 ";
    if (type[2] == '0') where += " and A.orderid<>-2 ";
    if (type[3] == '0') where += " and A.orderid<>-3 ";
    //else if (type == 2) where = " and A.orderid=-3 ";
    if (keyword != "")
    {
        switch (searchField)
        {
            case "id":
                where += " and A." + searchField + "=" + keyword + "";
                break;
            case "title":
                where += " and A." + searchField + " like '%" + keyword + "%'";
                break;
            case "userId":
                object userId = Sql.ExecuteScalar("select id from m_admin where uname=@uname", new MySqlParameter[]{
                        new MySqlParameter("uname",keyword)
                    });
                if (userId != null)
                {
                    where += " and A." + searchField + "=" + userId.ToString();
                }
                else
                {
                    where += " and A.userId=-1 ";
                }
                break;
            default:
                where += " and ";
                FieldInfo list = table.fields.Find(delegate (FieldInfo v)
                {
                    if (v.name == searchField) return true;
                    else return false;
                });
                if (list.type == "String")
                {
                    where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                    where += searchField + " like '%" + keyword + "%'";
                }
                else if (list.type == "DateTime")
                {
                    string[] item = keyword.Split(',');
                    where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                    where += searchField + ">='" + item[0].ToString() + "' and " + searchField + "<='" + item[1].ToString() + "'";
                }
                else
                {
                    string[] item = keyword.Split(',');
                    where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                    where += searchField + ">=" + item[0].ToString() + " and " + searchField + "<=" + item[1].ToString();
                }
                break;
        }
    }
    if (!p.audit) where += " and A.userId=" + loginInfo.value.id.ToString();
    ReturnPageData dataList = table.getDataList(moduleId, classId, pageNo, orderBy, sortDirection, where);
    object[] data = new object[] { fieldList, dataList };
    err.userData = data;
    return err;
        }
        */
        public ReturnValue upload(List<IFormFile> fileData)
        {
            string[] fp = new string[fileData.Count];
            List<File> list = new List<File>();
            for (int i = 0; i < fileData.Count; i++)
            {
                try
                {
                    string newfile = Lib.SaveImage(fileData[i], Config.tempPath, new string[] { "jpg", "gif", "png" });
                    list.Add(new File { title = fileData[i].FileName, size = fileData[i].Length, path = newfile });
                }
                catch
                {

                }
            }

            return ReturnValue.Success(list);
        }
    }
}
