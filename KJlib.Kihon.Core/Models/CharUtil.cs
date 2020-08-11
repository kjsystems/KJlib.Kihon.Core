using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KJlib.Kihon.Core.Models
{
    public class CharUtil
    {
        static public string delKaigyo(string buf)
        {
            StringBuilder sb = new StringBuilder();

            char[] tbl = { '\t', '\r', '\n' };
            foreach (char ch in buf)
            {
                if (Array.IndexOf(tbl, ch) >= 0) continue;
                sb.Append(ch);
            }

            return sb.ToString();
        }

        /**
         * すべて空白か
         * */
        static public bool isOnlySpace(string buf)
        {
            char[] tbl = new char[] { ' ', '　' };
            foreach (char ch in buf)
            {
                if (Array.IndexOf(tbl, ch) < 0) return false; //スペース以外なら抜ける
            }

            return true;
        }
        //static public string toZenkaku(string buf)   ==> VBUtil
        //static public string toHankaku(string buf)   ==> VBUtil

        static public bool isSjisUTFEntity(string ch)
        {
            if (ch.Length < 6) return false;
            if (ch[0] != '&') return false;
            if (ch.Substring(0, "&#x".ToLower().Length) == "&#x")
            {
                if (ch.Substring(ch.Length - 1, 1) == ";") return true;
            }

            return false;
        }

        /**
		 * entity(&#x9999;)をUTFに変換する
		 * */
        public static string sjis2utf(string buf)
        {
            StringBuilder sb = new StringBuilder();
            TagList taglst = new TagList();
            string ama = "";

            TagTextUtil.parseText(buf, ref taglst, true, ref ama);
            foreach (TagBase tag in taglst)
            {
                if (isSjisUTFEntity(tag.ToString()) == true)
                {
                    sb.Append(sjis2utf_ch(tag.ToString()));
                }
                else sb.Append(tag.ToString());
            }

            return sb.ToString();
        }

        /**
             * 
             * */
        public static string sjis2utf_ch(string ch)
        {
            if (ch.Length == 1) return ch;
            if (isSjisUTFEntity(ch) == true)
            {
                return getTextFromSjisEntity(ch);
            }

            //throw new Exception("UTFに変換できない(sjis2utf) ["+ch+"]");
            return ch;
        }

        /**
		 * &#x4e00; ==> 一
		 **/
        public static string getTextFromSjisEntity(string buf)
        {
#if BUG_PROG
			Console.WriteLine("CharUtil getTextFromSjisEntity 1 [{0}]",buf);
#endif
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < buf.Length; m++)
            {
                string ch = buf.Substring(m, 1);
#if BUG_PROG
				Console.WriteLine(" #{0} [{1}]", m + 1, ch);
#endif
                if (ch == "&" && buf.Substring(m, 3).ToLower() == "&#x") //&#x9999; or &#x99999;
                {
                    int idx = buf.Substring(m).IndexOf(";");
#if BUG_PROG
					Console.WriteLine(" ==> entity idx={0}", idx);
#endif
                    if (idx < 0) throw new Exception("&のあとに;がない buf=" + buf);
                    string code = buf.Substring(m + 3, idx - 3);
#if BUG_PROG
					Console.WriteLine(" ==> code(utf) code={0}", code);
#endif
                    string res = EncodingUtil.getTextFromHexText(code, Encoding.UTF32); //Encoding.GetEncoding("UTF-16")
#if BUG_PROG
					Console.WriteLine(" ==> res char={0}", res);
#endif
                    sb.Append(res);
                    m += idx;
                    continue;
                }

                sb.Append(ch);
            }
#if BUG_PROG
			Console.WriteLine("CharUtil getTextFromSjisEntity z res=[{0}]", sb.ToString());
#endif
            return sb.ToString();
        }

        /**
             * 数字
             * */
        public static bool isNumeric(string ch)
        {
            return Regex.IsMatch(ch, @"^[0-9]");
        }

        public static bool isNumeric(char ch)
        {
            return Regex.IsMatch(ch.ToString(), @"^[0-9]");
        }

        /**
		 * ひらがな
		 * */
        public static bool isHiragana(string ch)
        {
            //http://pentan.info/php/reg/is_hira.html
            return Regex.IsMatch(ch, @"^[ぁ-ゞ]");
        }

        public static bool isHiragana(char ch)
        {
            return isHiragana(ch.ToString());
        }

        public static bool isKatakana(string ch)
        {
            //http://pentan.info/php/reg/is_hira.html
            return Regex.IsMatch(ch, @"^[ァ-ヾ]");
        }

        /**
		 * 半角カタカナがあるか
		 * */
        public static bool isHiraganaInclude(string buf)
        {
            return Regex.IsMatch(buf, @"[ｱ-ﾟ]");
        }

        static Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

        public static string you2sei(string buf)
        {
            string tbl1 = "ゃゅょっ";
            string tbl2 = "やゆよつ";
            for (int i = 0; i < tbl1.Length; i++)
            {
                buf = buf.Replace(tbl1[i].ToString(), tbl2[i].ToString());
            }

            return buf;
        }

        /**
		 濁音を清音に変換
		*/
        public static string daku2sei(string buf)
        {
            string tbl1 = "がぎぐげござじずぜぞだぢづでどばびぶべぼ";
            string tbl2 = "かきくけこさしすせそたちつてとはひふへほ";
            for (int i = 0; i < tbl1.Length; i++)
            {
                buf = buf.Replace(tbl1[i].ToString(), tbl2[i].ToString());
            }

            return buf;
        }

        /**
		 * 全角
		 **/
        public static bool isZenkaku(string str)
        {
            return !isHankaku(str);
            /** UNICODE未対応
                        int num = sjisEnc.GetByteCount(str);
                        return num == str.Length * 2;
*/
        }

        /**
		 * 半角 
		 **/
        public static bool isHankaku(string str)
        {
            if (isHankakuChar(str) == true || isHankakuKakko(str) == true) return true;
            return false;

            // regexは¥がエスケープシーケンス
            //return Regex.IsMatch(str, @"^[a-zA-Z0-9;¥/?:¥@&=+¥$,%#¥(¥)<>¥[¥]");

            /** UNICODE未対応
                 int num = sjisEnc.GetByteCount(str);
                        return num == str.Length;
             */
        }

        public static bool isHankakuChar(string str)
        {
            return Regex.IsMatch(str, @"^[ \-~ a-zA-Z0-9;¥/?:¥@&=+¥$,%#･\.]");
            //return Regex.IsMatch(str, @"^[a-zA-Z0-9]");
        }

        public static bool isHankakuKakko(string str)
        {
            return Regex.IsMatch(str, @"^[¥(¥)<>¥[]");
        }

        /**
		 * 全角半角に分ける
		 **/
        public static List<string> divZenHan(string text)
        {
            List<string> lst = new List<string>();
            StringBuilder sb = new StringBuilder();

            bool prev = false;
            string str;
            for (int m = 0; m < text.Length; m++)
            {
                str = text.Substring(m, 1);
                if (m == 0)
                {
                    sb.Append(str);
                    prev = isZenkaku(str);
                    continue;
                }

                //前の文字と全半違えばリストに追加
                if (prev != isZenkaku(str))
                {
                    lst.Add(sb.ToString());
                    sb.Length = 0;
                }

                sb.Append(str);
                prev = isZenkaku(str);
            }

            lst.Add(sb.ToString());
            return lst;
        }
    }
}