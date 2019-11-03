using Helper;
using MWMS.DAL;
using MWMS.Helper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MWMS.DAL.Table
{
    public abstract class BaseColumn:TableHandle
    {
        /// <summary>
        ///数据id
        /// </summary>
        public double Id { get; set; }
        /// <summary>
        /// 父级id
        /// </summary>
        public double ParentId { get; set; }
        /// <summary>
        /// 栏目名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 助记名
        /// </summary>
        public string MnemonicName { get; set; }
        /// <summary>
        /// 栏目图片
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// 顶级id
        /// </summary>
        public double RootId { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// 层级
        /// </summary>
        public int Layer { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Info { get; set; }
        public BaseColumn() : base("class")
        {

        }
        public BaseColumn(double columnId) : base("class")
        {
            SetAttrubite(columnId);
        }
        /// <summary>
        /// 设置自身的属性
        /// </summary>
        /// <param name="id">栏目id</param>
        public virtual void SetAttrubite(double id)
        {
            Dictionary<string, object> model = this.GetModel(id);
            if (model == null) throw new Exception("栏目不存在");
            Id = id;
            ParentId = model["classId"].ToDouble();
            Name = model["className"].ToStr();
            MnemonicName = model["dirName"].ToStr();
            Picture = model["maxico"].ToStr();
            RootId = model["rootId"].ToDouble();
            OrderID = model["orderID"].ToInt();
            Layer = model["Layer"].ToInt();
            Info = model["info"].ToStr();
        }
    }
}
