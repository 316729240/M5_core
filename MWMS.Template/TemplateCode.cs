using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Web;
using System.Xml;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine;
using System.Collections;
using MWMS.DataExtensions;

namespace MWMS
{


    public class TemplateCode
    {
        struct pageBar
        {
            public int pageNo;
            public int recordCount;
            public int pageSize;
        }
        public int layerCount = 0;//深度
        public bool isEdit = false;//是否为编辑状态
        List<string> _viewList = new List<string>();

        Dictionary<string, string> variables = new Dictionary<string, string>();
        static Dictionary<string, object> temp = new Dictionary<string, object>(); //存储临时变量
        string _html,_fileName;
        public TemplateCode(string fileName,string code)
        {
            _html = code;
            _fileName = fileName;
        }
        public void addVariable(string name, string value)
        {
            variables[name] = value;
        }
        public void compile()
        {

            Regex r = new Regex(@"(\b(page|sys|config))\.((\w|\.|\[|\]){1,30})(\(|)", RegexOptions.IgnoreCase);
            _html = r.Replace(_html, new MatchEvaluator(_variable2));

             r = new Regex(@"view(.*?)\(", RegexOptions.IgnoreCase);
            _html = r.Replace(_html, new MatchEvaluator(_variable3));


            TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration
            {
                CatchPath = Request.MapPath("~" + Config.cachePath + "assembly/")
            };
            Razor.SetTemplateService(new TemplateService(templateConfig));
            _html = "@using System.Collections\r\n@{ Dictionary<string, string> sys=( Dictionary<string, string>)Model[0];Dictionary<string, object> page=( Dictionary<string, object>)Model[1];var loginUser=(new LoginInfo()).value;}" + _html;
            
            r = new Regex(@"(<|&lt;)!-- #(.*?)#[\s\S]*?--(>|&gt;)", RegexOptions.IgnoreCase);
            _html = r.Replace(_html, new MatchEvaluator(_variable4));
            RazorEngine.Razor.Compile(_html, typeof(object[]), _fileName , true);

        }
        string _variable2(Match m)
        {
            string newstr = "";
            string newvalue = m.Value.SubString("^", @"((\.\b(\w*)\()|$)");

            return m.Value.Replace(newvalue, "TemplateCode.getVariableString(\"" + newvalue + "\")");
        }
        string _variable4(Match m)
        {
            #region 自定义标签
            string Label = m.Value.SubString("<!-- ", "(\r\n)|(\n)");
            string LabelID = TemplateEngine.getFieldString(m.Value, "labelId");
            string Type = m.Value.SubString("{", "=");
            if (Label == "#Label#")//自定义标签视图
            {
                try
                {
                    string html = getLabel(m.Value);
                    return html;
                }
                catch
                {
                    return "<font color=\"#FF0000\">标签错误:</font>" + HttpContext.Current.Server.HtmlEncode(m.Value);
                }

            }
            else if (Label == "#SqlLabel#")
            {
                try
                {
                    string html = getSqlLabel(m.Value);
                    return html;
                }
                catch (Exception ex)
                {
                    return "<font color=\"#FF0000\">标签错误:</font>" + HttpContext.Current.Server.HtmlEncode(m.Value);
                }
            }
            else if (Label == "#PageBar#")
            {
                int showCount = getFieldInt(m.Value, "showCount");
                string pageBarId = getFieldString(m.Value, "pageBarId");
                string FG = m.Value.SubString("FG=", "(\r\n)|(\n)");
                string html = m.Value.SubString("<HtmlTemplate>", "</HtmlTemplate>");
                if (variables.ContainsKey("pageBar_" + pageBarId))
                {
                    return "";
                    //pageBar p = (pageBar)variables["pageBar_" + pageBarId];
                    //p.pageNo = (int)getVariable("public._pageNo");
                    //return showPage(html, p.recordCount, p.pageSize, showCount, p.pageNo, FG);

                }
                else
                {
                    return m.Value;
                    //throw new NullReferenceException("分页代码无效:无匹配数据标签，建议删除");
                }

            }
            else
            {

                return m.Value;
            }
            #endregion
        }
        public static string getVariableString(string name)
        {
            string newstr = "";
            string[] value = name.Split('.');
            try
            {
                if (value[0] == "sys")
                {
                    newstr = Config.systemVariables[value[1]].ToString();
                }
                else if (value[0] == "page") {
                    /*for (int i = 1; i < value.Length; i++)
                    {
                        newstr = Config.systemVariables[value[i]].ToString();
                        //newstr += "[\"" + value[i] + "\"]";
                    }*/
                } else if (value[0] == "config"){
                    XmlNode node = findControl(MWMS.Config.userConfig[value[1]], value[2]);
                    if (node != null)
                    {

                        if (node.ChildNodes[0].NodeType == XmlNodeType.Text)
                        {
                            return node.InnerText;
                        }
                        else
                        {
                            /*
                            object[] list = new object[node.ChildNodes.Count];
                            for (int i1 = 0; i1 < node.ChildNodes.Count; i1++) list[i1] = node.ChildNodes[i1];
                            return list;*/
                        }
                    }
                    return null;
                    //newstr=setVariable(Config.userConfig,name,null).ToString();
                }
            }catch
            {
                return "";
            }
            return newstr;
        }
        List<object> sql_p = null;
        #region 子域名地址处理
        public void replaceSubdomains(ref string Html, bool isMobile)
        {
            bool isMobileHost = BaseConfig.mobileUrl.IndexOf("http") == 0;//手机站点为独立域名
            #region 处理所有子域名问题
            if (Config.domainList != null)
            {
                for (int i = 0; i < Config.domainList.Count; i++)
                {

                    string url = Config.webPath + "/" + Config.domainList[i][1] + "/";
                    if (isMobile)
                    {
                        if (isMobileHost)
                        {
                            if (String.Compare(Config.domainList[i][0], HttpContext.Current.Request.Url.Host, true) == 0) {
                                Html = Regex.Replace(Html, "(?<=(href|src|action)=(\"|'| ))" + url + "(?=(.*?)(?=(\"|'| |>)))", BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                            }
                        }
                        else
                        {
                            if (!BaseConfig.urlConversion && String.Compare(Config.domainList[i][0], HttpContext.Current.Request.Url.Host, true) == 0)//非绝对路径转换
                            {
                                Html = Regex.Replace(Html, "(?<=(href|src|action)=(\"|'| ))" + url + "(?=(.*?)(?=(\"|'| |>)))", "/" + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                            }
                            else
                            {
                                Html = Regex.Replace(Html, "(?<=(href|src|action)=(\"|'| ))" + url + "(?=(.*?)(?=(\"|'| |>)))", url + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                            }
                        }
                    }
                    else
                    {
                        if (Config.domainList[i][0] == "") continue;
                        if (!BaseConfig.urlConversion && String.Compare(Config.domainList[i][0], HttpContext.Current.Request.Url.Host, true) == 0)//非绝对路径转换
                        {
                            Html = Regex.Replace(Html, "(?<=(href|src|action)=(\"|'| ))" + url + "(?=(.*?)(?=(\"|'| |>)))", "/", RegexOptions.IgnoreCase);
                        }
                        else
                        {
                            Html = Regex.Replace(Html, "(?<=(href|src|action)=(\"|'| ))" + url + "(?=(.*?)(?=(\"|'| |>)))", "http://" + Config.domainList[i][0] + "/", RegexOptions.IgnoreCase);
                        }
                    }
                }
            }
            #endregion
        }
        #endregion
        #region 路径处理
        public void replaceUrl(ref string html)
        {
            html = Regex.Replace(html, "(?<=(href|src|action)=(\"|'| ))(?!(http))(.*?)(?=(\"|'| |>))", new MatchEvaluator(_replaceUrl), RegexOptions.IgnoreCase);

        }
        public bool isMobile = false;
        bool isMobileHost = BaseConfig.mobileUrl.IndexOf("http") == 0;//手机站点为独立域名
        public static string _replaceUrl(string url)
        {
            return _replaceUrl(url, false, false);
        }
        public static string _replaceUrl(string url,bool isMobile, bool isMobileHost)
        {
            bool flag = false;//是否存在虚拟站点
            if (Config.domainList != null)
            {
                for (int i = 0; i < Config.domainList.Count; i++)
                {

                    string virtualWebDir = Config.webPath + "/" + Config.domainList[i][1] + "/";
                    if (Regex.IsMatch(url, virtualWebDir, RegexOptions.IgnoreCase))
                    {
                        if (Config.domainList[i][0] == "")
                        {
                            //绑定目录
                            if (isMobile)
                            {
                                if (isMobileHost)
                                {
                                    url = Regex.Replace(url, "^" + virtualWebDir, BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                                }
                                else
                                {
                                    if (!Regex.IsMatch(url, "^" + virtualWebDir + BaseConfig.mobileUrl, RegexOptions.IgnoreCase)) url = Regex.Replace(url, "^" + virtualWebDir, virtualWebDir + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                                }
                            }
                        }
                        else
                        {
                            //绑定域名
                            if (isMobile)
                            {
                                if (Config.domainList[i][2] != "")//绑定有手机域名
                                {
                                    url = Regex.Replace(url, "^" + virtualWebDir, "http://" + Config.domainList[i][2] + "/", RegexOptions.IgnoreCase);
                                }
                                else if (isMobileHost)
                                {
                                    url = Regex.Replace(url, "^" + virtualWebDir, BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                                }
                                else
                                {
                                    url = Regex.Replace(url, "^" + virtualWebDir, "http://" + Config.domainList[i][0] + "/" + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                                }
                            }
                            else
                            {
                                url = Regex.Replace(url, "^" + virtualWebDir, "http://" + Config.domainList[i][0] + "/", RegexOptions.IgnoreCase);
                            }

                        }
                        flag = true;
                        break;
                    }
                }
            }
            #region 不存在虚拟站点的地址处理
            if (!flag)
            {
                if (isMobile)
                {
                    if (isMobileHost)
                    {
                        url = Regex.Replace(url, "^" + Config.webPath + "/", BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        if (!Regex.IsMatch(url, "^" + Config.webPath + "/" + BaseConfig.mobileUrl, RegexOptions.IgnoreCase)) url = Regex.Replace(url, "^" + Config.webPath + "/", Config.webPath + "/" + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                    }
                }
            }
            #endregion
            if (BaseConfig.urlConversion && BaseConfig.mainUrl != null)
            {
                try
                {
                    Uri u = new Uri(BaseConfig.mainUrl, url);
                    url = u.ToString();
                }
                catch
                {
                }
            }
            return url;
        }
        string _replaceUrl(Match m)
        {
            if (!Regex.IsMatch(m.Value, "(" + BaseConfig.extension + "|/)$", RegexOptions.IgnoreCase)) return m.Value;
            string url =_replaceUrl(m.Value, isMobile,isMobileHost);
            return url;
        }
        #endregion
        string _variable3(Match m)
        {
            string item = m.Value.SubString(@"view\.", @"\(");
            //Dictionary<string, object> list =  (Dictionary<string, object>)Config.viewVariables[item[0]];
            //string viewId = list[item[1]].ToString();
            return "MWMS.TemplateCode.readView(\""+ item+"\"";
        }
        public static string readView(string viewPath)
        {
            string[] item = viewPath.Split('.');
            Dictionary<string, object> list =  (Dictionary<string, object>)Config.viewVariables[item[0]];
            object [] obj = (object [])list[item[1]];
            TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration
            {
                CatchPath = HttpContext.Current.Server.MapPath("~" + Config.cachePath + "assembly/")
            };
            Razor.SetTemplateService(new TemplateService(templateConfig));
            RazorEngine.Razor.Compile((string)obj[1], typeof(object[]),obj[0].ToString(), false);
            return RazorEngine.Razor.Run(obj[0].ToString(), new object[] { Config.systemVariables, null });
        }
        public static ArrayList getLabel(string labelId,string html, double moduleId,double classId,int pageSize,int recordCount,double datatypeId,int orderBy,string _fields,string attribute,string addWhere,bool debug, Hashtable p1)
        {
            StringBuilder Sql = new StringBuilder("");
            object temp_sql = addWhere;
            if (temp_sql != null) addWhere = temp_sql.ToString();
            SqlParameter[] sql_p = null;
            if (p1.Count > 0)
            {
                sql_p = new SqlParameter[p1.Count];
                int i = 0;
                foreach (DictionaryEntry item in p1)
                {
                    sql_p[i] =  new SqlParameter(item.Key.ToString(),item.Value);
                    i++;
                }
            }
            /*
            _variable4_sql
           p = new SqlParameter[sql_p.Count];
            for (int i = 0; i < sql_p.Count; i++)
            {
                p[i] = new SqlParameter("p_" + (i + 1).ToString(), sql_p[i]);
            }*/
            //renderSql(ref addWhere, ref sql_p);
            TableInfo tableInfo = new TableInfo(datatypeId);

            string orderByStr = "order by A.orderid desc,A.createdate desc";
            if (orderBy == 1) orderByStr = "order by A.orderid desc,A.createdate desc";
            else if (orderBy == 3) orderByStr = "order by A.clickcount desc";
            bool showPic = false;
            string isPic = TemplateEngine.getFieldString(html, "isPic");
            if (isPic == "true" || isPic == "1") showPic = true;
            string fieldList = "";
            bool classFlag = false;
            List<string> fields = new List<string>();
            string[] flist = _fields.Split(',');
            for (int i = 0; i < flist.Length; i++)
            {
                string name = flist[i];
                if (fields.IndexOf(name) == -1) fields.Add(name);
                //if (name == "pic") showPic = true;

            }
            bool infoFlag = false;
            for (int i = 0; i < fields.Count; i++)
            {
                if (i > 0) fieldList += ",";
                if (fields[i].IndexOf("u_") > -1)
                {
                    infoFlag = true;
                    fieldList += "B." + fields[i];
                }
                else
                {
                    if (fields[i].ToLower() == "classurl")
                    {
                        fieldList += "C.url classUrl";
                        classFlag = true;
                    }
                    else if (fields[i].ToLower() == "classname")
                    {

                        fieldList += "C." + fields[i];
                        classFlag = true;
                    }
                    else
                    {
                        fieldList += "A." + fields[i];
                    }
                }
            }
            //if (pageSize > 0) fieldList += ",row_number() OVER(" + orderByStr + ") row_number";
            string sql = "select ";
            string countSql = "select count(1) ";
            string maxSql = "select min(A.id),max(A.id) ";
            if (pageSize == 0 && recordCount > 0) { }
            else
            {
                fieldList += ",row_number() OVER(" + orderByStr + ") row_number";
            }
            sql += " " + fieldList + " from [mainTable] A WITH(NOLOCK) ";
            countSql += " from [mainTable] A WITH(NOLOCK) ";
            maxSql += " from [mainTable] A WITH(NOLOCK) ";

            if (infoFlag || (pageSize > 0 && addWhere.IndexOf("u_") > -1))
            {
                sql += " inner join [" + tableInfo.tableName + "] B WITH(NOLOCK) on A.id=B.id ";
                countSql += " inner join [" + tableInfo.tableName + "] B WITH(NOLOCK) on A.id=B.id ";
            }
            if (classFlag)
            {
                sql += " inner join [class] C WITH(NOLOCK) on A.classId=C.id ";
                countSql += " inner join [class] C WITH(NOLOCK) on A.classId=C.id ";
            }
            int pageNo = 1;
            StringBuilder where = new StringBuilder(" where  A.orderid>-1 and A.createdate<getdate() ");
            if (showPic) where.Append(" and A.pic<>'' ");
            string max_where = "";


            #region 属性文章
            if (attribute != "")
            {
                
                string[] attr = attribute.Split(',');
                string attrWhere = "";
                for (int nn = 0; nn < attr.Length; nn++)
                {
                    if (attr[nn].Trim() != "")
                    {
                        int attrC = int.Parse(attr[nn]);
                        if (attrC > 0 && attrC < 6)
                        {
                            if (attrWhere != "") attrWhere += " and ";
                            attrWhere += " A.attr" + (int.Parse(attr[nn]) - 1).ToString() + "=1";
                        }
                    }
                }
                if (attrWhere != "") where.Append(" and " + attrWhere);
            }
            #endregion
            if (classId > 0)
            {
                SqlDataReader rs2 = Helper.Sql.ExecuteReader("select rootId,childId,classId from [class] where  id=@id", new SqlParameter[] { new SqlParameter("id", classId) });
                if (rs2.Read())
                {
                    if (rs2.GetDouble(2) == 7)//是否为频道
                    {
                        where.Append(" and A.rootId=" + classId.ToString());
                        max_where = " A.rootId=" + classId.ToString();
                    }
                    else
                    {
                        where.Append(" and A.classId in (" + rs2[1].ToString() + ")");
                        max_where = " A.classId in (" + rs2[1].ToString() + ")";
                    }
                }
                else
                {
                    where.Append(" and A.classId=" + classId.ToString());
                    max_where = " A.classId=" + classId.ToString();
                }
                rs2.Close();
            }
            else if (moduleId > 0)
            {
                where.Append(" and A.moduleId=" + moduleId);
                max_where = " A.moduleId=" + moduleId;
            }
            //Regex r1 = new Regex("{(FunctionName|FieldName)[^{]*?}");
            //MatchCollection hc = r1.Matches(template.ToString());

            if (orderBy == 2)//随机查询
            {
                string num = Regex.Replace((labelId + HttpContext.Current.Request.Url.AbsolutePath).MD5(), "[A-Z]", "");
                if (num.Length > 9) num = num.Substring(0, 8);
                double minId = 0, maxId = 0;
                int chazhi = 0;
                SqlDataReader rs2 = Helper.Sql.ExecuteReader(maxSql + (max_where == "" ? "" : (" where " + max_where)));
                if (rs2.Read())
                {
                    if (!rs2.IsDBNull(0)) minId = rs2.GetDouble(0);
                    if (!rs2.IsDBNull(1)) maxId = rs2.GetDouble(1);
                }
                rs2.Close();
                chazhi = Convert.ToInt32(maxId - minId);
                Random rnd = new Random(int.Parse(num));
                where.Append(" and A.id>" + (minId + rnd.Next(chazhi)).ToString());
                orderByStr = "";
            }
            if (addWhere != "") where.Append(" and " + addWhere);
            ArrayList rs1 = null;
            int RC = 0;
            if (pageSize > 0)
            {
                pageBar p = new pageBar();
                p.recordCount = (int)Helper.Sql.ExecuteScalar(countSql + where, sql_p);
                SafeReqeust request = new SafeReqeust(0, 0);
                pageNo = (pageNo == 0 ? 1 : pageNo);
                p.pageSize = pageSize;
                p.pageNo = pageNo;
                int pageCount = (p.recordCount - 1) / p.pageSize + 1;
                if (pageNo < 1 || pageNo > pageCount)
                {
                    API.ERR404("页码不正确");
                }

                sql = "select top " + pageSize.ToString() + " * from (" + sql + where.ToString() + ")L where L.row_number>" + (pageSize * (pageNo - 1)).ToString();
                //sql = sql + " where A.id in (" + tempsql + ")";
                //sql = "select top " +pageSize.ToString() + " * from (" +
                //sql +where+
                //")L where L.row_number>"+(pageSize*(pageNo-1)).ToString();

                //if (debug) return "调试：" + sql;
                rs1 = Helper.Sql.ExecuteArray(sql, sql_p);

            }
            else
            {
                if (recordCount == 0) recordCount = 100000;
                sql += " inner join  (select top " + recordCount.ToString() + " A.id from maintable A WITH(NOLOCK) ";
                if (addWhere.IndexOf("u_") > -1) sql += " inner join [" + tableInfo.tableName + "] B WITH(NOLOCK) on A.id=B.id ";
                sql += where.ToString() + " " + orderByStr + ") H on A.id=H.id ";
                //if (debug) return "调试：" + sql;
                //rs1 = Helper.Sql.ExecuteReader(sql + where.ToString() + " " + orderByStr);
                rs1= Helper.Sql.ExecuteArray(sql, sql_p);
            }
            return rs1;
        }

        /// <summary>
        /// 获取变量参数
        /// </summary>
        /// <param name="pstr">参数</param>
        /// <returns></returns>
        string getFP(string html,string name,int type)
        {
            string value = html.SubString(name + "=", "(\r\n)|(\n)");
            if (value != "")
            {
                if (value.Substring(0, 1) == "@") return value;
            }
            if (type == 0)
            {
                value = value.Replace("\"", "\\\"");
                value = "\"" + value + "\"";
            }
            else if (type == 1)
            {
                if (value == "") value = "0";
             }
            else if (type == 2)
            {
                value = value.ToLower();
                if (value == "") value = "false";
                if (value == "0") value = "false";
                if (value == "1") value = "true";
                value = value == "true" ? "true" : "false";
            }
            return value;
        }

        string getFP(string html, string name)
        {
            return getFP(html,name,0);
        }

        string getSqlLabel(string html)
        {
            string labelId = TemplateEngine.getFieldString(html, "labelId");

            string pageSize = getFP(html, "pageSize", 1);
            string recordCount = getFP(html, "recordCount", 1);
            string debug = getFP(html, "debug", 2);
            string[] fields = getFieldString(html, "fields").Split(',');
            string sql = getFP(html, "sql");
            string template = API.GetStrFG(html, "<HtmlTemplate>", "</HtmlTemplate>");
            StringBuilder outhtml = new StringBuilder("");
            Regex r = new Regex(@"\@((\w|\.|\[|\]){1,30})", RegexOptions.IgnoreCase);
            outhtml.Append("@{\r\n");
            outhtml.Append("if(true){\r\n");
            outhtml.Append(buildSql(ref sql) + "\r\n");
            outhtml.Append("ArrayList list=MWMS.TemplateCode.getSqlLabel(\"");
            outhtml.Append(labelId + "\",");
            outhtml.Append(sql+ ",");
            outhtml.Append(pageSize + ",");
            outhtml.Append(recordCount + ",");
            outhtml.Append(debug + ",");
            outhtml.Append("p);\r\n");
            outhtml.Append("for(int index=0;index<list.Count;index++){\r\n");
            outhtml.Append("var item = (System.Collections.Generic.Dictionary<string, object>)list[index];\r\n");
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] == "url")
                {
                    outhtml.Append("var " + fields[i] + "=Config.webPath + item[\"" + fields[i] + "\"] + \".\" + BaseConfig.extension;\r\n");
                }
                else
                {
                    outhtml.Append("var " + fields[i] + "=item[\"" + fields[i] + "\"];\r\n");
                }
            }
            outhtml.Append(template);
            outhtml.Append("\r\n}\r\n");
            outhtml.Append("}\r\n");
            outhtml.Append("}");
            return outhtml.ToString();

        }
        /*
        static void renderSql(ref string html, ref SqlParameter[] p)
        {
            List<object> sql_p = new List<object>();
            Regex r = new Regex(@"({if(.*?)}[\s\S]*?{/if})|(\$\{(.[^}]*?)\})|(<|&lt;)!-- #(.*?)#[\s\S]*?--(>|&gt;)|(\$((\w|\.|\[|\]){1,30}))", RegexOptions.IgnoreCase);
            html = r.Replace(html, new MatchEvaluator(html));
            p = new SqlParameter[sql_p.Count];
            for (int i = 0; i < sql_p.Count; i++)
            {
                p[i] = new SqlParameter("p_" + (i + 1).ToString(), sql_p[i]);
            }
        }*/
        public static ArrayList getSqlLabel(string labelId,string sql, int pageSize,int recordCount,bool debug,Hashtable p1)
        {

            SqlParameter[] sql_p = null;
            if (p1.Count > 0)
            {
                sql_p = new SqlParameter[p1.Count];
                int i = 0;
                foreach (DictionaryEntry item in p1)
                {
                    sql_p[i] = new SqlParameter(item.Key.ToString(), item.Value);
                    i++;
                }
            }
            //renderSql(ref sql, ref sql_p);
            int pageNo = 0;
            string orderBy = "";
            if (pageSize > 0)
            {
                string[] temp = Regex.Split(sql, "order by", RegexOptions.IgnoreCase);
                if (temp.Length > 1)
                {
                    sql = temp[0];
                    orderBy = temp[1];
                }

                string fieldList = sql.SubString("select", "from");
                string tempsql = sql.Substring(("select" + fieldList).Length);
                string countSql = "select count(1) " + tempsql;
                sql = "select" + fieldList + ",row_number() OVER(order by " + (orderBy == "" ? "(select 0)" : orderBy) + ") row_number " + tempsql;
                //sql = sql.Replace(fieldList, fieldList + ",row_number() OVER(order by " + (orderBy == "" ? "(select 0)" : orderBy) + ") row_number ");
                pageBar p = new pageBar();
                p.recordCount = (int)Helper.Sql.ExecuteScalar(countSql, sql_p);
                SafeReqeust request = new SafeReqeust(0, 0);
               // pageNo = (int)getVariable("public._pageNo");
                pageNo = (pageNo == 0 ? 1 : pageNo);
                p.pageSize = pageSize;
                p.pageNo = pageNo;

                int pageCount = (p.recordCount - 1) / p.pageSize + 1;
                if (pageNo < 1 || pageNo > pageCount)
                {
                    API.ERR404("页码不正确");
                }
                //variables["pageBar_" + labelId] = p;
                sql = "select top " + pageSize.ToString() + " * from (" +
                sql +
                ")L where L.row_number>" + (pageSize * (pageNo - 1)).ToString();
            }
            else if (recordCount > 0 && sql.IndexOf(" top ") == -1) sql = Regex.Replace(sql, "^select ", "select top " + recordCount.ToString() + " ", RegexOptions.IgnoreCase);


            //if (debug == "true") return "调试：" + sql;
            return Helper.Sql.ExecuteArray(sql, sql_p);
        }
        string buildSql(ref string str)
        {
            StringBuilder code = new StringBuilder("Hashtable p = new Hashtable();\r\n");
            Regex r = new Regex(@"\@((\w|\.|\[|\]){1,30})", RegexOptions.IgnoreCase);
            int index = 0;
            str = r.Replace(str, new MatchEvaluator((Match m) => {
                string key = m.Value.Substring(1);
                code.Append(String.Format( "p[\"p{0}\"]=@{1};\r\n",index,key));
                //p[key] = @d
                return "@p" + index.ToString();
            }));
            return code.ToString();
        }
        string getLabel(string html)
        {
            string labelId = getFP(html, "labelId");
            string pageSize = getFP(html, "pageSize",1);
            string recordCount = getFP(html, "recordCount",1);
            string moduleId = getFP(html, "moduleId",1);
            string classId = getFP(html, "classId",1);
            if (moduleId == "0" && classId == "0") return "调用错误：必须指定模块或栏目id";
            string template =API.GetStrFG(html, "<HtmlTemplate>", "</HtmlTemplate>");
            string datatypeId = getFP(html, "datatypeId",1);
            string orderBy = getFP(html, "orderBy", 1);
            string [] fields = getFieldString(html, "fields").Split(',');
            string _fields = getFP(html, "fields");
            string attribute = getFP(html, "attribute");
            string addWhere = getFP(html, "where");
            string debug = getFP(html, "debug",2);
            StringBuilder outhtml = new StringBuilder("");
            Regex r = new Regex(@"\@((\w|\.|\[|\]){1,30})", RegexOptions.IgnoreCase);
            outhtml.Append("@{\r\n");
            outhtml.Append("if(true){\r\n");
            outhtml.Append(buildSql(ref addWhere)+"\r\n");
            outhtml.Append("ArrayList list =MWMS.TemplateCode.getLabel(");
            outhtml.Append(labelId+",");
            outhtml.Append("\"\"" + ",");
            outhtml.Append(moduleId + ",");
            outhtml.Append(classId + ",");
            outhtml.Append(pageSize + ",");
            outhtml.Append(recordCount + ",");
            outhtml.Append(datatypeId + ",");
            outhtml.Append(orderBy + ",");
            outhtml.Append(_fields + ",");
            outhtml.Append(attribute + ",");
            outhtml.Append(addWhere + ",");
            outhtml.Append(debug + ",");
            outhtml.Append("p);\r\n");
            outhtml.Append("for(int index=0;index<list.Count;index++){\r\n");
            outhtml.Append("var item = (System.Collections.Generic.Dictionary<string, object>)list[index];\r\n");
            for (int i= 0;i < fields.Length;i++)
            {
                if (fields[i] == "url") {
                    outhtml.Append("var " + fields[i] + "=Config.webPath + item[\"" + fields[i] + "\"] + \".\" + BaseConfig.extension;\r\n");
                }
                else { 
                    outhtml.Append("var "+ fields[i]+ "=item[\"" + fields[i] + "\"];\r\n");
                }
            }
            outhtml.Append(template);
            outhtml.Append("\r\n}\r\n");
            outhtml.Append("}\r\n");
            outhtml.Append("}\r\n");

            return outhtml.ToString();
        }
        
            
        /// <summary>
        /// 字符串变量替换
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        string replaceStringVariable(Match m)
        {
            string value = "";
            if (m.Value.Length > 2) value = m.Value.Substring(1, m.Value.Length - 2);
            value = value.Replace("{#39}", "'");
            value = value.Replace("{#34}", "\"");
            string name = m.Value.MD5();
            temp[name] = value;
            return "temp." + m.Value.MD5();
        }
        //0字符串 1浮点型 2 变量
        int getType(ref object value)
        {
            if (value == "") { value = null; return -1; }
            string v1 = (string)value;
            if (v1.Substring(0, 1) == "\"" || v1.Substring(0, 1) == "'")
            {
                value = v1.Substring(1, v1.Length - 2);
                return 0;
            }
            else if (Regex.IsMatch(v1, @"^(\d|-)"))
            {
                value = double.Parse(v1);
                return 1;
            }
            else
            {
                //value = getVariable(v1);
                return 2;
            }
            return -1;
        }
        
        object getVariable(Dictionary<string, object> v, string key)
        {
            return setVariable(v, key, null);
        }
        static XmlNode findControl(XmlNodeList list, string name)
        {
            for (int i = 0; i < list.Count; i++)
            {

                if (list[i].Attributes["name"] != null && list[i].Attributes["name"].Value == name)
                {
                    return list[i];
                }
                else
                {
                    if (list[i].Attributes["name"] == null && list[i].ChildNodes[0].NodeType != XmlNodeType.Text)
                    {
                        XmlNode node = findControl(list[i].ChildNodes, name);
                        if (node != null) return node;
                    }
                }
            }
            return null;
        }
        static object setVariable(Dictionary<string, object> v, string key, object value)
        {
            if (key == "") return "";
            //object value = null;
            string[] keys = key.Split('.');
            if (keys[0] == "temp")
            {
                if (temp.ContainsKey(keys[1])) return temp[keys[1]];
                else { return null; }
            }
            else if (keys[0] == "cookies")
            {
                HttpCookie cookies = HttpContext.Current.Request.Cookies[keys[1]];
                if (cookies == null) return "";
                return HttpUtility.UrlDecode(cookies.Value);
            }
            else if (keys[0] == "post")
            {
                string _v = HttpContext.Current.Request.Form[keys[1]];
                return _v == null ? "" : _v;
            }
            else if (keys[0] == "config")
            {
                XmlNode node = findControl(MWMS.Config.userConfig[keys[1]], keys[2]);
                if (node != null)
                {

                    if (node.ChildNodes[0].NodeType == XmlNodeType.Text)
                    {
                        return node.InnerText;
                    }
                    else
                    {
                        object[] list = new object[node.ChildNodes.Count];
                        for (int i1 = 0; i1 < node.ChildNodes.Count; i1++) list[i1] = node.ChildNodes[i1];
                        return list;
                    }
                }
                return null;
                //return MWMS.Config.userConfig[keys[1]].Item(keys[2]);
            }
            else if (keys[0] == "get")
            {
                string _v = HttpContext.Current.Request.QueryString[keys[1]];
                return _v == null ? "" : _v;
            }
            for (int i = 0; i < keys.Length; i++)
            {

                if (i < keys.Length - 1)
                {
                    if (v != null && v.ContainsKey(keys[i]))
                    {
                        object obj = v[keys[i]];
                        try
                        {
                            if (obj.GetType().Name == "XmlElement")
                            {
                                v = new Dictionary<string, object>();
                                foreach (XmlElement value2 in ((XmlElement)obj).ChildNodes)
                                {
                                    v.Add(value2.Name, value2.InnerText);
                                }
                            }
                            else
                            {
                                v = (Dictionary<string, object>)(obj);
                            }

                        }
                        catch
                        {
                            return obj;
                        }
                    }
                }
                else
                {
                    if (value != null)
                    {
                        v[keys[i]] = value;
                        return value;
                    }
                    else
                    {
                        string name = keys[i];
                        string index = (keys[i].SubString(@"\[", @"\]"));
                        if (index != "")
                        {
                            name = Regex.Replace(keys[i], @"\[.*\]", "");
                            if (v != null && v.ContainsKey(name))
                            {
                                int _index = Convert.ToInt32(index);
                                if (v[name].GetType().Name == "ArrayList")
                                {
                                    System.Collections.ArrayList arr = (System.Collections.ArrayList)v[name];
                                    //                                    List <Dictionary<string, object>> arr = (List<Dictionary<string, object>>)v[name];
                                    return arr[_index];
                                }
                                else
                                {
                                    string[] arr = (string[])v[name];
                                    if (_index > -1 && _index < arr.Length)
                                    {
                                        return arr[_index];
                                    }
                                    else
                                    {
                                        return "";
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (v != null && v.ContainsKey(keys[i])) return v[keys[i]];
                        }
                    }
                }

            }
            return null;
        }
        public static string getFieldString(string html, string name)
        {
            string value = html.SubString(name + "=", "(\r\n)|(\n)");
            return value;
        }
        public static int getFieldInt(string html, string name)
        {
            try
            {
                int value = int.Parse(getFieldString(html, name));
                return value;
            }
            catch { return 0; }
        }
        public static double getFieldDouble(string html, string name)
        {
            try
            {
                double value = double.Parse(getFieldString(html, name));
                return value;
            }
            catch { return 0; }
        }

        public string showPage(string H, int RecordCount, int PageSize, int ShowCount, int PageNo, string FG)
        {
            if (PageNo < 1) PageNo = 1;
            #region 查找当前域名是否在子域列表中

            string domain = HttpContext.Current.Request.Url.Host;
            bool tag = false;
            //if (Constant.DomainDoc != null)
            //{
            //    for (int i = 0; i < Constant.DomainDoc.Count; i++)
            //    {
            //        if (String.Compare(Constant.DomainDoc[i].InnerText, domain, true) == 0)
            //        {
            //            domain = "http://" + domain;
            //            if (HttpContext.Current.Request.Url.Port != 80) domain += ":" + HttpContext.Current.Request.Url.Port.ToString();
            //            i = Constant.DomainDoc.Count; tag = true;
            //        }
            //    }
            //}
            if (!tag) domain = "";
            #endregion
            //        string url = API.GetWebUrl() + HttpContext.Current.Request.RawUrl;
            string url = domain + HttpContext.Current.Request.RawUrl;
            int index = 0, index2 = 0;
            while (index > -1)
            {
                index2 = index;
                index = url.IndexOf("/", index + 1);
            }
            url = url.Substring(0, index2) + "/";

            #region
            int PageCount = (RecordCount - 1) / PageSize + 1;
            int js = ShowCount / 2;
            int StartN = PageNo - (PageNo - 1) % js - js;
            if (StartN < 1) StartN = 1;
            #endregion
            StringBuilder Html = new StringBuilder(H);
            StringBuilder PageNumber = new StringBuilder();
            StringBuilder Prev = new StringBuilder();
            StringBuilder Next = new StringBuilder();
            StringBuilder FirstPage = new StringBuilder();
            StringBuilder EndPage = new StringBuilder();
            string KZM = "." + BaseConfig.extension;
            string FileName = "";// (string)getVariable("public._fileName");
            if (FileName == null) FileName = "default";
            Regex r = new Regex(@"({(Label)[^{]*?})");
            MatchCollection mc = r.Matches(Html.ToString());
            string LabelName = "", Value = "", Css = "", Color1 = "";
          
            string filename2 = FileName + KZM;
            if (String.Compare(FileName, "default", true) == 0) filename2 = url;
            FileName = url + FileName;
            for (int n = 0; n < mc.Count; n++)
            {
                LabelName = API.GetHTMLValue(mc[n].Value, "LabelName");
                Value = API.GetHTMLValue(mc[n].Value, "Value");
                Css = API.GetHTMLValue(mc[n].Value, "Css");
                Css = (Css == "") ? "" : "class=\"" + Css + "\"";
                Color1 = API.GetHTMLValue(mc[n].Value, "Color");
                if (Color1 == "") Color1 = "#FF0000";
                if (LabelName == "Prev")
                {
                    #region Prev
                    if (PageNo > 1)
                    {
                        if (PageNo == 2)
                        {
                            Prev.Append("<a href=\"" + filename2 + "\" " + Css + " val=1 >");
                        }
                        else { Prev.Append("<a href=\"" + FileName + "_" + (PageNo - 1).ToString() + KZM + "\" " + Css + " val=" + (PageNo - 1).ToString() + " >"); }
                    }
                    else { Prev.Append("<span>"); }
                    Prev.Append(Value);
                    if (PageNo > 1) Prev.Append("</a>");
                    else { Prev.Append("</span>"); }
                    #endregion
                    Html.Replace(mc[n].Value, Prev.ToString());
                }
                else if (LabelName == "PageNumber")
                {
                    #region PageNumber
                    for (int n1 = 0; n1 < ShowCount; n1++)
                    {
                        if (n1 + StartN <= PageCount)
                        {
                            //if (n1 + StartN != PageNo)
                            //{
                            if (n1 + StartN == 1)
                            { PageNumber.Append("<a href=\"" + filename2 + "\" class=\"" + Css + "\" val=1 >"); }
                            else { PageNumber.Append("<a href=\"" + FileName + "_" + (n1 + StartN).ToString() + KZM + "\" " + Css + " val=" + (n1 + StartN).ToString() + " >"); }
                            //}
                            //else { PageNumber.Append("<a href=\"" + FileName + KZM + "\" class=\"" + Css + "\">"); }
                            if (n1 + StartN == PageNo) PageNumber.Append("<font color='" + Color1 + "'><b>");
                            PageNumber.Append((n1 + StartN).ToString());
                            if (n1 + StartN == PageNo) PageNumber.Append("</b></font>");
                            PageNumber.Append("</a>");
                            PageNumber.Append(FG);
                        }
                    }
                    #endregion
                    Html.Replace(mc[n].Value, PageNumber.ToString());
                }
                else if (LabelName == "Next")
                {
                    #region Next
                    if (PageNo < PageCount) Next.Append("<a href=\"" + FileName + "_" + (PageNo + 1).ToString() + KZM + "\" " + Css + " val=" + (PageNo + 1).ToString() + ">");
                    else { Next.Append("<span>"); }
                    Next.Append(Value);
                    if (PageNo < PageCount) Next.Append("</a>");
                    else { Next.Append("</span>"); }
                    #endregion
                    Html.Replace(mc[n].Value, Next.ToString());
                }
                else if (LabelName == "FirstPage")
                {
                    #region FirstPage
                    if (PageNo != 1) FirstPage.Append("<a href=\"" + filename2 + "\" " + Css + " val=1 >");
                    else { FirstPage.Append("<span>"); }
                    FirstPage.Append(Value);
                    if (PageNo != 1) FirstPage.Append("</a>");
                    else { FirstPage.Append("</span>"); }
                    #endregion
                    Html.Replace(mc[n].Value, FirstPage.ToString());
                }
                else if (LabelName == "EndPage")
                {
                    #region EndPage
                    if (PageNo != PageCount) EndPage.Append("<a href=\"" + FileName + "_" + PageCount.ToString() + KZM + "\" " + Css + " val="+ PageCount.ToString() + ">");
                    else { EndPage.Append("<span>"); }
                    EndPage.Append(Value);
                    if (PageNo != PageCount) EndPage.Append("</a>");
                    else { EndPage.Append("</span>"); }
                    #endregion
                    Html.Replace(mc[n].Value, EndPage.ToString());
                }
            }
            Html.Replace("[PageCount]", PageCount.ToString());
            Html.Replace("[PageSize]", PageSize.ToString());
            Html.Replace("[RecordCount]", RecordCount.ToString());
            Html.Replace("[PageNo]", PageNo.ToString());
            return (Html.ToString());
        }
        public static void replaceKeyword(ref string Html)
        {
            XmlNodeList xnl = Config.userConfig["keyword"].Item(0).ChildNodes;
            if (xnl != null && xnl.Count > 0)
            {
                foreach (XmlNode xnf in xnl)
                {
                    XmlNodeList xnf1 = xnf.ChildNodes;
                    if (xnf1.Item(0).InnerText != "") Html = Html.Replace(xnf1.Item(0).InnerText, xnf1.Item(1).InnerText);
                }
            }
        }
    }
    //public class TemplateVariable
    //{
    //    Dictionary<string, object> variables=null;
    //    public TemplateVariable()
    //    {

    //    }
    //}
}
