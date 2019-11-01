using MWMS.Helper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MWMS.DAL.Datatype
{
    [Serializable]
    public class Field
    {
        /// <summary>
        /// 转换方式
        /// </summary>
        public enum ConvertType
        {
            /// <summary>
            /// 用户数据类型
            /// </summary>
            UserData=0,
            /// <summary>
            /// Sql数据类弄型
            /// </summary>
            SqlData=1
        }
        public string name = "";
        public object value = "";
        public string text = "";
        public string type = "";
        public int minLenth = 0;
        public int width = 150;
        public string control = "";
        public bool visible = false;
        public bool isTitle = false;
        public bool isPublicField = false;//是否为公共字段
        public bool isNecessary = false;//是否必要字段
        public string format = "";//格式
        public Field(string _name, object _value)
        {
            this.name = _name;
            this.value = _value;
        }
        public Field(string structure)
        {
            string[] TS = structure.Split('|');
            for (int n = 0; n < TS.Length; n++)
            {
                if (TS[n] != "")
                {
                    string[] FL = TS[n].Split('-');
                    name = FL[0];
                    text = FL[1];
                    type = FL[2];
                    if (FL[3] != "") minLenth = int.Parse(FL[3]);
                    if (FL[4] != "") maxLenth = int.Parse(FL[4]);
                    if (FL[6] != "") format = FL[6];
                    control = FL[5];
                    if (name == "id") isNecessary = true;
                    if (name == "orderId") isNecessary = true;
                    if (name == "auditMsg") isNecessary = true;

                }
            }
        }
        int _maxLenth = 0;
        public int maxLenth
        {
            get { return _maxLenth; }
            set { width = value * 8; _maxLenth = value; if (width > 300) width = 300; }
        }
        public string GetTypeName()
        {
            string typeName = "";
            switch (type)
            {
                case "String":
                    typeName = "string";
                    break;
                case "Number":
                    typeName = "int";
                    break;
                case "Double":
                    typeName = "double";
                    break;
                case "DateTime":
                    typeName = "DateTime";
                    break;
                case "Files":
                    typeName = "MWMS.DAL.Datatype.FieldType.Files";
                    break;
                default:
                    typeName = "object";
                    break;
            }
            return typeName;
        }
        public object Convert(object data, ConvertType convertType)
        {
            object value = null;
            switch (type)
            {
                case "String":
                    value = data.ToStr();
                    break;
                case "Number":
                    value = data.ToInt();
                    break;
                case "Double":
                    value = data.ToDouble();
                    break;
                case "DateTime":
                    try
                    {
                        value =  DateTime.Parse(data.ToString());
                    }
                    catch
                    {
                        return null;
                    }
                    break;
                case "Files":
                    FieldType.Files file = FieldType.Files.Parse(data.ToString());
                    if (convertType == ConvertType.UserData)
                    {
                        value = file;
                    }
                    else
                    {
                        if (file != null) value = file.ToJson();
                        else { value = ""; }
                    }
                    break;
                case "Pictures":
                    FieldType.Pictures file2 = FieldType.Pictures.Parse(data.ToString());
                    if (convertType == ConvertType.UserData)
                    {
                        value = file2;
                    }
                    else
                    {
                        if (file2 != null) value = file2.ToJson();
                        else { value = ""; }
                    }
                    break;
                default:
                    value = data;
                    break;
            }
            return value;
        }
    }
}
