using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace acm_diy {
    public partial class Ranklist : System.Web.UI.Page {

        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime end;
        protected DateTime begin;
        protected string navList, navMonth;

        protected void Page_Load(object sender, EventArgs e) {
            begin = Functions.begin;
            end = Functions.end;
            navList = Functions.GetNavList("Ranklist.aspx", "", "");
            navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1));
        }

        protected void PrintPage() {
            SqlConnection conn = new SqlConnection(connstr);

            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT [qq],[name],[rating] FROM [nickname] WHERE [rating]>0 ORDER BY [rating] DESC", conn);

            SqlDataReader dr = cmd.ExecuteReader();
            Response.Write("<table class='full'>\n");
            Response.Write("<thead>\n");
            Response.Write("<tr><th colspan=3><strong>Rating 排行榜</strong></th></tr>\n");
            Response.Write("</thead>\n");
            Response.Write("<tbody><tr><th>排名</th><th>昵称(QQ)</th><th>Rating</th></tr>\n");
            int count = 0;
            while (dr.Read()) {
                count++;
                double rating = (double)dr["rating"];
                Response.Write(string.Format("<tr><th class='sub'>{0}</th><td><a href=\"Rating.aspx?qq={2}\" class=\"{4}\">{1}({2})</a></td><td>{3}</td></tr>\n", count, dr["name"], ((string)dr["qq"]).Trim(), (int)rating, Functions.GetColor(rating)));
            }
            dr.Close();

            Response.Write("</tbody></table>");
            conn.Close();
        }
    }
}
