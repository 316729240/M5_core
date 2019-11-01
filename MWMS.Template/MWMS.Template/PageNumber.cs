using M5;
using M5.Common;
using MWMS.Helper.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MWMS.Template
{
    public class PageNumber
    {
        public string Type { get; set; }
        public int Number { get; set; }
        public string Url { get; set; }
    }
    public class PageBar : IList<PageNumber>
    {
        public void Init(int showCount)
        {
            int PageNo = this.PageNo;
            int RecordCount = this.RecordCount;
            int PageSize = this.PageSize;
            List<PageNumber> list = new List<PageNumber>();
            if (PageNo < 1) PageNo = 1;
            #region 查找当前域名是否在子域列表中

            string domain = PageContext.Current.Request.Host.Host;
            bool tag = false;
            if (!tag) domain = "";
            #endregion
            //        string url = API.GetWebUrl() + PageContext.Current.Request.RawUrl;
            string url = domain + PageContext.Current.Request.Path;
            int index = 0, index2 = 0;
            while (index > -1)
            {
                index2 = index;
                index = url.IndexOf("/", index + 1);
            }
            url = url.Substring(0, index2) + "/";

            #region
            this.PageCount = (RecordCount - 1) / PageSize + 1;
            int js = showCount / 2;
            int StartN = PageNo - js > 0 ? PageNo - js : 0;// PageNo - (PageNo - 1) % js - js;
            if (StartN < 1) StartN = 1;
            #endregion
            StringBuilder PageNumber = new StringBuilder();
            StringBuilder Prev = new StringBuilder();
            StringBuilder Next = new StringBuilder();
            StringBuilder FirstPage = new StringBuilder();
            StringBuilder EndPage = new StringBuilder();
            string KZM = "." + BaseConfig.extension;
            string FileName = this.FileName;
            string par = PageContext.Current.Request.Query.Count > 0 ? "?" + PageContext.Current.Request.QueryString.ToString() : "";
            string filename2 = url + FileName + KZM;
            if (String.Compare(FileName, "default", true) == 0) filename2 = url;
            FileName = url + FileName;
            #region PageNumber
            for (int n1 = 0; n1 < showCount; n1++)
            {
                if (n1 + StartN <= PageCount)
                {
                    //if (n1 + StartN != PageNo)
                    //{
                    _list.Add(new PageNumber()
                    {
                        Number = n1 + StartN,
                        Type = "1",
                        Url = (n1 + StartN) == 1 ? filename2 : FileName + "_" + (n1 + StartN).ToString() + KZM
                    });

                }
            }
            #endregion
            this.FirstNumber = new PageNumber
            {
                Number = 1,
                Type = "1",
                Url = filename2 + par
            };
            this.LastNumber = new PageNumber
            {
                Number = PageNo < 3 ? 1 : PageNo - 1,
                Type = "1",
                Url = PageNo < 3 ? filename2 + par : FileName + "_" + (PageNo - 1).ToString() + KZM + par
            };
            this.NextNumber = new PageNumber
            {
                Number = PageNo < PageCount ? PageNo + 1 : PageCount,
                Type = "1",
                Url = PageNo < PageCount ? FileName + "_" + (PageNo + 1).ToString() + KZM + par : FileName + "_" + PageCount.ToString() + KZM + par
            };
            this.EndNumber = new PageNumber
            {
                Number = PageCount,
                Type = "1",
                Url = FileName + "_" + PageCount.ToString() + KZM + par
            };
        }
        List<PageNumber> _list = new List<PageNumber>();
        public int PageNo { get; set; }
        public int RecordCount { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public string FileName { get; set; }
        public PageNumber FirstNumber { get; set; }
        public PageNumber LastNumber { get; set; }
        public PageNumber NextNumber { get; set; }
        public PageNumber EndNumber { get; set; }
        public PageNumber this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }
        public void Add(PageNumber file)
        {
            _list.Add(file);
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

        public int IndexOf(PageNumber item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, PageNumber item)
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

        public bool Contains(PageNumber item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(PageNumber[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(PageNumber item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<PageNumber> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
