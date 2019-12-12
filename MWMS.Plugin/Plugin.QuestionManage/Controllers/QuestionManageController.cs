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
    public class QuestionManageController : ManagerBase
    {
        public QuestionManageController() : base("u_question")
        {
        }
        public ReturnValue setAnswerId()
        {
            ReturnValue info = new ReturnValue();
            double id = s_request.getDouble("id");
            double answerId = s_request.getDouble("answerId");
            Sql.ExecuteNonQuery("update u_question set u_question_answerId=@answerId where id=@id", new MySqlParameter[] { new MySqlParameter("answerId", answerId), new MySqlParameter("id", id) });
            return info;
        }
        public ReturnValue readAnswer()
        {
            double id = s_request.getDouble("id");
            ReturnValue info = new ReturnValue();
            Dictionary<string, object> data = Sql.ExecuteDictionary("select content,dataId,id from u_question_answer where id=@id", new MySqlParameter[] { new MySqlParameter("id", id) });
            info.userData = data;
            return info;
        }
        public ReturnValue delData(string ids,double moduleId,double classId,int tag)
        {
            double dataTypeId = -1;
            Permissions p = null;
            if (classId < 8 || classId == moduleId)
            {
                MySqlDataReader rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new MySqlParameter[] { new MySqlParameter("moduleId", moduleId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
                p = loginInfo.value.getModulePermissions(moduleId);

            }
            else
            {
                MySqlDataReader rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new MySqlParameter[] { new MySqlParameter("classId", classId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
                p = loginInfo.value.getColumnPermissions(classId);
            }
            ReturnValue info = new ReturnValue();
            if (p.delete)
            {
                if (p.audit)
                {
                    info = TableInfo.delData(dataTypeId, ids, tag == 1, loginInfo.value);
                }
                else
                {
                    info = TableInfo.delData(dataTypeId, ids, tag == 1, loginInfo.value);
                }
                if (tag == 1) Sql.ExecuteNonQuery("delete from u_question_answer where dataId in (" + ids + ")");

            }
            else
            {
                info.errNo = -1;
                info.errMsg = "权限不足";
            }
            return info;
        }
        public ReturnValue delAnswer()
        {
            ReturnValue info = new ReturnValue();
            double dataId = s_request.getDouble("dataId");
            string ids = s_request.getString("ids");
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            Sql.ExecuteNonQuery("delete from u_question_answer where id in (" + ids + ")");
            Sql.ExecuteNonQuery("update u_question set u_question_answerCount=(select count(1) from u_question_answer where u_question.id=dataId) where id=@dataId", new MySqlParameter[] {
            new MySqlParameter("dataId",dataId)
        });
            return info;
        }
        public ReturnValue list()
        {
            double dataId = s_request.getDouble("dataId");
            ReturnValue info = new ReturnValue();
            ArrayList data = Sql.ExecuteArray("select id,content,createDate from u_question_answer where dataId=@dataId order by createDate desc", new MySqlParameter[] { new MySqlParameter("dataId", dataId) });
            info.userData = data;
            return info;
        }
        public ReturnValue editAnswer()
        {
            ReturnValue info = new ReturnValue();
            double dataId = s_request.getDouble("dataId");
            double id = s_request.getDouble("id");
            string content = s_request.getString("content");
            if (id > 0)
            {
                Sql.ExecuteNonQuery("update u_question_answer set content=@content where id=@id", new MySqlParameter[] {
                new MySqlParameter("id",id),
                new MySqlParameter("dataId",dataId),
                new MySqlParameter("content",content)
            });
            }
            else
            {
                Sql.ExecuteNonQuery("insert into  u_question_answer  (id,dataId,content,userId,createDate)values(@id,@dataId,@content,@userId,getDate()) ", new MySqlParameter[] {
                new MySqlParameter("id",double.Parse(Tools.GetId())),
                new MySqlParameter("dataId",dataId),
                new MySqlParameter("content",content),
                new MySqlParameter("userId",loginInfo.value.id)
            });
            }

            Sql.ExecuteNonQuery("update u_question set u_question_answerCount=(select count(1) from u_question_answer where u_question.id=dataId) where id=@dataId", new MySqlParameter[] {
            new MySqlParameter("dataId",dataId)
        });
            return info;
        }
        public override ReturnValue edit(double classId)
        {
            ReturnValue info = new ReturnValue();
            RecordClass value = new RecordClass(32123421234,loginInfo.value);
            value.tableName = "u_question";
            double id = s_request.getDouble("id");
            Permissions p = loginInfo.value.getColumnPermissions(classId);
            if (!p.write)
            {
                info.errNo = -1;
                info.errMsg = "没有权限";
                return info;
            }
            value.addField("classId", classId);
            value.addField("title", s_request.getString("title"));
            value.addField("u_keyword", s_request.getString("u_keyword"));
            string u_content = s_request.getString("u_content");
            value.addField("u_content", u_content);
            value.addField("u_answerCount", 0);
            if (id > 0)
            {
                info.userData = value.update(id);
                RecordClass.addKeyword(id, s_request.getString("u_keyword"), 32123421234);
            }
            else
            {
                if (!p.delete && !p.audit) value.addField("orderId", -1);
                double dataId = value.insert();
                info.userData = dataId;
                RecordClass.addKeyword(dataId, s_request.getString("u_keyword"), 32123421234);
            }
            return info;
        }

    }

}
