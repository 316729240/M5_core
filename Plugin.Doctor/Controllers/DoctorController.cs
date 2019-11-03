using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using M5.Common;
using M5.Main.Manager;
using MWMS;
using MWMS.Helper.Extensions;
using MWMS.Helper;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MWMS.Plugin
{
    [LoginAuthorzation]
    public class DoctorController : ManagerBase
    {
        public DoctorController() : base("u_doctor")
        {
        }
    }

}
