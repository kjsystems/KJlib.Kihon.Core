namespace kj.kihon
{
    public class IniItem
    {
        public string name;
        public string value;
        public IniItem(string n, string v) { name = n; value = v; }
        public IniItem(IniItem item) { name = item.name; value = item.value; }
        public override string ToString()
        {
            return string.Format("{0}={1}", name, value);
        }
    }
}