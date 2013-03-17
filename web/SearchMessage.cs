using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SharpICTCLAS;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Text;

namespace acm_diy {
    public class SearchResult {
        public string[] words;
        public int[] documentID;
        public double[] tfidf;
    }

    public class SearchMessage {
        protected string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;

        private WordSegment wordSegment;

        private static DateTime lastVisit = new DateTime(1900, 1, 1);
        private static Dictionary<string, int> cacheID;
        private static int cacheN;

        public SearchMessage(string dictpath) {
            wordSegment = new WordSegment();
            wordSegment.InitWordSegment(dictpath);
        }

        private string[] WordResultToString(List<WordResult[]> result) {
            List<string> ret = new List<string>();
            for (int i = 0; i < result.Count; i++) {
                for (int j = 1; j < result[i].Length - 1; j++) {
                    //if (Utility.GetPOSString(result[i][j].nPOS) == "w\0")
                    //    continue;
                    string s = result[i][j].sWord.ToLower();
                    ret.Add(s);
                }
            }
            return ret.ToArray();
        }

        private string[] WordResultToStringContent(List<WordResult[]> result) {
            List<string> ret = new List<string>();
            for (int i = 0; i < result.Count; i++) {
                for (int j = 1; j < result[i].Length - 1; j++) {
                    string s = result[i][j].sWord;
                    ret.Add(s);
                }
            }
            return ret.ToArray();
        }

        private int[] CombineLists(int[] p1, int[] p2, double[] t1, double[] t2, out double[] t) {
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

        private double[] getTfIdf(int[] tf, int df, int N) {
            double idf = Math.Log(1.0 * N / df);
            double[] ret = new double[tf.Length];
            for (int i = 0; i < tf.Length; i++)
                ret[i] = tf[i] * idf;
            return ret;
        }

        public SearchResult Search(string str) {
            Dictionary<string, int> ID;
            int N;

            SqlConnection conn = new SqlConnection(connstr);
            conn.Open();

            if (lastVisit == Functions.end) {
                ID = cacheID;
                N = cacheN;
            } else {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [data]", conn);
                N = (int)cmd.ExecuteScalar();

                ID = new Dictionary<string, int>();
                cmd = new SqlCommand("SELECT [id],[word] FROM [Index]", conn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read()) {
                    ID.Add(((string)dr["word"]).Trim(), (int)dr["id"]);
                }
                dr.Close();

                cacheID = ID;
                cacheN = N;
                lastVisit = Functions.end;
            }

            //str = PreProcessUtility.ToSimplifyString(str);
            str = str.Replace("\r", " ").Replace("\n", " ").Replace("/", " / ").Replace(":", " : ").Replace("<", " < ").Replace(">", " > ").Replace("\"", " \" ");
            str = str.Replace("&", " & ").Replace("(", " ( ").Replace(")", " ) ").Replace("-", " - ").Replace("'", " ' ");

            string[] words = WordResultToString(wordSegment.Segment(str, 1));

            int[] results = null;
            double[] tfidf = null;

            for (int i = 0; i < words.Length; i++) {
                if (!ID.ContainsKey(words[i].ToLower())) {
                    results = null;
                    break;
                }
                SqlCommand cmd = new SqlCommand("SELECT [df], [tf], [posts] FROM [Index] WHERE [ID]=@id", conn);
                cmd.Parameters.Add("id", SqlDbType.Int).Value = ID[words[i].ToLower()];
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
                }
                dr.Close();
            }

            conn.Close();


            SearchResult ret = new SearchResult();
            if (results == null) {
                ret.documentID = new int[0];
                ret.tfidf = new double[0];
            } else {
                Array.Sort(tfidf, results);
                Array.Reverse(tfidf);
                Array.Reverse(results);
                ret.documentID = results;
                ret.tfidf = tfidf;
            }
            ret.words = words;

            return ret;
        }


        public static int[] BytesToInts(byte[] bytes) {
            int size = bytes.Length;
            if (size % 4 != 0) {
                return null;
            }
            int[] ret = new int[size / 4];
            for (int i = 0; i < size; i += 4) {
                ret[i / 4] = BitConverter.ToInt32(bytes, i);
            }
            return ret;
        }


        private string TextToHTML(string str) {
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        /// <summary>
        /// 获得关键词加强的文档
        /// </summary>
        /// <returns></returns>
        private string getEmDoc(string[] words, string contents) {
            StringBuilder ret = new StringBuilder();
            string lowContents = contents.ToLower();
            for (int i = 0; i < contents.Length; ) {
                string t = lowContents.Substring(i);
                bool find = false;
                foreach (string word in words) {
                    if (t.StartsWith(word)) {
                        ret.Append(string.Format("<em>{0}</em>", TextToHTML(contents.Substring(i, word.Length))));
                        i += word.Length;
                        find = true;
                        break;
                    }
                }
                if (!find) {
                    ret.Append(TextToHTML(contents[i].ToString()));
                    i++;
                }
            }
            return ret.ToString();
        }


        public string getSnippet(string[] words, string content) {
            //如果原文过长，则生成摘要
            if (content.Length > 200) {

                //获取不重复的搜索词表
                Dictionary<string, int> dWords = new Dictionary<string, int>();
                int cnt = 0;
                for (int i = 0; i < words.Length; i++) {
                    if (!dWords.ContainsKey(words[i])) {
                        dWords.Add(words[i], cnt);
                        cnt++;
                    }
                }
                string[] newWords = new string[cnt];
                foreach (var o in dWords) {
                    newWords[o.Value] = o.Key;
                }

                content = content.Substring(0, 200);

                //分词
                string tcontent = content.Replace("\r", " ").Replace("\n", " ").Replace("/", " / ").Replace(":", " : ").Replace("<", " < ").Replace(">", " > ").Replace("\"", " \" ");
                tcontent = tcontent.Replace("&", " & ").Replace("(", " ( ").Replace(")", " ) ").Replace("-", " - ").Replace("'", " ' ");

                string[] contents = WordResultToStringContent(wordSegment.Segment(tcontent, 1));

                if (contents.Length > 200) {
                    //寻找最密集的200词

                }
            }

            return getEmDoc(words, content);
        }
    }
}
