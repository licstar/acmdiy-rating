using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Collections;

namespace acm_diy
{
    public partial class HittingRating : System.Web.UI.Page
    {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime end;
        protected DateTime begin;
        protected string navList, navMonth;

        protected void Page_Load(object sender, EventArgs e)
        {
            begin = Functions.begin;
            end = Functions.end;
            navList = Functions.GetNavList("HittingRating.aspx", "", "");
            navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1));
        }

        protected void PrintPage()
        {
            Hashtable hnick = Functions.hashNick;
            Hashtable hrating = Functions.hashRating;

            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT [Rating].[QQ] qq,[Rating].[Rating] rating,[Rating].[Time] time FROM [Rating], (SELECT MAX(Rating) r,[Time] FROM [Rating] GROUP BY [Time]) t1 WHERE t1.[Time]=[Rating].[Time] and t1.r=[Rating].[Rating] ORDER BY t1.r DESC, [Rating].[Time] ASC", conn);

            Response.Write("<table class='full'>\n");
            Response.Write("<thead><tr><th colspan=4>第一位到达某Rating的成员</th></tr></thead>");
            Response.Write("<tbody><tr><th>#</th><th>昵称(QQ)</th><th>时间</th><th>Rating</th></tr>\n");

            SqlDataReader dr = cmd.ExecuteReader();
            int count = 1;
            DateTime lastTime = DateTime.Now.AddDays(1);
            while (dr.Read())
            {
                DateTime date = (DateTime)dr["time"];
                if (date > lastTime)
                    continue;
                lastTime = date;

                double rating = (double)dr["rating"];
                string qq = ((string)dr["QQ"]).Trim();


                Response.Write(string.Format("<tr><th class='sub'>{0}</th><td><a href=\"Rating.aspx?qq={2}\" class=\"{5}\">{1}({2})</a></td><td><a href=\"Overview.aspx?date={6}\">{6}</a></td><td><span class=\"{4}\">{3}</span></td></tr>\n",
    count, hnick[qq], qq, (int)rating, Functions.GetColor(rating), Functions.GetColor((double)hrating[qq]), date.ToShortDateString()));
                count++;

            }
            dr.Close();

            conn.Close();

            Response.Write("</tbody></table>");
            //Response.Write(Environment.TickCount - t1);
        }
    }
}
