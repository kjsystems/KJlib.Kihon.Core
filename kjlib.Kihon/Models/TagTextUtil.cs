using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace kjlib.Kihon.Models
{
    /**
   * タグを分解する
   **/
    public class TagTextUtil
    {
        ILogger log_;

        public TagTextUtil(ILogger log)
        {
            log_ = log;
        }

        /**
         * 
         * */
        static public void createListFromText(string buf, ref List<string> strlst)
        {
            string cur = "";
            foreach (char ch in buf)
            {
                cur += ch;
                if (ch == '\n')
                {
                    strlst.Add(cur);
                    cur = "";
                }
            }

            if (cur.Length > 0)
            {
                strlst.Add(cur);
            }
        }

        /**
         * 
         * */
        static public void createTagListFromStrList(List<string> strlst, ref TagList taglst, bool bClear = true)
        {
            if (bClear == true) taglst.Clear();
            int gyono = 0;

            foreach (string str in strlst)
            {
                string ama = "";
                TagTextUtil.parseText(str, ref gyono, ref taglst, false, ref ama);
            }
        }

        /**
         * TagListからList<string>を作成
         * */
        static public void createListFromTagList(TagList taglst, ref List<string> strlst)
        {
            string cur = "";
            foreach (TagBase tag in taglst)
            {
                if (tag.isText() != true)
                {
                    cur += tag.ToString();
                    continue;
                }

                int idx = tag.ToString().IndexOf("\n");
                if (idx >= 0)
                {
                    cur += tag.ToString().Substring(0, idx + 1);
                    strlst.Add(cur);
                    cur = "";
                    //最後でなければそれまでを追加
                    if (idx != tag.ToString().Length - 1)
                    {
                        cur += tag.ToString().Substring(idx + 1);
                    }

                    continue;
                }

                cur += tag.ToString();
            }

            if (cur.Length > 0)
            {
                strlst.Add(cur);
            }
        }

        /**
         * タグと一文字ずつ
         * */
        static public List<string> createCharList(string buf)
        {
            List<string> reslst = new List<string>();

            TagList lst = new TagList();
            string ama = "";
            parseText(buf, ref lst, true, ref ama); //[&IMGxxx] &#x9999;もそれぞれ分ける
            foreach (TagBase tag in lst)
            {
                if (tag.ToString()[0] == '<')
                {
                    reslst.Add(tag.ToString());
                    continue;
                }

                if (tag.ToString()[0] == '[')
                {
                    reslst.Add(tag.ToString());
                    continue;
                }

                if (tag.ToString()[0] == '&')
                {
                    reslst.Add(tag.ToString());
                    continue;
                }

                foreach (char ch in tag.ToString())
                {
                    reslst.Add(ch.ToString()); //1文字ずつ
                }
            }

            return reslst;
        }

        /**
     * <ruby>タグから親文字だけ取得する
     * 他のタグも無視
     * */
        static public string getOyaTextFromTagList(string buf)
        {
            StringBuilder sb = new StringBuilder();
            TagList taglst = new TagList();
            string ama = "";
            bool bRT = false;
            int gyono = 0;

            TagTextUtil.parseText(buf, ref gyono, ref taglst, false, ref ama);
            foreach (TagBase tag in taglst)
            {
                if (tag.getName().ToLower() == "rt")
                {
                    bRT = tag.isOpen();
                    continue;
                }

                if (tag.isTag() == true) continue;
                //以下は文字列
                if (bRT == true) continue; //ルビ内は無視
                sb.Append(tag.ToString());
            }

            return sb.ToString();
        }

        /**
     * <ruby>タグから親文字だけ取得する
     * 他のタグはイキ
     * */
        static public string getOyaTextFromRubyTag(string buf)
        {
            StringBuilder sb = new StringBuilder();
            TagList taglst = new TagList();
            string ama = "";
            bool bRT = false;
            int gyono = 0;
            TagTextUtil.parseText(buf, ref gyono, ref taglst, false, ref ama);
            foreach (TagBase tag in taglst)
            {
                if (tag.getName().ToLower() == "rt")
                {
                    bRT = tag.isOpen();
                    continue;
                }

                if (tag.getName().ToLower() == "ruby") continue;
                if (bRT == true) continue;
                sb.Append(tag.ToString());
            }

            return sb.ToString();
        }

        /**
     * [&C9999],&#9999;かどうかを判断する
     * TagList.addTextで使用
     **/
        static public bool isTaregtToten(string buf, int idx, char sttToken, char endToken)
        {
            if (buf[idx] == sttToken && buf.Substring(idx).IndexOf(endToken) > 0)
            {
                return true;
            }

            return false;
        }

        /**
     * 改行付きテキストから
     * */
        public void parseTextFromTextWiteCRLF(string bufall, Encoding enc, ref TagList taglst, string srcpath,
            int gyono)
        {
            List<string> lst = new List<string>();
            StrUtil.createListFromTextCRLF(bufall, ref lst);
            //int gyono = 0;
            string amaritxt = "";
            bool bAddCRLF = true;
            for (int m = 0; m < lst.Count; m++)
            {
                string buf = lst[m];
                gyono++;
                char eof = (char)0x001a;
                if (buf.IndexOf(eof) >= 0)
                {
                    log_.LogError(srcpath, gyono, "parsetxt", "EOFを無視します");
                    buf = buf.Replace(eof.ToString(), "");
                }

                if (amaritxt.Length > 10000)
                {
                    throw new Exception("タグを解釈できない2 gyo=" + buf + "\r\namari=" + amaritxt);
                }

                if (amaritxt.Length > 0) buf = amaritxt + buf;
                if (bAddCRLF == true) buf += "\r\n";
                parseText(buf, ref gyono, ref taglst, false, ref amaritxt); //タグ開始の文字列が来たらamariに入る
            }
        }

        /**
     * 
     **/
        public const char QUOT_SINGLE = '\'';
        public const char QUOT_DOUBLE = '\"';

        public string srcpath_;
        int gyono_;

        public void parseTextFromStream(System.IO.StreamReader sr, ref TagList taglst,
            bool bAddCRLF /*addCRLF 出力しない場合はfalseでOK*/)
        {
            gyono_ = 0;
            string buf = null;
            string amaritxt = "";
            try
            {
                long lastpos = sr.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

                long prepos = 0;
                while ((buf = sr.ReadLine()) != null)
                {
                    gyono_++;
                    //if (gyono_ % 100 == 0) Console.WriteLine("parsetxt g={0} buf={1}", gyono_, buf);
                    long fpos = sr.BaseStream.Position;
                    prepos = fpos;
                    char eof = (char)0x001a;
                    if (buf.IndexOf(eof) >= 0)
                    {
                        log_.LogError(srcpath_, gyono_, "parsetxt", "EOFを無視します");
                        buf = buf.Replace(eof.ToString(), "");
                    }

                    if (amaritxt.Length > 500000 + 100000)
                    {
                        log_.LogError(srcpath_, gyono_, "parsetxt",
                            "タグを解釈できない1 buf=" + buf + " amari len=" + amaritxt.Length + " buf" + amaritxt);
                        throw new Exception("タグを解釈できない1 gyo=" + buf + "\r\namari=" + amaritxt);
                    }

                    if (amaritxt.Length > 0) buf = amaritxt + buf;
                    if (bAddCRLF == true) buf += "\r\n";
                    parseText(buf, ref gyono_, ref taglst, false, ref amaritxt); //タグ開始の文字列が来たらamariに入る
                }

                sr.Close();
            }
            catch (Exception ex)
            {
                log_.LogError(srcpath_, gyono_, "parstext", ex.Message);
            }
        }

        public TagList parseTextFromPath(string path, Encoding enc)
        {
            TagList taglst = new TagList();
            TagTextUtil util = new TagTextUtil(log_);
            util.parseTextFromPath(path, enc, ref taglst, true /*bAddCRLF*/);
            return taglst;
        }

        public void parseTextFromPath(string path, Encoding enc, ref TagList taglst,
            bool bAddCRLF = true /*addCRLF 出力しない場合はfalseでOK*/)
        {
            try
            {
                if (System.IO.File.Exists(path) != true)
                {
                    throw new Exception("読み込むファイルがありません PATH=" + path);
                }

                srcpath_ = path;
                System.IO.StreamReader sr = new System.IO.StreamReader(path, enc);
                parseTextFromStream(sr, ref taglst, bAddCRLF);
            }
            catch (Exception ex)
            {
                log_.LogError(path, gyono_, "parsetxt", ex.Message);
            }
        }

        static public bool parseText(string buf, ref int gyono, ref TagList taglst, bool bNameSP, ref string amaritxt)
        {
            return parseText(buf, ref gyono, ref taglst, '<', '>', bNameSP, ref amaritxt);
        }

        static public bool parseText(string buf, ref TagList taglst, bool bNameSP, ref string amaritxt)
        {
            int gyono = 0;
            return parseText(buf, ref gyono, ref taglst, '<', '>', bNameSP, ref amaritxt);
        }

        static public TagList parseText(string buf, ref int gyono, bool bNameSP)
        {
            string amaritxt = "";
            TagList taglst = new TagList();
            parseText(buf, ref gyono, ref taglst, '<', '>', bNameSP, ref amaritxt);
            return taglst;
        }

        static public TagList parseText(string buf)
        {
            int gyono = 0;
            string amaritxt = "";
            TagList taglst = new TagList();
            parseText(buf, ref gyono, ref taglst, '<', '>', false /*bNameSP*/, ref amaritxt);
            return taglst;
        }

        static public TagList parseTextFromPath(string path, Encoding enc, bool bNameSP)
        {
            string amaritxt = "";
            TagList taglst = new TagList();
            int gyono = 0;
            parseText(FileUtil.getTextFromPath(path, enc), ref gyono, ref taglst, '<', '>', bNameSP, ref amaritxt);
            return taglst;
        }
        /**
     * 
     **/
        /*static public bool parseText(string buf, int gyono, ref TagList taglst, bool bNameSP, ref string amaritxt)
    {
      return parseText(buf, gyono, ref taglst, '<', '>', bNameSP, ref amaritxt);
    }*/

        static bool isCommentEnd(string buf)
        {
            string[] tbl = { ">" }; //{"-->"};
            foreach (string txt in tbl)
            {
                if (buf.Length > 0 && buf.Substring(buf.Length - txt.Length, txt.Length) == txt)
                {
                    return true;
                }
            }

            return false;
        }

        static string getMaxText(string buf)
        {
            if (buf.Length < 100) return buf;
            return buf.Substring(0, 100);
        }

        /**
     * 区切り文字を複数にする  ...「」...『』...
     * コメントは未対応 <!--
     * */
        static public bool parseTextKugiriMulti(string buf, int gyono, ref TagList taglst,
            string open_tag, string close_tag, bool bNameSP, ref string amaritxt)
        {
            List<char> kugilst = new List<char>();
#if BUG_PROG
			Console.WriteLine("parse kugirimulti 0 open={0} close={1} all={2}",open_tag,close_tag, buf);
#endif
            amaritxt = "";
            bool res = true;
            //タグと文字列に分ける
            StringBuilder cur = new StringBuilder();
            //bool bOpen = false;
            foreach (char ch in buf)
            {
#if BUG_PROG
				Console.WriteLine("parse kugirimulti 1 cur={0}", cur);
#endif
                //タグの始まりでそれまでの文字を追加
                if (open_tag.IndexOf(ch) >= 0)
                {
#if BUG_PROG
					Console.WriteLine("parse kugirimulti 2 open ch={0} kugilist.count={1}", ch, kugilst.Count);
#endif
                    if (kugilst.Count == 0 && cur.Length > 0)
                    {
                        taglst.addText(cur.ToString(), bNameSP, ref gyono); //文字列
                        cur.Length = 0;
                    }

                    //bOpen = true;
                    kugilst.Add(ch);
                    //continue;    //<は追加しない
                }

                if (close_tag.IndexOf(ch) >= 0)
                {
#if BUG_PROG
					Console.WriteLine("parse kugirimulti 3 close ch={0} kugilist.count={1}", ch,kugilst.Count);
#endif
                    if (kugilst.Count == 0)
                        throw new Exception("閉じタグが先にきた,もしくは閉じタグが多い " + cur + ch + " hbn=" + getMaxText(buf));
                    //普通のタグの閉じ、コメント内は無視
                    if (kugilst.Count == 1 && cur.Length > 0) // >で行が始まる場合
                    {
                        cur.Append(ch);
                        taglst.addTag(cur.ToString(), kugilst[0], close_tag[close_tag.IndexOf(ch)], gyono); //タグ内（括弧内）
                        cur.Length = 0;
                        kugilst.Remove(kugilst[kugilst.Count - 1]);
                        continue;
                    }

                    //bOpen = false;
                    kugilst.Remove(kugilst[kugilst.Count - 1]);
                }

                cur.Append(ch);
            }

            //残り
            if (cur.Length > 0)
            {
                if (open_tag.IndexOf(cur.ToString()[0]) >= 0)
                //if (cur.ToString().Substring(0, 1) == "<")
                {
                    amaritxt = cur.ToString();
                    res = false;
                }
                else
                {
                    taglst.addText(cur.ToString(), bNameSP, ref gyono);
                }
            }

            return res;
        }

        /**
     * 
     * */
        public void createTextTreeFromTagList(TagList taglst, ref TagBase root, string path)
        {
            //TREE構造にする
            var parent = root;
            foreach (TagBase tag in taglst)
            {
                if (parent == null)
                {
                    log_.LogError(path, tag.GyoNo, "parstree", "親タグがnullはおかしい");
                }

                if (tag.isOpen() == true && tag.isShort() != true)
                {
                    parent.addChild(tag); //親を切り替え
                    parent = tag;
                    //tag.Parent = cur;
                    tag.ChildList = new TagList(); //ここで空を作らないと閉じタグが出力されない
                    continue;
                }

                if (tag.isClose() == true)
                {
                    parent = parent.Parent; //親をひとつあげる（注意：タグは追加しない）
                    continue;
                }

                //あとは追加していくだけ
                parent.addChild(tag);
            }
        }

        /**
     * XMLからTREEを構築する
     * */
        public void parseTextTreeFromXmlPath(string path, Encoding enc, ref TagBase root)
        {
            try
            {
                TagList taglst = new TagList();
                parseTextFromPath(path, enc, ref taglst, true);
                //TREE構造にする
                createTextTreeFromTagList(taglst, ref root, path);
            }
            catch (Exception ex)
            {
                log_.LogError(path, 0, "parstree", ex.Message);
            }
        }

        /**
     * XMLからTREEを構築する
     * */
        public void parseTextTreeFromText(string buf, ref TagBase root)
        {
            int gyono = 0;

            try
            {
                TagList taglst = new TagList();
                string ama = "";
                parseText(buf, ref gyono, ref taglst, true, ref ama);
                //TREE構造にする
                createTextTreeFromTagList(taglst, ref root, "" /*path*/);
            }
            catch (Exception ex)
            {
                log_.LogError("parstree", ex.Message);
            }
        }

        static public bool parseText(string buf, ref TagList taglst,
            char open_tag, char close_tag, bool bNameSP, ref string amaritxt)
        {
            int gyono = 0;
            return parseText(buf, ref gyono, ref taglst, open_tag, close_tag, bNameSP, ref amaritxt);
        }

        // 戻り値がfalseだとタグ開始の文字列あり
        static public bool parseText(string buf, ref int gyono, ref TagList taglst,
            char open_tag, char close_tag, bool bNameSP, ref string amaritxt)
        {
            const char CHAR_QUOT = '"';
            amaritxt = "";
#if BUG_PROG
			Console.WriteLine("tagtextutil parseText 1 buf="+buf);
#endif
            if (buf.Length == 0) return true;

            bool res = true;
            //タグと文字列に分ける
            StringBuilder cur = new StringBuilder();
            bool bCommentTag = false;
            bool bOpen = false;
            bool bAttr = false; //<a aaa="<abc>>" 属性内でタグを許容 2016.3.18
            for (int m = 0; m < buf.Length; m++)
            {
                char ch = buf[m];
#if BUG_PROG
//Console.WriteLine("parseText 2 ch={0} cur={1} bComment={2}", ch, cur, bCommentTag);
#endif
                //""内でタグは許容
                if (bOpen == true && ch == CHAR_QUOT)
                {
                    bAttr = !bAttr;
                    cur.Append(ch);
                    continue;
                }

                if (bAttr == true) //属性内は文字として
                {
                    if (ch == open_tag || ch == close_tag)
                    {
                        cur.Append(ch);
                        continue;
                    }
                }

                //タグの始まりでそれまでの文字を追加
                if (ch == open_tag /*'<'*/) //aaaaaaa<  ==> aaaaaaを追加
                {
                    //それまでを追加
                    if (cur.Length > 0)
                    {
                        if (bOpen == true)
                        {
                            throw new Exception("起こし<タグが続いています [" + cur + "] " + buf);
                        }
#if BUG_PROG
						Console.WriteLine("tagtextutil parseText 3 addtext cur={0}", cur);
#endif
                        taglst.addText(cur.ToString(), bNameSP, ref gyono);
                        cur.Length = 0;
                    }

                    bOpen = true;
                    //continue;    //<は追加しない
                }

                if (ch == close_tag /*'>'*/) //   <aaaaaaa> ==> 追加  aaa>
                {
                    //普通のタグの閉じ、コメント内は無視
                    if (cur.Length > 0) // >で行が始まる場合
                    {
                        if (bOpen != true)
                        {
                            throw new Exception($"閉じ>タグが続いています cur=[{cur}] buf={buf}");
                        }

                        bOpen = false;
                        cur.Append(ch);
#if BUG_PROG
						Console.WriteLine("tagtextutil parseText 4 addtag cur={0}", cur);
#endif
                        taglst.addTag(cur.ToString(), open_tag, close_tag, gyono);
                        cur.Length = 0;
                        bCommentTag = false;
                        continue;
                    }
                }

                cur.Append(ch);

                //コメントの終了
                if (bCommentTag == true && ch == close_tag /*isCommentEnd(cur.ToString())==true*/)
                {
#if BUG_PROG
					Console.WriteLine("tagtextutil parseText 5 addcomment cur={0}", cur);
#endif
                    taglst.addTag(cur.ToString(), open_tag, close_tag, gyono); // <!-- xxxxx -->  までが追加される
                    cur.Length = 0;
                    bCommentTag = false;
                    continue;
                }

                //コメントの開始
                if (bCommentTag != true && cur.Length >= 4 && cur.ToString().Substring(0, 4) == "<!--")
                {
                    bCommentTag = true;
                }
            }

            //残り
            if (cur.Length > 0)
            {
                if (cur.ToString()[0] == open_tag)
                //if (cur.ToString().Substring(0, 1) == "<")
                {
                    amaritxt = cur.ToString();
                    res = false;
                }
                else
                {
                    taglst.addText(cur.ToString(), bNameSP, ref gyono);
                }
            }

            return res;
        }
    }
}