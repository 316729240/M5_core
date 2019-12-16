using MWMS.Helper.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace M5.Base.Common
{

    public class PageCache
    {
        static int cacheCycle = 10;//分钟
        static DateTime cacheTime = System.DateTime.Parse("2000-1-1");//缓存过期时间
        struct cacheConfig
        {
            public string regstr;
            public int cacheCycle;
            public DateTime cacheTime;
        }
        static List<cacheConfig> cacheConfigList = new List<cacheConfig>();//自定义缓存配制信息
        Queue<string> task = new Queue<string>();
        string _path = "";
        public PageCache(string path)
        {
            this._path = path;
            //loadConfig();
            //System.Timers.Timer myTimer = new System.Timers.Timer(200);
            //myTimer.Elapsed += new System.Timers.ElapsedEventHandler(runTask);
            //myTimer.Enabled = true;
        }
        void runTask(object source, System.Timers.ElapsedEventArgs e)
        {
            if (task.Count == 0) return;
            string url = task.Dequeue();

        }
        public void addTask(string url)
        {
            task.Enqueue(url);
        }
        /// <summary>
        /// 缓存文件是否过期
        /// </summary>
        /// <param name="filePath">缓存文件路径</param>
        /// <returns></returns>
        bool expiration(string url, FileInfo filePath)
        {
            if (filePath.Exists)
            {
                DateTime f = filePath.LastWriteTime;
                int _cacheCycle = cacheCycle;
                DateTime _cacheTime = cacheTime;
                for (int i = 0; i < cacheConfigList.Count; i++)
                {
                    Regex regex = new Regex("^" + cacheConfigList[0].regstr + "$", RegexOptions.IgnoreCase);
                    if (regex.IsMatch(url))
                    {
                        _cacheCycle = cacheConfigList[0].cacheCycle;
                        _cacheTime = cacheConfigList[0].cacheTime;
                        i = cacheConfigList.Count;
                    }
                }
                return (_cacheTime > f || (System.DateTime.Now - f).TotalMinutes > _cacheCycle);
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string Get(string url,bool isForce=false)
        {
            FileInfo filePath = getCacheFileName(url);
            if (expiration(url, filePath) && !isForce)//缓存是否过期
            {
                return null;
            }
            else
            {
                return File.ReadAllText(filePath.FullName);
            }
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="url"></param>
        /// <param name="html"></param>
        public void Set(string url,string html)
        {
            FileInfo filePath = getCacheFileName(url);
            File.WriteAllText(filePath.FullName,html);

        }
        FileInfo getCacheFileName(string url)
        {
            string str = url.MD5();
            string dir = "";
            for (int i = 0; i < 16; i += 2)
            {
                dir += str.Substring(i, 2) + @"\";
            }
            string cacheDir =this._path+dir;
            if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
            return (new FileInfo(cacheDir + str + ".cac"));
        }
        int _pageNo = 1;
        string _fileName = "";
        string _replaceUrl(Match m)
        {
            _pageNo = int.Parse(Regex.Match(m.Value, @"(?<=_)((\d){1,5})(?=\.)").Value);
            if (Regex.IsMatch(m.Value, "^default_", RegexOptions.IgnoreCase))
            {
                return "";
            }
            else
            {
                return Regex.Replace(m.Value, @"_((\d){1,5})", "");
            }
        }
    }
}
