using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Collections;
using System.Data;

namespace acm_diy
{
    public partial class Overview : System.Web.UI.Page
    {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime begin, end, dt;
        protected string navList, navMonth, navDay;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Functions.CheckDate(Request["date"], out dt))
            {
                Response.Write("日期错误");
                Response.End();
            }

            begin = Functions.begin;
            end = Functions.end;

            Page.Title = string.Format("{0} 水王排行榜", dt.ToShortDateString());
            navList = Functions.GetNavList("Overview.aspx", "d", dt.ToShortDateString());
            navMonth = Functions.GetNavMonth(dt);
            navDay = Functions.GetNavDay(dt);

            begin = dt;
            end = begin.AddDays(1);
        }


        protected void PrintPage()
        {
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT TOP 11 [Message],[groupNum] FROM [data] WHERE [time]>=@t1 and [time]<@t2 AND [groupNum]>1 ORDER BY [groupNum] DESC", conn);
            cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = dt;
            cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = dt.AddDays(1);

            Response.Write("<table class='full'>\n");
            Response.Write(string.Format("<thead><tr><th colspan='3'>热门消息</th></tr></thead>\n"));
            Response.Write("<tbody><tr><th>排名</th><th>消息</th><th>次数</th></tr>\n");

            SqlDataReader dr = cmd.ExecuteReader();
            int cnt = 0;
            while (dr.Read())
            {
                string s = PreProcessUtility.HTML2Text((string)dr["Message"]);

                if (s != "")
                {
                    cnt++;
                    Response.Write(string.Format("<tr><th class='sub'>{0}</th><td>{1}</td><td>{2}</td></tr>\n", cnt, dr["Message"], dr["groupNum"]));
                }
            }

            Response.Write("</tbody></table>");
            dr.Close();

            cmd = new SqlCommand("SELECT [Message] FROM [data] WHERE [time]>=@t1 and [time]<@t2", conn);
            cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = dt;
            cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = dt.AddDays(1);
            dr = cmd.ExecuteReader();

            Dictionary<string, int> picCount = new Dictionary<string, int>();
            while (dr.Read())
            {
                string txt = (string)dr["Message"];

                int p = txt.IndexOf("<IMG");
                while (p != -1)
                {
                    int e = txt.IndexOf(">", p);
                    string s = txt.Substring(p, e - p + 1);
                    txt = txt.Remove(p, e - p + 1);

                    int st = s.IndexOf("\"");
                    int ed = s.IndexOf("\"", st + 1);
                    string url = s.Substring(st + 1, ed - st - 1);
                    if (url.Contains("msg"))
                    {
                        if (url == "msg/{3829986E-A56D-B399-818B-F5F378ACFBF1}.gif")
                            url = "msg/{CC350580-F5C6-7354-D337-29AD641E0C54}.gif";
                        if (picCount.ContainsKey(url))
                        {
                            picCount[url]++;
                        }
                        else
                        {
                            picCount.Add(url, 1);
                        }
                    }
                    p = txt.IndexOf("<IMG");
                }
            }
            dr.Close();
            conn.Close();

            int[] cnts = new int[picCount.Count];
            string[] paths = new string[picCount.Count];
            cnt = 0;
            foreach (var o in picCount)
            {
                cnts[cnt] = o.Value;
                paths[cnt] = o.Key;
                cnt++;
            }

            Array.Sort(cnts, paths);
            Array.Reverse(cnts);
            Array.Reverse(paths);

            cnt = 0;

            Response.Write("<hr /><table class='full'>\n");
            Response.Write(string.Format("<thead><tr><th colspan='3'>热门图片</th></tr></thead>\n"));
            Response.Write("<tbody><tr><th>排名</th><th>图片</th><th>次数</th></tr>\n");
            for (int i = 0; i < 5 && i < cnts.Length && cnts[i] > 1; i++)
            {
                cnt++;
                Response.Write(string.Format("<tr><th class='sub'>{0}</th><td><img src=\"{1}\" onload=\"javascript:if(480<this.width){{this.width=480}}\"  onerror=\"javascript:this.src='img/e.gif'\" /></td><td>{2}</td></tr>\n", cnt, paths[i], cnts[i]));
            }
            Response.Write("</tbody></table>");
        }
    }
}
