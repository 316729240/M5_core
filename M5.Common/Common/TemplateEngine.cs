using MWMS.Helper.Extensions;
using MWMS.Helper;
using MWMS.SqlHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using M5.Base;

namespace M5.Common
{
    public class TE_statistical
    {
        public int callCount = 0;//渲染调用次数
        public int viewCount = 0;//视图数
        public int labelCount = 0;//标签数
    }

    public class TemplateEngine
    {
        struct pageBar
        {
            public int pageNo;
            public int recordCount;
            public int pageSize;
        }
        public TE_statistical TE_statistical = null;
        public int layerCount = 0;//深度
        public bool isEdit = false;//是否为编辑状态
        List<string> _viewList = new List<string>();

        Dictionary<string, object> variables = new Dictionary<string, object>();
        Dictionary<string, object> temp = new Dictionary<string, object>(); //存储临时变量
        //string _html;
        public TemplateEngine()
        {
            //_html = html;
        }
        public void addVariable(string name, object value)
        {
            variables[name] = value;
        }
        public void render(ref string html)
        {
            TE_statistical.callCount++;
            if (layerCount > 10) html = "递归层次过深";
            Regex r = new Regex(@"({foreach(.*?)}[\s\S]*?{/foreach})|({if(.*?)}[\s\S]*?{/if})|(\$\{(.[^}]*?)\})|(<|&lt;)!-- #(.*?)#[\s\S]*?--(>|&gt;)|(\$((\w|\.|\[|\]){1,50}))", RegexOptions.IgnoreCase);
            html = r.Replace(html, new MatchEvaluator(_variable4));
        }
        List<object> sql_p = null;
        public void renderSql(ref string html, ref MySqlParameter[] p)
        {
            sql_p = new List<object>();
            TE_statistical.callCount++;
            if (layerCount > 10) html = "递归层次过深";
            Regex r = new Regex(@"({if(.*?)}[\s\S]*?{/if})|(\$\{(.[^}]*?)\})|(<|&lt;)!-- #(.*?)#[\s\S]*?--(>|&gt;)|(\$((\w|\.|\[|\]){1,30}))", RegexOptions.IgnoreCase);
            html = r.Replace(html, new MatchEvaluator(_variable4_sql));
            p = new MySqlParameter[sql_p.Count];
            for (int i = 0; i < sql_p.Count; i++)
            {
                p[i] = new MySqlParameter("p_" + (i + 1).ToString(), sql_p[i]);
            }
        }
        string _variable4_sql(Match m)
        {
            if (m.Value.IndexOf("${") == 0)
            {
                string key = m.Value.SubString(@"{", "}");
                key = operation(key);
                object value = getVariable(key);
                temp.Clear();
                if (value == null)
                {
                    sql_p.Add(m.Value);
                    return "@p_" + sql_p.Count.ToString();
                }
                else
                {
                    sql_p.Add(value);
                    return "@p_" + sql_p.Count.ToString();
                }
            }
            else
            {
                string key = m.Value.Substring(1);
                object value = getVariable(variables, key);
                if (value == null)
                {
                    sql_p.Add(m.Value);
                    return "@p_" + sql_p.Count.ToString();
                }
                else
                {
                    sql_p.Add(value);
                    return "@p_" + sql_p.Count.ToString();
                }
            }
        }
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
                            if (String.Compare(Config.domainList[i][0], M5.PageContext.Current.Request.Host.Host, true) == 0)
                            {
                                Html = Regex.Replace(Html, "(?<=(href|src|action)=(\"|'| ))" + url + "(?=(.*?)(?=(\"|'| |>)))", BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                            }
                        }
                        else
                        {
                            if (!BaseConfig.urlConversion && String.Compare(Config.domainList[i][0], M5.PageContext.Current.Request.Host.Host, true) == 0)//非绝对路径转换
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
                        if (!BaseConfig.urlConversion && String.Compare(Config.domainList[i][0], M5.PageContext.Current.Request.Host.Host, true) == 0)//非绝对路径转换
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
        public static string _replaceUrl(string url, bool isMobile, bool isMobileHost)
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
            string url = _replaceUrl(m.Value, isMobile, isMobileHost);
            return url;
        }
        #endregion
        string _variable4(Match m)
        {
            if (m.Value.IndexOf("<") == 0)
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
                        return "<font color=\"#FF0000\">标签错误:</font>" + System.Web.HttpUtility.HtmlEncode(m.Value);
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
                        return "<font color=\"#FF0000\">标签错误:</font>" + System.Web.HttpUtility.HtmlEncode(m.Value);
                    }
                }
                else if (Label == "#PageBar#")
                {
                    int showCount = getFieldInt(m.Value, "showCount");
                    string pageBarId = getFieldString(m.Value, "pageBarId");
                    string FG = m.Value.SubString("FG=", "(\r\n)|(\n)");
                    string html = m.Value.SubString("<htmlTemplate>", "</htmlTemplate>");
                    if (variables.ContainsKey("pageBar_" + pageBarId))
                    {
                        pageBar p = (pageBar)variables["pageBar_" + pageBarId];
                        p.pageNo = (int)getVariable("public._pageNo");
                        return showPage(html, p.recordCount, p.pageSize, showCount, p.pageNo, FG);

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
            else if (m.Value.IndexOf("{foreach") == 0)
            {
                string e = m.Value.SubString("{foreach", "}").Trim();
                object[] list = (object[])getVariable(e);
                //(System.Xml.XmlChildNodes)v
                string newHtml = "";
                foreach (var value in list)
                {
                    string html = m.Value.SubString("{foreach(.*?)}", "{/foreach}");
                    TemplateEngine view = new TemplateEngine();
                    view.TE_statistical = TE_statistical;
                    view.layerCount = layerCount + 1;
                    view.addVariable("sys", Config.systemVariables);
                    view.addVariable("view", Config.viewVariables);
                    view.addVariable("public", variables["public"]);
                    view.addVariable("this", value);
                    view.render(ref html);
                    newHtml += html;
                }
                return newHtml;
            }
            else if (m.Value.IndexOf("{if") == 0)
            {
                string[] item = Regex.Split(m.Value, @"{else", RegexOptions.IgnoreCase);
                for (int i = 0; i < item.Length; i++)
                {
                    int fg = item[i].IndexOf("}") + 1;
                    string e = item[i].Substring(0, fg).SubString(@"if\(", @"\)\}");
                    string v = "1";
                    if (e != "")
                    {
                        v = operation(e);
                        //v = calculate(e);
                        v = getVariable(v).ToString();
                        temp.Clear();
                    }
                    if (v == "1")
                    {
                        string content = item[i].Substring(fg);
                        content = Regex.Replace(content, "{/if}$", "", RegexOptions.IgnoreCase);
                        render(ref content);
                        return content;
                    }
                }
                return "";
            }
            else if (m.Value.IndexOf("${") == 0)
            {
                string key = m.Value.SubString(@"{", "}");
                key = operation(key);
                object value = getVariable(key);
                temp.Clear();
                if (value == null)
                {
                    return m.Value;
                }
                else
                {
                    return value.ToString();
                }
            }
            else
            {
                string key = m.Value.Substring(1);
                object value = getVariable(variables, key);
                if (value == null)
                {
                    return m.Value;
                }
                else
                {
                    return value.ToString();
                }
            }
        }
        string _variable3(Match m)
        {
            string Label = m.Value.SubString("<!-- ", "(\r\n)|(\n)");
            string LabelID = TemplateEngine.getFieldString(m.Value, "labelId");
            string Type = m.Value.SubString("{", "=");
            if (Label == "#Label#")//自定义标签视图
            {
                string html = getLabel(m.Value);
                return html;

            }
            else if (Label == "#SqlLabel#")
            {
                string html = getSqlLabel(m.Value);
                return html;
            }
            else if (Label == "#PageBar#")
            {
                int showCount = getFieldInt(m.Value, "showCount");
                string pageBarId = getFieldString(m.Value, "pageBarId");
                string FG = m.Value.SubString("FG=", "(\r\n)|(\n)");
                string html = m.Value.SubString("<htmlTemplate>", "</htmlTemplate>");
                if (variables.ContainsKey("pageBar_" + pageBarId))
                {
                    pageBar p = (pageBar)variables["pageBar_" + pageBarId];
                    p.pageNo = (int)getVariable("public._pageNo");
                    return showPage(html, p.recordCount, p.pageSize, showCount, p.pageNo, FG);

                }
                else
                {
                    throw new NullReferenceException("分页代码无效:无匹配数据标签，建议删除");
                }

            }
            else
            {

                return m.Value;
            }
        }
        string getSqlLabel(string html)
        {
            TE_statistical.labelCount++;
            string labelId = TemplateEngine.getFieldString(html, "labelId");

            int pageSize = TemplateEngine.getFieldInt(html, "pageSize");
            int recordCount = int.Parse(getValue(getFieldString(html, "recordCount")).ToString());
            string sql = TemplateEngine.getFieldString(html, "sql");
            object temp_sql = getValue(sql);
            if (temp_sql != null) sql = temp_sql.ToString();

            //sql= (string)getValue(sql);
            string debug = TemplateEngine.getFieldString(html, "debug");
            //TemplateEngine sqlTemplate = new TemplateEngine();
            //sqlTemplate.layerCount = layerCount + 1;
            //sqlTemplate.variables = this.variables;
            MySqlParameter[] sql_p = null;
            renderSql(ref sql, ref sql_p);
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
                p.recordCount =int.Parse( Sql.ExecuteScalar(countSql, sql_p).ToString());
                SafeReqeust request = new SafeReqeust(0, 0);
                pageNo = (int)getVariable("public._pageNo");
                pageNo = (pageNo == 0 ? 1 : pageNo);
                p.pageSize = pageSize;
                p.pageNo = pageNo;

                int pageCount = (p.recordCount - 1) / p.pageSize + 1;
                if (pageNo < 1 || pageNo > pageCount)
                {
                    Page.ERR404("页码不正确");
                }
                variables["pageBar_" + labelId] = p;
                sql = "select top " + pageSize.ToString() + " * from (" +
                sql +
                ")L where L.row_number>" + (pageSize * (pageNo - 1)).ToString();
            }
            else if (recordCount > 0 && sql.IndexOf(" top ") == -1) sql = Regex.Replace(sql, "^select ", "select top " + recordCount.ToString() + " ", RegexOptions.IgnoreCase);

            string template = html.SubString("<htmlTemplate>", "</htmlTemplate>");

            if (debug == "true") return "调试：" + sql;
            MySqlDataReader rs1 = Sql.ExecuteReader(sql, sql_p);
            #region 数据显示
            int index = 0;
            StringBuilder value = new StringBuilder("");
            while (rs1.Read() && (index < recordCount || recordCount == 0))
            {
                TemplateEngine page = new TemplateEngine();
                page.TE_statistical = TE_statistical;
                page.layerCount = layerCount + 1;
                foreach (KeyValuePair<string, object> entry in this.variables)
                {
                    page.addVariable(entry.Key, entry.Value);
                }

                page.addVariable("index", index + 1);
                for (int i = 0; i < rs1.FieldCount; i++)
                {
                    string fieldName = rs1.GetName(i);
                    if (fieldName == "url")
                    {
                        string url = rs1.IsDBNull(i) ? "" : rs1.GetString(i);
                        if (url.Length > 0 && url.Substring(url.Length - 1) == @"/")
                        {
                            page.addVariable(fieldName, Config.webPath + url);
                        }
                        else
                        {
                            page.addVariable(fieldName, Config.webPath + url + "." + BaseConfig.extension);
                        }
                    }
                    else
                    {
                        page.addVariable(fieldName, rs1.IsDBNull(i) ? "" : rs1[i]);
                    }
                }
                string _template = template;
                page.render(ref _template);
                value.Append(_template);
                index++;
            }
            #endregion
            rs1.Close();
            return value.ToString();
        }


