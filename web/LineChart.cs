using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Web;

namespace Jim.GoogleChart {
    static class Helper {
        public static string ToHex(this Color c) {
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
        public static string ToHexArgb(this Color c) {
            return c.ToHex() + c.A.ToString("X2");
        }

        public static string Join<T>(IEnumerable<T> objs, string sep) {
            return Join(objs, o => o, sep);
        }

        public static string Join<T>(IEnumerable<T> objs, Func<T, object> func, string sep) {
            bool first = true;
            var buf = new StringBuilder();
            foreach (var obj in objs) {
                if (!first) {
                    buf.Append(sep);
                }
                buf.Append(func(obj));
                first = false;
            }
            return buf.ToString();
        }
    }

    public class LineChart {
        static class Formats {
            public static readonly Color BgGrey = Color.FromArgb(229, 229, 229);
            public static readonly Color BgGreen = Color.FromArgb(191, 233, 191);
            public static readonly Color BgBlue = Color.FromArgb(217, 217, 255);
            public static readonly Color BgYellow = Color.FromArgb(246, 242, 191);
            public static readonly Color BgRed = Color.FromArgb(251, 191, 191);
            public static readonly Color BgGarnet = Color.FromArgb(162, 85, 109);
            public static readonly Color LineColor = Color.Black;

            public static readonly int Green = 900;
            public static readonly int Blue = 1200;
            public static readonly int Yellow = 1500;
            public static readonly int Red = 2200;
            public static readonly int Garnet = 3000;
            public static readonly int MaxRating = 4000;

            public static readonly Color MarkerGrey = Color.FromArgb(153, 153, 153);
            public static readonly Color MarkerGreen = Color.FromArgb(0, 169, 0);
            public static readonly Color MarkerBlue = Color.FromArgb(102, 102, 255);
            public static readonly Color MarkerYellow = Color.FromArgb(221, 204, 0);
            public static readonly Color MarkerRed = Color.FromArgb(255, 0, 0);
            public static readonly Color MarkerGarnet = Color.FromArgb(56, 1, 12);

            public static Color GetMarkerColor(double rating) {
                if (rating < Green) return MarkerGrey;
                if (rating < Blue) return MarkerGreen;
                if (rating < Yellow) return MarkerBlue;
                if (rating < Red) return MarkerYellow;
                if (rating < Garnet) return MarkerRed;
                return MarkerGarnet;
            }
        }

        private readonly List<DateTime> times;
        private readonly List<double> ratings;
        private StringBuilder url;
        private double min, max, avg;
        private int ymin, ymax;
        private DateTime firstDay;

        /// 公开变量
        public int Width = 700, Height = 360;
        public double MarkerSize = 7;
        public string Title = String.Empty;

        /// 常量
        private static readonly string GoogleUrl = "http://chart.apis.google.com/chart";
        private static readonly int DayCount = 40;
        private static readonly int DaysPerLabel = 7; // 这二者整除最好

        public LineChart(List<DateTime> times, List<double> ratings) {
            if (times.Count != ratings.Count) {
                throw new Exception("时间和Rating必须一一对应。");
            }
            if (times.Count > DayCount + 1) {
                throw new Exception(String.Format("最多只能支持{0}天。", DayCount));
            }
            if ((times[times.Count - 1] - times[0]).TotalDays > DayCount) {
                throw new Exception(String.Format("时间跨度不能超过{0}天。", DayCount));
            }

            this.times = times;
            this.ratings = ratings;
        }

        public string GetUrl() {
            this.url = new StringBuilder(GoogleUrl);
            this.setBasicUrl();
            this.calcData();
            this.addRange();
            this.addPosition();
            this.addLabel();
            this.addData();
            this.addDataScale();
            this.addBackground();
            this.addMarker();

            return url.ToString();
        }

