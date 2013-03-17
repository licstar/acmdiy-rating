using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using SharpICTCLAS;

namespace acm_diy {
    /// <summary>
    /// $codebehindclassname$ 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://coder.buct.edu.cn/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]



    public class auto : IHttpHandler {

        static int[] frequent;
        static string[] words;
        static string[] wordsLower;
        static string[] pinyin;

        private void LoadData(string path) {
            StreamReader sr = new StreamReader(path);
            List<string> w = new List<string>();
            List<int> f = new List<int>();
            while (!sr.EndOfStream) {
                string s = sr.ReadLine();
                if (s == "")
                    continue;
                int p = s.IndexOf(" ");
                //if (int.Parse(s.Substring(0, p)) >= 20) {
                f.Add(int.Parse(s.Substring(0, p)));
                w.Add(s.Substring(p + 1));
                //}
            }
            sr.Close();
            words = w.ToArray();
            frequent = f.ToArray();
            Array.Sort(frequent, words);
            Array.Reverse(words);

            wordsLower = new string[words.Length];
            pinyin = new string[words.Length];
            for (int i = 0; i < words.Length; i++) {
                wordsLower[i] = Utility.ToDBC(words[i]).ToLower();
                pinyin[i] = Hz2Py.Convert(wordsLower[i]).ToLower();
            }

            //Array.Reverse(frequent);
            /*StreamWriter sw = new StreamWriter(@"d:\acmdiy\1111121.txt");
            for (int i = 0; i < frequent.Length; i++) {
                if (frequent[i] < 20)
                    sw.WriteLine(words[i]);

            }
            sw.Close();*/
        }

        public void ProcessRequest(HttpContext context) {
            context.Response.ContentType = "text/plain";
            if (!string.IsNullOrEmpty(context.Request["query"])) {
                if (frequent == null) {
                    LoadData(context.Server.MapPath("autodata.txt"));
                }

                string q = context.Request["query"];
                context.Response.Write("{");
                context.Response.Write(string.Format("query:'{0}',", q));
                context.Response.Write("suggestions:[");
                //
                q = Utility.ToDBC(q).ToLower();
                string pyq = Hz2Py.Convert(q).ToLower();
                int cnt = 0, cntinner = 0;
                //理想情况，开头10个，中间5个；开头不够，中间补齐；中间不够，开头补齐
                List<string> retHead = new List<string>();
                List<string> retInner = new List<string>();

                for (int i = 0; i < frequent.Length; i++) {

                    if (wordsLower[i].StartsWith(q)) {
                        retHead.Add(words[i]);
                        cnt++;
                    } else if (cntinner < 15) {
                        if (wordsLower[i].Contains(q)) {
                            retInner.Add(words[i]);
                            cntinner++;
                        } else if (pinyin[i].Contains(pyq)) {
                            bool ok = true;
                            for (int j = 0; j < q.Length; j++) {
                                if (q[j] > 128) {
                                    if (!wordsLower[i].Contains(q[j])) {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            if (ok) {
                                retInner.Add(words[i]);
                                cntinner++;
                            }
                        }
                    }
                    if (cnt >= 10 && cnt + cntinner >= 15)
                        break;
                }

                retHead.AddRange(retInner);
                for (int i = 0; i < retHead.Count && i < 15; i++) {
                    if (i == 0)
                        context.Response.Write(string.Format("'{0}'", (retHead[i].Replace("<", "&lt;").Replace(">", "&gt;"))));
                    else
                        context.Response.Write(string.Format(",'{0}'", (retHead[i].Replace("<", "&lt;").Replace(">", "&gt;"))));
                }

                context.Response.Write("]}");

            }
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}