        string getLabel(string html)
        {
            TE_statistical.labelCount++;
            #region 取得标签参数
            StringBuilder _Sql = new StringBuilder("");
            string labelId = TemplateEngine.getFieldString(html, "labelId");
            render(ref labelId);
            int pageSize = 0;
            string _pageSize = getFieldString(html, "pageSize");
            if (_pageSize != "") pageSize = int.Parse(getValue(_pageSize).ToString());
            int recordCount = int.Parse(getValue(getFieldString(html, "recordCount")).ToString());
            object _classId = getValue(getFieldString(html, "classId"));
            double classId = Convert.ToDouble(_classId);
            string template = Tools.GetStrFG(html, "<htmlTemplate>", "</htmlTemplate>");
            double datatypeId = TemplateEngine.getFieldDouble(html, "datatypeId");
            object _moduleId = getValue(getFieldString(html, "moduleId"));
            double moduleId = Convert.ToDouble(_moduleId); ;
            int orderBy = TemplateEngine.getFieldInt(html, "orderBy");
            string _fields = TemplateEngine.getFieldString(html, "fields");
            string attribute = TemplateEngine.getFieldString(html, "attribute");
            string addWhere = TemplateEngine.getFieldString(html, "where");
            object temp_sql = getValue(addWhere);
            if (temp_sql != null) addWhere = temp_sql.ToString();
            MySqlParameter[] sql_p = null;
            renderSql(ref addWhere, ref sql_p);
            string debug = TemplateEngine.getFieldString(html, "debug");
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
            sql += " " + fieldList + " from maintable A WITH(NOLOCK) ";
            countSql += " from maintable A WITH(NOLOCK) ";
            maxSql += " from maintable A WITH(NOLOCK) ";

            if (infoFlag || (pageSize > 0 && addWhere.IndexOf("u_") > -1))
            {
                sql += " inner join [" + tableInfo.tableName + "] B WITH(NOLOCK) on A.id=B.id ";
                countSql += " inner join [" + tableInfo.tableName + "] B WITH(NOLOCK) on A.id=B.id ";
            }
            if (classFlag)
            {
                sql += " inner join class C WITH(NOLOCK) on A.classId=C.id ";
                countSql += " inner join class C WITH(NOLOCK) on A.classId=C.id ";
            }
            int pageNo = 1;
            StringBuilder where = new StringBuilder(" where  A.orderid>-1 and A.createdate<getdate() ");
            if (showPic) where.Append(" and A.pic<>'' ");
            string max_where = "";
            #endregion


            #region 属性文章
            if (attribute != "")
            {
                if (moduleId == 0 && classId == 0) return "调用错误：必须指定模块或栏目id";
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
                MySqlDataReader rs2 = Sql.ExecuteReader("select rootId,childId,classId from class where  id=@id", new MySqlParameter[] { new MySqlParameter("id", classId) });
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
                string num = Regex.Replace((labelId +M5.PageContext.Current.Request.Path).MD5(), "[A-Z]", "");
                if (num.Length > 9) num = num.Substring(0, 8);
                double minId = 0, maxId = 0;
                int chazhi = 0;
                MySqlDataReader rs2 = Sql.ExecuteReader(maxSql + (max_where == "" ? "" : (" where " + max_where)));
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
            MySqlDataReader rs1 = null;
            int RC = 0;
            if (pageSize > 0)
            {
                pageBar p = new pageBar();
                p.recordCount = (int)Sql.ExecuteScalar(countSql + where, sql_p);
                SafeReqeust request = new SafeReqeust(0, 0);
                pageNo = (int)getVariable("public._pageNo");
                pageNo = (pageNo == 0 ? 1 : pageNo);
                p.pageSize = pageSize;
                p.pageNo = pageNo;
                int pageCount = (p.recordCount - 1) / p.pageSize + 1;
                if (pageNo < 1 || pageNo > pageCount)
                {
                    Page.ERR404("页码不正确");
                }

                variables["pageBar_" + labelId] = p;

                sql = "select top " + pageSize.ToString() + " * from (" + sql + where.ToString() + ")L where L.row_number>" + (pageSize * (pageNo - 1)).ToString();
                //sql = sql + " where A.id in (" + tempsql + ")";
                //sql = "select top " +pageSize.ToString() + " * from (" +
                //sql +where+
                //")L where L.row_number>"+(pageSize*(pageNo-1)).ToString();

                if (debug == "true") return "调试：" + sql;
                rs1 = Sql.ExecuteReader(sql, sql_p);

            }
            else
            {
                if (recordCount == 0) recordCount = 100000;
                sql += " inner join  (select top " + recordCount.ToString() + " A.id from maintable A WITH(NOLOCK) ";
                if (addWhere.IndexOf("u_") > -1) sql += " inner join [" + tableInfo.tableName + "] B WITH(NOLOCK) on A.id=B.id ";
                sql += where.ToString() + " " + orderByStr + ") H on A.id=H.id ";
                if (debug == "true") return "调试：" + sql;
                //rs1 = Sql.ExecuteReader(sql + where.ToString() + " " + orderByStr);
                rs1 = Sql.ExecuteReader(sql, sql_p);
            }
            #region 数据显示
            int index = 0;
            StringBuilder value = new StringBuilder("");
            while (rs1.Read() && (index < recordCount || recordCount == 0))
            {
                TemplateEngine page = new TemplateEngine();
                page.TE_statistical = TE_statistical;
                page.layerCount = layerCount + 1;
                foreach (KeyValuePair<string, object> entry in this.variables)
                {
                    page.addVariable(entry.Key, entry.Value);
                }
                page.addVariable("index", index + 1);
                for (int i = 0; i < rs1.FieldCount; i++)
                {
                    string fieldName = rs1.GetName(i);
                    if (fieldName == "url")
                    {
                        page.addVariable(fieldName, Config.webPath + rs1[i] + "." + BaseConfig.extension);
                    }
                    else
                    {

                        page.addVariable(fieldName, rs1.IsDBNull(i) ? "" : rs1[i]);
                    }
                }
                string _template = template;
                page.render(ref _template);
                value.Append(_template);
                index++;
            }
            #endregion
            rs1.Close();
            return value.ToString();
        }
        string _variable(Match m)
        {
            string key = m.Value.Substring(1);
            object value = getVariable(variables, key);
            if (value == null)
            {
                return m.Value;
            }
            else
            {
                return value.ToString();
            }
        }
        //string _variable2(Match m)
        //{
        //    string key = m.Value.SubString(@"{", "}");
        //    key = operation(key);
        //    key = getVariable(key).ToString();
        //    temp.Clear();
        //    return key;


        //    string[] p = key.Split('(');

        //    if (p.Length > 1)//函数体
        //    {
        //        key = p[0];//函数名
        //        string parameter = p[1].SubString("^", @"\)");//参数
        //        object obj = getVariable(variables, key);
        //        if (obj == null)//系统函数
        //        {
        //            string[] keys = key.Split('.');
        //            return getFunction(keys[keys.Length - 1], parameter);
        //        }
        //        else//自定义函数
        //        {
        //            string html = obj.ToString();
        //            //System.Diagnostics.Stopwatch oTime = new System.Diagnostics.Stopwatch();
        //            //oTime.Start(); //记录开始时间
        //            html = getView(parameter, html);
        //            //oTime.Stop();   //记录结束时间
        //            //html += "<!-- 视图运行时间:" + oTime.Elapsed.Milliseconds.ToString() + "-->";
        //            return html;
        //        }

        //    }
        //    key = operation(key);
        //    object value = getVariable(variables, key);
        //    if (value == null)
        //    {
        //        return m.Value;
        //    }
        //    else
        //    {
        //        return value.ToString();
        //    }
        //}
        object getFunction(string fname, string parameter)
        {
            if (fname == "end")
            {
                M5.PageContext.Current.Response.End();
                return "";
            }
            string[] p = parameter.Split(',');
            string p0 = calculate(p[0]);
            object objv = getValue(p0);
            if (objv == null) return "";
            if (fname == "getSqlData")
            {
                //object sql = getValue(p[0]);
                return Sql.ExecuteArray(objv.ToString());
            }
            else if (fname == "format")
            {
                if (p.Length < 2) return "函数[" + fname + "]参数不正确";
                object formatStr = getValue(p[1]);
                string value = "";
                string type = objv.GetType().Name;
                if (type == "DateTime")
                {
                    DateTime d = (DateTime)objv;
                    value = d.ToString((string)formatStr);
                }
                else if (type == "Double")
                {
                    Double d = (Double)objv;
                    value = d.ToString((string)formatStr);
                }
                return value;
            }
            else if (fname == "write")
            {
                if (objv != null)
                {
                    M5.PageContext.Current.Response.WriteAsync(objv.ToString());
                }
            }
            else if (fname == "left")
            {
                if (p.Length < 2) return "函数[" + fname + "]参数不正确";
                string value = "";
                if (objv != null)
                {
                    value = Tools.nohtml(getValue(p[0]).ToString());
                    value = Tools.GetString(value, Convert.ToInt32(getValue(p[1]))).String;
                }
                return value;
            }
            else if (fname == "split")
            {

                string value = objv.ToString();
                string fg = getValue(p[1]).ToString();
                return Regex.Split(value, fg);
            }
            else if (fname == "readUrl")
            {

                string value = objv.ToString();
                Encoding e = System.Text.Encoding.Default;
                if (p.Length > 0)
                {
                    try
                    {
                        e = System.Text.Encoding.GetEncoding(getValue(p[1]).ToString());
                    }
                    catch
                    {
                    }
                }
                value = "";// Helper.Http.getUrl(value, e);

                return value;
            }
            else if (fname == "dateTime")
            {
                string f = getValue(p[0]).ToString();
                return System.DateTime.Now.ToString(f);
            }
            else if (fname == "redirect")
            {
                string f = getValue(p[0]).ToString();
                M5.PageContext.Current.Response.Redirect(f);
                return "";
            }
            else if (fname == "page")
            {
                string f = getValue(p[0]).ToString();
                int pageNo = (int)getVariable("public._pageNo") - 1;
                string[] s = Regex.Split(f, "<!-- PageSpacer -->", RegexOptions.IgnoreCase);
                if ((pageNo + 1) > s.Length || pageNo < 0) Page.ERR404();
                return s[pageNo];
            }
            else if (fname == "pageBar")
            {

                StringBuilder O = new StringBuilder("");
                string f = getValue(p[0]).ToString();
                int pageNo = (int)getVariable("public._pageNo");
                string fileName = (string)getVariable("public._fileName");
                string[] s = Regex.Split(f, "<!-- PageSpacer -->", RegexOptions.IgnoreCase);
                if (s.Length == 1) return "";
                if (pageNo - 1 < 1)
                {
                    O.Append("<li>首页</li>");
                    O.Append("<li>上一页</li>");
                }
                else
                {
                    O.Append("<li><a href=\"" + fileName + "." + BaseConfig.extension + "\">首页</a></li>");
                    O.Append("<li><a href=\"" + fileName + "_" + (pageNo - 1).ToString() + "." + BaseConfig.extension + "\">上一页</a></li>");
                }
                int ShowCount = 10;
                int js = ShowCount / 2;
                int StartN = pageNo - (pageNo - 1) % js - js;
                if (StartN < 1) StartN = 1;
                for (int n1 = 0; n1 < ShowCount; n1++)
                {
                    if (n1 + StartN <= s.Length)
                    {
                        if (n1 + StartN == pageNo)
                        {
                            O.Append("<li>" + (n1 + StartN).ToString() + "</li>");
                        }
                        else
                        {
                            string url = "";
                            if ((n1 + StartN) == 1)
                            {
                                url = fileName + "." + BaseConfig.extension;
                            }
                            else
                            {
                                url = fileName + "_" + (n1 + StartN).ToString() + "." + BaseConfig.extension;
                            }
                            O.Append("<li><a href='" + url + "'>" + (n1 + StartN).ToString() + "</a></li>");
                        }
                    }

                }
                if (pageNo + 1 > s.Length)
                {
                    O.Append("<li>下一页</li><li>尾页</li>");
                }
                else
                {
                    O.Append("<li><a href=\"" + fileName + "_" + (pageNo + 1).ToString() + "." + BaseConfig.extension + "\">下一页</a></li><li><a href=\"" + fileName + "_" + s.Length.ToString() + "." + BaseConfig.extension + "\">尾页</a></li>");
                }
                return "<ul pagecount=" + s.Length.ToString() + " >" + O.ToString() + "</ul>";
            }
            else if (fname == "item")
            {
                if (p.Length < 2) return "函数[" + fname + "]参数不正确";
                string _item = "";
                object v = getValue(p[1]);
                int index = 0;
                int type = 0;
                if (v.GetType().Name == "Double")
                {
                    type = 1;
                    index = Convert.ToInt32(v);
                }
                else
                {
                    _item = v.ToString();
                }
                if (objv.GetType().IsArray)
                {
                    string[] stringList = ((string[])objv);
                    if (index > -1 && index < stringList.Length) return stringList[index];
                    return "";
                }
                string value = "";
                try
                {
                    XmlDocument xmlDoc;
                    xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(objv.ToString());
                    if (type == 0)
                    {
                        XmlNodeList list = xmlDoc.SelectNodes("/variables/item[@name='" + _item + "']");
                        if (list.Count > 0) value = list[0].InnerText;
                    }
                    else
                    {
                        XmlNode xn = xmlDoc.SelectSingleNode("variables");
                        value = xn.ChildNodes.Item(index).InnerText;
                    }


                }
                catch { value = ""; }
                return value;
            }
            return "";
        }
        object getValue(string p)
        {
            object value = p;
            int type = getType(ref value);//0字符串 1浮点型 2 变量
            if (type == 2)
            {
                return getVariable(variables, p);

            }
            else
            {
                return value;
            }
        }
        string getView(string parameter, string html)
        {
            TE_statistical.viewCount++;
            try
            {
                TemplateEngine view = new TemplateEngine();
                view.TE_statistical = TE_statistical;
                view.layerCount = layerCount + 1;
                view.addVariable("sys", Config.systemVariables);
                view.addVariable("view", Config.viewVariables);
                view.addVariable("public", variables["public"]);
                if (parameter != "")
                {
                    string[] p = parameter.Split(',');
                    for (int i = 0; i < p.Length; i++)
                    {
                        object value = operation(p[i]);
                        int type = getType(ref value);//0字符串 1浮点型 2 变量
                        if (type == 2)
                        {
                            //参数是变量时
                            if (p[i] == "this")
                            {
                                view.addVariable("parameter" + (i + 1).ToString(), variables);
                            }
                            else
                            {
                                view.addVariable("parameter" + (i + 1).ToString(), getVariable(value.ToString()));
                            }
                        }
                        else
                        {
                            view.addVariable("parameter" + (i + 1).ToString(), value);
                        }
                    }
                }
                view.render(ref html);
                return html;
            }
            catch
            {
                return "视图错误";
            }
        }
        /// <summary>
        /// 解析函数
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        string compileFunction(Match m)
        {
            string name = m.Value.MD5();
            string key = m.Value.Split('(')[0];
            string[] keys = key.Split('.');
            if (keys.Length < 2) return "";
            string p = m.Value.SubString(@"\(", @"\)");
            object value = "";
            if (keys[0] == "api")
            {
                value = getFunction(keys[keys.Length - 1], p);
            }
            else
            {
                object obj = getVariable(variables, key);
                if (obj == null)
                {
                    throw new NullReferenceException("视图[" + key + "]不存在");
                    return null;
                }
                value = getView(p, obj.ToString());
                if (this.isEdit)
                {
                    string viewValue = "";
                    if (p != "")
                    {
                        string[] p1 = p.Split(',');
                        for (int i = 0; i < p1.Length; i++)
                        {
                            object v = getValue(p1[i]);
                            if (viewValue != "") viewValue += ",";
                            if (v.GetType().Name == "String")
                            {
                                viewValue += "\"" + v.ToString() + "\"";
                            }
                            else
                            {

                                viewValue += v.ToString();
                            }
                        }
                    }
                    //    value = "<div class='m5_view' viewValue=\"" + System.Web.HttpUtility.HtmlEncode(key + "(" + viewValue + ")") + "\" >" + value + "</div>";
                    value = "<div class='m5_view' viewValue=\"" + System.Web.HttpUtility.HtmlEncode(key + "(" + viewValue + ")") + "\" ></div>" + value;
                }

                //value= getView(p, html);
            }

            temp[name] = value;
            return "temp." + name;
        }
        string bracketOperation(Match m)
        {
            string v = calculate(m.Value.Substring(1, m.Value.Length - 2));
            return v;
        }
        /// <summary>
        /// 计算有括号的算式
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string operation(string text)
        {
            #region 将表达式中的字符串转为临时变量
            text = text.Replace("\\'", "{#39}");
            text = text.Replace("\\\"", "{#34}");
            Regex r = new Regex(@"('[^']*')|(" + "\"" + "[^\\\"]*\")");
            text = r.Replace(text, new MatchEvaluator(replaceStringVariable));
            #endregion

            Regex r2 = new Regex(@"(api|view)([^\(]*)\(([^\)]*)\)", RegexOptions.IgnoreCase);
            text = r2.Replace(text, new MatchEvaluator(compileFunction));
            r = new Regex(@"\([^\)|^\(]*\)", RegexOptions.IgnorePatternWhitespace);

            while (Regex.IsMatch(text, @"\([^\)|^\(]*\)"))
            {
                text = r.Replace(text, new MatchEvaluator(bracketOperation));
            }
            text = calculate(text);
            string value = "";
            /*
            Regex r = new Regex(@"(?<=\()                          #普通字符“(”
                            (                       #分组构造，用来限定量词“*”修饰范围
                                (?<Open>\()         #命名捕获组，遇到开括弧’Open’计数加1
                            |                       #分支结构
                                (?<-Open>\))        #狭义平衡组，遇到闭括弧’Open’计数减1
                           |                       #分支结构
                                [^()]+              #非括弧的其它任意字符
                            )*                      #以上子串出现0次或任意多次
                            (?(Open)(?!))           #判断是否还有’Open’，有则说明不配对，什么都不匹配
                        (?=\))", RegexOptions.IgnorePatternWhitespace);
            MatchCollection M = r.Matches(text);
            string value = "";
            for (int i = 0; i < M.Count; i++)
            {
                string v = operation(M[i].Value);
                //string name = M[i].Value.MD5();
                //if (Regex.IsMatch(v.ToString(), @"^(\d|-)")) v = double.Parse(v.ToString());
                //temp[name] = v;
                //text = text.Replace("(" + M[i].Value + ")", "temp." + name);
                text = text.Replace("(" + M[i].Value + ")", v);
            }
            */
            //    int type = -1;
            //value = calculate(text);//计算剩余不带括号运算式

            return text;
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
        string calculate(string text)
        {
            try
            {
                List<string> ys = new List<string>();
                #region 将表达式中的字符串转为临时变量
                text = text.Replace("\\'", "{#39}");
                text = text.Replace("\\\"", "{#34}");
                Regex r = new Regex(@"('[^']*')|(" + "\"" + "[^']*\")");
                text = r.Replace(text, new MatchEvaluator(replaceStringVariable));
                #endregion
                string[] list = Regex.Split(text, @"(\%|\+|\-|\*|\/|\>\=|\<\=|\>|\<|\=\=|\!\=|\=|&&|\|\|)");
                int _index = 0;
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i] != "") ys.Add(list[i]);
                }
                string[] fha = new string[] { @"*/%", "+-", ">=<==!=", "&&||", "=" };
                while (ys.Count > 1)
                {
                    for (int i1 = 0; i1 < fha.Length; i1++)
                    {
                        for (int i = 0; i < ys.Count; i++)
                        {
                            object a1 = null, a2 = null;
                            if ((ys[i] != "=" && fha[i1].IndexOf(ys[i]) > -1) || ys[i] == fha[i1])
                            {
                                int top = i, end = i;
                                if (i > 0) { a1 = ys[i - 1]; top = i - 1; }
                                if (i < ys.Count - 1) { a2 = ys[i + 1]; end = i + 1; }
                                object v = null;
                                if (a1 == null && ys[i] == "-")//负数处理
                                {
                                    v = (string)ys[i] + (string)ys[i + 1];
                                }
                                else
                                {
                                    int type1 = getType(ref a1);
                                    int type2 = getType(ref a2);
                                    //if (type1 + type2==1)//如果计算双方数据不匹配 以数值型为主
                                    //{
                                    //    if (type1 == 0) try { a1 = float.Parse(a1.ToString()); }
                                    //        catch { a1 = (float)0; }
                                    //    if (type2 == 0) try { a2 = float.Parse(a2.ToString()); }
                                    //        catch { a2 = (float)0; }
                                    //    type1 = type2 = 1;
                                    //}
                                    v = calculate(type1, a1, type2, a2, ys[i]);
                                    string name = v.ToString().MD5();
                                    temp[name] = v;
                                    v = "temp." + name;
                                }
                                ys.RemoveRange(top, end - top + 1);
                                ys.Insert(top, v.ToString());
                                i1 = fha.Length;
                                i = ys.Count;
                            }
                        }
                    }
                }
                return ys[0];
            }
            catch
            {
                throw new NullReferenceException("表达式无法计算：" + text);
                return null;
            }
        }
        object calculate(int type1, object a1, int type2, object a2, string fh)
        {
            #region 赋值操作
            if (fh == "=")
            {
                if (type2 == 2)
                {
                    if (a2.ToString() == "this")
                    {
                        setVariable("public." + a1.ToString(), variables);
                    }
                    else
                    {
                        setVariable("public." + a1.ToString(), getVariable(a2.ToString()));
                    }
                }
                else
                {
                    setVariable("public." + a1.ToString(), a2);
                }
                return "";
            }
            #endregion
            #region 获取变量值用以计算
            if (type1 == 2)
            {
                a1 = getVariable(a1.ToString());
                if (a1 == null) a1 = "";
                if (a1.GetType().Name == "String")
                {
                    type1 = 0;
                }
                else
                {
                    a1 = Convert.ToDouble(a1);
                    type1 = 1;
                }
            }
            if (type2 == 2)
            {
                a2 = getVariable(a2.ToString());
                if (a2.GetType().Name == "String")
                {
                    type2 = 0;
                }
                else
                {
                    a2 = Convert.ToDouble(a2);
                    type2 = 1;
                }
            }
            #endregion
            if (type1 + type2 == 1)//如果计算双方数据不匹配 以第一个为主
            {
                //if (type1 == 0) a1 = Convert.ToDouble(a1); 
                if (type1 == 0) a2 = a2.ToString();
                else if (type1 == 1)
                {
                    try
                    {
                        a2 = Convert.ToDouble(a2);
                    }
                    catch { a2 = 0.0; }
                }
                type2 = type1;
            }
            return calculate(type1, a1, a2, fh);
        }
        object calculate(int type, object a1, object a2, string fh)
        {
            object value = null;
            switch (fh)
            {
                case "+":
                    if (type == 1) value = (double)a1 + (double)a2;
                    if (type == 0) value = (string)a1 + (string)a2;
                    break;
                case "-":
                    if (type == 1) value = (double)a1 - (double)a2;
                    break;
                case "*":
                    if (type == 1) value = (double)a1 * (double)a2;
                    break;
                case "/":
                    if (type == 1) value = (double)a1 / (double)a2;
                    break;
                case "%":
                    if (type == 1) value = (double)a1 % (double)a2;
                    break;
                case ">":
                    if (type == 1) value = (double)a1 > (double)a2 ? 1 : 0;
                    break;
                case ">=":
                    if (type == 1) value = (double)a1 >= (double)a2 ? 1 : 0;
                    break;
                case "<":
                    if (type == 1) value = (double)a1 < (double)a2 ? 1 : 0;
                    break;
                case "<=":
                    if (type == 1) value = (double)a1 <= (double)a2 ? 1 : 0;
                    break;
                case "==":
                    if (type == 1) value = (double)a1 == (double)a2 ? 1 : 0;
                    if (type == 0) value = (string)a1 == (string)a2 ? 1 : 0;
                    break;
                case "!=":
                    if (type == 1) value = (double)a1 != (double)a2 ? 1 : 0;
                    if (type == 0) value = (string)a1 != (string)a2 ? 1 : 0;
                    break;
                case "&&":
                    if (type == 1) value = ((double)a1 * (double)a2) == 0 ? 0 : 1;
                    break;
                case "||":
                    if (type == 1) value = ((double)a1 + (double)a2) == 0 ? 0 : 1;
                    break;

            }
            return value;
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
        object getVariable(string key)
        {
            return (getVariable(variables, key));
        }
        object setVariable(string key, object value)
        {
            return (setVariable(variables, key, value));
        }
        object getVariable(Dictionary<string, object> v, string key)
        {
            return setVariable(v, key, null);
        }
        XmlNode findControl(XmlNodeList list, string name)
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
        object setVariable(Dictionary<string, object> v, string key, object value)
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
                string cookies = M5.PageContext.Current.Request.Cookies[keys[1]];
                if (cookies == null) return "";
                return "";
                // return HttpUtility.UrlDecode(cookies.Value);
            }
            else if (keys[0] == "post")
            {
                string _v = M5.PageContext.Current.Request.Form[keys[1]];
                return _v == null ? "" : _v;
            }
            else if (keys[0] == "userConfig")
            {
                XmlNode node = findControl(Config.userConfig[keys[1]], keys[2]);
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
                string _v = M5.PageContext.Current.Request.Query[keys[1]];
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

            string domain = M5.PageContext.Current.Request.Host.Host;
            bool tag = false;
            //if (Constant.DomainDoc != null)
            //{
            //    for (int i = 0; i < Constant.DomainDoc.Count; i++)
            //    {
            //        if (String.Compare(Constant.DomainDoc[i].InnerText, domain, true) == 0)
            //        {
            //            domain = "http://" + domain;
            //            if (M5.PageContext.Current.Request.Url.Port != 80) domain += ":" + M5.PageContext.Current.Request.Url.Port.ToString();
            //            i = Constant.DomainDoc.Count; tag = true;
            //        }
            //    }
            //}
            if (!tag) domain = "";
            #endregion
            //        string url = API.GetWebUrl() + M5.PageContext.Current.Request.RawUrl;
            string url = domain;// + M5.PageContext.Current.Request.RawUrl;
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
            string FileName = (string)getVariable("public._fileName");
            if (FileName == null) FileName = "default";
            Regex r = new Regex(@"({(Label)[^{]*?})");
            MatchCollection mc = r.Matches(Html.ToString());
            string LabelName = "", Value = "", Css = "", Color1 = "";

            string filename2 = FileName + KZM;
            if (String.Compare(FileName, "default", true) == 0) filename2 = url;
            FileName = url + FileName;
            for (int n = 0; n < mc.Count; n++)
            {
                LabelName = Tools.GetHTMLValue(mc[n].Value, "LabelName");
                Value = Tools.GetHTMLValue(mc[n].Value, "Value");
                Css = Tools.GetHTMLValue(mc[n].Value, "Css");
                Css = (Css == "") ? "" : "class=\"" + Css + "\"";
                Color1 = Tools.GetHTMLValue(mc[n].Value, "Color");
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
                    if (PageNo != PageCount) EndPage.Append("<a href=\"" + FileName + "_" + PageCount.ToString() + KZM + "\" " + Css + " val=" + PageCount.ToString() + ">");
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
