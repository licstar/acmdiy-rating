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
    public partial class Default : System.Web.UI.Page {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime end;
        protected DateTime begin;
        protected string navList, navMonth;


        protected void Page_Load(object sender, EventArgs e) {
            begin = Functions.begin;
            end = Functions.end;
            navList = Functions.GetNavList("Default.aspx", "", "");
            navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1));
        }

        protected void PrintPage() {
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT [Time] FROM [Activity] WHERE [Hour]=24", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            Hashtable hash = new Hashtable();
            while (dr.Read()) {
                hash.Add((DateTime)dr["Time"], 1);
            }
            dr.Close();
            conn.Close();


            for (DateTime now = new DateTime(end.Year, end.Month, 1); now >= new DateTime(begin.Year, begin.Month, 1); now = now.AddMonths(-1)) {
                Response.Write("<div class=\"calendar\">\n");

                Response.Write("<table class=\"title\">");
                Response.Write("<tr align=\"center\">");
                Response.Write(string.Format("<td><a href=\"Monthly.aspx?date={2}\">{0} 年 {1} 月</a></td>", now.Year, now.Month, now.ToShortDateString()));
                Response.Write("</tr>");
                Response.Write("");
                Response.Write("</table>");

                Response.Write(@"<table class=""week"">
	<tr class=""bg""><th class=""fcgreen"">日</th>
	<th>一</th><th>二</th><th>三</th><th>四</th><th>五</th>
	<th class=""fcgreen"">六</th></tr></table>");

                Response.Write("<table class=\"month\">\n");

                DateTime bd = new DateTime(now.Year, now.Month, 1);
                for (DateTime d = bd; d < bd.AddMonths(1); ) {
                    Response.Write("<tr align=\"center\">");

                    for (int i = 0; i < (int)d.DayOfWeek; i++)
                        Response.Write("<td class='nothave'></td>");

                    for (; d < bd.AddMonths(1) && d.DayOfWeek != DayOfWeek.Saturday; d = d.AddDays(1)) {
                        if (d >= begin && d <= end && hash.Contains(d)) {
                            Response.Write(string.Format("<td class='have'><a href='Overview.aspx?date={1}'>{0}</a></td>", d.Day, d.Date.ToShortDateString()));
                        } else {
                            Response.Write(string.Format("<td class='nothave'>{0}</td>", d.Day));
                        }
                    }

                    if (d < bd.AddMonths(1)) {
                        if (d >= begin && d <= end && hash.Contains(d)) {
                            Response.Write(string.Format("<td class='have'><a href='Overview.aspx?date={1}'>{0}</a></td>", d.Day, d.Date.ToShortDateString()));
                        } else {
                            Response.Write(string.Format("<td class='nothave'>{0}</td>", d.Day));
                        }
                        d = d.AddDays(1);
                    }

                    for (int i = (int)d.DayOfWeek; i != 0 && i < 7; i++)
                        Response.Write("<td class='nothave'></td>");

                    Response.Write("</tr>");
                }
                Response.Write("</table>");
                Response.Write("</div>\n");
            }
        }
    }
}
