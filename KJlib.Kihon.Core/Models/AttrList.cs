using System;
using System.Collections.Generic;
using System.Text;

namespace KJlib.Kihon.Core.Models
{
    /**
     * 
     **/
    //[Serializable()]
    public class AttrItem : System.IComparable
    {
        public int CompareTo(object obj)
        {
            //nullより大きい
            if (obj == null)
            {
                return 1;
            }

            //Priceを比較する
            return this.Name.CompareTo(((AttrItem) obj).Name);
            //または、次のようにもできる
            //return this.Price - ((Product)other).Price;
        }

        string name_, value_;

        public string Name
        {
            get { return name_; }
            set { name_ = value; }
        }

        public string Value
        {
            get { return value_; }
            set { value_ = value; }
        }

        public string getValue(bool delQuot, char sep = '"')
        {
            if (delQuot == true) return TokenUtil.delQuot(value_, sep);
            return value_;
        }

        public void write(System.IO.BinaryWriter bw)
        {
            BinUtil b = new BinUtil(bw);
            b.w(Name);
            b.w(Value);
        }

        public void read(System.IO.BinaryReader br)
        {
            BinUtil b = new BinUtil(br);
            Name = b.rt();
            Value = b.rt();
        }

        public override string ToString()
        {
            if (Name.Length == 0) return Value; //KJ_REFACT:20121110  <柱文 "首の姫と首なし騎士" /> の出力で
            return string.Format("{0}={1}", Name, Value);
        }

        public AttrItem(string n, string v)
        {
            name_ = n;
            value_ = v;
        }

        public AttrItem(AttrItem item)
        {
            name_ = item.name_;
            value_ = item.value_;
        }

        public AttrItem()
        {
        }
    }

    /**
     * 
     * */
    //[Serializable()]
    public class AttrList : List<AttrItem>
    {
        public void createListFromText(string buf, char kugi1, ref AttrList lst)
        {
            Tokens t2 = new Tokens(buf, new char[] {kugi1});
            if (t2.Count != 2)
            {
                lst.add("", t2[0]);
                //throw new Exception("分解できない(AttrList:createListFromText) [" + buf + "]");
                return;
            }

            lst.add(t2[0], t2[1]); //specでは名前のスペースを利用するのでここでtrimしない
        }

        //最初に,で分ける、その後 name=valueでわける
        public void createListFromText(string buf, char kugi1, char kugi2, ref AttrList lst)
        {
            Tokens t1 = new Tokens(buf, new char[] {kugi1});
            foreach (string token in t1)
            {
                createListFromText(token, kugi2, ref lst);
            }

            if (lst.Count == 0)
            {
                throw new Exception("項目を取得できない(AttrList:createListFromText) [" + buf + "]");
            }
        }


        /**
         * 重複を避ける
         * */
        public void add(AttrItem item)
        {
            foreach (AttrItem i in this)
            {
                if (i.Name.ToLower() == item.Name.ToLower())
                {
                    i.Value = item.Value;
                    return;
                }
            }

            this.Add(item);
        }

        public AttrItem findFromName(string name)
        {
            foreach (AttrItem item in this)
            {
                if (item.Name == name) return item;
            }

            return null;
        }

        public AttrList()
        {
        }

        public AttrList(AttrList lst)
        {
            foreach (AttrItem item in lst) this.Add(new AttrItem(item));
        }
        //------
    public void read(System.IO.BinaryReader br)
		{
			BinUtil b = new BinUtil(br);
			int num = b.ri();
			for (int m = 0; m < num; m++)
			{
				AttrItem item = new AttrItem();
				item.read(br);
				this.Add(item);
			}
		}
		public void write(System.IO.BinaryWriter bw)
		{
			BinUtil b = new BinUtil(bw);
			b.wi(this.Count);
			foreach (AttrItem item in this)
			{
				item.write(bw);
			}
		}
        public int findValueInt(string name, bool bLower, bool delQuot, int def)
        {
            return ConvUtil.toInt(findValue(name, bLower, delQuot), def);
        }

        public long findValueLong(string name, bool bLower, bool delQuot, long def)
        {
            return ConvUtil.toLong(findValue(name, bLower, delQuot), def);
        }

        public string findValue(string name, bool bLower, bool delQuot, char sep = '"')
        {
            AttrItem item = Find(
                delegate(AttrItem i)
                {
                    if (bLower == true) return i.Name.ToLower() == name.ToLower();
                    return i.Name == name;
                }
            );
            if (item != null) return item.getValue(delQuot, sep);
            return "";
        }

        public string findValue(string name)
        {
            return findValue(name, false /*小文字にしない*/, false);
        }

        public void clear(string[] nodellst)
        {
            //Console.WriteLine("AttrList clear 1 count={0}",Count);
            for (int m = this.Count - 1; m >= 0; m--)
            {
                bool bNotDel = false;
                foreach (string tag in nodellst)
                {
                    if (tag == this[m].Name)
                    {
                        //Console.WriteLine("AttrList clear 2 この属性は残します name={0} value={1}", this[m].Name, this[m].Value);
                        bNotDel = true;
                        break;
                    }
                }

                if (bNotDel == true) continue;
                this.Remove(this[m]);
            }
        }

        /**
         * 
         **/
        string toStringToken_ = ",";

        public string ToStringToken
        {
            set { toStringToken_ = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < this.Count; m++)
            {
                if (m != 0) sb.Append(toStringToken_);
                sb.Append(this[m].ToString());
            }

            return sb.ToString();
        }

        /**
         * 
         * */
        public void remove(string name)
        {
            foreach (AttrItem item in this)
            {
                if (item.Name == name)
                {
                    this.Remove(item);
                    break;
                }
            }
        }

        public void add(string name, string value, bool addQuotForce)
        {
            if (addQuotForce == true) value = TokenUtil.addQuotForce(value);
            add(name, value);
        }

        /**
         * 
         **/
        public void add(string name, string value)
        {
            foreach (AttrItem item in this)
            {
                // 名前があれば更新 ただし名前なしは複数あり
                if ( !string.IsNullOrEmpty(name) && item.Name == name)
                {
                    item.Value = value;
                    return;
                }
            }

            this.Add(new AttrItem(name, value));
        }

        /**
         * 
         **/
        public bool isValid(string name)
        {
            foreach (AttrItem attr in this)
            {
                if (attr.Name == name) return true;
            }

            return false;
        }

        /**
         * 
         **/
        public string this[string name]
        {
            get
            {
                foreach (AttrItem item in this)
                {
                    if (item.Name == name) return item.Value;
                }

                return "";
            }
            set
            {
                foreach (AttrItem item in this)
                {
                    if (item.Name == name)
                    {
                        item.Value = value;
                        return;
                    }
                }

                this.add(name, value);
            }
        }
    }
}