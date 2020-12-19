using System;
using System.Collections.Generic;
using System.Text;

namespace KJlib.Kihon.Core.Models
{
    public class IniGroup
    {
        public void remove(string name)
        {
            for (int m = 0; m < items_.Count; m++)
            {
                if (items_[m].name == name)
                {
                    items_.RemoveAt(m);
                    break;
                }
            }
        }

        public enum TYPE { INI, CSV }
        TYPE type_;
        int gyono_;
        public int GyoNo { get { return gyono_; } set { gyono_ = value; } }
        public TYPE FileType { get { return type_; } set { type_ = value; } }

        public enum VIEWTYPE { Normal, WakaShiori, WakaShoriType }
        VIEWTYPE viewType_ = VIEWTYPE.Normal;
        public VIEWTYPE ViewType { get { return viewType_; } set { viewType_ = value; } }

        public string name;
        IniItems items_ = new IniItems();
        public IniItems Items { get { return items_; } set { items_ = value; } }
        public IniGroup(string n, int gyono) { name = n; type_ = TYPE.INI; gyono_ = gyono; }
        public IniGroup(IniGroup g) { name = g.name; type_ = g.type_; copyItems(g.Items); }

        public IniGroup(string n, IniItems item) { name = n; items_.AddRange(item); type_ = TYPE.INI; gyono_ = 0; }

        public void clear() { name = ""; items_.Clear(); }

        public string this[string name]
        {
            get { return getValue(name); }
            set { items_[name] = value; }
        }


        string getValue(string name)
        {
            string res = Items[name];
            if (res == null) return "";
            return res;
        }
        public int getInt(string name, int def)
        {
            int result = def;
            //Console.WriteLine("ini getint 1 value="+Items[name]);
            if (Items[name].Length == 0) return def;
            int.TryParse(Items[name], out result);
            //Console.WriteLine("ini getint 2 result=" + result);
            return result;
        }
        /**
		 * 
		 **/
        public void copyItems(IniItems items)
        {
            items_.Clear();
            foreach (IniItem item in items)
            {
                items_.Add(new IniItem(item));
            }
        }
        /**
		 * 
		 * */
        string getTypeText(string type)
        {
            switch (type)
            {
                case "ku": return "句";
                case "goi": return "語彙";
                case "utaban": return "歌番";
            }
            return "???";
        }
        /**
		 * 
		 **/
        string getWakaViewText(bool bShurui)
        {
            StringBuilder sb = new StringBuilder();
            if (bShurui == true)
            {
                sb.Append(getTypeText(Items["種類"]));
                sb.Append("\t");
            }
            sb.Append(this.name);
            sb.Append("\t");
            bool bFirst = true;
            foreach (IniItem item in this.Items)
            {
                if (item.value.Length > 0)
                {
                    if (item.name == "種類") continue;
                    if (bFirst != true) sb.Append(",");
                    sb.Append(item.value);
                    bFirst = false;
                }
            }
            return sb.ToString();
        }

        //WEB和歌　ListBoxに出力で使用
        public override string ToString()
        {
            if (ViewType == VIEWTYPE.WakaShiori)
            {
                return getWakaViewText(false);
            }
            if (ViewType == VIEWTYPE.WakaShoriType)
            {
                return getWakaViewText(true);  //語彙/句 表示
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[{this.name}]");
            foreach (var item in this.Items)
            {
                sb.AppendLine(item.ToString());
            }
            sb.AppendLine("");
            return sb.ToString();
        }
    }
    /**
	 *
	 * */
    public class IniGroups : List<IniGroup>
    {
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (IniGroup gr in this)
            {
                sb.AppendLine(gr.ToString());
            }
            return sb.ToString();
        }

        public IniGroup this[string name]
        {
            get
            {
                foreach (IniGroup item in this)
                {
                    if (item.name == name) return item;
                }
                return null;
            }
        }
        /**
		 * nameのつく登録されていない名前を取得する
		 * */
        public string getUniqueName(string name)
        {
            string res = "";
            int index = 0;
            while (true)
            {
                index++;
                res = string.Format("{0}{1}", name, index);
                bool found = false;
                foreach (IniGroup grp in this)
                {
                    if (grp.name == res)
                    {
                        found = true;
                        break;
                    }
                }
                if (found == false) break;
            }
            return res;
        }

        //最後のグループにアイテムを追加する
        public void addItem(string name, string value/*nullあり*/)
        {
            if (Count == 0)
            {
                throw new Exception(string.Format("親の項目が１つもありません(IniGroups)\nName={0} Value={1}", name, value));
            }
            if (value == null) value = "";
            this[Count - 1].Items.Add(new IniItem(name, value));
        }
        //Groupを探して追加する
        public void addItemWithGroup(string grpname, string name, string value/*nullあり*/)
        {
            IniGroup tgtGroup = null;
            foreach (IniGroup grp in this)
            {
                if (grp.name == grpname)
                {
                    tgtGroup = grp;
                    break;
                }
            }
            if (tgtGroup == null)
            {
                tgtGroup = addGroup(grpname, 0);
            }
            tgtGroup.Items.Add(new IniItem(name, value));
        }
        /**
		 * 
		 **/
        public IniGroup addGroup(string name, int gyono)
        {
            IniGroup grp = new IniGroup(name, gyono);
            this.Add(grp);
            return grp;
        }
        /**
		 * 
		 **/
        public void addGroupData(IniGroup tgt)
        {
            foreach (IniGroup grp in this)
            {
                if (grp.name == tgt.name)
                {
                    grp.copyItems(tgt.Items);
                    return;
                }
            }
            this.Add(tgt);
        }
        /**
		 * 
		 * */
        public IniGroup findGroupFromNameData(string name, string value)
        {
            foreach (IniGroup grp in this)
            {
                if (grp[name] == value) return grp;
            }
            return null;
        }
        public IniGroup findGroup(string name)
        {
            foreach (IniGroup grp in this)
            {
                if (grp.name == name) return grp;
            }
            return null;
        }
    }
}
