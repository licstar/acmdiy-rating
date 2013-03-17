using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Collections;
using System.Data;

namespace acm_diy {
    public partial class Message : System.Web.UI.Page {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime begin, end, dt;
        protected string navList, navMonth, navDay;
        int qun = 1;

        protected void Page_Load(object sender, EventArgs e) {
            if (!Functions.CheckDate(Request["date"], out dt)) {
                Response.Write("日期错误");
                Response.End();
            }

            begin = Functions.begin;
            end = Functions.end;

            if (Request["qun"] != null)
                if (Request["qun"] == "2")
                    qun = 2;

            navList = Functions.GetNavList("Message.aspx?qun=" + qun, "d", dt.ToShortDateString());
            navMonth = Functions.GetNavMonth(dt);
            navDay = Functions.GetNavDay(dt);

            this.Title = dt.ToShortDateString() + " " + (qun == 1 ? "旧群" : "新群") + "的聊天记录";
        }

        protected void PrintPage() {
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT [qq],[name],[rating] FROM [nickname]", conn);
            SqlDataReader dr = cmd.ExecuteReader();

            Hashtable hnick = new Hashtable();
            Hashtable hrating = new Hashtable();
            while (dr.Read()) {
                if (dr["rating"] == DBNull.Value) continue;
                string qq = ((string)dr["qq"]).Trim();
                string name = (string)dr["name"];
                double rating = (double)dr["rating"];
                hnick.Add(qq, name);
                hrating.Add(qq, rating);
            }
            dr.Close();

            cmd = new SqlCommand("SELECT * FROM [data] WHERE [time]>=@t1 and [time]<@t2 AND qun=@qun ORDER BY [time]", conn);
            cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = dt;
            cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = dt.AddDays(1);
            cmd.Parameters.Add("qun", SqlDbType.Int).Value = qun;

            dr = cmd.ExecuteReader();
            Hashtable hash = new Hashtable();

            int cnt = 0;
            Response.Write(string.Format("<table class='full'><thead><tr><th>日期: {0}</th></tr></thead>\n", dt.ToShortDateString()));
            while (dr.Read()) {
                string msg = (string)dr["message"];
                int p = msg.IndexOf("<font style=");
                while (p != -1) {
                    int ee = msg.IndexOf("'>", p);
                    string style = msg.Substring(p, ee - p + 2);
                    if (!hash.Contains(style)) {
                        hash.Add(style, cnt++);
                    }
                    msg = msg.Remove(p, ee - p + 2).Insert(p, string.Format("<font class='a{0}'>", hash[style]));
                    p = msg.IndexOf("<font style=", p + 1);
                }
                string qq = dr["qq"].ToString().Trim();
                msg = msg.Replace("翻墙", "<img src=\"img/h.gif\" />");
                Response.Write(string.Format("<tr><td><a name=\"m{5}\"></a><div class='a'><div class='b'><a href=\"Rating.aspx?qq={0}\" class=\"{4}\">{3}({0})</a></div>{1}</div><div class='c'>{2}</div></td></tr>\n", qq, dr["time"], msg, hnick[qq], Functions.GetColor((double)hrating[qq]), dr["id"]));
            }

            Response.Write("</table>");

            if (hash.Count != 0) {
                Response.Write("<style type=\"text/css\">\n");
                foreach (System.Collections.DictionaryEntry objDE in hash) {
                    string style = (string)objDE.Key;
                    int p1 = style.IndexOf("\"");
                    int p2 = style.IndexOf("\"", p1 + 1);
                    int p3 = style.IndexOf("'", p2 + 1);
                    int p4 = style.IndexOf("'", p3 + 1);
                    Response.Write(string.Format(".a{0} {{ {1} color:#{2} }}\n", objDE.Value, style.Substring(p1 + 1, p2 - p1 - 1).Replace(",'MS Sans Serif',sans-serif", ""), style.Substring(p3 + 1, p4 - p3 - 1)));
                }
                Response.Write("</style>");
            }
            dr.Close();
            conn.Close();
        }
    }
}
