<%@ WebHandler Language="C#" Class="ajax" %>

using System;
using System.Web;
using System.Collections.Generic;
using MWMS;
using Helper;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Xml;
using System.Data.OleDb;
public class ajax : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        DataTable table= GetExcelToDataTableBySheet(@"C:\Users\Administrator\Desktop\车型数据-蒙迪欧-测试用新11.xlsx", "车型表");

        string sql = "CREATE TABLE [dbo].[T_车型表]( ";
        for (int i = 0; i < table.Columns.Count; i++)
        {
            sql+="[C_"+ table.Rows[0][i].ToString()+"] [nvarchar](255) NULL, ";
        }
        sql += "[C_年款] [nvarchar](255) NULL, ";
        sql += "[C_版本] [nvarchar](255) NULL ";
        sql += ") ON [PRIMARY]";

        int c=(int)Sql.ExecuteScalar("SELECT  count(1) FROM dbo.SysObjects WHERE ID = object_id(N'[T_车型表]') AND OBJECTPROPERTY(ID, 'IsTable') = 1");
        if (c == 0) Sql.ExecuteNonQuery(sql);
        for(int i = 1; i < table.Rows.Count; i++)
        {
            string car_type = "",car_number="";
            string f1 = "", f2 = "";
            SqlParameter[] op = new SqlParameter[table.Columns.Count+2];
            for (int i1 = 0; i1 < table.Columns.Count; i1++)
            {
                f1 += "[C_" + table.Rows[0][i1].ToString() + "],";
                f2 += "@" + table.Columns[i1].ColumnName+",";
                op[i1] = new SqlParameter(table.Columns[i1].ColumnName,table.Rows[i][i1].ToString());
                if (table.Rows[0][i1].ToString() == "车型名称") car_type = table.Rows[i][i1].ToString();
                if (table.Rows[0][i1].ToString() == "车辆型号") car_number = table.Rows[i][i1].ToString();

            }
            if (car_number == "")
            {
                continue;
            }
            string[] item = car_type.Split(' ');
            string[] item2 = car_type.Split('款');
            f1 += "[C_年款],";
            f2 += "@C_年款,";
            f1 += "[C_版本]";
            f2 += "@C_版本";
            op[table.Columns.Count+0] = new SqlParameter("C_年款",item[1]);
            op[table.Columns.Count+1] = new SqlParameter("C_版本",(item2[1]).Trim());
            int c1=(int)Sql.ExecuteScalar("SELECT  count(1) FROM [T_车型表] where C_车辆型号=@car_number and C_车型名称=@name",new SqlParameter[] {new SqlParameter("car_number",car_number),new SqlParameter("name",car_type) });
            if(c1==0)Sql.ExecuteNonQuery("insert into [T_车型表] ("+f1+")values("+f2+")",op);
        }
    }

    DataTable GetExcelToDataTableBySheet(string FileFullPath, string SheetName)
    {
        try {
            //string strConn = "Provider=Microsoft.Jet.OleDb.4.0;" + "data source=" + FileFullPath + ";Extended Properties='Excel 8.0; HDR=NO; IMEX=1'"; //此连接只能操作Excel2007之前(.xls)文件
            string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + FileFullPath + ";Extended Properties='Excel 12.0; HDR=NO; IMEX=1'"; //此连接可以操作.xls与.xlsx文件
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            DataSet ds = new DataSet();
            OleDbDataAdapter odda = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}$]", SheetName), conn);                    //("select * from [Sheet1$]", conn);
            odda.Fill(ds, SheetName);
            conn.Close();
            return ds.Tables[0];
        }
        catch (Exception ex)
        {
            return null;
        }

    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}