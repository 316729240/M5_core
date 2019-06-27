using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MWMS.Helper
{

    public class SafeReqeust
    {
        int requestType = 0;
        int errType = 0;
        /// <summary>
        /// 获取网页参数
        /// </summary>
        /// <param name="_requestType">获取方式 0 自动 1query 2form</param>
        /// <param name="_errType">错误处理 0屏蔽错误 1输出错误信息 2不处理</param>
        public SafeReqeust(int _requestType,int _errType)
        {
            requestType = _requestType;
            errType=_errType;
        }
        /// <summary>
        /// 获取字符串类型数据
        /// </summary>
        /// <param name="name">参数名</param>
        /// <returns></returns>
        public string getString(string name)
        {
            object value=getObject(0, name);
            return value==null?"":(string)value;
        }
        /// <summary>
        /// 获取整型数据
        /// </summary>
        /// <param name="name">参数名</param>
        /// <returns></returns>
        public int getInt(string name)
        {
            object value = getObject(1, name);
            return value == null ? 0 : (int)value;
        }
        /// <summary>
        /// 获取整型数据
        /// </summary>
        /// <param name="name">参数名</param>
        /// <returns></returns>
        public byte getByte(string name)
        {
            object value = getObject(4, name);
            return value == null ? (byte)0 : (byte)value;
        }
        /// <summary>
        /// 获取浮点型数据
        /// </summary>
        /// <param name="name">参数名</param>
        /// <returns></returns>
        public float getFloat(string name)
        {
            object value = getObject(2, name);
            return value == null ? 0 : (float)value;
        }
        /// <summary>
        /// 获取双精度浮点型数据
        /// </summary>
        /// <param name="name">参数名</param>
        /// <returns></returns>
        public double getDouble(string name)
        {
            object value = getObject(3, name);
            return value == null ? 0 : (double)value;
        }
        /// <summary>
        /// 获取指定类型数据
        /// </summary>
        /// <param name="type">0字符型 1整型 2浮点型 3双精度浮点</param>
        /// <param name="name">参数名</param>
        /// <returns></returns>
        public object getObject(int type,string name)
        {
            object value=null;
            string temp = "";
            try
            {

                if (requestType == 0) temp = HttpContext.Current.Request[name].ToString();
                else if (requestType == 1) temp = HttpContext.Current.Request.QueryString[name].ToString();
                else
                {
                    temp = HttpContext.Current.Request.Form[name].ToString();
                }
                if (type == 1) value = int.Parse(temp);
                else if (type == 2) value = float.Parse(temp);
                else if (type == 3) value = double.Parse(temp);
                else if (type == 4) value = byte.Parse(temp);
                else { value = temp; }
            }
            catch (Exception ex)
            {
                if (requestType == 1)
                {
                    throw new NullReferenceException("参数[" + name + "]获取失败");
                }
                else if (requestType == 2)
                {
                    throw new NullReferenceException(ex.ToString());
                }
            }
            return value;
        }
    }
    
}
