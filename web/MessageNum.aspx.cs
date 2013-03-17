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
    public partial class MessageNum : System.Web.UI.Page
    {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
        protected DateTime begin, end, dt;
        protected string navList, navMonth, navDay;
        protected string type, stat; //类型 y、m、d，统计方式 m、d、h
        protected string title;

        protected void Page_Load(object sender, EventArgs e)
        {
            type = Functions.GetAcceptableType(Request["type"]);
            stat = Functions.GetAcceptableStat(type, Request["stat"]);

            begin = Functions.begin;
            end = Functions.end.AddDays(1);

            if (type == "y")
            {
                title = Page.Title = string.Format("消息数总计");
                navList = Functions.GetNavList("MessageNum.aspx", "", "");
                navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1), "MessageNum.aspx", "MessageNum.aspx", "&type=m&stat=" + stat);
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
                    title = Page.Title = string.Format("{0} 年 {1} 月，消息数", dt.Year, dt.Month);
                    navList = Functions.GetNavList("MessageNum.aspx", "m", dt.ToShortDateString());
                    navMonth = Functions.GetNavMonth(dt, "MessageNum.aspx?stat=" + stat, "MessageNum.aspx", "&type=m&stat=" + stat);
                    begin = new DateTime(dt.Year, dt.Month, 1);
                    navDay = Functions.GetNavDay(new DateTime(dt.Year, dt.Month, dt.Day, 10, 0, 0), string.Format("MessageNum.aspx?date={0}&type=m", dt.ToShortDateString()), "MessageNum.aspx", "&type=d");

                    end = begin.AddMonths(1);

                }
                else //d
                {
                    title = Page.Title = string.Format("{0} 消息数", dt.ToShortDateString());
                    navList = Functions.GetNavList("MessageNum.aspx", "d", dt.ToShortDateString());
                    navMonth = Functions.GetNavMonth(dt, "MessageNum.aspx", "MessageNum.aspx", "&type=m");
                    navDay = Functions.GetNavDay(dt, string.Format("MessageNum.aspx?date={0}&type=m", dt.ToShortDateString()), "MessageNum.aspx", "&type=d");

                    begin = dt;
                    end = begin.AddDays(1);
                }
            }
        }

        protected void PrintPage()
        {
            long t1 = Environment.TickCount;

            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            List<string> col1 = new List<string>(); //第一列的显示文本
            List<string> col2 = new List<string>(); //第二列的显示文本
            List<double> barNumber = new List<double>(); //第二列显示的数量
            string caption = "";

            bool needPrint = true;

            if (stat == "h")
            {
                //计算出总共有几天
                SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT [Time]) FROM [Activity] WHERE [Time]>=@t1 AND [Time]<@t2", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = begin;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = end;
                int dayNumber = (int)cmd.ExecuteScalar();
                if (dayNumber == 0)
                    dayNumber = 1;

                cmd = new SqlCommand("SELECT [Hour], SUM([MessageNumber]) s1 FROM [Activity] WHERE[Time]>=@t1 AND [Time]<@t2 GROUP BY [Hour]", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = begin;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = end;
                SqlDataReader dr = cmd.ExecuteReader();
                int[] num = new int[25];
                while (dr.Read())
                {
                    num[(int)dr["Hour"]] = (int)dr["s1"];
                }
                dr.Close();

                if (num[24] == 0)
                {
                    Response.Write("没有发言记录");
                    needPrint = false;
                }
                else
                {
                    if (type == "d")
                        caption = string.Format("当天消息总数 {0}", num[24]);
                    else
                        caption = string.Format("平均每天消息数 {0:F2}", 1.0 * num[24] / dayNumber);

                    for (int i = 0; i < 24; i++)
                    {
                        col1.Add(string.Format("{0:00}:00", i));
                        barNumber.Add(num[i]);

                        if (type == "d")
                            col2.Add(string.Format("{0:F2}% ({1})", 100.0 * num[i] / num[24], num[i]));
                        else
                            col2.Add(string.Format("{0:F2}% ({1:F2})", 100.0 * num[i] / num[24], 1.0 * num[i] / dayNumber));

                        //Response.Write(string.Format("{0:F2}\n", 1.0 * num[i] / num[24]));
                    }
                }
            }
            else if (stat == "d")
            {
                SqlCommand cmd = new SqlCommand("SELECT [Time],[MessageNumber] FROM [Activity] WHERE[Time]>=@t1 AND [Time]<@t2 AND [Hour]=24", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = begin;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = end;
                SqlDataReader dr = cmd.ExecuteReader();

                int sum = 0, count = 0;
                while (dr.Read())
                {
                    int num = (int)dr["MessageNumber"];
                    col1.Add(((DateTime)dr["Time"]).ToShortDateString());
                    barNumber.Add(num);
                    sum += num;
                    count++;
                }
                dr.Close();

                caption = string.Format("消息总数 {0}，平均每天消息数 {1:F2}", sum, 1.0 * sum / count);
                for (int i = 0; i < col1.Count; i++)
                    col2.Add(string.Format("{0:F2}% ({1})", 100 * barNumber[i] / sum, (int)barNumber[i]));

            }
            else //y
            {
                int sum = 0, count = 0;
                for (DateTime now = new DateTime(begin.Year, begin.Month, 1); now <= end; now = now.AddMonths(1))
                {
                    SqlCommand cmd = new SqlCommand("SELECT SUM([MessageNumber]) FROM [Activity] WHERE [Time]>=@t1 AND [Time]<@t2 AND [Hour]=24", conn);
                    cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = now;
                    cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = now.AddMonths(1);
                    int num = (int)cmd.ExecuteScalar();
                    col1.Add(string.Format("{0} 年 {1} 月", now.Year, now.Month));
                    barNumber.Add(num);
                    count++;
                    sum += num;
                }

                caption = string.Format("消息总数 {0}，平均每月消息数 {1:F2}", sum, 1.0 * sum / count);
                for (int i = 0; i < col1.Count; i++)
                    col2.Add(string.Format("{0:F2}% ({1})", 100 * barNumber[i] / sum, (int)barNumber[i]));

            }

            conn.Close();

            if (needPrint)
            {
                Response.Write("<h5>");
                if (type != "d")
                {
                    if (stat == "h")
                        Response.Write("<span><strong>小时</strong></span>");
                    else
                        Response.Write(string.Format("<span><a href=\"MessageNum.aspx?type={0}&stat=h{1}\">小时</a></span>", type, (type != "y") ? ("&date=" + dt.ToShortDateString()) : ""));
                }

                if (type == "m" || type == "y")
                {
                    if (stat == "d")
                        Response.Write(" | <span><strong>日</strong></span>");
                    else
                        Response.Write(string.Format(" | <span><a href=\"MessageNum.aspx?type={0}&stat=d{1}\">日</a></span>", type, (type != "y") ? ("&date=" + dt.ToShortDateString()) : ""));
                }

                if (type == "y")
                {
                    if (stat == "m")
                        Response.Write(" | <span><strong>月</strong></span>");
                    else
                        Response.Write(string.Format(" | <span><a href=\"MessageNum.aspx?type={0}&stat=m{1}\">月</a></span>", type, (type != "y") ? ("&date=" + dt.ToShortDateString()) : ""));
                }
                Response.Write("</h5>");
                double max = barNumber.Max();

                Response.Write(string.Format("<table class='full'><thead><tr><th colspan=2>{0}</th></tr></thead>\n", caption));
                Response.Write("<tbody><tr><th>时间</th><th>消息数</th></tr>\n");

                for (int i = 0; i < barNumber.Count; i++)
                {
                    Response.Write(string.Format("<tbody><tr><td>{0}</td><td class=\"graph_variable_bar\"><div class=\"graph_hbar\" style=\"width: {2}%\"> </div><span class=\"graph_label\">{1}</span></td></tr>\n", col1[i], col2[i], barNumber[i] / max * 70));
                }
                Response.Write("</tbody></table>");
            }
        }
    }
}
