using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MWMS.Helper.Extensions
{
    public static class ConvertType
    {
        

        public static int ToInt(this object obj)
        {
            try
            {
                return Convert.ToInt32(obj);
            }
            catch
            {
                return 0;
            }
        }

        public static double ToDouble(this object obj)
        {
            try
            {
                return Convert.ToDouble(obj);
            }
            catch
            {
                return 0;
            }
        }

        public static string ToStr(this object obj)
        {
            try
            {
                return Convert.ToString(obj);
            }
            catch
            {
                return "";
            }
        }
    }
}
