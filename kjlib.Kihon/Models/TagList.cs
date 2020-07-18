using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kjlib.kihon.Models
{
    /**
	 * 
	 **/
    public abstract class TagBase : System.IComparable
    {
        public int CompareTo(object obj)
        {
            //nullより大きい
            if (obj == null)
            {
                return 1;
            }
            //Priceを比較する
            return this.getName().CompareTo(((TagBase)obj).getName());
            //または、次のようにもできる
            //return this.Price - ((Product)other).Price;
        }

        public enum TYPE { Open, Close, Comment, Text, CDATA, Hatena, InValid };
        public TYPE Type
        {
            get
            {
                if (isComment() == true) return TYPE.Comment;
                if (isOpen() == true) return TYPE.Open;
                if (isClose() == true) return TYPE.Close;
                if (isText() == true) return TYPE.Text;
                if (isCDATA() == true) return TYPE.CDATA;
                if (isHatena() == true) return TYPE.Hatena;  //<?xml version="1.0" encoding="UTF-8"?>
                return TYPE.InValid;
            }
        }
        abstract public bool isHatena();
        abstract public bool isCDATA();
        abstract public bool isComment();
        abstract public bool isTag();
        abstract public bool isOpen();
        abstract public bool isClose();
        abstract public bool isText();
        abstract public string getName();
        abstract public string Name { get; }
        abstract public string getText();
        abstract public bool isShort();
        abstract public void setShort(bool value);
        abstract public AttrList Attrs { get; set; }
        abstract public string getValue(string argname);
        abstract public string getValue(string argname, bool delQuot, char sep = '"');
        int gyono_ = 0;
        public int GyoNo { get { return gyono_; } }

        protected TagBase parent_;
        public TagBase Parent { get { return parent_; } set { parent_ = value; } }
        protected TagList childlst_;
        public TagList ChildList { get { return childlst_; } set { childlst_ = value; } }
        public TagList getChildList() { return childlst_; }


        public void addChildList(TagList chlst)
        {
            if (childlst_ == null) childlst_ = new TagList();
            childlst_.AddRange(chlst);
        }

        /**
		 * TREEなのでLISTで作り直し
		 * <div class="numeric">398,751,587</div>　で出力される
		 * */
        static public TagList createListFromTree(TagList tree)
        {
            TagList taglst = new TagList();
            int gyono = 0;
            foreach (TagBase tmp in tree)
            {
                string txt = tmp.ToString();        //
                TagList chlst = new TagList();
                string ama = "";
                TagTextUtil.parseText(txt, ref gyono, ref chlst, false, ref ama);
                foreach (TagBase tag in chlst)
                {
                    taglst.Add(tag);
                }
            }
            return taglst;
        }

        public TagBase addChild(TagBase tag)
        {
            if (childlst_ == null) childlst_ = new TagList();
            childlst_.Add(tag);
            tag.Parent = this;
            return this;
        }
        public string DumpTree(int level)
        {
            if (childlst_ == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach (TagBase tag in childlst_)
            {
                for (int m = 0; m < level; m++) sb.Append(" ");
                if (tag.isOpen() == true)
                {
                    sb.Append("<" + tag.getName());
                    if (tag.isShort() == true) sb.Append("/");
                    sb.Append(">");
                }
                else if (tag.isClose() == true)
                {
                    sb.AppendFormat("</{0}>", tag.getName());
                }
                else
                {
                    string buf = tag.ToString();
                    if (buf.Length >= 20)
                    {
                        buf = buf.Substring(0, 20);
                    }
                    buf = StrUtil.chgTextCRLF(buf);
                    sb.AppendFormat("[{0}](p={1}) {2}", tag.Type, tag.Parent.getName(), buf);
                }
                sb.AppendLine("");
                if (tag.ChildList != null)
                {
                    sb.Append(tag.DumpTree(level + 1));
                }
            }
            return sb.ToString();
        }
        /**
		 * 
		 * */
        public string getChildText()
        {
            StringBuilder sb = new StringBuilder();
            if (childlst_ != null)
            {
                foreach (TagBase tag in childlst_)
                {
                    sb.Append(tag.ToString());   //ToString()がポイント!! 閉じタグも出力される
                }
            }
            return sb.ToString();
        }
        /**
		 * タグは除いて文字列だけ出力
		 * */
        public string getChildTextOnly()
        {
            string buf = getChildText();
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(getChildText(), ref taglst, false, ref ama);

            StringBuilder sb = new StringBuilder();
            foreach (TagBase tag in taglst)
            {
                if (tag.isText() == true) sb.Append(tag.ToString());
            }
            return sb.ToString();
        }
        protected bool forceAddQuot_;
        public bool ForceAddQuot { set { forceAddQuot_ = value; } }   //ToStringで出力時にXMLで出力したい場合にONにする
        public TagBase(int gyono) { gyono_ = gyono; }

        public bool isSutaMokuji()
        {
            return (this.findValue("スタ") == "目次");
        }
        public string findValue(string name)
        {
            return this.Attrs.findValue(name, true, true);
        }
        virtual public char OpenTag { get { return '*'; } }
        virtual public char CloseTag { get { return '*'; } }
    }
    /**
	 * 
	 * */
    public class TagHatena : TagBase
    {
        override public bool isHatena() { return true; }
        override public bool isCDATA() { return false; }
        override public bool isComment() { return false; }
        override public bool isTag() { return false; }
        override public bool isText() { return false; }
        override public bool isOpen() { return false; }
        override public bool isClose() { return false; }
        override public string getName() { return ""; }
        override public string Name { get { return getName(); } }
        override public string getText() { return ToString(); }
        override public bool isShort() { return false; }
        override public void setShort(bool value) { }
        override public string getValue(string argname) { return ""; }
        override public string getValue(string argname, bool delQuot, char sep = '"') { return ""; }
        public override AttrList Attrs { get { return null; } set { } }
        /**
		 * <?xml version="1.0" encoding="UTF-8"?>
		 * */
        public TagHatena(string t) : base(0)
        {
            value_ = t.Substring(2, t.Length - 4);
        }
        string value_;  //<? ?> を除いた値 AttrListに分解していない
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?");
            sb.Append(value_);
            sb.Append("?>");
            return sb.ToString();
        }
    }
    /**
	 * 
	 * */
    public class TagText : TagBase
    {
        override public bool isHatena() { return false; }
        override public bool isCDATA() { return false; }
        override public bool isComment() { return false; }
        override public bool isTag() { return false; }
        override public bool isText() { return true; }
        override public bool isOpen() { return false; }
        override public bool isClose() { return false; }
        override public string getName() { return ""; }
        override public string Name { get { return getName(); } }
        override public string getText() { return text_; }
        public void setText(string text) { text_ = text; }
        override public bool isShort() { return false; }
        override public void setShort(bool value) { }
        override public string getValue(string argname) { return ""; }
        override public string getValue(string argname, bool delQuot, char sep = '"') { return ""; }

        public override AttrList Attrs { get { return null; } set { } }
        public string text_;  //文字列
        public TagText(string t, int gyono)
            : base(gyono)
        {
            text_ = t;
        }
        public override string ToString()
        {
            return text_;
        }
        public bool isCIDorHojo()
        {
            return TagTextUtil.isTaregtToten(text_, 0, '[', ']');
        }
        public int getCIDBango()
        {
            if (isCID() != true) return 0;
            return ConvUtil.toInt(text_.Substring("[&C".Length, text_.Length - "[&C".Length - 1), 0);
        }
        public bool isCID()
        {
            if (isCIDorHojo() != true) return false;
            if (text_.ToUpper().IndexOf("[&C") >= 0) return true;
            return false;
        }
        public string getHojoHex()
        {
            if (isHojo() != true) return "???";
            return text_.Substring("[&H".Length, text_.Length - "[&H".Length - 1);
        }
        public bool isHojo()
        {
            if (isCIDorHojo() != true) return false;
            if (text_.ToUpper().IndexOf("[&H") >= 0) return true;
            return false;
        }
        public bool isIMG()
        {
            if (isNamespace() != true) return false;
            if (text_.ToUpper().IndexOf("[&IMG") >= 0) return true;
            return false;
        }

        public bool isUTF()
        {
            //return TagTextUtil.isTaregtToten(text_, 0, '&', ';');
            return CharUtil.isSjisUTFEntity(text_);
        }
        public string getUTFCode()
        {
            string res = "";
            if (isUTF() != true) return res;

            //最初の&と最後の;は約束されている
            if (text_.ToLower().IndexOf("&#x") >= 0)
            {
                res = text_.Substring(3, text_.Length - 4);
                return res;
            }
            if (text_.ToLower().IndexOf("&#") >= 0)
            {
                res = text_.Substring(2, text_.Length - 3);
                return res;
            }
            throw new Exception("形式が&#x9999;でない [" + text_ + "]");
        }
        public bool isNamespace()
        {
            if (isCIDorHojo() == true || isUTF() == true) return true;
            return false;
        }
        public int getCID()
        {
            if (isCID() != true) return -1;
            // [&C9999]
            int res = ConvUtil.toInt(text_.Substring(3, text_.Length - 4), 0);
            return res;
        }
        // [&H3f3f]  ==> 3f3f
        public string getHojoCode()
        {
            if (isHojo() != true) return "";

            return text_.Substring(3, text_.Length - 4);
        }
    }
    /**
	 * 
	 * */
    public class TagComment : TagBase
    {
        /*
		 * <!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
		 * */
        public bool isDOCTYPE()
        {
            const string STR_DOCTYPE = "DOCTYPE";
            if (value_.Length > STR_DOCTYPE.Length && value_.Substring(0, STR_DOCTYPE.Length) == STR_DOCTYPE)
            {
                return true;
            }
            return false;
        }

        const string CDATA_TOP = "[CDATA[";
        const string CDATA_END = "]]";
        override public bool isCDATA()
        {
            if (value_.Length < 9) return false;
            if (value_.Substring(0, CDATA_TOP.Length) == CDATA_TOP &&
                value_.Substring(value_.Length - CDATA_END.Length) == CDATA_END)
            {
                return true;
            }
            return false;
        }
        public string getCDATA()
        {
            if (isCDATA() != true) return "";
            return value_.Substring(CDATA_TOP.Length, value_.Length - CDATA_TOP.Length - CDATA_END.Length);
        }
        override public bool isHatena() { return false; }
        override public bool isComment() { return true; }
        override public bool isTag() { return true; }
        override public bool isText() { return false; }
        override public bool isOpen() { return false; }
        override public bool isClose() { return false; }
        override public string getName() { return "!"; }
        override public string Name { get { return getName(); } }
        override public string getText() { return ""; }
        override public AttrList Attrs { get { return null; } set {; } }
        override public bool isShort() { return false; }
        override public void setShort(bool value) { }
        override public string getValue(string argname) { return ""; }
        override public string getValue(string argname, bool delQuot, char sep = '"') { return ""; }

        string value_;
        public TagComment(string t, int gyono)
            : base(gyono)
        {
            //Console.WriteLine("TagComment 1 tag {0}",t);
            string text_notag = t.Substring(1, t.Length - 2);
            //Console.WriteLine("TagComment 2 notag {0}", text_notag);
            if (text_notag.Substring(0, 1) != "!") throw new Exception("先頭がコメントでない " + t);
            //Console.WriteLine("TagComment 3 notag {0}", text_notag);
            value_ = text_notag.Substring(1, text_notag.Length - 1);
            //Console.WriteLine("TagComment 4 value {0}", value_);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<!");
            sb.Append(value_);  //!--の--から出力
            sb.Append(">");
            return sb.ToString();
        }
    }
    /**
	 * 
	 * */
    public class TagItem : TagBase
    {
        override public char OpenTag { get { return opentag_; } }
        override public char CloseTag { get { return closetag_; } }

        override public bool isHatena() { return false; }
        override public bool isCDATA() { return false; }
        override public bool isComment() { return false; }
        override public bool isTag() { return true; }
        override public bool isText() { return false; }
        override public bool isOpen() { if (name_.Substring(0, 1) != "/") return true; return false; }
        override public bool isClose() { return !isOpen(); }
        override public string getName()
        {
            if (isClose() == true) return name_.Substring(1)/*.ToLower()*/; return name_/*.ToLower()*/;
        }
        override public string Name { get { return getName(); } }
        override public string getText() { return ""; }
        override public string getValue(string argname, bool delQuot, char sep = '"'/*'か"*/)
        {
            if (isClose() == true) return "";
            if (isComment() == true) return "";
            return attrs_.findValue(argname, false/*小文字にしない*/, delQuot, sep);
        }
        override public string getValue(string argname)
        {
            return getValue(argname, true/*false*/);
        }
        public string name_;  //タグ名
        AttrList attrs_ = new AttrList();
        public override AttrList Attrs { get { return attrs_; } set { attrs_ = value; } }

        public override bool isShort() { return bShort_; }
        override public void setShort(bool value) { bShort_ = value; }
        bool bShort_ = false;

        char opentag_ = '<', closetag_ = '>';
        /**
		 * タグ名を取得する
		 * */
        void getTagNameFromTagText(string t, char close_tag)
        {
            string res = "";
            for (int m = 0; m < t.Length; m++)
            {
                char ch = t[m];
                var tbl = new[] { ' ', '\r', '\n', close_tag };
                if (Array.IndexOf(tbl, ch) >= 0)
                    break;

                //if (ch == ' ' || ch == '\r' || ch == close_tag) break;				//KJ_REFACT:20131105
                //省略タグは 2文字目以降
                if (m == t.Length - 1 && ch == '/') break;
                res += ch;
            }
            name_ = res;
            //省略タグ
            if (t[t.Length - 1] == '/')
            {
                bShort_ = true;
            }
            //Console.WriteLine("TagItem getTagNameFromTagText z buf="+t+" ==> short=" + bShort_ + " name=" + name_);
        }
        /**
		 * 
		 * */
        void createAttrList(string t)
        {
            //Console.WriteLine("TagTextUtil createAttrList 1 buf={0}",t);
            Tokens tokens = new Tokens(t, new char[] { ' ', '\n' }, true/*anyQuot*/);  //A=B C=D E=F をSPで分ける
            if (tokens.Count == 1) return;
            for (int m = 1; m < tokens.Count; m++)
            {
                string token = tokens[m];
                string name = "";
                string value = "";
                for (int p = 0; p < token.Length; p++)
                {
                    string ch = token.Substring(p, 1); ;
                    if (ch == "=")
                    {
                        name = value;
                        value = token.Substring(p + 1);
                        break;
                    }
                    value += ch;
                }
                //Console.WriteLine("TagTextUtil createAttrList 3 token={0} ==> name={1},value={2}",token,name,value);
                if (name.Length == 0 && value.Length == 0)
                {
                    continue;
                }
                Attrs.add(name, value);
            }
        }
        /**
		 * aaa abc="1"
		 **/
        public TagItem(string t, char opentag, char closetag, int gyono)
            : base(gyono)
        {
            opentag_ = opentag;
            closetag_ = closetag;
#if BUG_PROG
			Console.WriteLine("TagItem const 1 all="+t);
#endif
            if (opentag_ != '<')//<タグ>で取得でない(%xxx%みたいなの)でなければタグ名はすべて
            {
                name_ = t.Substring(1, t.Length - 2);
            }
            else
            {   //通常のタグ
                string text_notag = t.Substring(1, t.Length - 2);   //<柱文 "首の姫と首なし騎士" /> ==> 柱文 "首の姫と首なし騎士" /
                getTagNameFromTagText(text_notag, closetag);

                Tokens tokens = new Tokens(t, new char[] { ' ', '\r', '\n' });  //A=B C=D E=F をSPで分ける
                if (tokens.Count >= 2)
                {
                    if (text_notag.Substring(text_notag.Length - 1, 1) == "/")   //おしりに残っていたら削除
                    {
                        text_notag = text_notag.Substring(0, text_notag.Length - 1);
                    }
                    text_notag = text_notag.Trim();
                    createAttrList(text_notag);
                }
                if (name_ == null || name_.Length == 0)
                {
                    throw new Exception("タグ名を取得できない [" + t + "]");
                }
            }
#if BUG_PROG
			Console.WriteLine("TagItem const z name={0} attr={1}",name_,attrs_.ToString());
#endif
        }
        public TagItem(string t)
            : base(0/*gyono*/)
        {
            name_ = t;
            bShort_ = false;
            opentag_ = '<';
            closetag_ = '>';
        }
        /**
         * 
         **/
        static public TagItem createTagFromText(string t, int gyono)
        {
            if (t[0] != '<' || t[t.Length - 1] != '>')
            {
                throw new Exception("タグではないのでTagTextを作れない(createTagFromText) buf[" + t + "]");
            }
            return new TagItem(t, t[0], t[t.Length - 1], gyono);
        }
        /**
         * 
         **/
        public TagItem(string t, bool bShort, char opentag, char closetag, int gyono)
            : base(gyono)
        {
            name_ = t;
            bShort_ = bShort;
            opentag_ = opentag;
            closetag_ = closetag;
        }

        public string toString(char op, char cl)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(op);
            if (isClose()) sb.Append("/");
            sb.Append(getName());
            if (attrs_ != null && attrs_.Count > 0)
            {
                foreach (AttrItem attr in attrs_)
                {
                    sb.Append(" ");
                    if (attr.Name.Length > 0)
                    {
                        sb.Append(attr.Name + "=");
                    }
                    string v = attr.Value;
                    if (forceAddQuot_ == true) v = TokenUtil.addQuotForce(v);
                    sb.Append(v);
                }
            }
            if (bShort_ == true) sb.Append(" /");
            sb.Append(cl);

            //TREE構造の場合
            if (ChildList != null)
            {
                foreach (TagBase child in ChildList)
                {
                    sb.Append(child.ToString());
                }
                sb.Append(string.Format("</{0}>", getName()));
            }
            return sb.ToString();
        }
        /**
		 * 
		 **/
        public override string ToString()
        {
            return toString(opentag_, closetag_);
        }
    }
    /**
	 * 
	 **/
    public class TagList : List<TagBase>
    {
        public TagBase Last { get { if (this.Count == 0) return null; return this[this.Count - 1]; } }

        public void addTextNormal(string buf, int gyono)
        {
            this.Add(new TagText(buf, gyono));
        }
        /**
		 * 
		 * */
        public void addTextNameSpace(string buf, int gyono)
        {
            //[&C9999]、&#x9999;を分ける
            string cur = "";
            for (int idx = 0; idx < buf.Length; idx++)
            {
                char ch = buf[idx];
                char[] tbl = { '[', ']', '&', ';' };    //２個ずつ
                bool bFound = false;
                for (int m = 0; m < tbl.Length; m += 2)
                {
                    char sttToken = tbl[m];
                    char endToken = tbl[m + 1];
                    if (TagTextUtil.isTaregtToten(buf, idx, sttToken, endToken) == true)
                    {
                        //それまでを追加する
                        if (cur.Length > 0) this.Add(new TagText(cur, gyono));
                        int len = buf.Substring(idx).IndexOf(endToken);
                        string nsch = buf.Substring(idx, len + 1);
                        this.Add(new TagText(nsch, gyono));
                        //Console.WriteLine("tagtext addtext 2 文字とnamespaceを追加 ns=[{0}] ch={1}", nsch, cur);

                        cur = "";
                        idx += len;
                        bFound = true;
                        break;
                    }
                    if (bFound == true) break;
                }
                if (bFound == true) continue;  //追加したのでSKIPする
                cur += ch;
            }
            if (cur.Length > 0)
            {
                //Console.WriteLine("tagtext addtext 3 最後に追加 [{0}]",cur);
                this.Add(new TagText(cur, gyono));
            }
        }
        /**
		 * 
		 **/
        public void addText(string buf, bool bNameSP, ref int gyono)
        {
            if (bNameSP == true)
                addTextNameSpace(buf, gyono);
            else
                addTextNormal(buf, gyono);
            //改行の数を数えて追加する
            gyono += buf.ToCharArray().Count(c => c == '\n');
        }
        /**
		 * 
		 * */
        public void addTag(string t, char opentag, char closetag, int gyono)
        {
            // <aaaaaaa>  <!-- aaaaaaaa -->
            string text_notag = t.Substring(1, t.Length - 2);
            if (t.Length >= 2 && text_notag.Substring(0, 1) == "!")         //<!...はコメント
            {
                this.Add(new TagComment(t, gyono));   //コンストラクタでタグをとる
            }
            else if (t.Length > 4 && t[1] == '?' && t[t.Length - 2] == '?')
            {
                this.Add(new TagHatena(t));   //コンストラクタでタグをとる
            }
            else
            {
                this.Add(new TagItem(t, opentag, closetag, gyono));   //コンストラクタでタグをとる
            }
            //Console.WriteLine("TagList addtag z");
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (TagBase tag in this)
            {
                sb.Append(tag.ToString());
            }
            return sb.ToString();
        }
    }
}
