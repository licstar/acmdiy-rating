using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Text;

namespace acm_diy
{
    public partial class King : System.Web.UI.Page
    {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime begin, end, dt;
        protected string navList, navMonth, navDay;
        protected string type;
        protected string title;

        protected void Page_Load(object sender, EventArgs e)
        {
            type = Functions.GetAcceptableType(Request["type"]);

            begin = Functions.begin;
            end = Functions.end.AddDays(1);

            if (type == "y")
            {
                title = Page.Title = string.Format("水王总榜");
                navList = Functions.GetNavList("King.aspx", "", "");
                navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1), "King.aspx", "King.aspx", "&type=m");
            }
            else
            {
                if (!Functions.CheckDate(Request["date"], out dt))
                {
                    Response.Write("日期错误");
                    Response.End();
                }

                if (type == "m")
                {
                    title = Page.Title = string.Format("{0} 年 {1} 月，水王排行榜", dt.Year, dt.Month);
                    navList = Functions.GetNavList("King.aspx", "m", dt.ToShortDateString());
                    navMonth = Functions.GetNavMonth(dt, "King.aspx", "King.aspx", "&type=m");
                    begin = new DateTime(dt.Year, dt.Month, 1);
                    navDay = Functions.GetNavDay(new DateTime(dt.Year, dt.Month, dt.Day, 10, 0, 0), string.Format("King.aspx?date={0}&type=m", dt.ToShortDateString()), "King.aspx", "&type=d");

                    end = begin.AddMonths(1);

                }
                    else if (type == "yy")
                {
                    title = Page.Title = string.Format("{0} 水王排行榜", dt.ToShortDateString());
                    navList = Functions.GetNavList("King.aspx", "d", dt.ToShortDateString());
                    navMonth = Functions.GetNavMonth(dt, "King.aspx", "King.aspx", "&type=m");
                    navDay = Functions.GetNavDay(dt, string.Format("King.aspx?date={0}&type=m", dt.ToShortDateString()), "King.aspx", "&type=d");

                    begin = dt;
                    end = begin.AddYears(1);
                }
                else //d
                {
                    title = Page.Title = string.Format("{0} 水王排行榜", dt.ToShortDateString());
                    navList = Functions.GetNavList("King.aspx", "d", dt.ToShortDateString());
                    navMonth = Functions.GetNavMonth(dt, "King.aspx", "King.aspx", "&type=m");
                    navDay = Functions.GetNavDay(dt, string.Format("King.aspx?date={0}&type=m", dt.ToShortDateString()), "King.aspx", "&type=d");

                    begin = dt;
                    end = begin.AddDays(1);
                }
            }
        }

        private static DateTime lastVisit = new DateTime(1900, 1, 1);
        private static StringBuilder totalKing;

        protected void PrintPage()
        {
            Hashtable hnick = Functions.hashNick;
            Hashtable hrating = Functions.hashRating;

            StringBuilder content = new StringBuilder();

            if (type == "y" && lastVisit == Functions.end)
            {
                content = totalKing;
            }
            else
            {
                SqlConnection conn = new SqlConnection(connstr);
                conn.Open();
                Dictionary<string, int> scores = new Dictionary<string, int>();

                SqlCommand cmd = new SqlCommand("SELECT [qq],sum(score) as s1,sum(d1)as sd1,sum(d2)as sd2,sum(d3)as sd3,sum(d4)as sd4,sum(d5)as sd5,sum(d6)as sd6 FROM [rating] WHERE [time]>=@t1 AND [time]<@t2 GROUP BY [qq] ORDER BY s1 DESC", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = begin;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = end;

                SqlDataReader dr = cmd.ExecuteReader();


                content.Append(string.Format("<table class='full'><thead><tr><th colspan=9>{0}</th></tr></thead>\n", title));
                content.Append("<tbody><tr><th>排名</th><th>昵称(QQ)</th><th>得分</th><th>条数</th><th>10</th><th>100</th><th>图</th><th>早起</th><th>破</th></tr>\n");

                int count = 0;

                while (dr.Read())
                {
                    count++;
                    string qq = dr["qq"].ToString().Trim();
                    content.Append(string.Format("<tr><th class='sub'>{0}</th>" +
                        "<td><a href=\"Rating.aspx?qq={1}\" class=\"{3}\">{2}({1})</a></td>" +
                        "<td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td><td>{9}</td><td>{10}</td></tr>\n",
                        count, qq, hnick[qq], Functions.GetColor((double)hrating[qq]), dr["s1"],
                        dr["sd1"], dr["sd2"], dr["sd3"], dr["sd4"], dr["sd5"], dr["sd6"]));
                }
                dr.Close();
                conn.Close();

                content.Append("</tbody></table>");

                if (type == "y")
                {
                    totalKing = content;
                    lastVisit = Functions.end;
                }
            }
            Response.Write(content);
        }
    }
}
