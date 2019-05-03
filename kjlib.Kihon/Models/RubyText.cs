namespace kjlib.Kihon.Models
{
    public class RubyText
    {
        public string Oya { get; set; }
        public string Ruby { get; set; }
        public int Gyono { get; set; }

        public RubyText(int gyono = 0)
        {
            Gyono = gyono;
        }

        public bool hasRuby
        {
            get { return !string.IsNullOrEmpty(Ruby); }
        }

        public override string ToString()
        {
            return toTag();
        }

        public string toTag()
        {
            if (hasRuby != true)
                return Oya;
            return $"<ruby>{Oya}<rt>{Ruby}</rt></ruby>";
        }

        public string toTagRtOnly()
        {
            if (hasRuby != true)
                return Oya;
            return $"{Oya}<rt>{Ruby}</rt>";
        }

        //モノルビ：あ（ア）
        //グループ：い（イ）
        public string toKakko()
        {
            if (hasRuby != true)
                return Oya;
            if (Oya.Length == 1)
                return $"{Oya}({Ruby})";
            return $"{{{Oya}}}({Ruby})";
        }
    }
}