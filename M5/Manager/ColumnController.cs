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
using Microsoft.AspNetCore.Mvc;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace M5.Main.Manager
{
    [LoginAuthorzation]
    public class ColumnController : ManagerBase
    {
        public ReturnValue editDirName(double id, string dirName)
        {
            ReturnValue info = new ReturnValue();
            info = ColumnClass.editDirName(id, dirName, this.loginInfo.value);
            return info;
        }
        public ReturnValue editModuleDirName(double id, string dirName)
        {
            ReturnValue info = new ReturnValue();
            info = ModuleClass.editDirName(id, dirName, this.loginInfo.value);
            return info;
        }

        public ReturnValue moduleEdit(string moduleName,string dirName,int type,double saveDataType,string custom,int watermark, int thumbnailWidth=0,int thumbnailHeight=0,int thumbnailForce=0, double id = 0, int saveRemoteImages = 0, int inherit = 0, string _domainName = "", string domainName = "", string keyword = "", string info = "", int titleRepeat = 0)
        {

            ModuleInfo moduleInfo = new ModuleInfo();
            moduleInfo.id = id;
            moduleInfo.moduleName = moduleName;
            moduleInfo.dirName = dirName;
            moduleInfo.type = type == 1;
            moduleInfo.saveDataType = saveDataType;

            moduleInfo.custom = custom;
            moduleInfo.watermark = watermark;
            moduleInfo.thumbnailWidth = thumbnailWidth;
            moduleInfo.thumbnailHeight = thumbnailHeight;
            moduleInfo.thumbnailForce = thumbnailForce;
            moduleInfo.saveRemoteImages = saveRemoteImages;
            moduleInfo.inherit = inherit;
            moduleInfo._domainName = _domainName;
            moduleInfo.domainName = domainName;
            moduleInfo.keyword = keyword;
            moduleInfo.info = info;
            moduleInfo.titleRepeat = titleRepeat;

            ReturnValue err = new ReturnValue();
            Permissions p = this.loginInfo.value.getModulePermissions(moduleInfo.id);
            if (!p.all)
            {
                err.errNo = -1;
                err.errMsg = "没有编辑该模块的权限";
                return err;
            }

            err = ModuleClass.edit(moduleInfo, this.loginInfo.value);
            Config.loadDomain();
            return err;
        }
        public ReturnValue columnEdit(double classId,double moduleId,string maxIco,double skinId,double contentSkinId,double _skinId,double _contentSkinId, string className, string dirName, int type, double saveDataType, string custom, int watermark, int thumbnailWidth = 0, int thumbnailHeight = 0, int thumbnailForce = 0, double id = 0, int saveRemoteImages = 0, int inherit = 0, string _domainName = "", string domainName = "", string keyword = "", string info = "", int titleRepeat = 0)
        {

            ColumnInfo columnInfo = new ColumnInfo();
            columnInfo.id = id;
            columnInfo.className = className;
            columnInfo.classId = classId;
            columnInfo.moduleId = moduleId;
            columnInfo.dirName = dirName;
            columnInfo.keyword = keyword;
            columnInfo.maxIco = maxIco;
            columnInfo.saveDataType = saveDataType;
            columnInfo.skinId = skinId;
            columnInfo.contentSkinId = contentSkinId;
            columnInfo._skinId = _skinId;
            columnInfo._contentSkinId = _contentSkinId;
            columnInfo.info = info;
            columnInfo.watermark = watermark;

            columnInfo.custom = custom;

            columnInfo.thumbnailWidth = thumbnailWidth;
            columnInfo.thumbnailHeight = thumbnailHeight;
            columnInfo.thumbnailForce = thumbnailForce;
            columnInfo.saveRemoteImages = saveRemoteImages;
            columnInfo.inherit = inherit;
            columnInfo.domainName = domainName;
            columnInfo._domainName = _domainName;

            columnInfo.titleRepeat = titleRepeat;
            ReturnValue err = new ReturnValue();
            Permissions p = null;
            if (columnInfo.classId == 7)
            {
                p = loginInfo.value.getModulePermissions(columnInfo.moduleId);//获取上线栏目权限
            }
            else
            {
                ColumnInfo parentColumn = ColumnClass.get(columnInfo.classId);
                columnInfo.rootId = parentColumn.rootId;
                p = this.loginInfo.value.getColumnPermissions(parentColumn);//获取上线栏目权限
            }
            if (!p.all)
            {
                err.errNo = -1;
                err.errMsg = "没有编辑该栏目的权限";
                return err;
            }
            ColumnClass.edit(columnInfo, this.loginInfo.value);
            Config.loadDomain();
            return err;
        }
        public ReturnValue moduleInfo(double id)
        {
            ReturnValue err = new ReturnValue();
            Permissions p = this.loginInfo.value.getModulePermissions(id);
            if (!p.read)
            {
                err.errNo = -1;
                err.errMsg = "没有查看该栏目的权限";
                return err;
            }
            ModuleInfo info = ModuleClass.get(id);
            if (info == null)
            {
                err.errNo = -1;
                err.errMsg = "没有找到指定id数据";
            }
            else
            {
                err.userData = info;
            }
            return err;
        }
        public ReturnValue columnInfo(double id)
        {
            ReturnValue err = new ReturnValue();
            ColumnInfo info = ColumnClass.get(id);
            if (info == null)
            {
                err.errNo = -1;
                err.errMsg = "没有找到指定id数据";
            }
            else
            {
                Permissions p = this.loginInfo.value.getColumnPermissions(info.id);
                if (p.read)
                {
                    err.userData = info;
                }
                else
                {
                    err.errNo = -1;
                    err.errMsg = "没有查看该栏目的权限";
                }
            }
            return err;
        }


        public ReturnValue setColumnStatus(double classId, int status)
        {
            ReturnValue err = new ReturnValue();
            Permissions p = this.loginInfo.value.getColumnPermissions(classId);
            if (!p.write)
            {
                err.errNo = -1;
                err.errMsg = "没有编辑该栏目的权限";
                return err;
            }
            string childId = "";
            MySqlDataReader rs = Sql.ExecuteReader("select childId from class where id=@id", new MySqlParameter[] {
                new MySqlParameter ("id",classId )
        });
            if (rs.Read()) childId = rs[0].ToString();
            rs.Close();
            if (childId == "")
            {
                err.errNo = -1;
                err.errMsg = "指定栏目无效";
                return err;
            }
            Sql.ExecuteNonQuery("update class set orderId=" + (status == 1 ? "0" : "-1") + " where id in (" + childId + ")");
            return err;
        }
    }

}
