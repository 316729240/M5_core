using Microsoft.AspNetCore.Http;
using MWMS.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace M5.Common
{
    public class Lib
    {
        public static string SaveImage(IFormFile file, string filePath,string [] extensions=null)
        {
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName;


                string path = Config.webPath + filePath + System.DateTime.Now.ToString("yyyy-MM/");
                if (!System.IO.Directory.Exists(Tools.MapPath(path))) System.IO.Directory.CreateDirectory(Tools.MapPath(path));
                string kzm = "";
                if (file.FileName.LastIndexOf(".") > -1) kzm = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1).ToLower();
                if (extensions!=null && !Regex.IsMatch(kzm, "("+String.Join("|",extensions)+")"))
                {
                    throw new Exception("文件类型不合法，只能上传jpg,gif,png");
                }
                string fileName = Tools.GetId() + "." + kzm;
                string new_path=Tools.MapPath(path + fileName);
                // 创建新文件
                using (FileStream fs = System.IO.File.Create(new_path))
                {
                // 复制文件
                    file.CopyTo(fs);
                    // 清空缓冲区数据
                    fs.Flush();
                }
            return path + fileName;
            
        }

        public static string Watermark(string oldfilename)
        {

            XmlNodeList list = Config.userConfig["watermark"];
            if (list == null) return oldfilename;
            string markpic = Tools.MapPath(list[0].InnerText);
            if (!System.IO.File.Exists(markpic)) return oldfilename;

            string _fileName = oldfilename;
            oldfilename = Tools.MapPath("~" + oldfilename);
            string filename = oldfilename;
            FileInfo f = new FileInfo(oldfilename);
            FileInfo markfile = new FileInfo(markpic); 
            int zl = int.Parse(list[6].InnerText);
            float proportion = float.Parse(list[5].InnerText), transparency = float.Parse(list[4].InnerText) / 100;
            int margins = int.Parse(list[3].InnerText), X = int.Parse(list[1].InnerText), Y = int.Parse(list[2].InnerText);
            return Picture.Watermark(f, markfile, proportion, transparency, margins, X, Y, list[7].InnerText == "1", zl);
        }
    }
}