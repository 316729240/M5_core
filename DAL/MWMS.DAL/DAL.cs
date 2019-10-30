using MWMS.DataExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MWMS.DAL
{
    public class DAL{
            public static TableHandle M(string table)
            {
                return new TableHandle(table);
            }
    }
}