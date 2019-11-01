using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using M5.Common;
using M5.Main.Manager;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;
using Microsoft.AspNetCore.Http;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MWMS.Plugin
{
    [LoginAuthorzation]
    public class FileManageController : ManagerBase
    {
        public ReturnValue createDir(string path, string name)
        {
            ReturnValue err = new ReturnValue();
    string rootPath = Tools.MapPath(@"\" + Config.webPath);
    path = Tools.MapPath(@"~/" + path + @"\");
            DirectoryInfo di = new DirectoryInfo(path);
    string newPath = di.FullName + @"/" + name;
            try
            {
                System.IO.Directory.CreateDirectory(newPath);
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
    dictionary["text"] = name;
                dictionary["path"] = newPath.Replace(rootPath, "").Replace(@"\", "//");
    err.userData = dictionary;
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            return err;
        }
        public ReturnValue editFileName(string path,string oldName,string name)
        {
            ReturnValue err = new ReturnValue();
System.IO.FileInfo f = new FileInfo(Tools.MapPath(@"~\" + path + @"\" + oldName));
            try
            {
            f.MoveTo(Tools.MapPath(@"~\"+path+@"\"+name));
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            return err;
        }
        public ReturnValue editDir(string path,string name)
        {
            ReturnValue err = new ReturnValue();
string rootPath = Tools.MapPath(@"\" + Config.webPath);
path = Tools.MapPath("~/" +path + @"\") ;
            DirectoryInfo di = new DirectoryInfo(path);
string newPath = di.Parent.FullName + @"/" + name;
            try
            {
                di.MoveTo(newPath);
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
dictionary["text"] = name;
                dictionary["path"] = newPath.Replace(rootPath, "").Replace(@"\", "//");
err.userData = dictionary;
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            return err;
        }
        public ReturnValue delDir(string path)
        {
            ReturnValue err = new ReturnValue();
path = Tools.MapPath("~/" + path + @"\") ;
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.GetDirectories().Length == 0)
            {
                di.Delete(true);
            }
            else
            {
                err.errNo = -1;
                err.errMsg = "删除文件夹失败，不允许删除包含子目录的文件夹";
            }
            return err;
        }
        public ReturnValue delFile(string _files,string path)
        {
            ReturnValue err = new ReturnValue();
string[] files =_files.Split(',');
path = Tools.MapPath("~/" + path + @"\") ;
            try
            {
                for (int i = 0; i<files.Length; i++)
                {
                    FileInfo f = new FileInfo(path + files[i]);
                    if (f.Exists) f.Delete();
                }
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            return err;
        }
        public ReturnValue readDir(string path)
        {
            ReturnValue err = new ReturnValue();
string rootPath = Tools.MapPath(@"\" + Config.webPath);
            string nowpath = Tools.MapPath(@"\" + Config.webPath + path);
            DirectoryInfo dir = new DirectoryInfo(nowpath);
DirectoryInfo[] list = dir.GetDirectories();
object[] dirlist = new object[list.Length];
            for (int i = 0; i<list.Length; i++)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
dictionary["text"] = list[i].Name;
                dictionary["path"] = list[i].FullName.Replace(rootPath, "").Replace(@"\", "//");
dictionary["ico"] = "fa-folder-o";
                dirlist[i] = dictionary;
            }
            err.userData = dirlist;
            return err;
        }
        public ReturnValue readFiles(string path, int pageNo = 1)
        {
            if (path == "\\") path = "";
            ReturnValue err = new ReturnValue();
            if (pageNo == 0) pageNo = 1;
            string rootPath = Tools.MapPath(@"\" + Config.webPath);
            string nowpath = Tools.MapPath(@"\" + Config.webPath+ path);
            DirectoryInfo dir = new DirectoryInfo(nowpath);
FileInfo[] list = dir.GetFiles();
ReturnPageData page = new ReturnPageData();
page.pageNo = pageNo;
            page.recordCount = list.Length;
            for (int i = (pageNo - 1) * page.pageSize; i<pageNo* page.pageSize; i++)
            {
                if (i<list.Length)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
dictionary["text"] = list[i].Name;
                    dictionary["path"] = list[i].FullName.Replace(rootPath, "").Replace(@"\", "//");
dictionary["updateTime"] = list[i].LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                    dictionary["size"] = list[i].Length.FormatFileSize();
                    page.data.Add(dictionary);
                }
            }
            err.userData = page;
            return err;
        }
        public ReturnValue saveFile(string path,string fileName,string encoding,string content)
        {
            ReturnValue err = new ReturnValue();
Dictionary<string, object> value = new Dictionary<string, object>();
path = @"\" + Config.webPath + path + @"\";
            try
            {
                FileInfo f = new FileInfo(Tools.MapPath(path + fileName));
System.IO.File.WriteAllText(f.FullName, content, System.Text.Encoding.GetEncoding(encoding));
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = "发生异常："+ex.Message;
            }
            return err;
        }
        public ReturnValue getFile(string path,string fileName)
        {
            ReturnValue err = new ReturnValue();
Dictionary<string, object> value = new Dictionary<string, object>();
path = @"\" + Config.webPath + path + @"\";
FileInfo f = new FileInfo(Tools.MapPath(path + fileName));
Microshaoft.Text.IdentifyEncoding code = new Microshaoft.Text.IdentifyEncoding();
string name = code.GetEncodingName(f);
System.Text.Encoding e = code.GetEncoding(name);
string text = System.IO.File.ReadAllText(f.FullName, e);
value.Add("encoding", e.WebName);
            value.Add("content", text);
            err.userData = value;
            return err;
        }
        public ReturnValue upload(int covered,string path,List<IFormFile> fileData,string editDate){
            ReturnValue info = new ReturnValue();
            string _path = Tools.MapPath(@"~\" + path+ @"\") ;
            //string filePath = s_request.getString("filePath");
            if (!Directory.Exists(_path)) Directory.CreateDirectory(_path);
            for(int i=0;i<fileData.Count;i++){
                                if (fileData[i].FileName.IndexOf(@"/.") > -1)//为目录时
                {
                    return info;
                }
                FileInfo f = new FileInfo(_path + fileData[i].FileName);
                if (covered !=1)
                {
                    if (f.Exists)
                    {
                        info.errNo = -2;
                        info.errMsg = fileData[i].FileName+ "文件已存在";
                        Dictionary<string, object> dictionary = new Dictionary<string, object>();
                        //context.Request.Files[i].
                        dictionary["oldFile"] = new object[] { f.Name, f.Length.FormatFileSize(), f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") };
                        dictionary["newFile"] = new object[] { f.Name, fileData[i].Length.FormatFileSize(), editDate };
                        info.userData = dictionary;
                        return info;
                    }
                }
                if (!f.Directory.Exists) f.Directory.Create();
                using (FileStream fs = System.IO.File.Create(f.FullName))
                {
                // 复制文件
                    fileData[i].CopyTo(fs);
                    // 清空缓冲区数据
                    fs.Flush();
                }
            }
                        return info;
        }
    }
 
}
