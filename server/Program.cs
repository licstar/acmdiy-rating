using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using SharpICTCLAS;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ACM_DIY_Update {

    class QQRecord {
        public string QQ { get; set; }
        public DateTime Time { get; set; }
        public string Text { get; set; }
        public int ImageCount { get; set; }
    }


    class Record {
        public int CharCount { get; set; }
        public int ItemCount { get; set; }
        public int ImageCount { get; set; }
    }

    class PairIntString : IComparable<PairIntString> {
        public int first;
        public string second;

        public PairIntString(int f, string s) {
            first = f;
            second = s;
        }

        #region IComparable<PairIntString> 成员

        public int CompareTo(PairIntString other) {
            return other.first.CompareTo(this.first);
        }

        #endregion
    }

    class Program {
        const double K = 8;

        static string connstr = "Data Source=(local);Initial Catalog='acmdiy';user='sa';password='不告诉你密码'";

        static string imagePath = @"C:\wwwroot\acm_diy\msg\";

        /// <summary>
        /// 下载网页
        /// </summary>
        /// <param name="Url">网址</param>
        /// <param name="myEncoding">编码</param>
        /// <returns>网页内容</returns>
        public static string DownHtml(string Url, Encoding myEncoding) {
            while (true) {
                int cnt = 0;
                try {
                    HttpWebRequest loHttp = (HttpWebRequest)WebRequest.Create(Url);
                    loHttp.Timeout = 5000;
                    loHttp.Referer = "http://qun.qq.com/air/";
                    loHttp.CookieContainer = cookie;
                    loHttp.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)";
                    HttpWebResponse loWebResponse = (HttpWebResponse)loHttp.GetResponse();
                    StreamReader loResponseStream = new StreamReader(loWebResponse.GetResponseStream(), myEncoding);
                    string html = loResponseStream.ReadToEnd();
                    loWebResponse.Close();
                    return html;
                } catch {
                    cnt++;
                    if (cnt == 5)
                        return null;
                }
            }
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="Url">网址</param>
        /// <returns>网页内容</returns>
        public static byte[] DownImage(string Url, bool decode) {
            try {
                Uri u = new Uri(Url, decode);
                HttpWebRequest loHttp = (HttpWebRequest)WebRequest.Create(u);
                loHttp.Timeout = 60000;
                loHttp.KeepAlive = true;
                loHttp.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C)";
                loHttp.CookieContainer = cookie;
                loHttp.Referer = "http://qun.qq.com/air/";
                //StreamReader sr = new StreamReader(loHttp.GetRequestStream(), Encoding.Default);
                loHttp.Accept = "image/jpeg, application/x-ms-application, image/gif, application/xaml+xml, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
                loHttp.Headers.Add("Accept-Encoding", "gzip, deflate");
                loHttp.Headers.Add("Accept-Language", "zh-cn");
                loHttp.AllowAutoRedirect = true;//.Accept += "Accept: image/jpeg, application/x-ms-application, image/gif, application/xaml+xml, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
                //Console.WriteLine(sr.ReadToEnd());

                HttpWebResponse loWebResponse = (HttpWebResponse)loHttp.GetResponse();
                Stream s = loWebResponse.GetResponseStream();
                byte[] buffer = new byte[65536];
                List<byte> ret = new List<byte>();
                int count = s.Read(buffer, 0, buffer.Length);
                while (count != 0) {
                    for (int i = 0; i < count; i++) {
                        ret.Add(buffer[i]);
                    }
                    count = s.Read(buffer, 0, buffer.Length);
                }
                //StreamReader loResponseStream = new StreamReader(loWebResponse.GetResponseStream(), myEncoding);
                //string html = loResponseStream.ReadToEnd();
                loWebResponse.Close();
                return ret.ToArray();
                //return html;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static CookieContainer cookie;

        static Dictionary<string, string> images = null;

        static void downloadImages() {
            foreach (var o in images) {
                if (!File.Exists(imagePath + o.Key)) {
                    byte[] img = DownImage(o.Value, true);
                    int cnt = 0;
                    while (img == null) {
                        if (cnt > 5)
                            break;
                        img = DownImage(o.Value, cnt % 2 == 1);
                        cnt++;
                        Thread.Sleep(500);
                    }
                    Console.WriteLine(o.Value);
                    if (img != null) {
                        File.WriteAllBytes(imagePath + o.Key, img);
                        Console.WriteLine("OK{1} {0}", o.Key, cnt);
                    } else {
                        Console.WriteLine("XX {0}", o.Key);
                    }
                }
            }
        }


        static int CalcScore(string text, out int d1, out int d2, out int d3, out int d4) {
            text = text.ToLower();
            int ret = 0;
            int p = text.IndexOf("<img");
            while (p != -1) {
                int e = text.IndexOf(">", p);
                text = text.Remove(p, e - p + 1);
                p = text.IndexOf("<img");
                ret++;
            }
            /*
            p = text.IndexOf("<");
            while (p != -1)
            {
                int e = text.IndexOf(">", p);
                text = text.Remove(p, e - p + 1);
                p = text.IndexOf("<");
            }

            text = text.Replace("&lt;", "<").Replace("&gt;", ">");*/
            text = PreProcessUtility.HTML2Text(text);

            d1 = d2 = d3 = d4 = 0;
            int score = 1;
            d1 = 1;
            if (text.Length > 10) {
                score++;
                d2 = 1;
            }
            if (text.Length > 100) {
                score++;
                d3 = 1;
            }
            if (ret > 0) {
                score++;
                d4 = 1;
            }
            return score;
        }

        static double Calp(double r2, double r1) {
            return 1.0 / (1.0 + Math.Pow(10.0, (r2 - r1) / 400.0));
        }

        /// <summary>
        /// 更新所有昵称
        /// </summary>
        static void UpdateNickname() {

            string[] quns = new string[] { "48866438", "66459919" };
            Dictionary<string, string> newName = new Dictionary<string, string>();

            foreach (string qun in quns) {

                string s = DownHtml("http://cgi.qun.qq.com/gscgi/s4/mygroup/getgmcard.do?callback=jsonp1319004682832&_=1319017843531&retype=2&gls=%5B" + qun + "%5D", Encoding.UTF8);
                s = s.Substring(s.IndexOf("(") + 1);
                s = s.Substring(0, s.Length - 2);
                while (s.Length < 30) {
                    Console.WriteLine("获取昵称出错，正在重试");
                    Thread.Sleep(60000);
                    s = DownHtml("http://cgi.qun.qq.com/gscgi/s4/mygroup/getgmcard.do?callback=jsonp1319004682832&_=1319017843531&retype=2&gls=%5B" + qun + "%5D", Encoding.UTF8);
                    s = s.Substring(s.IndexOf("(") + 1);
                    s = s.Substring(0, s.Length - 2);
                }
                JObject o = JObject.Parse(s);
                Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>(o["data"][qun]["list"].ToString());

                foreach (var p in d) {
                    if (!newName.ContainsKey(p.Key)) {
                        newName.Add(p.Key, p.Value);
                    }
                }
            }

            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            Dictionary<string, string> oldName = new Dictionary<string, string>();

            SqlCommand cmd = new SqlCommand("SELECT * FROM [nickname]", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) {
                string qq = dr["qq"].ToString().Trim();
                string nick = (string)dr["name"];
                oldName.Add(qq, nick);
            }
            dr.Close();

            foreach (var o in newName) {
                string nick = o.Value;
                if (oldName.ContainsKey(o.Key)) { //用户已存在
                    string oldNick = oldName[o.Key];
                    if (nick != oldNick) { //用户改昵称
                        cmd = new SqlCommand("UPDATE [nickname] SET [name]=@name WHERE [qq]=@qq", conn);
                        cmd.Parameters.Add("qq", SqlDbType.Char).Value = o.Key;
                        cmd.Parameters.Add("name", SqlDbType.NVarChar).Value = nick;
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("{0} {1} -> {2}", o.Key, oldNick, nick);
                    }
                } else { //用户不存在
                    cmd = new SqlCommand("INSERT INTO [nickname] ([QQ],[name]) VALUES (@qq, @name)", conn);
                    cmd.Parameters.Add("qq", SqlDbType.Char).Value = o.Key;
                    cmd.Parameters.Add("name", SqlDbType.NVarChar).Value = nick;
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("+{0} {1}", o.Key, nick);
                }
            }

            conn.Close();
        }

        static void QQFace() {
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM [data]", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            int[] count = new int[105];
            string s1 = "<img src=\"";
            while (dr.Read()) {
                string s = ((string)dr["message"]).ToLower();
                int p = s.IndexOf(s1);
                while (p != -1) {
                    p = s.IndexOf("img/", p);
                    int e = s.IndexOf("\"", p);
                    string ss = s.Substring(p + 4, e - p - 8);
                    int t;
                    if (int.TryParse(ss, out t)) {
                        count[t]++;
                    }
                    //Console.WriteLine(ss);
                    p = s.IndexOf(s1, e);
                }
            }
            conn.Close();
            //<IMG src="img/53.gif">
            int[] index = new int[105];
            for (int i = 0; i < 105; i++) {
                index[i] = i;
            }


            StreamWriter sw = new StreamWriter(@"C:\Users\licstar\Desktop\pic.html");
            sw.WriteLine("<table>");
            for (int i = 0; i < 105; ) {
                sw.WriteLine("<tr>");
                for (int j = 0; j < 15; j++, i++) {
                    sw.WriteLine("<td><IMG src='img/{0}.gif'> {1}</td>", i, count[i]);
                }
                sw.WriteLine("</tr>");
            }
            sw.WriteLine("</table>");

            Array.Sort(count, index);
            for (int i = 104; i >= 0; i--) {
                sw.WriteLine("{1} <IMG src='img/{0}.gif'><br />", index[i], count[i]);
            }
            sw.Close();
        }


        static void CalcQQFace(DateTime begin, DateTime end) {
            SqlConnection conn = new SqlConnection(connstr);

            conn.Open();

            for (DateTime d = begin; d <= end; d = d.AddDays(1)) {
                //DateTime now = new DateTime(d.Year, d.Month, d.Day, hour, 0, 0);
                SqlCommand cmd = new SqlCommand("SELECT [Message] FROM [data] WHERE [Time]>=@t1 AND [Time]<@t2", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = d;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = d.AddDays(1);

                SqlDataReader dr = cmd.ExecuteReader();
                int[] count = new int[135];
                string s1 = "<img src=\"";
                while (dr.Read()) {
                    string s = ((string)dr["message"]).ToLower();
                    int p = s.IndexOf(s1);
                    while (p != -1) {
                        p = s.IndexOf("img/", p);
                        int e = s.IndexOf("\"", p);
                        string ss = s.Substring(p + 4, e - p - 8);
                        int t;
                        if (int.TryParse(ss, out t)) {
                            count[t]++;
                        }
                        //Console.WriteLine(ss);
                        p = s.IndexOf(s1, e);
                    }
                }
                dr.Close();

                for (int i = 0; i < count.Length; i++) {
                    if (count[i] == 0) continue;
                    cmd = new SqlCommand("INSERT INTO [Face] ([Time], [FaceID], [Number]) VALUES(@time, @faceid, @number)", conn);
                    cmd.Parameters.Add("time", SqlDbType.DateTime).Value = d;
                    cmd.Parameters.Add("faceid", SqlDbType.Int).Value = i;
                    cmd.Parameters.Add("number", SqlDbType.Int).Value = count[i];
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine(d);
            }
            conn.Close();
        }

        static Dictionary<string, int> hash = new Dictionary<string, int>();

        static List<List<int>> post = new List<List<int>>();
        static List<List<int>> tf = new List<List<int>>();
        static List<int> df = new List<int>();

        static void AddResult(List<WordResult[]> result, int tid) {
            //Console.WriteLine();
            Dictionary<string, int> dict = new Dictionary<string, int>();
            for (int i = 0; i < result.Count; i++) {
                for (int j = 1; j < result[i].Length - 1; j++) {
                    // if (Utility.GetPOSString(result[i][j].nPOS) == "w\0")
                    //     continue;
                    string s = result[i][j].sWord.ToLower();
                    if (dict.ContainsKey(s)) {
                        dict[s]++;
                    } else {
                        dict.Add(s, 1);
                    }
                }
                //Console.Write("{0} /{1} ", result[i][j].sWord, Utility.GetPOSString(result[i][j].nPOS));

                //Console.WriteLine();
            }
            foreach (var o in dict) {
                int id = -1;
                if (hash.ContainsKey(o.Key)) {
                    id = hash[o.Key];
                    post[id].Add(tid);
                    tf[id].Add(o.Value);
                    df[id]++;
                } else {
                    id = hash.Count;
                    hash.Add(o.Key, id);

                    List<int> a = new List<int>();
                    List<int> b = new List<int>();

                    post.Add(a);
                    tf.Add(b);

                    post[id].Add(tid);
                    tf[id].Add(o.Value);
                    df.Add(1);
                }
            }
        }

        static byte[] toByte(int[] array) {
            byte[] ret = new byte[array.Length * 4];

            for (int i = 0, k = 0; i < array.Length; i++) {
                byte[] t = BitConverter.GetBytes(array[i]);
                for (int j = 0; j < 4; j++, k++) {
                    ret[k] += t[j];
                }
            }
            /*BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memStream = new MemoryStream();
            formatter.Serialize(memStream, array);
            memStream.Position = 0;
            byte[] b = memStream.GetBuffer();
            memStream.Close();*/
            return ret;
        }

        static byte[] connectBytes(byte[] a, byte[] b) {
            byte[] ret = new byte[a.Length + b.Length];
            int j = 0;
            for (int i = 0; i < a.Length; i++, j++)
                ret[j] = a[i];
            for (int i = 0; i < b.Length; i++, j++)
                ret[j] = b[i];
            return ret;
        }

        static void CalcWords(DateTime begin, DateTime end) {
            //string DictPath = Path.Combine(Environment.CurrentDirectory, "Data") + Path.DirectorySeparatorChar;

            //WordDictionary dict = new WordDictionary();
            //dict.Load(DictPath + "coreDict.dct");

            //dict.AddItem("后缀数组", Utility.GetPOSValue("n"), 10);
            //dict.AddItem("膜拜", Utility.GetPOSValue("v"), 100);
            //dict.AddItem("蒟蒻", Utility.GetPOSValue("n"), 10);

            //dict.Save(DictPath + "coreDictNew.dct");


            WordSegment wordSegment = new WordSegment();
            string dictpath = Path.Combine(Environment.CurrentDirectory, "Data") + Path.DirectorySeparatorChar;
            //string dictpath = @"d:\acmdiy\data\";//
            wordSegment.InitWordSegment(dictpath);


            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            Dictionary<string, int> dictID = new Dictionary<string, int>();
            SqlCommand cmd = new SqlCommand("SELECT [word],[id] FROM [Index]", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) {
                dictID.Add(((string)dr["word"]).Trim(), (int)dr["id"]);
            }
            dr.Close();


            for (DateTime d = begin; d <= end; d = d.AddDays(1)) {
                //DateTime now = new DateTime(d.Year, d.Month, d.Day, hour, 0, 0);
                cmd = new SqlCommand("SELECT [Message],[groupid] FROM [data] WHERE [Time]>=@t1 AND [Time]<@t2 ORDER BY [id] ASC", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = d;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = d.AddDays(1);

                dr = cmd.ExecuteReader();
                hash = new Dictionary<string, int>();
                post = new List<List<int>>();
                df = new List<int>();
                tf = new List<List<int>>();

                Dictionary<int, bool> chkid = new Dictionary<int, bool>();

                while (dr.Read()) {
                    string s = PreProcessUtility.HTML2Text((string)dr["message"]);
                    s = s.Replace("\r", " ").Replace("\n", " ").Replace("/", " / ").Replace(":", " : ").Replace("<", " < ").Replace(">", " > ").Replace("\"", " \" ");
                    s = s.Replace("&", " & ").Replace("(", " ( ").Replace(")", " ) ").Replace("-", " - ").Replace("'", " ' ");
                    //try
                    {
                        List<WordResult[]> ret = wordSegment.Segment(s, 1);
                        int id = (int)dr["groupid"];
                        if (chkid.ContainsKey(id)) {
                            continue;
                        }
                        chkid.Add(id, true);
                        AddResult(ret, id);
                    }
                    //catch (Exception e)
                    //{
                    //    Console.WriteLine(s);
                    //    Console.WriteLine(e.Message);
                    //    Console.WriteLine("===============");
                    //    continue;
                    //}
                }
                dr.Close();

                Console.WriteLine("{0} {1}", d, hash.Count);

                foreach (var o in hash) {
                    int len = Encoding.Default.GetBytes(o.Key).Length;
                    if (len <= 64) {
                        //try {
                        //cmd = new SqlCommand("SELECT COUNT(*) FROM [Index] WHERE [word]=@word", conn);
                        //cmd.Parameters.Add("word", SqlDbType.Char).Value = o.Key;
                        //int hid = -1;

                        if (!dictID.ContainsKey(o.Key)) {  //首次出现，插入
                            cmd = new SqlCommand("INSERT INTO [Index] ([word], [df], [tf], [posts]) VALUES (@word, @df, @tf, @posts);SELECT @@IDENTITY", conn);

                            cmd.Parameters.Add("word", SqlDbType.Char).Value = o.Key;
                            cmd.Parameters.Add("df", SqlDbType.Int).Value = df[o.Value];
                            cmd.Parameters.Add("tf", SqlDbType.Image).Value = toByte(tf[o.Value].ToArray());
                            cmd.Parameters.Add("posts", SqlDbType.Image).Value = toByte(post[o.Value].ToArray());
                            int t = (int)((decimal)cmd.ExecuteScalar());
                            dictID.Add(o.Key, t);
                        } else { //第二次出现，添加
                            int hid = dictID[o.Key];
                            cmd = new SqlCommand("SELECT [posts],[tf] FROM [Index] WHERE [id]=@id", conn);
                            cmd.Parameters.Add("id", SqlDbType.Int).Value = hid;
                            dr = cmd.ExecuteReader();
                            dr.Read();
                            byte[] p1 = (byte[])dr["posts"];
                            byte[] t1 = (byte[])dr["tf"];
                            dr.Close();

                            cmd = new SqlCommand("UPDATE [Index] SET [df]=[df]+@df, [tf]=@tf, [posts]=@posts WHERE [id]=@id", conn);
                            cmd.Parameters.Add("id", SqlDbType.Int).Value = hid;
                            //cmd.Parameters.Add("word", SqlDbType.Char).Value = o.Key;
                            cmd.Parameters.Add("df", SqlDbType.Int).Value = df[o.Value];
                            cmd.Parameters.Add("tf", SqlDbType.Image).Value = connectBytes(t1, toByte(tf[o.Value].ToArray()));
                            cmd.Parameters.Add("posts", SqlDbType.Image).Value = connectBytes(p1, toByte(post[o.Value].ToArray()));
                            cmd.ExecuteNonQuery();
                        }
                        //} catch (Exception e) {
                        //    Console.WriteLine(o.Key);
                        //    Console.WriteLine(e.Message);

                        //    Console.WriteLine("===============");
                        //}
                    }
                }

                //string[] a = new string[hash.Count];
                //int[] b = new int[hash.Count];
                //int cnt = 0;
                //foreach (var o in hash)
                //{
                //    a[cnt] = o.Key;
                //    b[cnt] = o.Value;
                //    cnt++;
                //}
                //Array.Sort(b, a);

                ////foreach (var o in hash)
                //for (int i = b.Length - 1, t = 0; i >= 0 && t <= 500; i--, t++)
                //{
                //    //if (a[i].Length == 1)
                //    //    continue;
                //    Console.Write("{0} {1}\t", a[i], b[i]);
                //}
                //Console.WriteLine("=============");
            }


            conn.Close();


        }

        static string[] WordResultToString(List<WordResult[]> result) {
            List<string> ret = new List<string>();
            for (int i = 0; i < result.Count; i++) {
                for (int j = 1; j < result[i].Length - 1; j++) {
                    if (Utility.GetPOSString(result[i][j].nPOS) == "w\0")
                        continue;
                    string s = result[i][j].sWord.ToLower();
                    ret.Add(s);
                }
            }
            return ret.ToArray();
        }

        static int[] CombineLists(int[] p1, int[] p2, double[] t1, double[] t2, out double[] t) {
            Array.Sort(p1);
            Array.Sort(p2);
            List<int> ret = new List<int>();
            List<double> tfidf = new List<double>();

            for (int i = 0, j = 0; i < p1.Length && j < p2.Length; ) {
                if (p1[i] == p2[j]) {
                    ret.Add(p1[i]);
                    tfidf.Add(t1[i] + t2[j]);
                    i++;
                    j++;
                } else if (p1[i] < p2[j]) {
                    i++;
                } else {
                    j++;
                }
            }
            t = tfidf.ToArray();
            return ret.ToArray();
        }

        static double[] getTfIdf(int[] tf, int df, int N) {
            double idf = Math.Log(1.0 * N / df);
            double[] ret = new double[tf.Length];
            for (int i = 0; i < tf.Length; i++)
                ret[i] = tf[i] * idf;
            return ret;
        }

        static void Search(string str) {
            long t1 = Environment.TickCount;

            WordSegment wordSegment = new WordSegment();
            string dictpath = Path.Combine(Environment.CurrentDirectory, "Data") + Path.DirectorySeparatorChar;
            wordSegment.InitWordSegment(dictpath);

            //str = PreProcessUtility.ToSimplifyString(str);
            str = str.Replace("\r", " ").Replace("\n", " ").Replace("/", " / ").Replace(":", " : ").Replace("<", " < ").Replace(">", " > ").Replace("\"", " \" ");
            str = str.Replace("&", " & ").Replace("(", " ( ").Replace(")", " ) ").Replace("-", " - ").Replace("'", " ' ");


            string[] words = WordResultToString(wordSegment.Segment(str, 1));


            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [data]", conn);

            int N = (int)cmd.ExecuteScalar();

            int[] results = null;
            double[] tfidf = null;

            for (int i = 0; i < words.Length; i++) {
                cmd = new SqlCommand("SELECT [df], [tf], [posts] FROM [Index] WHERE [word]=@word", conn);
                cmd.Parameters.Add("word", SqlDbType.Char).Value = words[i].ToLower();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read()) {
                    int[] tf = BytesToInts((byte[])dr["tf"]);
                    int[] posts = BytesToInts((byte[])dr["posts"]);
                    int df = (int)dr["df"];

                    double[] tfidf2 = getTfIdf(tf, df, N);

                    if (results == null) {
                        results = posts;
                        tfidf = tfidf2;
                    } else {
                        results = CombineLists(results, posts, tfidf, tfidf2, out tfidf);
                    }

                    Console.Write("{0} {1} {2}:", words[i], df, tf.Length);

                    Console.WriteLine();
                } else {
                    //考虑考虑怎么办？标点符号可以忽略。别的词呢……
                    Console.WriteLine("{0}", words[i]);
                }
                dr.Close();
            }

            if (results == null) {
                Console.WriteLine("没找到");
            } else {

                for (int j = 0; j < results.Length; j++) {
                    //  Console.Write(" {0}", results[j]);
                }
                Console.WriteLine();

                Array.Sort(tfidf, results);
                Array.Reverse(tfidf);
                Array.Reverse(results);
                int cnt = 0;
                for (int j = 0; j < results.Length && cnt < 10; j++) {
                    cmd = new SqlCommand("SELECT [Message] FROM [data] WHERE [id]=@id", conn);
                    cmd.Parameters.Add("id", SqlDbType.Int).Value = results[j];
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read()) {
                        Console.WriteLine(PreProcessUtility.HTML2Text(((string)dr["message"])));
                        Console.WriteLine("========== {0} == {1} =========", tfidf[j], results[j]);

                    }
                    dr.Close();
                    cnt++;
                }
            }

            conn.Close();

            Console.WriteLine("共找到 {0}", results.Length);
            Console.WriteLine("用时 {0}", Environment.TickCount - t1);
        }


        public static int[] BytesToInts(byte[] bytes) {
            //得到结构体的大小
            int size = bytes.Length;
            //byte数组长度小于结构体的大小
            if (size % 4 != 0) {
                //返回空
                return null;
            }
            int[] ret = new int[size / 4];
            for (int i = 0; i < size; i += 4) {
                ret[i / 4] = BitConverter.ToInt32(bytes, i);
            }
            return ret;
        }


        static void CalcMessageGroups(DateTime begin, DateTime end) {
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            for (DateTime d = begin; d <= end; d = d.AddDays(1)) {
                //DateTime now = new DateTime(d.Year, d.Month, d.Day, hour, 0, 0);
                SqlCommand cmd = new SqlCommand("SELECT [id],[Message] FROM [data] WHERE [Time]>=@t1 AND [Time]<@t2 ORDER BY [id] ASC", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = d;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = d.AddDays(1);

                List<int> ids = new List<int>();
                List<int> gids = new List<int>();
                List<int> counts = new List<int>();

                Dictionary<string, int> hash = new Dictionary<string, int>();
                //Dictionary<string, int> count = new Dictionary<string, int>();
                Dictionary<string, int> countID = new Dictionary<string, int>();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read()) {
                    string msg = (string)dr["message"];
                    int id = (int)dr["id"];
                    msg = PreProcessUtility.HTML2Text(msg);
                    int len = Encoding.Default.GetBytes(msg).Length;

                    int gid = id;

                    if (hash.ContainsKey(msg)) {
                        gid = hash[msg];
                        counts[countID[msg]]++;
                    } else {
                        hash.Add(msg, id);
                        countID.Add(msg, ids.Count);
                    }

                    ids.Add(id);
                    gids.Add(gid);
                    counts.Add(1);
                }
                dr.Close();

                for (int i = 0; i < gids.Count; i++) {
                    //cmd = new SqlCommand("SELECT COUNT(*) FROM [data] WHERE [GroupID]=@gid AND [ID]=@id", conn);
                    //cmd.Parameters.Add("id", SqlDbType.Int).Value = ids[i];
                    //cmd.Parameters.Add("gid", SqlDbType.Int).Value = gids[i];
                    //if ((int)cmd.ExecuteScalar() != 1) {
                    //    int aaaaa = 1;
                    //}

                    cmd = new SqlCommand("UPDATE [data] SET [GroupID]=@gid,[groupNum]=@gnum WHERE [ID]=@id", conn);
                    cmd.Parameters.Add("id", SqlDbType.Int).Value = ids[i];
                    cmd.Parameters.Add("gid", SqlDbType.Int).Value = gids[i];
                    cmd.Parameters.Add("gnum", SqlDbType.Int).Value = counts[i];
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine(d);
            }
            conn.Close();
        }

        static void CalcFrequentWords(DateTime begin, DateTime end) {

            WordSegment wordSegment = new WordSegment();
            string dictpath = Path.Combine(Environment.CurrentDirectory, "Data") + Path.DirectorySeparatorChar;
            wordSegment.InitWordSegment(dictpath);


            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            Dictionary<string, int> dictID = new Dictionary<string, int>();
            SqlCommand cmd = new SqlCommand("SELECT [word],[id] FROM [Index]", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) {
                dictID.Add(((string)dr["word"]).Trim(), (int)dr["id"]);
            }
            dr.Close();


            for (DateTime d = begin; d <= end; d = d.AddDays(1)) {

                Dictionary<int, string> dict = new Dictionary<int, string>();
                Dictionary<int, int[]> ps = new Dictionary<int, int[]>();


                cmd = new SqlCommand("SELECT [Message],[groupid] FROM [data] WHERE [Time]>=@t1 AND [Time]<@t2 ORDER BY [id] ASC", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = d;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = d.AddDays(1);

                dr = cmd.ExecuteReader();
                hash = new Dictionary<string, int>();
                post = new List<List<int>>();
                df = new List<int>();
                tf = new List<List<int>>();

                Dictionary<int, bool> chkid = new Dictionary<int, bool>();

                while (dr.Read()) {
                    string s = PreProcessUtility.HTML2Text((string)dr["message"]);
                    s = s.Replace("\r", " ").Replace("\n", " ").Replace("/", " / ").Replace(":", " : ").Replace("<", " < ").Replace(">", " > ").Replace("\"", " \" ");
                    s = s.Replace("&", " & ").Replace("(", " ( ").Replace(")", " ) ").Replace("-", " - ").Replace("'", " ' ");
                    {
                        List<WordResult[]> ret = wordSegment.Segment(s, 1);
                        int id = (int)dr["groupid"];
                        if (chkid.ContainsKey(id)) {
                            continue;
                        }
                        chkid.Add(id, true);
                        AddResult(ret, id);
                    }
                }
                dr.Close();

                Console.WriteLine("{0} {1}", d, hash.Count);

                foreach (var o in hash) {
                    int len = Encoding.Default.GetBytes(o.Key).Length;
                    if (len <= 64) {

                        //cmd.Parameters.Add("word", SqlDbType.Char).Value = o.Key;
                        //cmd.Parameters.Add("df", SqlDbType.Int).Value = df[o.Value];

                        //cmd.Parameters.Add("posts", SqlDbType.Image).Value = toByte(post[o.Value].ToArray());

                        int id = dictID[o.Key];
                        string word = o.Key.Trim();
                        dict.Add(id, word);
                        int[] posts = post[o.Value].ToArray();
                        ps.Add(id, posts);
                    }
                }

                Apriori(dict, ps, conn);
                transWords();
            }


            conn.Close();




            //SqlConnection conn = new SqlConnection(connstr);
            //conn.Open();
            //SqlCommand cmd = new SqlCommand("SELECT [id], [word], [posts] FROM [Index] WHERE [df]>=" + supply, conn);

            //SqlDataReader dr = cmd.ExecuteReader();
            //while (dr.Read())
            //{

            //}
            //dr.Close();


        }


        static void chkImages() {
            images = new Dictionary<string, string>();
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM [data] WHERE [Time]>='2011-1-2'", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) {
                string s = (string)dr["message"];
                int p = s.IndexOf("<IMG");
                while (p != -1) {
                    s = s.Substring(p);
                    if (s.StartsWith("<IMG src=\"img/")) {
                        s = s.Substring(1);
                        p = s.IndexOf("<IMG");
                        continue;
                    }
                    p = s.IndexOf("\"");
                    int e = s.IndexOf("\"", p + 1);
                    string file = s.Substring(p + 1, e - p - 1);
                    if (!File.Exists(@"D:\wwwroot\acm_diy\" + file)) {
                        file = file.Substring(4);
                        string qun = "48866438";
                        if ((int)dr["qun"] == 1)
                            qun = "66459919";

                        DateTime d11 = DateTime.Parse("2011-01-01 00:00:00");
                        int time = 1293811200 + (int)((DateTime)dr["time"] - d11).TotalSeconds;

                        // byte[] b = DownImage(string.Format("http://qun.qq.com/cgi/svr/chatimg/get?pic=msg/{0}&gid=48866438&time=1295328965", file));
                        string url = string.Format("http://qun.qq.com/cgi/svr/chatimg/get?pic={0}&gid={1}&time={2}", file, qun, time);
                        if (!images.ContainsKey(file)) {
                            images.Add(file, url);
                            Console.WriteLine("{0} {1} {2}", (DateTime)dr["time"], file, time);
                        }
                        //Console.WriteLine(file);
                    }

                    p = s.IndexOf("<IMG", 1);
                }
            }
            conn.Close();
            //
            downloadImages();
        }

        /// <summary>
        /// 登陆QQ（目前只是简单地导入一个能用的cookies）
        /// </summary>
        static void Login() {
            Uri u = new Uri("http://cgi.qun.qq.com");
            cookie = new CookieContainer(20);

            string coo = "用一个浏览器登陆qun.qq.com，然后把cookies字符串复制过来……";
            string[] ccoo = coo.Split(';');
            for (int i = 0; i < ccoo.Length; i++) {
                int p = ccoo[i].IndexOf("=");
                string s1 = ccoo[i].Substring(0, p).Trim();
                string s2 = ccoo[i].Substring(p + 1).Trim();
                cookie.Add(u, new Cookie(s1, s2));
            }
        }

        static DateTime getTimeFromTicks(long ticks) {
            DateTime d = new DateTime(1970, 1, 1, 8, 0, 0);
            d = d.Add(new TimeSpan(ticks * 10000));
            return d;
        }

        static long getTicksFromTime(DateTime time) {
            return (time - new DateTime(1970, 1, 1, 8, 0, 0)).Ticks / 10000;
        }


        /// <summary>
        /// 下载指定某天的聊天记录
        /// </summary>
        /// <param name="time"></param>
        static void downloadOneDay(DateTime time, string qun) {
            string s = DownHtml("http://cgi.qun.qq.com/gscgi/api/roamchatlogdates?callback=jsonp1319004682821&_=1319004702610&gid=" + qun + "&retype=2", Encoding.UTF8);
            //上面这句已经改过了，半成品
            JObject o = null;
            while (true) {
                s = s.Substring(s.IndexOf("(") + 1);
                s = s.Substring(0, s.Length - 2);
                o = JObject.Parse(s);
                if (o["result"] != null)
                    break;
                Console.WriteLine("获取群消息时发生错误，20秒后重试");
                Thread.Sleep(20000);
                s = DownHtml("http://cgi.qun.qq.com/gscgi/api/roamchatlogdates?callback=jsonp1319004682821&_=1319004702610&gid=" + qun + "&retype=2", Encoding.UTF8);
            }

            int begin = 0;
            int end = 0;
            foreach (var p in o["result"]["info"]) {
                if ((int)p["ymd"] == time.Year * 10000 + time.Month * 100 + time.Day) {
                    begin = (int)p["begseq"];
                    end = (int)p["endseq"];
                    break;
                }
            }

            s = DownHtml("http://cgi.qun.qq.com/gscgi/api/roamchatlog?callback=jsonp1319004682830&_=1319017843531&gid=" + qun + "&ps=10&bs=" + begin + "&es=" + end + "&mode=1&retype=2", Encoding.UTF8);

            while (true) {
                s = s.Substring(s.IndexOf("(") + 1);
                s = s.Substring(0, s.Length - 2);
                o = JObject.Parse(s);
                if (o["result"] != null && o["result"]["cl"] != null)
                    break;
                Console.WriteLine("获取群消息时发生错误，20秒后重试");
                Thread.Sleep(20000);
                s = DownHtml("http://cgi.qun.qq.com/gscgi/api/roamchatlog?callback=jsonp1319004682830&_=1319017843531&gid=" + qun + "&ps=10&bs=" + begin + "&es=" + end + "&mode=1&retype=2", Encoding.UTF8);
            }


            List<QQRecord> recs = new List<QQRecord>();

            foreach (var p in o["result"]["cl"]) {
                DateTime dt = new DateTime(1970, 1, 1, 8, 0, 0).AddSeconds((int)p["t"]);
                if (!(dt >= time && dt < time.AddDays(1)))
                    continue;

                string user = ((long)p["u"]).ToString();
                QQRecord rec = new QQRecord();
                rec.ImageCount = 0;
                string text = "";
                if (p["il"] != null) {
                    foreach (var data in p["il"]) {
                        int type = (int)data["t"];
                        if (type == 0) { //文本
                            string t = (string)data["v"];
                            t = t.Replace("&", "&amp;");
                            t = t.Replace("<", "&lt;");
                            t = t.Replace(">", "&gt;");
                            t = t.Replace("\r", "<br />");
                            text += t;
                        } else if (type == 1) { //表情
                            text += string.Format("<IMG src=\"img/{0}.gif\">", data["v"]);
                            rec.ImageCount++;
                        } else if (type == 2) { //图片
                            text += string.Format("<IMG src=\"msg/{0}\" onload=\"javascript:if(480<this.width){{this.width=480}}\" onerror=\"javascript:this.src='img/e.gif'\" />", data["v"]);
                            rec.ImageCount++;
                            string name = (string)data["v"];
                            string url = string.Format("http://qun.qq.com/cgi/svr/chatimg/get?pic={0}&gid={1}", name, qun);
                            if (!images.ContainsKey(name)) {
                                images.Add(name, url);
                            }
                        } else {
                            Console.WriteLine("发现聊天记录中的新类型 {0}", type);
                        }
                    }
                }
                rec.Text = text;
                rec.QQ = user;
                rec.Time = dt;
                recs.Add(rec);
            }

            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            for (int i = 0; i < recs.Count; i++) {
                SqlCommand cmd = new SqlCommand("INSERT INTO [data] ([QQ],[Message],[Time],[qun]) VALUES (@qq, @mesg,@time,@qun)", conn);
                cmd.Parameters.Add("qq", SqlDbType.Char).Value = recs[i].QQ;
                cmd.Parameters.Add("mesg", SqlDbType.Text).Value = recs[i].Text;
                cmd.Parameters.Add("time", SqlDbType.DateTime).Value = recs[i].Time;
                cmd.Parameters.Add("qun", SqlDbType.Int).Value = qun == "48866438" ? 2 : 1;
                cmd.ExecuteNonQuery();
            }
            conn.Close();

        }


        static void Main(string[] args) {
            //transWords();
            //return;
            //chkImages();
            //return;
            //List<string> newQQ2 = InsertMessageTemp();
            //DateTime begin2 = new DateTime(2010, 12, 30);
            //DateTime end2 = new DateTime(2011, 1, 1);
            //CalcRating(begin2, end2);
            //CalcOnline(begin2, end2);
            //CalcQQFace(begin2, end2);
            //CalcMessageGroups(begin2, end2);
            //CalcWords(begin2, end2);

            //UpdateNickname(newQQ2.ToArray());

            //return;
            //CalcFrequentWords(begin2, end2);
            //return;

            //QQFace();
            //  return;
            // doupdatename();
            //   return;
            /*SqlConnection conn2 = new SqlConnection(connstr);
            conn2.Open();
            SqlCommand cmd2 = new SqlCommand("TRUNCATE TABLE [Index]", conn2);
            cmd2.ExecuteNonQuery();
            conn2.Close();*/

            //DateTime begin2 = new DateTime(2009, 2, 7);
            ////DateTime begin2 = new DateTime(2012, 9, 6);
            //DateTime end2 = new DateTime(2012, 9, 6);
            //CalcRating(begin2, end2);
            //return;


            //byte[] b_arr2 = { 30, 30, 30, 30 };
            //int[] aaaa = (int[])BytesToInts(b_arr2);

            //WordSegment wordSegment = new WordSegment();
            //string dictpath = Path.Combine(Environment.CurrentDirectory, "Data") + Path.DirectorySeparatorChar;
            //wordSegment.InitWordSegment(dictpath);



            //string s = PreProcessUtility.ToSimplifyString(PreProcessUtility.HTML2Text("<font style=\"font-size:9pt;font-family:'宋体','MS Sans Serif',sans-serif;\" color='000000'>仰慕HDU&nbsp;MM队的身体……</font>"));
            //        s = s.Replace("\r", " ").Replace("\n", " ").Replace("/", " / ").Replace(":", " : ").Replace("<", " < ").Replace(">", " > ").Replace("\"", " \" ");
            //        s = s.Replace("&", " & ").Replace("(", " ( ").Replace(")", " ) ").Replace("-", " - ").Replace("'", " ' ");
            //        //try

            //            List<WordResult[]> ret = wordSegment.Segment(s, 1);

            //Search("仰慕身体");
            // Search("快速排序复杂度");
            //// byte[] ss = Encoding.Default.GetBytes("中文eng");
            //// string s = PreProcessUtility.HTML2Text("&#39;");
            // return;
            //JObject o =  JObject.Parse(@"{""retcode"":0,""result"":{""cl"":[{""u"":609833026,""t"":1319043921,""il"":[{""v"":""鸭梨很大"",""t"":0}]},{""u"":609833026,""t"":1319084026,""il"":[{""v"":""test：\r`+_)(*\u0026^%$#@!~|}{\"":?\u003e\u003c,./;\u0027[]\\"",""t"":0}]}],""bs"":2317,""es"":2318}}");
            //JObject o = JObject.Parse(File.ReadAllText(@"C:\Users\licstar\Desktop\北京场内排行榜\2011北京赛区现场赛排行榜\2011北京赛区现场赛排行榜\bin\Debug\popup_r.js"));
            //return;


            Login();

            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT MAX([time]) FROM [data]", conn);
            DateTime start = (DateTime)cmd.ExecuteScalar();
            conn.Close();
            Console.WriteLine(start);
            start = start.Date.AddDays(1);

            var t = new Thread(() => {
                while (true) {
                    UpdateNickname();
                    Thread.Sleep(300000);
                }
            });
            t.Start();


            for (; ; start = start.AddDays(1)) {
                Console.WriteLine("准备更新 {0}", start);

                DateTime limit = start.AddDays(1).AddMinutes(5);// new DateTime(2010, 3, 20, 0, 5, 0);

                while (DateTime.Now < limit) {
                    Console.WriteLine((limit - DateTime.Now).TotalMinutes);
                    
                    Thread.Sleep((int)Math.Min(300000, 
                        (limit - DateTime.Now).TotalSeconds + 5));
                }

                //UpdateNickname();

                DateTime begin = start;// new DateTime(2010, 3, 19);
                DateTime end = start;//DateTime.Now.Date.AddDays(-1);// new DateTime(2010, 3, 19);

                images = new Dictionary<string, string>();
                downloadOneDay(start, "66459919"); //旧群
                downloadOneDay(start, "48866438"); //新群
                downloadImages();

                CalcRating(begin, end);
                CalcOnline(begin, end);
                CalcQQFace(begin, end);
                CalcMessageGroups(begin, end);
                CalcWords(begin, end);

            }
        }



        /// <summary>
        /// 计算指定时间区间的在线人数
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        static void CalcOnline(DateTime begin, DateTime end) {
            SqlConnection conn = new SqlConnection(connstr);

            conn.Open();

            for (DateTime d = begin; d <= end; d = d.AddDays(1)) {
                //DateTime now = new DateTime(d.Year, d.Month, d.Day, hour, 0, 0);
                SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT [QQ]) c1,COUNT(*) c2 FROM [data] WHERE [Time]>=@t1 AND [Time]<@t2", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = d;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = d.AddDays(1);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                int count = (int)dr["c1"];
                int number = (int)dr["c2"];
                dr.Close();
                if (count != 0) {
                    cmd = new SqlCommand("INSERT INTO [Activity] ([Time],[Hour],[MessageNumber],[OnlineNumber]) VALUES (@time, @hour, @number, @count)", conn);
                    cmd.Parameters.Add("time", SqlDbType.DateTime).Value = d;
                    cmd.Parameters.Add("hour", SqlDbType.Int).Value = 24;
                    cmd.Parameters.Add("number", SqlDbType.Int).Value = number;
                    cmd.Parameters.Add("count", SqlDbType.Int).Value = count;
                    cmd.ExecuteNonQuery();
                }
                for (int hour = 0; hour < 24; hour++) {
                    DateTime now = new DateTime(d.Year, d.Month, d.Day, hour, 0, 0);
                    cmd = new SqlCommand("SELECT COUNT(DISTINCT [QQ]) c1,COUNT(*) c2 FROM [data] WHERE [Time]>=@t1 AND [Time]<@t2", conn);
                    cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = now;
                    cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = now.AddHours(1);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    count = (int)dr["c1"];
                    number = (int)dr["c2"];
                    dr.Close();
                    if (count != 0) {
                        cmd = new SqlCommand("INSERT INTO [Activity] ([Time],[Hour],[MessageNumber],[OnlineNumber]) VALUES (@time, @hour, @number, @count)", conn);
                        cmd.Parameters.Add("time", SqlDbType.DateTime).Value = d;
                        cmd.Parameters.Add("hour", SqlDbType.Int).Value = hour;
                        cmd.Parameters.Add("number", SqlDbType.Int).Value = number;
                        cmd.Parameters.Add("count", SqlDbType.Int).Value = count;
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine(d);
            }
            conn.Close();
        }


        /// <summary>
        /// 计算指定时间区间的rating
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        static void CalcRating(DateTime begin, DateTime end) {
            SqlConnection conn = new SqlConnection(connstr);

            conn.Open();

            for (DateTime d = begin; d <= end; d = d.AddDays(1)) {
                Console.WriteLine(d);
                Dictionary<string, int> score = new Dictionary<string, int>();

                Dictionary<string, int>[] hd = new Dictionary<string, int>[4];
                for (int i = 0; i < 4; i++) {
                    hd[i] = new Dictionary<string, int>();
                }
                Dictionary<string, double> breakScore = new Dictionary<string, double>(); //破光环的得分

                Dictionary<string, int> hgetup = new Dictionary<string, int>();

                string getupKing = "";
                int maxScore = 0;
                DateTime tl = d.AddHours(5.5);
                //统计当天发言信息
                SqlCommand cmd = new SqlCommand("SELECT * FROM [data] WHERE [time]>=@t1 AND [time]<@t2 ORDER BY [time]", conn);
                cmd.Parameters.Add("t1", SqlDbType.DateTime).Value = d;
                cmd.Parameters.Add("t2", SqlDbType.DateTime).Value = d.AddDays(1);
                SqlDataReader dr = cmd.ExecuteReader();

                DateTime lastTime = d;

                while (dr.Read()) {
                    string qq = dr["qq"].ToString().Trim();
                    string msg = dr["message"].ToString();
                    DateTime time = (DateTime)dr["time"];
                    if (time >= tl && getupKing == "") {
                        getupKing = qq;
                    }

                    double bs = 0;
                    if (time.Hour < 1 || time.Hour >= 7) {
                        if (time.Hour >= 7 && lastTime.Hour < 7)
                            lastTime = new DateTime(lastTime.Year, lastTime.Month, lastTime.Day, 7, 0, 0);
                        bs = Math.Pow((time - lastTime).TotalHours, 1.5) * 50; //1小时计50分
                        lastTime = time;
                    }

                    int[] di = new int[4];
                    if (score.ContainsKey(qq)) {
                        score[qq] += CalcScore(msg, out di[0], out di[1], out di[2], out di[3]);
                        for (int i = 0; i < 4; i++) {
                            hd[i][qq] += di[i];
                        }
                        breakScore[qq] += bs;
                    } else {
                        score.Add(qq, CalcScore(msg, out di[0], out di[1], out di[2], out di[3]));
                        for (int i = 0; i < 4; i++) {
                            hd[i].Add(qq, di[i]);
                        }
                        breakScore.Add(qq, bs);
                    }
                    maxScore = Math.Max(score[qq], maxScore);
                }
                dr.Close();

                //叠加破光环的得分
                foreach (var o in breakScore) {
                    score[o.Key] += Math.Min((int)o.Value, 100);
                }

                //存在早起帝
                if (getupKing != "") {
                    //int ts = Math.Min(20, maxScore / 3);
                    int ts = 20;
                    score[getupKing] += ts;
                    hgetup.Add(getupKing, ts);
                }
                List<PairIntString> vp = new List<PairIntString>();
                Dictionary<string, double> rate = new Dictionary<string, double>();


                foreach (KeyValuePair<string, int> p in score) {

                    cmd = new SqlCommand("SELECT * FROM [nickname] WHERE [qq]=@qq", conn);
                    cmd.Parameters.Add("qq", SqlDbType.Char).Value = p.Key;
                    dr = cmd.ExecuteReader();
                    if (dr.Read()) {
                        if (dr["rating"] == DBNull.Value) {
                            rate.Add(p.Key, 1200);
                        } else {
                            rate.Add(p.Key, (double)dr["rating"]);
                        }
                        dr.Close();
                    } else {
                        dr.Close();
                        cmd = new SqlCommand("INSERT INTO [nickname] ([QQ],[name]) VALUES (@qq, @name)", conn);
                        cmd.Parameters.Add("qq", SqlDbType.Char).Value = p.Key;
                        cmd.Parameters.Add("name", SqlDbType.NVarChar).Value = "未知昵称";
                        cmd.ExecuteNonQuery();
                        rate.Add(p.Key, 1200);
                    }
                    vp.Add(new PairIntString(p.Value, p.Key));
                }
                vp.Sort();

                Dictionary<string, double> frank = new Dictionary<string, double>();
                Dictionary<string, double> rank = new Dictionary<string, double>();
                for (int i = 0; i < vp.Count; ) {
                    int j;
                    for (j = i + 1; j < vp.Count; ++j) {
                        if (vp[j].first != vp[i].first) {
                            break;
                        }
                    }
                    --j;
                    double mid = (i + j) / 2.0;
                    int f = i;
                    for (; i <= j; ++i) {
                        rank[vp[i].second] = mid;
                        frank[vp[i].second] = f + 1;
                    }
                }
                Dictionary<string, double> shouldberank = new Dictionary<string, double>();
                for (int i = 0; i < vp.Count; ++i) {
                    double sum = 0;
                    for (int j = 0; j < vp.Count; ++j) {
                        if (j == i) {
                            continue;
                        }
                        sum += Calp(rate[vp[i].second], rate[vp[j].second]);
                    }
                    shouldberank[vp[i].second] = sum;
                }
                for (int i = 0; i < vp.Count; ++i) {
                    double add = K * (shouldberank[vp[i].second] - rank[vp[i].second]);
                    if (add < 0 && -add > rate[vp[i].second] / 10) { //最多只跌10%
                        add = -rate[vp[i].second] / 10;
                    }
                    rate[vp[i].second] += add;
                }

                cmd = new SqlCommand("UPDATE [nickname] SET [rating]=[rating]-1 WHERE NOT ([rating] IS NULL)", conn);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand("UPDATE [nickname] SET [rating]=0 WHERE NOT ([rating] IS NULL) and (rating < 0)", conn);
                cmd.ExecuteNonQuery();

                foreach (KeyValuePair<string, double> p in rate) {
                    cmd = new SqlCommand("INSERT INTO [rating] ([QQ],[Rating],[Time],[score],[d1],[d2],[d3],[d4],[d5],[d6],[rank]) VALUES (@qq,@rating,@time,@score,@d1,@d2,@d3,@d4,@d5,@d6,@rank)", conn);
                    cmd.Parameters.Add("qq", SqlDbType.Char).Value = p.Key;
                    cmd.Parameters.Add("rating", SqlDbType.Float).Value = p.Value;
                    cmd.Parameters.Add("time", SqlDbType.DateTime).Value = d;
                    cmd.Parameters.Add("score", SqlDbType.Int).Value = score[p.Key];
                    cmd.Parameters.Add("d1", SqlDbType.Int).Value = hd[0][p.Key];
                    cmd.Parameters.Add("d2", SqlDbType.Int).Value = hd[1][p.Key];
                    cmd.Parameters.Add("d3", SqlDbType.Int).Value = hd[2][p.Key];
                    cmd.Parameters.Add("d4", SqlDbType.Int).Value = hd[3][p.Key];
                    cmd.Parameters.Add("d5", SqlDbType.Int).Value = hgetup.ContainsKey(p.Key) ? hgetup[p.Key] : 0;
                    cmd.Parameters.Add("d6", SqlDbType.Int).Value = Math.Min((int)breakScore[p.Key], 100);
                    cmd.Parameters.Add("rank", SqlDbType.Int).Value = frank[p.Key];
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("{0}   {1}", p.Key, p.Value);

                    cmd = new SqlCommand("UPDATE [nickname] SET [rating]=@rating WHERE [qq]=@qq", conn);
                    cmd.Parameters.Add("qq", SqlDbType.Char).Value = p.Key;
                    cmd.Parameters.Add("rating", SqlDbType.Float).Value = p.Value;
                    cmd.ExecuteNonQuery();
                }
            }
            conn.Close();
        }

        #region Data Mining



        static int[] CombineLists(int[] p1, int[] p2) {
            List<int> ret = new List<int>();
            for (int i = 0, j = 0; i < p1.Length && j < p2.Length; ) {
                if (p1[i] == p2[j]) {
                    ret.Add(p1[i]);
                    i++;
                    j++;
                } else if (p1[i] < p2[j]) {
                    i++;
                } else {
                    j++;
                }
            }
            return ret.ToArray();
        }

        static int[] tryCombine(int[] a, int[] b) {
            // Dictionary<int, bool> t = new Dictionary<int, bool>();
            for (int i = 1; i < a.Length; i++)
                if (a[i] != b[i - 1])
                    return new int[0];

            int[] ret = new int[a.Length + 1];
            for (int i = 0; i < a.Length; i++)
                ret[i] = a[i];
            ret[a.Length] = b[b.Length - 1];
            //Array.Sort(ret);
            return ret;
        }

        static bool ListContains(List<int[]> newids, int[] id) {
            for (int i = 0; i < newids.Count; i++) {
                bool ok = true;
                for (int j = 0; j < newids[i].Length; j++) {
                    if (newids[i][j] != id[j]) {
                        ok = false;
                        break;
                    }
                }
                if (ok) {
                    return true;
                }
            }
            return false;
        }


        static int chkString(string str, string[] words, int depth) {
            if (depth == words.Length) return 0;
            int p = str.IndexOf(words[depth]);
            while (p != -1 && p <= 1) {
                if (!(p == 1 && words[depth][0] > 128 && words[depth - 1][0] > 128 || p == 1 && str[0] != ' ')) {
                    int t = chkString(str.Substring(p + words[depth].Length), words, depth + 1);
                    if (t != -1)
                        return t + p + words[depth].Length;
                }
                p = str.IndexOf(words[depth], p + 1);
            }
            return -1;
        }

        static string chkString(string str, string[] words) {
            try {
                string s = str.ToLower();
                int p = s.IndexOf(words[0]);
                while (p != -1) {
                    int t = chkString(s.Substring(p + words[0].Length), words, 1);
                    if (t != -1)
                        return str.Substring(p, t + words[0].Length);
                    p = s.IndexOf(words[0], p + 1);
                }
                return "";
            } catch {
                return "";
            }
        }


        static void transWords() {
            List<string> list = new List<string>();
            List<int> num = new List<int>();
            StreamReader sr = new StreamReader("1.txt");
            while (!sr.EndOfStream) {
                string s = sr.ReadLine();
                if (s == "") continue;
                if (s.StartsWith("k"))
                    continue;
                int p = s.IndexOf("#$#$#$");
                s = s.Substring(0, p - 1);
                p = s.IndexOf(" ");
                int n = int.Parse(s.Substring(0, p - 1));
                s = s.Substring(p + 1);
                list.Add(s);
                num.Add(n);
            }
            sr.Close();

            list.Reverse();
            num.Reverse();

            StreamWriter sw = new StreamWriter("2.txt");
            List<string> ret = new List<string>();
            List<int> retn = new List<int>();
            for (int i = 0; i < list.Count; i++) {
                bool ok = true;
                for (int j = 0; j < ret.Count; j++) {
                    if (ret[j].IndexOf(list[i]) != -1) {
                        if (retn[j] >= num[i] / 2) {
                            ok = false;
                            break;
                        }
                    }
                }
                if (ok) {
                    sw.WriteLine("{0} {1}", num[i], list[i]);
                    //Console.WriteLine("{0} {1}", num[i], list[i]);
                    ret.Add(list[i]);
                    retn.Add(num[i] * (int)Math.Log(list[i].Length));
                }
            }
            string[] aret = ret.ToArray();
            int[] aretn = retn.ToArray();
            Array.Sort(aretn, aret);
            Array.Reverse(aret);
            Array.Reverse(aretn);
            for (int i = 0; i < 20; i++) {
                Console.WriteLine(aret[i]);
            }
            sw.Close();
        }

        static string checkDoc(int docid, SqlConnection conn, int[] lst, Dictionary<int, string> dict) {
            string[] words = new string[lst.Length];
            for (int i = 0; i < lst.Length; i++) {
                words[i] = dict[lst[i]];
            }
            if (data[docid] != null && data[docid] != "") {
                return chkString(data[docid], words);
            }
            SqlCommand cmd = new SqlCommand("SELECT [Message] FROM [data] WHERE [id]=@id", conn);
            cmd.Parameters.Add("id", SqlDbType.Int).Value = docid;
            SqlDataReader dr = cmd.ExecuteReader();
            dr.Read();
            data[docid] = PreProcessUtility.HTML2Text((string)dr["Message"]);
            dr.Close();
            return chkString(data[docid], words);
        }

        const int supply = 5;

        static void Apriori(Dictionary<int, string> dict, Dictionary<int, int[]> _ps, SqlConnection conn) {
            List<int[]> ids = new List<int[]>();
            Dictionary<int, int[]> ps = new Dictionary<int, int[]>();
            int cnt = 0;
            foreach (var o in dict) {
                ids.Add(new int[1] { o.Key });
                ps.Add(cnt, _ps[o.Key]);
                cnt++;
            }

            StreamWriter sw = new StreamWriter("1.txt");
            int len = 2;
            while (true) {
                if (ids.Count == 0) {
                    break;
                }

                sw.WriteLine("k={0}:", len);
                List<int[]> newids = new List<int[]>();
                Dictionary<int, int[]> newps = new Dictionary<int, int[]>();
                for (int i = 0; i < ids.Count; i++) {
                    for (int j = 0; j < ids.Count; j++) {
                        int[] nx = tryCombine(ids[i], ids[j]);
                        if (nx.Length != ids[i].Length + 1)
                            continue;
                        int[] list = CombineLists(ps[i], ps[j]);
                        if (list.Length >= supply) {
                            cnt = 0;
                            List<int> newlst = new List<int>();

                            Dictionary<string, int> exps = new Dictionary<string, int>();
                            for (int k = 0; k < list.Length; k++) {
                                string t = checkDoc(list[k], conn, nx, dict);
                                if (t != "") {
                                    if (exps.ContainsKey(t)) {
                                        exps[t]++;
                                    } else {
                                        exps.Add(t, 1);
                                    }
                                    /*if (example == "")
                                        example = t;
                                    else if (example.Length > t.Length)
                                        example = t;*/
                                    cnt++;
                                    newlst.Add(list[k]);
                                }
                                if (cnt + list.Length - k < supply)
                                    break;
                            }
                            if (cnt >= supply) {
                                list = newlst.ToArray();
                                if (!ListContains(newids, nx)) {

                                    string example = "";
                                    int expnum = 0;
                                    int minlen = int.MaxValue;
                                    foreach (var o in exps) {
                                        if (minlen > o.Key.Length) {
                                            minlen = o.Key.Length;
                                            example = o.Key;
                                            expnum = o.Value;
                                        } else if (minlen == o.Key.Length) {
                                            if (expnum < o.Value) {
                                                example = o.Key;
                                                expnum = o.Value;
                                            }
                                        }
                                    }

                                    newps.Add(newids.Count, list);
                                    newids.Add(nx);

                                    sw.Write("{0}: {1} #$#$#$ ", list.Length, example);
                                    for (int k = 0; k < nx.Length; k++) {
                                        sw.Write("{0} ", dict[nx[k]]);
                                    }
                                    sw.WriteLine();
                                }
                            }
                        }
                    }
                }
                ids = newids;
                ps = newps;
                /*foreach (var o in ps) {
                    Console.Write("{0}: ", o.Value.Length);
                    for (int k = 0; k < ids[o.Key].Length; k++) {
                        Console.Write("{0} ", dict[ids[o.Key][k]]);
                    }
                }*/
                len++;
            }
            sw.Close();
        }

        static string[] data = new string[1591408];

        static void DataMining() {
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT [id], [word], [posts] FROM [Index] WHERE [df]>=" + supply, conn);
            Dictionary<int, string> dict = new Dictionary<int, string>();
            Dictionary<int, int[]> ps = new Dictionary<int, int[]>();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) {
                int id = (int)dr["id"];
                string word = ((string)dr["word"]).Trim();
                dict.Add(id, word);
                int[] posts = BytesToInts((byte[])dr["posts"]);
                ps.Add(id, posts);
            }
            dr.Close();


            Apriori(dict, ps, conn);
            conn.Close();
        }
        #endregion



        /// <summary>
        /// 临时加入数据库
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="qun"></param>
        static List<string> InsertMessageTemp() {
            List<string> ret = new List<string>();
            List<QQRecord> r = new List<QQRecord>();// combine(folder);
            DateTime begin = new DateTime(2011, 1, 1);
            DateTime end = new DateTime(2011, 1, 2);

            StreamReader sr = new StreamReader(@"C:\Users\licstar\Desktop\xq.txt", Encoding.Default);
            //[]
            int cnt = 0;
            string s = sr.ReadLine();

            while (!sr.EndOfStream) {
                cnt++;

                int p = s.IndexOf(" ", s.IndexOf(" ") + 1);
                string time = s.Substring(0, p);
                p = s.LastIndexOf("(");
                string qq = s.Substring(p + 1);
                qq = qq.Substring(0, qq.Length - 1);
                string content = sr.ReadLine();
                s = sr.ReadLine(); //空行
                while (!sr.EndOfStream) {
                    p = s.IndexOf(" ", s.IndexOf(" ") + 1);
                    if (p != -1) {
                        DateTime td;
                        if (DateTime.TryParse(s.Substring(0, p), out td)) {
                            break;
                        }
                    }

                    content = content + "<br />" + s;
                    s = sr.ReadLine();

                }
                if (content.EndsWith("<br />")) {
                    content = content.Substring(0, content.Length - "<br />".Length);
                }

                content = content.Replace("[图片]", "<IMG src=\"img/e.gif\">");
                content = content.Replace("[表情]", "<IMG src=\"img/e.gif\">");

                QQRecord rr = new QQRecord();
                rr.QQ = qq;
                rr.Text = content;
                rr.Time = DateTime.Parse(time);

                Console.WriteLine(rr.Time);
                r.Add(rr);
            }

            sr.Close();
            //return ret;
            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();
            Hashtable names = new Hashtable();
            for (int i = 0; i < r.Count; i++) {
                if (r[i].Time.Date >= begin.Date && r[i].Time.Date <= end.Date) {
                    SqlCommand cmd = new SqlCommand("INSERT INTO [data] ([QQ],[Message],[Time],[qun]) VALUES (@qq, @mesg,@time,@qun)", conn);
                    cmd.Parameters.Add("qq", SqlDbType.Char).Value = r[i].QQ;
                    cmd.Parameters.Add("mesg", SqlDbType.Text).Value = r[i].Text;
                    cmd.Parameters.Add("time", SqlDbType.DateTime).Value = r[i].Time;
                    cmd.Parameters.Add("qun", SqlDbType.Int).Value = 2;
                    cmd.ExecuteNonQuery();
                    if (!names.Contains(r[i].QQ)) {
                        names.Add(r[i].QQ, 1);
                    }
                }
            }

            foreach (System.Collections.DictionaryEntry o in names) {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM nickname WHERE qq=@qq", conn);
                cmd.Parameters.Add("qq", SqlDbType.Char).Value = (string)o.Key;
                if ((int)cmd.ExecuteScalar() == 0) {
                    ret.Add((string)o.Key);
                    Console.WriteLine((string)o.Key);
                    cmd = new SqlCommand("INSERT INTO [nickname] ([QQ],[name]) VALUES (@qq, @name)", conn);
                    cmd.Parameters.Add("qq", SqlDbType.Char).Value = (string)o.Key;
                    cmd.Parameters.Add("name", SqlDbType.NVarChar).Value = "未知昵称";
                    cmd.ExecuteNonQuery();
                }
            }

            conn.Close();
            return ret;
        }


    }
}
