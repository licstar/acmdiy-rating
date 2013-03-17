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
    public partial class Face : System.Web.UI.Page
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
                title = Page.Title = string.Format("表情总榜");
                navList = Functions.GetNavList("Face.aspx", "", "");
                navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1), "Face.aspx", "Face.aspx", "&type=m");
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
                    title = Page.Title = string.Format("{0} 年 {1} 月，表情榜", dt.Year, dt.Month);
                    navList = Functions.GetNavList("Face.aspx", "m", dt.ToShortDateString());
                    navMonth = Functions.GetNavMonth(dt, "Face.aspx", "Face.aspx", "&type=m");
                    begin = new DateTime(dt.Year, dt.Month, 1);
                    navDay = Functions.GetNavDay(new DateTime(dt.Year, dt.Month, dt.Day, 10, 0, 0), string.Format("Face.aspx?date={0}&type=m", dt.ToShortDateString()), "Face.aspx", "&type=d");

                    end = begin.AddMonths(1);

                }
                else //d
                {
                    title = Page.Title = string.Format("{0} 表情榜", dt.ToShortDateString());
                    navList = Functions.GetNavList("Face.aspx", "d", dt.ToShortDateString());
                    navMonth = Functions.GetNavMonth(dt, "Face.aspx", "Face.aspx", "&type=m");
                    navDay = Functions.GetNavDay(dt, string.Format("Face.aspx?date={0}&type=m", dt.ToShortDateString()), "Face.aspx", "&type=d");

                    begin = dt;
                    end = begin.AddDays(1);
                }
            }
        }

        protected void PrintPage()
        {
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            Dictionary<string, int> scores = new Dictionary<string, int>();

            SqlCommand cmd = new SqlCommand("SELECT [FaceID], SUM([Number]) s1 FROM [Face] WHERE [time]>=@t1 AND [time]<@t2 GROUP BY [FaceID]", conn);
            cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = begin;
            cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = end;

            int[] count = new int[135];
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                count[(int)dr["FaceID"]] = (int)dr["s1"];
            }
            dr.Close();
            conn.Close();

            Response.Write(string.Format("<table class='full'><thead><tr><th colspan=15>{0}</th></tr></thead>\n", title));
            Response.Write("<tbody>\n");
            for (int i = 0; i < count.Length; i += 15)
            {
                Response.Write("<tr>");
                for (int j = i; j < i + 15; j++)
                {
                    Response.Write(string.Format("<td><img src='img/{0}.gif' /><br />{1}</td>", j, count[j]));
                }
                Response.Write("</tr>");
            }
            Response.Write("</tbody></table>");

            int[] id = new int[count.Length];
            for (int i = 0; i < id.Length; i++)
                id[i] = i;

            Array.Sort(count, id);

            int num = 0;
            for (int i = count.Length - 1; i >= 0; i--)
                if (count[i] > 0)
                    num++;

            if (num > 30) num = 30;

            if (num > 0)
            {

                Response.Write(string.Format("<hr /><table class='full'><thead><tr><th colspan=3>表情 TOP {0}</th></tr></thead>\n", num));
                Response.Write("<tbody>\n");
                Response.Write("<tr><th>排名</th><th>表情</th><th>数量</th></tr>");
                for (int i = 0; i < num; i++)
                {
                    int t = count.Length - i - 1;
                    Response.Write(string.Format("<tr><th class='sub'>{0}</th><td><img src='img/{1}.gif' /></td><td>{2}</td></tr>", i + 1, id[t], count[t]));
                }
                Response.Write("</tbody></table>");
            }

        }
    }
}
