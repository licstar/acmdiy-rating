using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace acm_diy
{
    public partial class Rating : System.Web.UI.Page
    {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime end;
        protected DateTime begin;
        protected string navList, navMonth;

        protected string qq = "10000";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["qq"] != null)
                qq = Request["qq"];
            Page.Title = qq + " 的 Rating 记录";

            begin = Functions.begin;
            end = Functions.end;
            navList = Functions.GetNavList("Rating.aspx", "", "");
            navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1));
        }

        protected void PrintPage()
        {
            SqlConnection conn = new SqlConnection(connstr);

            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT [qq],[name],[rating] FROM [nickname] WHERE [qq]=@qq", conn);
            cmd.Parameters.Add("qq", SqlDbType.Char).Value = qq;
            SqlDataReader dr = cmd.ExecuteReader();
            string nickname = "";
            if (dr.Read())
            {
                nickname = (string)dr["name"];
            }
            dr.Close();


            cmd = new SqlCommand("SELECT * FROM [rating] WHERE [qq]=@qq order by [time] DESC", conn);
            cmd.Parameters.Add("qq", SqlDbType.Char).Value = qq;

            Response.Write("<table class='full'>\n");

            Response.Write(string.Format("<thead><tr><th colspan='4'>{0}({1}) 的 Rating 记录</th></tr></thead>\n", nickname, qq));
            Response.Write("<tbody><tr><th>日期</th><th>Rating</th><th>排名</th><th>得分</th></tr>\n");
            dr = cmd.ExecuteReader();
            List<DateTime> tdate = new List<DateTime>();
            List<double> trating = new List<double>();
            int count = 0;
            bool more = false;
            int top1 = 0, top5 = 0;
            double avg = 0;
            while (dr.Read())
            {
                DateTime t = (DateTime)dr["time"];
                double r = (double)dr["rating"];
                int rank = (int)dr["rank"];
                if (rank == 1) top1++;
                if (rank <= 5) top5++;
                avg += rank;
                count++;
                if (count <= 20)
                    Response.Write(string.Format("<tr><th class='sub'><a href=\"Overview.aspx?date={0}\">{0}</a></th><td><span class=\"{4}\">{1}</span></td><td>{3}</td><td>{2}</td></tr>\n", t.ToShortDateString(), (int)r, (int)dr["score"], rank, Functions.GetColor(r)));
                else
                    more = true;
                tdate.Add(t);
                trating.Add(r);
            }
            if (more)
            {
                Response.Write(string.Format("<tr><td colspan=4 align='right'><a href=\"MoreRating.aspx?qq={0}\">更多</a></td></tr>\n", qq));
            }
            Response.Write(string.Format("<tr><td colspan=4 align='right'>第一：{0}次</td></tr>\n", top1));
            Response.Write(string.Format("<tr><td colspan=4 align='right'>前五：{0}次</td></tr>\n", top5));
            Response.Write(string.Format("<tr><td colspan=4 align='right'>平均排名：{0}</td></tr>\n", (avg / count).ToString("0.00")));
            Response.Write("</tbody></table>");
            dr.Close();
            conn.Close();

            if (tdate.Count > 0)
            {
                tdate.Reverse();
                trating.Reverse();

                List<DateTime> date = new List<DateTime>();
                List<double> rating = new List<double>();
                DateTime limit = tdate[tdate.Count - 1].AddDays(-40);
                for (int i = 0; i < tdate.Count; i++)
                {
                    if (tdate[i] >= limit)
                    {
                        date.Add(tdate[i]);
                        rating.Add(trating[i]);
                    }
                }

                Response.Write("<hr /><h4>最近 40 天的 Rating 趋势图</h4>\n");
                Jim.GoogleChart.LineChart line = new Jim.GoogleChart.LineChart(date, rating);
                Response.Write(string.Format("<img src=\"{0}\">", line.GetUrl()));
            }
        }
    }
}