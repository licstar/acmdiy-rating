

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Collections;

namespace acm_diy {
    public partial class Monthly : System.Web.UI.Page {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;

        protected DateTime dt, begin, end;
        protected string navList, navMonth, navDay;


        protected void Page_Load(object sender, EventArgs e) {
            if (Request["date"] == null) {
                Response.Write("日期错误");
                Response.End();
            }
            if (!DateTime.TryParse(Request["date"], out dt)) {
                Response.Write("日期错误");
                Response.End();
            }
            dt = new DateTime(dt.Year, dt.Month, 1);
            Page.Title = string.Format("{0} 年 {1} 月，每天 TOP 3", dt.Year, dt.Month);

            begin = Functions.begin;
            end = Functions.end;

            begin = new DateTime(begin.Year, begin.Month, 1);// begin.Date;
            end = end.Date;

            if (dt < begin || dt > end) {
                Response.Write("日期超范围");
                Response.End();
            }

            navList = Functions.GetNavList("Monthly.aspx", "m", dt.ToShortDateString());
            navMonth = Functions.GetNavMonth(dt);
            navDay = Functions.GetNavDay(new DateTime(dt.Year, dt.Month, dt.Day, 10, 0, 0));
        }


        protected void PrintPage() {

            Hashtable hnick = Functions.hashNick;
            Hashtable hrating = Functions.hashRating;

            SqlConnection conn = new SqlConnection(connstr);

            conn.Open();

            Response.Write(string.Format("<table class='full'><thead><tr><th colspan=8>{0} 年 {1} 月，每天 TOP 3</th></tr></thead>\n", dt.Year, dt.Month));
            Response.Write("<tbody><tr><th>日期</th><th colspan='2'>冠军</th><th colspan='2'>亚军</th><th colspan='2'>季军</th></tr>\n");

            for (DateTime d = dt; d < dt.AddMonths(1); d = d.AddDays(1)) {
                SqlCommand cmd = new SqlCommand("SELECT * FROM [rating] WHERE [time]>=@t1 and [time]<@t2 order by [score] DESC", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = d;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = d.AddDays(1);

                string[] qq = new string[3];
                int[] score = new int[3];
                int cnt = 0;
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read()) {
                    string q = dr["qq"].ToString().Trim();
                    int sc = (int)dr["score"];
                    if (cnt < 3) {
                        qq[cnt] = q;
                        score[cnt] = sc;
                    }
                    cnt++;
                }
                dr.Close();

                if (cnt != 0) {
                    Response.Write(string.Format("<tr><th class='sub'><a href=\"Overview.aspx?date={1}\">{0}</a></th>", d.Day, d.ToShortDateString()));
                    for (int i = 0; i < Math.Min(cnt, 3); i++) {
                        Response.Write(string.Format("<td><a href=\"Rating.aspx?qq={0}\" class=\"{3}\">{2}({0})</a></td><td>{1}</td>\n", qq[i], score[i], hnick[qq[i]], Functions.GetColor((double)hrating[qq[i]])));
                    }
                    for (int i = Math.Min(cnt, 3); i < 3; i++) {
                        Response.Write("<td>&nbsp;</td><td>&nbsp;</td>");
                    }
                    Response.Write("</tr>\n");
                }
            }
            Response.Write("</tbody></table>");

            conn.Close();
        }
    }
}
