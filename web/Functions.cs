using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Web;
using System.Data.SqlClient;
using System.Collections;
using System.IO;

namespace acm_diy
{
    static class Functions
    {
        private static string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;

        private static DateTime lastVisit = new DateTime(1900, 1, 1);
        private static DateTime cacheDate = new DateTime(1900, 1, 1);
        public static DateTime begin = new DateTime(2009, 2, 7);
        public static DateTime end
        {
            get
            {
                if ((DateTime.Now - lastVisit).TotalMinutes > 15)
                {
                    using (SqlConnection conn = new SqlConnection(connstr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("SELECT MAX([time]) FROM [data]", conn);
                        DateTime end = (DateTime)cmd.ExecuteScalar();
                        lastVisit = DateTime.Now;
                        cacheDate = end.Date;
                    }
                }
                return cacheDate;
            }
        }

        private static void updateRatingNickname()
        {
            if ((DateTime.Now - lastVisitRating).TotalMinutes > 15)
            {
                using (SqlConnection conn = new SqlConnection(connstr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT [qq],[name],[rating] FROM [nickname]", conn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    cacheNick = new Hashtable();
                    cacheRating = new Hashtable();
                    while (dr.Read())
                    {
                        if (dr["rating"] == DBNull.Value) continue;
                        string qq = ((string)dr["qq"]).Trim();
                        string name = (string)dr["name"];
                        double rating = (double)dr["rating"];
                        cacheNick.Add(qq, name);
                        cacheRating.Add(qq, rating);
                    }
                    dr.Close();
                }
                lastVisitRating = DateTime.Now;
            }
        }

        private static DateTime lastVisitRating = new DateTime(1900, 1, 1);
        private static Hashtable cacheNick, cacheRating;

        public static Hashtable hashRating
        {
            get
            {
                updateRatingNickname();
                return cacheRating;
            }
        }

        public static Hashtable hashNick
        {
            get
            {
                updateRatingNickname();
                return cacheNick;
            }
        }

        public static string GetColor(double score)
        {
            if (score >= 3000) return "coderTextGarnet";
            if (score >= 2200) return "coderTextRed";
            if (score >= 1500) return "coderTextYellow";
            if (score >= 1200) return "coderTextBlue";
            if (score >= 900) return "coderTextGreen";
            return "coderTextGray";
        }

        public static string GetLi(string text, string page, string url)
        {
            if (url.StartsWith(page))
            {
                return "<li class=\"active\"><strong>" + text + "</strong></li>";
            }
            else
            {
                return "<li><a href=\"" + url + "\">" + text + "</a></li>";
            }
        }

        /// <summary>
        /// 获得左边的导航栏
        /// </summary>
        /// <param name="page">当前页面网址</param>
        /// <param name="type">传入参数类型</param>
        /// <param name="date">传递日期</param>
        /// <returns></returns>
        public static string GetNavList(string page, string type, string date)
        {
            StringBuilder ret = new StringBuilder();

            ret.Append("<h6 class='vlist'>导航</h6>");
            ret.Append("<ul class='vlist'>");
            if (type == "")
            { //年
                string[] text = new string[] { "概览", "Rating 排名", "水王总榜", "单日水王排行", "历史最高 Rating", "首次到达 Rating", "活跃人数", "消息数", "表情榜", "FAQ" };
                string[] url = new string[] { "Default.aspx", "Ranklist.aspx", "King.aspx", "HighestScore.aspx", "HighestRating.aspx", "HittingRating.aspx", "Online.aspx", "MessageNum.aspx", "Face.aspx", "FAQ.aspx" };

                ret.Append(GetLi(text[0], page, url[0]));
                ret.Append("<li><a>英雄榜</a>");
                ret.Append("<ul>");
                for (int i = 1; i <= 5; i++)
                    ret.Append(GetLi(text[i], page, url[i]));
                ret.Append("</ul>");
                ret.Append("</li>");

                ret.Append("<li><a>群统计</a>");
                ret.Append("<ul>");
                for (int i = 6; i <= 8; i++)
                    ret.Append(GetLi(text[i], page, url[i]));
                ret.Append("</ul>");
                ret.Append("</li>");

                ret.Append(GetLi(text[9], page, url[9]));

            }
            else if (type == "m")
            {
                string[] text = new string[] { "概览", "月水王排行", "活跃人数", "消息数", "表情榜" };
                string[] url = new string[] { "Monthly.aspx", "King.aspx", "Online.aspx", "MessageNum.aspx", "Face.aspx" };
                bool[] needType = new bool[] { false, true, true, true, true };

                for (int i = 0; i < text.Length; i++)
                    ret.Append(GetLi(text[i], page, url[i] + "?date=" + date + (needType[i] ? "&type=m" : "")));

            }
            else if (type == "d")
            {
                string[] text = new string[] { "概览", "当天水王排行", "活跃人数", "消息数", "表情榜" };
                string[] url = new string[] { "Overview.aspx", "King.aspx", "Online.aspx", "MessageNum.aspx", "Face.aspx" };
                bool[] needType = new bool[] { false, true, true, true, true };

                for (int i = 0; i < text.Length; i++)
                    ret.Append(GetLi(text[i], page, url[i] + "?date=" + date + (needType[i] ? "&type=d" : "")));

                ret.Append(GetLi("旧群的聊天记录", page, "Message.aspx?qun=1&date=" + date));
                if (DateTime.Parse(date) >= new DateTime(2010, 3, 6))
                    ret.Append(GetLi("新群的聊天记录", page, "Message.aspx?qun=2&date=" + date));

            }

            ret.Append("</ul>");

            return ret.ToString();
        }

        /// <summary>
        /// 生成月份导航栏
        /// </summary>
        /// <param name="date"></param>
        /// <param name="main"></param>
        /// <param name="sub"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static string GetNavMonth(DateTime date, string main, string sub, string arg)
        {
            StringBuilder ret = new StringBuilder();
            date = new DateTime(date.Year, date.Month, 1);
            int lastYear = -1;
            if (date.Year == 1970)
            {
                ret.Append("<li class=\"active\"><strong>总排名</strong></li>\n");
            }
            else
            {
                ret.Append(string.Format("<li><a href=\"{0}\">总排名</a></li>\n", main));
            }
            for (DateTime now = new DateTime(begin.Year, begin.Month, 1); now <= end; now = now.AddMonths(1))
            {
                if (now == date)
                {
                    ret.Append(string.Format("<li class=\"active\"><strong>{0}</strong></li>\n", now.Year == lastYear ? now.Month.ToString() : now.Year.ToString() + "." + now.Month.ToString()));
                }
                else
                {
                    ret.Append(string.Format("<li><a href=\"{2}?date={0}{3}\">{1}</a></li>\n", now.Date.ToShortDateString(), now.Year == lastYear ? now.Month.ToString() : now.Year.ToString() + "." + now.Month.ToString(), sub, arg));
                }
                lastYear = now.Year;
            }
            return ret.ToString();
        }

        /// <summary>
        /// 生成月份导航栏
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetNavMonth(DateTime date)
        {

            return GetNavMonth(date, "Default.aspx", "Monthly.aspx", "");
        }

        /// <summary>
        /// 生成日期导航栏
        /// </summary>
        /// <param name="date"></param>
        /// <param name="main"></param>
        /// <param name="sub"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static string GetNavDay(DateTime date, string main, string sub, string arg)
        {
            StringBuilder ret = new StringBuilder();
            if (date.Hour == 10)
            {
                ret.Append("<li class=\"active\"><strong>月排名</strong></li>\n");
            }
            else
            {
                ret.Append(string.Format("<li><a href=\"{0}\">月排名</a></li>\n", main));
            }
            DateTime beginDate = begin;
            DateTime endDate = end.AddDays(1);
            DateTime firstDay = new DateTime(date.Year, date.Month, 1);

            if (beginDate < firstDay)
                beginDate = firstDay;
            if (endDate > firstDay.AddMonths(1))
                endDate = firstDay.AddMonths(1);

            for (DateTime now = beginDate; now < endDate; now = now.AddDays(1))
            {
                if (date.Hour == 0 && now == date)
                {
                    ret.Append(string.Format("<li class=\"active\"><strong>{0}</strong></li>\n", now.Day));
                }
                else
                {
                    ret.Append(string.Format("<li><a href=\"{2}?date={0}{3}\">{1}</a></li>\n", now.Date.ToShortDateString(), now.Day, sub, arg));
                }
            }
            return ret.ToString();
        }

        /// <summary>
        /// 生成日期导航栏
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetNavDay(DateTime date)
        {
            return GetNavDay(date, "Monthly.aspx?date=" + date.ToShortDateString(), "Overview.aspx", "");
        }

        /// <summary>
        /// 把输入的Type参数转换为正确的形式
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetAcceptableType(string type)
        {
            if (type == null)
            {
                return "y";
            }
            else if (type == "m" || type == "M")
            {
                return "m";
            } else if (type == "d" || type == "D") {
                return "d";
            } else if (type == "yy" || type == "YY") {
                return "yy";
            }
            else
            {
                return "y";
            }
        }

        public static bool CheckDate(string date, out DateTime dt)
        {
            dt = DateTime.Now;
            if (date == null)
                return false;

            if (!DateTime.TryParse(date, out dt))
                return false;

            if (dt < new DateTime(begin.Year, begin.Month, 1)) //超出边界
                return false;

            if (dt > end) //超出边界
                return false;

            return true;
        }

        public static string GetAcceptableStat(string type, string stat)
        {
            if (stat == null)
            {
                return "h";
            }
            else if ((stat == "d" || stat == "D") && (type == "m" || type == "y"))
            {
                return "d";
            }
            else if ((stat == "m" || stat == "M") && type == "y")
            {
                return "m";
            }
            else
            {
                return "h";
            }
        }
    }
}