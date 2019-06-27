using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RazorEngine.Core
{
    public static class Common
    {
        public static string StrToMD5(string inputValue)
        {
            string result3 = "";
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inputValue));
                var strResult = BitConverter.ToString(result);
                 result3 = strResult.Replace("-", "");
            }
            return result3;

        }
    }
}
