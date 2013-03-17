using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SharpICTCLAS;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Text;

namespace acm_diy {
    public partial class Search : System.Web.UI.Page {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime end;
        protected DateTime begin;
        protected string navList, navMonth;

        protected string key;

        protected static SearchMessage search = null;
        protected static Dictionary<DateTime, int> minID = null, maxID = null;

        protected int rank = 1; //1默认排序 2时间升序 3时间降序
        protected int page = 1;
        protected const int maxPage = 50; //最大页数

        protected void Page_Load(object sender, EventArgs e) {
            begin = Functions.begin;
            end = Functions.end;
            navList = Functions.GetNavList("Search.aspx", "", "");
            navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1));

            key = "";
            if (Request["q"] != null)
                key = Request["q"];



            if (Request["p"] != null) {
                int.TryParse(Request["p"], out page);
                if (page < 1 || page > maxPage)
                    page = 1;
            }

            if (Request["r"] != null) {
                if (Request["r"].ToLower() == "asc") {
                    rank = 2;
                } else if (Request["r"].ToLower() == "dec") {
                    rank = 3;
                }
            }

            if (key == "")
                Page.Title = "ACM_DIY 搜索";
            else
                Page.Title = key + " - ACM_DIY 搜索";
        }

        protected string checkQuery(Hashtable hnick, out DateTime sBegin, out DateTime sEnd, out string sAuthor, out string newKey) {
            sBegin = begin; //先初始化为最大的时间区间
            sEnd = end;
            newKey = "";
            sAuthor = "";

            if (key.Length > 128) {
                return "您搜索的关键词太长";
            }

            string[] keys = key.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < keys.Length; i++) { //寻找是否有限定符
                if (keys[i].StartsWith("from:")) {
                    if (!Functions.CheckDate(keys[i].Substring(5), out sBegin) || sBegin < begin) {
                        //newKey += " " + keys[i];
                        return "开始日期格式不正确或超范围";
                    }
                } else if (keys[i].StartsWith("to:")) {
                    if (!Functions.CheckDate(keys[i].Substring(3), out sEnd)) {
                        //newKey += " " + keys[i];
                        return "结束日期格式不正确或超范围";
                    }
                } else if (keys[i].StartsWith("author:")) {
                    string qq = keys[i].Substring(7);
                    if (hnick.Contains(qq)) {
                        sAuthor = qq;
                    } else {
                        return string.Format("找不到 QQ:{0} 的发言记录", qq);
                    }
                } else {
                    newKey += " " + keys[i];
                }
            }

            if (newKey == "") {
                return "请输入关键词";
            }

            if (sBegin > sEnd) {
                return "查询的起始日期不能晚于结束日期";
            }

            return ""; //检验通过
        }

        protected void initMinMaxID(SqlConnection conn, DateTime sbegin, DateTime send) {
            if (minID == null)
                minID = new Dictionary<DateTime, int>();
            if (maxID == null)
                maxID = new Dictionary<DateTime, int>();
            if (!minID.ContainsKey(sbegin)) {
                SqlCommand cmd = new SqlCommand("SELECT MIN(id) FROM [data] WHERE [time]>=@t1", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = sbegin;
                int id = (int)cmd.ExecuteScalar();
                minID.Add(sbegin, id);
            }
            if (!maxID.ContainsKey(send)) {
                SqlCommand cmd = new SqlCommand("SELECT MAX(id) FROM [data] WHERE [time]<@t1", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = send.AddDays(1);
                int id = (int)cmd.ExecuteScalar();
                maxID.Add(send, id);
            }
        }

        protected string getRankStr(int rank) {
            if (rank == 2) {
                return "&r=asc";
            } else if (rank == 3) {
                return "&r=dec";
            }
            return "";
        }

        protected void PrintPage() {
            long t1 = Environment.TickCount;
            DateTime sBegin, sEnd;
            string newKey, sAuthor;

            Hashtable hnick = Functions.hashNick;
            Hashtable hrating = Functions.hashRating;

            string chk = checkQuery(hnick, out sBegin, out sEnd, out sAuthor, out newKey);

            if (chk != "") {
                Response.Write("<br />" + chk);
                return;
            }

            if (search == null) //第一次搜索，加载搜索模块
                search = new SearchMessage(Server.MapPath("Data") + "\\");

            SearchResult result = search.Search(newKey);


            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            if (result.documentID.Length == 0) {
                Response.Write(string.Format("<br />" + "找不到和您的查询 \"<strong>{0}</strong>\" 相符的内容或信息。", key));
            } else {

                StringBuilder content = new StringBuilder();


                if (sBegin != begin || sEnd != end) {
                    initMinMaxID(conn, sBegin, sEnd);//第一次搜索时间，加载时间加速模块

                    int min = minID[sBegin];
                    int max = maxID[sEnd];
                    List<int> res = new List<int>();
                    List<double> tfidf = new List<double>();
                    for (int i = 0; i < result.documentID.Length; i++) {
                        if (result.documentID[i] >= min && result.documentID[i] <= max) {
                            res.Add(result.documentID[i]);
                            tfidf.Add(result.tfidf[i]);
                        }
                    }
                    result.documentID = res.ToArray();
                    result.tfidf = tfidf.ToArray();
                }

                int cnt = 0;

                int pageNum = (result.documentID.Length - 1) / 10 + 1;
                if (pageNum > maxPage)
                    pageNum = maxPage;
                if (page > pageNum)
                    page = pageNum;

                if (rank >= 2) {
                    Array.Sort(result.documentID);
                    if (rank == 3) {
                        Array.Reverse(result.documentID);
                    }
                }

                for (int j = (page - 1) * 10; j < result.documentID.Length && cnt < 10; j++) {
                    SqlCommand cmd = new SqlCommand("SELECT [Message],[qq],[time],[qun],[groupnum] FROM [data] WHERE [id]=@id", conn);
                    cmd.Parameters.Add("id", SqlDbType.Int).Value = result.documentID[j];
                    SqlDataReader dr = cmd.ExecuteReader();

                    dr.Read();
                    string msg = PreProcessUtility.HTML2Text((string)dr["message"]);
                    msg = search.getSnippet(result.words, msg);
                    string qq = ((string)dr["qq"]).Trim();
                    int qun = (int)dr["qun"];
                    DateTime time = (DateTime)dr["time"];
                    int msgCount = (int)dr["groupnum"];

                    dr.Close();

                    content.Append(string.Format("<tr><td><div class='a'><div class='b'><a href=\"Rating.aspx?qq={0}\" class=\"{4}\">{3}({0})</a></div>{1} <a href=\"Message.aspx?date={6}&qun={7}#m{5}\">查看</a>{8}</div><div class='c'>{2}</div></td></tr>\n", qq, time, msg, hnick[qq], Functions.GetColor((double)hrating[qq]), result.documentID[j], time.ToShortDateString(), qun, (msgCount == 1 ? "" : " " + msgCount + " 条相同消息")));

                    cnt++;
                }

                if (pageNum != 1) {
                    content.Append("<tr><th class='sub'>");
                    int pb = Math.Max(1, page - 5);
                    int pe = Math.Min(pageNum, page + 5);

                    if (pb > 1) {
                        content.Append(string.Format(" <a href=\"Search.aspx?q={0}&p={1}{2}\"><strong>...</strong></a> ", key, Math.Max(pb - 5, 1), getRankStr(rank)));
                    }

                    for (int i = pb; i <= pe; i++) {
                        if (i == page) {
                            content.Append(string.Format(" [{0}] ", i));
                        } else {
                            content.Append(string.Format(" <a href=\"Search.aspx?q={0}&p={1}{2}\">{1}</a> ", key, i, getRankStr(rank)));
                        }
                    }

                    if (pe < pageNum) {
                        content.Append(string.Format(" <a href=\"Search.aspx?q={0}&p={1}{2}\"><strong>...</strong></a> ", key, Math.Min(pe + 5, maxPage), getRankStr(rank)));
                    }
                    content.Append("</th></tr>");
                }
                content.Append("</table>");

                string strDate = "";
                if (sBegin != begin || sEnd != end) {
                    if (sBegin == begin) {
                        strDate = string.Format("从开始到 <strong>{0}</strong> ", sEnd.ToShortDateString());
                    } else if (sEnd == end) {
                        strDate = string.Format("从 <strong>{0}</strong> 到现在", sBegin.ToShortDateString());
                    } else {
                        if (sBegin == sEnd) {
                            strDate = string.Format("在 <strong>{0}</strong> ", sBegin.ToShortDateString());
                        } else {
                            strDate = string.Format("从 <strong>{0}</strong> 到 <strong>{1}</strong> ", sBegin.ToShortDateString(), sEnd.ToShortDateString());
                        }
                    }
                }
                string strAuthor = "";
                if (sAuthor != "") {
                    strAuthor = string.Format("由 <a href=\"Rating.aspx?qq={1}\" class=\"{2}\">{0}({1})</a> ", (string)hnick[sAuthor], sAuthor, Functions.GetColor((double)hrating[sAuthor]));
                }

                string strDescription = strDate + strAuthor;
                if (strDescription != "")
                    strDescription += " 发表的";

                string strSort = "";
                if (rank == 1) {
                    strSort = string.Format("<strong>默认排序</strong> | <a href=\"Search.aspx?q={0}&p={1}&r=asc\">时间排序↑</a>", key, page);
                } else if (rank == 2) {
                    strSort = string.Format("<a href=\"Search.aspx?q={0}&p={1}\">默认排序</a> | <a href=\"Search.aspx?q={0}&p={1}&r=dec\"><strong>时间排序↑</strong></a>", key, page);
                } else {
                    strSort = string.Format("<a href=\"Search.aspx?q={0}&p={1}\">默认排序</a> | <a href=\"Search.aspx?q={0}&p={1}&r=asc\"><strong>时间排序↓</strong></a>", key, page);
                }

                Response.Write(string.Format("<table class='full'><thead><tr><th style=\"font-weight:normal\">搜索 {3} <strong>{0}</strong>，获得 {1} 条搜索结果 （用时 {2:F3} 秒） | {4}</th></tr></thead>\n",
                   newKey, result.documentID.Length, (Environment.TickCount - t1) / 1000.0, strDescription, strSort));
                Response.Write(content.ToString());
            }

            SqlCommand cmdRec = new SqlCommand("INSERT INTO [SearchHistory] ([keyword],[IP],[time],[sort],[page]) VALUES (@keyword, @ip, @time, @sort, @page)", conn);
            cmdRec.Parameters.Add("keyword", SqlDbType.Char).Value = key;
            cmdRec.Parameters.Add("ip", SqlDbType.Char).Value = Request.ServerVariables["REMOTE_ADDR"];
            cmdRec.Parameters.Add("time", SqlDbType.DateTime).Value = DateTime.Now;
            cmdRec.Parameters.Add("sort", SqlDbType.TinyInt).Value = rank;
            cmdRec.Parameters.Add("page", SqlDbType.TinyInt).Value = page;
            cmdRec.ExecuteNonQuery();

            conn.Close();
        }
    }
}
