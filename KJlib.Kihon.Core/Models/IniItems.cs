using System;
using System.Collections.Generic;
using System.Text;
using kj.kihon;

namespace kjlib.lib.Models
{
    public class IniItems : List<IniItem>
    {
        public string this[string name]
        {
            get
            {
                foreach (IniItem item in this)
                {
                    if (item.name == name) return item.value;
                }
                return "";
            }
            set
            {
                //名前があれば上書きなければ追加
                foreach (IniItem item in this)
                {
                    if (item.name == name)
                    {
                        item.value = value;
                        return;
                    }
                }
                this.Add(new IniItem(name, value));
            }
        }

    }
}
