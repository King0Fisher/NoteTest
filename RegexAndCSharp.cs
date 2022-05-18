using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    public class Bool2
    {
        public bool value;
        public bool isAnd;

        public Bool2(bool value)
        {
            this.value = value;
            isAnd = true;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string str = "&&HaveItem(Party, BloodstainedRing, true)&&Fun(A,B,C)";//&&Fun(A,B,C)&&...

            Stopwatch wth = new Stopwatch();
            #region 非正则表达式
            //wth.Start();
            //bool isInCode = false;
            //int depth = 0;
            //Bool2 cur = null;
            //List<object> curL = new List<object>();
            //List<object> list = curL;
            //List<int> index = new List<int>();
            //index.Add(0);
            //StringBuilder sb = new StringBuilder();
            //foreach (var c in str)
            //{
            //    if (c == ' ') continue;
            //    else if (c == '(')
            //    {
            //        if (!isInCode)
            //        {
            //            List<object> tmp = new List<object>();
            //            curL.Add(tmp);
            //            curL = tmp;
            //            index[depth]++;
            //            index.Add(0);
            //            depth++;
            //        }
            //        else sb.Append(c);
            //    }
            //    else if (c == ')')
            //    {
            //        if (isInCode)
            //        {
            //            isInCode = false;
            //            sb.Append(c);
            //            //Console.WriteLine(sb);
            //            cur = new Bool2(true);//Check()
            //            curL.Add(cur);
            //            index[depth]++;
            //            sb.Remove(0, sb.Length);
            //        }
            //        else
            //        {
            //            index.RemoveAt(depth);
            //            depth--;
            //            curL = list;
            //            if (depth >= 1)
            //                for (int i = 0; i < depth; i++)
            //                {
            //                    curL = (List<object>)curL[index[i] - 1];
            //                }
            //        }
            //    }
            //    else
            //    {
            //        if (isInCode) sb.Append(c);
            //        else if (c == '&')
            //        {
            //            cur.isAnd = true;
            //        }
            //        else if (c == '|')
            //        {
            //            cur.isAnd = false;
            //        }
            //        else
            //        {
            //            isInCode = true;
            //            sb.Append(c);
            //        }
            //    }
            //}
            ////DateTime currentTime2 = System.DateTime.Now;
            ////int ms2 = currentTime2.Millisecond;
            //wth.Stop();

            //Console.WriteLine(wth.Elapsed.ToString());

            #endregion

            #region 正则表达式
            wth.Reset();
            wth.Start();
            //Bool2 cur = null;
            //List<object> curL = new List<object>();
            //List<object> list = curL;

            string pattern = "(?:((?<=&&)|\\w+)[(](.*?)[)])";//"(.*?)[(](.*?)[)]";
            Regex regex = new Regex(pattern);
            MatchCollection mc = regex.Matches(str);
            wth.Stop();
            //int ms1 = currentTime1.Millisecond;

            foreach (Match m in mc)
            {
                for (int i = 0; i < m.Groups.Count; i++)
                {
                    Console.WriteLine(i + " " + m.Groups[i]);
                }
                //if (m.Groups.Count < 3)
                //{
                //    break;
                //}
                //else
                //{
                //    cur = new Bool2(Check(m.Groups[1].Value, m.Groups[2].Value));
                //    //switch (m.Groups[1].Value)
                //    //{
                //    //    case "&&":
                //    //        cur.isAnd = true;
                //    //        break;
                //    //    case "||":
                //    //        cur.isAnd= false;
                //    //        break;
                //    //}

                //    list.Add(cur);
                //}
            }
            //return Recursion(list).value;

            //DateTime currentTime1 = System.DateTime.Now;

            Console.WriteLine(wth.Elapsed.ToString());
            #endregion
        }
    }
}
