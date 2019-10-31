using System;
using System.Collections.Generic;
using System.Text;

namespace MWMS
{
    public class ReturnValue
    {
        public int errNo = 0;
        public string errMsg = "";
        public object userData = null;//用户数据
        public ReturnValue()
        {
        }
        public ReturnValue(object data)
        {
            this.userData = data;
        }
        public static ReturnValue Err(string msg=null, int err=-1)
        {
            ReturnValue info=new ReturnValue();
            info.errNo = err;
            info.errMsg = msg;
            return info;
        }
        public static ReturnValue Success(object data)
        {
            ReturnValue info = new ReturnValue(data);
            return info;
        }
    }
}