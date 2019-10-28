using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M5.Common
{

    [Serializable]
    public class FieldInfo
    {
        public string name = "";
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
        public FieldInfo()
        {
        }
        int _maxLenth = 0;
        public int maxLenth
        {
            get { return _maxLenth; }
            set { width = value * 8; _maxLenth = value; if (width > 300) width = 300; }
        }
    }
}
