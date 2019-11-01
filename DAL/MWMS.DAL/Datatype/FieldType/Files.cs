
using System;
using System.Collections.Generic;
using System.Collections;
using MWMS.Helper.Extensions;

namespace MWMS.DAL.Datatype.FieldType
{
    public class File
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string path = "";
        /// <summary>
        /// 文件大小
        /// </summary>
        public long size = 0;
        /// <summary>
        /// 标题
        /// </summary>
        public string title = "";
        /// <summary>
        /// 是否已删除
        /// </summary>
        public int isDel = 0;
    }
    public class Files : File,IList<File>
    {
        List<File> _list = new List<File>();
        /// <summary>
        /// 将字符串转换为Files字段数据类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Files Parse(string data)
        {
            Files files = new Files();
            List<File> list = null;
            try
            {
                list = data.ParseJson<List<File>>();
                if (list == null) return null;
            }
            catch { return null; }
            try
            {
                foreach (File file in list)
                {
                    if (file.isDel == 0)
                    {
                        files.Add(file);
                    }
                    else
                    {
                        #region 删除无效文件 
                        try
                        {
                            string path =AppContext.BaseDirectory + file.path;
                            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                        }
                        catch
                        {

                        }
                        #endregion
                    }
                }
            }
            catch
            {
            }
            if (files.Count > 0) {
                #region 设置默认值
                files.title = files[0].title;
                files.path = files[0].path;
                files.size = files[0].size;
                files.isDel = files[0].isDel;
                #endregion
            }
            return files;
        }
        public File this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }
        public void Add(File file)
        {
            _list.Add(file);
        }
        public override string ToString()
        {
            return path;
        }
        public int Count {get{return _list.Count;} }

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

        public int IndexOf(File item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, File item)
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

        public bool Contains(File item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(File[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(File item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<File> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
