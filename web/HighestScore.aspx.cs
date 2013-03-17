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
    public partial class HighestScore : System.Web.UI.Page
    {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime end;
        protected DateTime begin;
        protected string navList, navMonth;

        protected void Page_Load(object sender, EventArgs e)
        {
            begin = Functions.begin;
            end = Functions.end;
            navList = Functions.GetNavList("HighestScore.aspx", "", "");
            navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1));
        }

        protected void PrintPage()
        {
            SqlConnection conn = new SqlConnection(connstr);

            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT [qq],[name],[rating] FROM [nickname]", conn);
            SqlDataReader dr = cmd.ExecuteReader();

            Hashtable hnick = new Hashtable();
            Hashtable hrating = new Hashtable();
            while (dr.Read())
            {
                if (dr["rating"] == DBNull.Value) continue;
                string qq = ((string)dr["qq"]).Trim();
                string name = (string)dr["name"];
                double rating = (double)dr["rating"];
                hnick.Add(qq, name);
                hrating.Add(qq, rating);
            }
            dr.Close();


            cmd = new SqlCommand("SELECT TOP 100 * FROM [rating] order by [score] DESC", conn);

            Response.Write(string.Format("<table class='full'>\n"));
            Response.Write("<thead><tr><th colspan=10>单日水王排行</th></tr></thead>\n");
            Response.Write("<tbody><tr><th>排名(当天排名)</th><th>日期</th><th>昵称(QQ)</th><th>得分</th><th>条数</th><th>10</th><th>100</th><th>图</th><th>早起</th><th>破</th></tr>\n");
            dr = cmd.ExecuteReader();
            int count = 0;
            while (dr.Read())
            {
                string qq = dr["qq"].ToString().Trim();
                int score = (int)dr["score"];
                count++;
                Response.Write(string.Format("<tr><th class='sub'>{10}({8})</th><td><a href=\"Overview.aspx?date={11}\">{11}</a></td><td><a href=\"Rating.aspx?qq={0}\" class=\"{9}\">{7}({0})</a></td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{12}</td></tr>\n", qq, score, dr["d1"], dr["d2"], dr["d3"], dr["d4"], dr["d5"], hnick[qq], dr["rank"], Functions.GetColor((double)hrating[qq]), count, ((DateTime)dr["time"]).ToShortDateString(),dr["d6"]));
            }
            Response.Write("</tbody></table>");
            dr.Close();

        }
    }
}
