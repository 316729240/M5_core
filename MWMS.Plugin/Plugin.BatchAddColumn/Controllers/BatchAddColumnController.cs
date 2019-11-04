using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using M5.Common;
using M5.Main.Manager;
using MWMS;
using MWMS.Helper.Extensions;
using System.Text.RegularExpressions;
using MWMS.Helper;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MWMS.Plugin
{
    [LoginAuthorzation]
    public class BatchAddColumnController : ManagerBase
    {
        public ReturnValue addColumn(double moduleId,double classId,double dataTypeId,string list)
        {
            ReturnValue errinfo = new ReturnValue();
            Permissions p = null;
            if (classId<8)
            {
                classId = 7;
                p=loginInfo.value.getModulePermissions(moduleId);
            }
            else
            {
                p = loginInfo.value.getColumnPermissions(classId);
            }
            if (!p.all)
            {
                errinfo.errNo = -1;
                errinfo.errMsg = "没有编辑该栏目的权限";
                return errinfo;
            }
            string [] _list =list.Split('\n');
            string msg = "";
            double[] parentid = new double[] { classId, -1, -1, -1, -1, -1, -1 };
            for (int i = 0; i < _list.Length; i++)
            {
                if (_list[i].Trim() != "")
                {
                    int layer = Regex.Split(_list[i], "  ").Length-1;
                    classId=parentid[layer];
                    string[] temp = _list[i].Trim().Split('\t');
                    string classname = temp[0].Trim(), dirname = "",keyword="";
                    if (temp.Length > 1) dirname = temp[1];
                    else
                    {
                        dirname = classname.GetPinYin();
                        if (dirname.Length > 15) dirname = classname.GetPinYin('2');
                        dirname = Regex.Replace(dirname, "[ " + @"\-_" + "`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）——|{}【】‘；：”“'。，、？]", "");
                    }
                    if (temp.Length > 2) keyword = temp[2];
                    ColumnInfo info = new ColumnInfo();
                    info.className = classname;
                    info.classId = classId;
                    info.keyword = keyword;
                    if(classId!=7){
                        ColumnInfo parent = ColumnClass.get(classId);
                        if (parent != null) info.rootId = parent.rootId;
                    }
                    info.moduleId =moduleId;
                    info.dirName = dirname;
                    info.saveDataType = dataTypeId;
                    ReturnValue err = new ReturnValue();
                    if (classId <1)
                    {
                        err.errNo = -1;
                        err.errMsg = "上级栏目不存在";
                    }
                    else
                    {
                         ColumnClass.edit(info, loginInfo.value);
                    }
                    if (err.errNo > -1)
                    {
                        double newId = Convert.ToDouble(err.userData);
                        if (newId > 0) parentid[layer+1] = newId;
                    }
                    else
                    {
                        msg += "["+classname+"]";
                        //errinfo.errNo = -1;
                        //errinfo.errMsg += classname+"添加失败 "+err.errMsg + "<br>";
                    }
                }
            }
            errinfo.userData = msg;
            return errinfo;
        }
    }
 
}
