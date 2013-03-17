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
    public partial class MoreRating : System.Web.UI.Page
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
            Page.Title = qq + " 的全部 Rating 记录";

            begin = Functions.begin;
            end = Functions.end;
            navList = Functions.GetNavList("MoreRating.aspx", "", "");
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

            Response.Write(string.Format("<a href=\"Rating.aspx?qq={0}\">Rating 曲线</a>\n", qq));

            cmd = new SqlCommand("SELECT * FROM [rating] WHERE [qq]=@qq order by [time] DESC", conn);
            cmd.Parameters.Add("qq", SqlDbType.Char).Value = qq;

            Response.Write("<table class='full'>\n");

            Response.Write(string.Format("<thead><tr><th colspan='4'>{0}({1}) 的全部 Rating 记录</th></tr></thead>\n", nickname, qq));
            Response.Write("<tbody><tr><th>日期</th><th>Rating</th><th>排名</th><th>得分</th></tr>\n");
            dr = cmd.ExecuteReader();
            int count = 0;
            while (dr.Read())
            {
                DateTime t = (DateTime)dr["time"];
                double r = (double)dr["rating"];
                count++;
                Response.Write(string.Format("<tr><th class='sub'><a href=\"Overview.aspx?date={0}\">{0}</a></th><td><span class=\"{4}\">{1}</span></td><td>{3}</td><td>{2}</td></tr>\n", t.ToShortDateString(), (int)r, (int)dr["score"], (int)dr["rank"], Functions.GetColor(r)));
            }
            Response.Write("</table>");
            dr.Close();
            conn.Close();
        }
    }
}
