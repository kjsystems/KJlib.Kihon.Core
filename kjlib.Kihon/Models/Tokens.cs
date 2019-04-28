using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace kjlib.Kihon.Models
{

    public class Tokens : IEnumerable
    {
        /**
		 * 
		 **/
        static public int getToken(string buf, char kugiri, out string res)
        {
            res = "";
            int idx = 0;
            for (int m = 0; m < buf.Length; m++)
            {
                idx = m;
                if (buf[m] == kugiri)
                {
                    break;
                }
                res += buf[m];
            }
            idx++;   //なければbuf.Length あれば+1
            return idx;   //kugiriまでを含めて返す 文字列はkugiriを除く
        }
        /**
         * 
        **/
        static public List<string> createStrListFromText(string buf, char token)
        {
            if (string.IsNullOrEmpty(buf))
                return new List<string>();
            Tokens tokens = new Tokens(buf, new char[] { token });
            List<string> lst = new List<string>();
            foreach (string v in tokens)
            {
                lst.Add(v);
            }
            return lst;
        }
        static public List<int> createIntListFromText(string buf, char token)
        {
            if (buf == null || buf.Length == 0) return null;
            Tokens tokens = new Tokens(buf, new char[] { token });
            List<int> lst = new List<int>();
            foreach (string v in tokens)
            {
                lst.Add(ConvUtil.toInt(v, 0));
            }
            return lst;
        }

        private List<string> elements = new List<string>();
        public List<string> Elements { get { return elements; } set { elements = value; } }

        /**
		 * 
		 **/
        public List<string> toStringList()
        {
            List<string> lst = new List<string>();
            foreach (string buf in this) lst.Add(buf);
            return lst;
        }
        /**
		 * 
		 * */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string token in this)
            {
                if (sb.ToString().Length > 0) sb.Append(",");
                sb.Append(TokenUtil.addQuot(token));
            }
            return sb.ToString();
        }

#if false //----		
		public Tokens(string source, char ch1, char ch2) {
		}
#endif //------
        /**
		 * delimiterを分ける場合、山口さんのテキスト   ○xxxx●とか
		 * */
        //char quot_='\"';
        public Tokens(string source, char ch1, char ch2)
        {
            initTokenDifDelimiter(source, ch1, ch2, TagTextUtil.QUOT_DOUBLE);
        }
        public Tokens(string source, char ch1, char ch2, char quot/*='\"' HTMLは'\'' */)
        {
            initTokenDifDelimiter(source, ch1, ch2, quot);
        }
        void initTokenDifDelimiter(string source, char ch1, char ch2, char quot)
        {
            //quot_ = quot;
            //bool bTag = false;
            string token = "";
            foreach (char ch in source)
            {
                if (ch == ch1)
                {
                    if (token.Length > 0)
                    {
                        elements.Add(token);
                    }
                    token = "";
                    token += ch;
                    //bTag = true;
                    continue;
                }
                else if (ch == ch2)
                {    //閉じ
                    token += ch;
                    elements.Add(token);
                    token = "";
                    //bTag = false;
                }
                else token += ch;
            }
            if (token.Length > 0)
            {
                elements.Add(token);
            }
        }
        /**
		 * ""でも''でも対応  
		 * <meta name="generator" content=7AeEdinet ver.1.652'></meta>
		 * <span style='font-family:"ＭＳ 明朝"'>
		 * */
        public Tokens(string buf, char[] delimiters, bool quotAny)
        {
            initTokens2(buf, delimiters);
        }
        /**
		 * 
		 * */
        void initTokens2(string buf, char[] delimiters)
        {
            const char quot_s = '\'';
            const char quot_d = '\"';
            char curquot = '?';

            // Parse the string into tokens:
            //elements = source.Split(delimiters);
            string cur = "";
            bool bQuot = false;   //""は無視
            for (int m = 0; m < buf.Length; m++)
            {
                char ch = buf[m];
                if (bQuot != true && Array.IndexOf(delimiters, ch) >= 0)
                {
                    //空でも追加
                    elements.Add(cur);
                    cur = "";
                }
                else
                {
                    cur += ch;
                    if (ch == quot_s || ch == quot_d)
                    {
                        if (bQuot == true && curquot == ch)     //先にQUOTの方がQUOT "'aaa'"などを対処
                        {
                            bQuot = false;
                            curquot = '?';
                        }
                        else if (bQuot != true)
                        {
                            bQuot = true;
                            curquot = ch;
                        }
                    }
                }
            }
            //空でも追加
            elements.Add(cur);
        }
        /**
		 * 
		 * */
        public Tokens(string source, char[] delimiters, bool bDelQuot, char quot)
        {
            //quot_ = quot;
            initTokens(source, delimiters, bDelQuot, quot);
        }
        /**
		 * 
		 */
        public Tokens(string source, char[] delimiters)
        {
            initTokens(source, delimiters, true/*delQuot*/, '"');
        }
        public Tokens(string source, char[] delimiters, char quot)
        {
            initTokens(source, delimiters, true/*delQuot*/, quot);
        }
        /**
		 * 
		 * */
        void initTokens(string source, char[] delimiters, bool bDelQuot, char quot)
        {
            //Console.WriteLine("Tokens const 1 delQuot={0} source=[{1}]", bDelQuot,source);
            if (delimiters == null || delimiters.Length == 0)
            {
                throw new Exception("区切り文字がありません(Tokens)");
            }
            // Parse the string into tokens:
            //elements = source.Split(delimiters);
            string token = "";
            bool bQuot = false;   //""は無視
            foreach (char ch in source)
            {
                if (bQuot != true && Array.IndexOf(delimiters, ch) >= 0)
                {
                    //空でも追加　KJ_REFACT:100501
                    string buf = token;
                    if (bDelQuot == true) buf = TokenUtil.delQuot(buf);
                    //Console.WriteLine("Tokens const 2 token=[{0}]", buf);
                    elements.Add(buf);
                    token = "";
                }
                else
                {
                    token += ch;
                    if (ch == quot/*'"'*/)
                    {
                        if (bQuot == true) bQuot = false;
                        else bQuot = true;
                    }
                }
            }
            {
                string buf = token;
                if (bDelQuot == true) buf = TokenUtil.delQuot(buf);
                //Console.WriteLine("Tokens const 3 token=[{0}]", buf);
                elements.Add(buf);
            }
        }

        // IEnumerable Interface Implementation:
        //   Declaration of the GetEnumerator() method 
        //   required by IEnumerable
        public IEnumerator GetEnumerator()
        {
            return new TokenEnumerator(this);
        }
        public string this[int index]
        {
            get
            {
                if (index > elements.Count - 1)
                    return "";
                return elements[index];
            }
            set
            {
                while (index > elements.Count - 1)
                {
                    elements.Add("");
                }
                elements[index] = value;
            }
        }
        public int Count
        {
            get { return elements.Count; }
        }

        // Inner class implements IEnumerator interface:
        private class TokenEnumerator : IEnumerator
        {
            private int position = -1;
            private Tokens t;

            public TokenEnumerator(Tokens t)
            {
                this.t = t;
            }

            // Declare the MoveNext method required by IEnumerator:
            public bool MoveNext()
            {
                if (position < t.elements.Count - 1)
                {
                    position++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Declare the Reset method required by IEnumerator:
            public void Reset()
            {
                position = -1;
            }

            // Declare the Current property required by IEnumerator:
            public object Current
            {
                get
                {
                    return t.elements[position];
                }
            }
        }
    }
}
