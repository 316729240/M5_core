using Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;
using MWMS.Helper.Extensions;
using MWMS.Helper;

namespace MWMS.DAL.Datatype.FieldType
{
    public class Picture : File
    {
        /// <summary>
        /// 缩略图路径
        /// </summary>
        public string minPath = "";
    }
    public class Pictures : Picture, IList<Picture>
    {
        List<Picture> _list = new List<Picture>();
        /// <summary>
        /// 将字符串转换为Files字段数据类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Pictures Parse(string data)
        {
            if (data == "") return null;
            Pictures files = new Pictures();
            List<Picture> list = new List<Picture>();
            if (data.Substring(0, 1) == "[")
            {
                list = data.ParseJson<List<Picture>>();
            }
            else if (data.Substring(0, 1) == "{")
            {
                Picture pic = data.ParseJson<Picture>();
                if (list == null) return null;
            }
            if (list == null) return null;
            if (list.Count == 0)
            {
                try
                {
                    list.Add(data.ParseJson<Picture>());
                }
                catch { }
            }
            //
                foreach (Picture file in list)
                {
                    if (file.isDel == 0)
                    {
                        files.Add(file);
                    }
                    else
                    {
                        #region 删除无效文件 
                            string path = Tools.MapPath("~" + file.path);
                            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                            string minpath = Tools.MapPath("~" + file.minPath);
                            if (System.IO.File.Exists(minpath)) System.IO.File.Delete(minpath);

                        #endregion
                    }
                }
            if (files.Count > 0)
            {
                #region 设置默认值
                files.title = files[0].title;
                files.path = files[0].path;
                files.title = files[0].title;
                files.isDel = files[0].isDel;
                files.minPath = files[0].minPath;
                #endregion
            }
            return files;
        }
        public Picture this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }
        public void Add(Picture file)
        {
            _list.Add(file);
        }
        public override string ToString()
        {
            return path;
        }
        public int Count { get { return _list.Count; } }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ToJson()
        {
            return _list.ToJson();
        }

        public int IndexOf(Picture item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Picture item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Picture item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Picture[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Picture item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Picture> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
