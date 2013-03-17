using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace acm_diy {
    public partial class testabcsh : System.Web.UI.Page {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) {

        }

        protected void PrintPage() {
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            SqlCommand cmdRec = new SqlCommand("SELECT [keyword],COUNT(*)c,MAX([time])t1,MIN([time])t2 FROM [SearchHistory] GROUP BY [keyword] ORDER BY t1 DESC", conn);
            SqlDataReader dr = cmdRec.ExecuteReader();
            Response.Write("<table cellspacing=1 border=1>");
            int cnt = 1;
            while (dr.Read()) {
                Response.Write("<tr>");
                Response.Write("<td>");
                Response.Write(cnt);
                Response.Write("</td>");
                Response.Write("<td>");
                Response.Write(((string)dr["keyword"]).Trim());
                Response.Write("</td>");
                Response.Write("<td>");
                Response.Write((int)dr["c"]);
                Response.Write("</td>");
                Response.Write("<td>");
                Response.Write((DateTime)dr["t2"]);
                Response.Write("</td>");
                Response.Write("<td>");
                Response.Write((DateTime)dr["t1"]);
                Response.Write("</td>");
                Response.Write("</tr>");
                cnt++;
            }
            Response.Write("</table>");
            conn.Close();
        }
    }
}
