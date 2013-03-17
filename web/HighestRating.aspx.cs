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
    public partial class HighestRating : System.Web.UI.Page
    {

        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime end;
        protected DateTime begin;
        protected string navList, navMonth;

        protected void Page_Load(object sender, EventArgs e)
        {
            begin = Functions.begin;
            end = Functions.end;
            navList = Functions.GetNavList("HighestRating.aspx", "", "");
            navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1));
        }

        protected void PrintPage()
        {
            Hashtable hnick = Functions.hashNick;
            Hashtable hrating = Functions.hashRating;

            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT [Rating].[QQ] qq,[Rating],[Time] FROM [Rating], (SELECT MAX(Rating) r,[QQ] FROM [Rating] GROUP BY [QQ]) t1 WHERE t1.[QQ]=[Rating].[QQ] and t1.r=[Rating].[Rating] ORDER BY t1.r DESC, [Time] ASC", conn);
            SqlDataReader dr = cmd.ExecuteReader();

            Response.Write("<table class='full'>\n");
            Response.Write("<thead><tr><th colspan=4>各成员最高Rating的排名</th></tr></thead>\n");

            Response.Write("<tbody><tr><th>排名</th><th>昵称(QQ)</th><th>时间</th><th>Rating</th></tr>\n");
            int count = 1;

            string lastQQ = "";
            while (dr.Read())
            {
                string qq = dr["qq"].ToString().Trim();
                if (qq == lastQQ)
                    continue;
                int rating = (int)((double)dr["rating"]);
                Response.Write(string.Format("<tr><th class='sub'>{0}</th><td><a href=\"Rating.aspx?qq={2}\" class=\"{6}\">{1}({2})</a></td><td><a href=\"Overview.aspx?date={5}\">{5}</a></td><td><span class=\"{4}\">{3}</span></td></tr>\n",
                   count, hnick[qq], qq, rating, Functions.GetColor(rating), ((DateTime)dr["Time"]).ToShortDateString(), Functions.GetColor((double)hrating[qq])));
                count++;
                lastQQ = qq;
            }
            conn.Close();
            Response.Write("</tbody></table>");
        }
    }
}