        private void setBasicUrl() {
            url.AppendFormat("?cht=lxy&chs={0}x{1}", Width, Height); // cht=lxy设置为可跳点的折线图，chs设置图大小，最高只能是30万像素
            url.AppendFormat("&chco={0}&chxt=x,y,r", Formats.LineColor.ToHexArgb()); // chco设置线条颜色，chxt设置三条坐标轴，分别是x,y,right
            if (!String.IsNullOrEmpty(Title)) {
                url.AppendFormat("&chtt={0}", Title);
            }
        }

        private void calcData() {
            min = ratings.Min();
            max = ratings.Max();
            avg = ratings.Average();

            ymin = (int)Math.Max(min - 100, 0);
            ymax = (int)Math.Min(max + 100, Formats.MaxRating);

            firstDay = times[0];
        }

        private void addRange() {
            url.Append("&chxr=");
            url.AppendFormat("0,-1,{0}", DayCount); // DayCount+1??
            url.AppendFormat("|1,{0},{1}", ymin, ymax);
            url.AppendFormat("|2,{0},{1}", ymin, ymax);
        }

        private void addPosition() {
            url.Append("&chxp=");
            url.Append("0");
            for (int i = 0; i <= DayCount; i += DaysPerLabel) {
                url.AppendFormat(",{0}", i);
            }
            url.AppendFormat("|2,{0},{1},{2}", min, avg, max);
        }

        private void addLabel() {
            url.Append("&chxl=");
            url.Append("0:");
            var time = firstDay;
            for (int i = 0; i <= DayCount; i += DaysPerLabel) {
                url.AppendFormat("|{0:yy/M/d}", time);
                time = time.AddDays(DaysPerLabel);
            }
            url.AppendFormat("|2:|Min:{0}|Avg:{1:F1}|Max:{2}", (int)min, avg, (int)max);
        }

        private void addData() {
            url.Append("&chd=t:");
            url.Append(Helper.Join(times, t => (int)(t - firstDay).TotalDays, ","));
            url.Append("|");
            url.Append(Helper.Join(ratings, r => (int)r, ","));
        }

        private void addDataScale() {
            url.AppendFormat("&chds={0},{1},{2},{3}", -1, DayCount, ymin, ymax);
        }

        private void addBackground() {
            url.Append("&chf=c,ls,90"); double last = 0.0;
            if (ymin < Formats.Green) {
                var green = calcPercent(Formats.Green, ymin, ymax);
                addBacgroundStrip(Formats.BgGrey, green - last);
                last = green;
            }
            if (ymin < Formats.Blue) {
                var blue = calcPercent(Formats.Blue, ymin, ymax);
                addBacgroundStrip(Formats.BgGreen, blue - last);
                last = blue;
            }
            if (ymin < Formats.Yellow) {
                var yellow = calcPercent(Formats.Yellow, ymin, ymax);
                addBacgroundStrip(Formats.BgBlue, yellow - last);
                last = yellow;
            }
            if (ymin < Formats.Red) {
                var red = calcPercent(Formats.Red, ymin, ymax);
                addBacgroundStrip(Formats.BgYellow, red - last);
                last = red;
            }
            if (ymin < Formats.Garnet) {
                var garnet = calcPercent(Formats.Garnet, ymin, ymax);
                addBacgroundStrip(Formats.BgRed, garnet - last);
                last = garnet;
            }
            if (ymax > Formats.Garnet) {
                addBacgroundStrip(Formats.BgGarnet, 1.0 - last);
            }
        }

        private void addMarker() {
            url.Append("&chm=");
            var markers = ratings.Select((r, i) => String.Format("s,{0},0,{1},{2:F1}", Formats.GetMarkerColor(r).ToHex(), i, MarkerSize));
            url.Append(Helper.Join(markers, "|"));
        }
        private void addBacgroundStrip(Color c, double l) {
            url.AppendFormat(",{0},{1:F4}", c.ToHex(), l);
        }

        private static double calcPercent(double val, double min, double max) {
            return (val - min) / (max - min);
        }
    }
}
