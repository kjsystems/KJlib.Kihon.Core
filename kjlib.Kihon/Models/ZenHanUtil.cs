using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kj.kihon.Utils
{
    public class ZenHanUtil
    {
        public enum Convert
        {
            Zen2Han,Han2Zen
        };

        const string Hankaku = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ[\w!#$%&'*+/=?^_@{}\|~-]";
        const string Zenkaku = @"０１２３４５６７８９ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ［￥ｗ！＃＄％＆’＊＋／＝？＾＿＠｛｝￥｜￣－］";

        public static string ConvertTo(string buf,Convert conv)
        {
            var sb = new StringBuilder();
            foreach (var ch in buf)
            {
                var tbl1 = (conv == Convert.Han2Zen) ? Hankaku: Zenkaku;
                var tbl2 = (conv == Convert.Han2Zen) ? Zenkaku: Hankaku;
                if (Array.IndexOf(tbl1.ToArray(), ch) >= 0)
                {
                    var idx = Array.IndexOf(tbl1.ToArray(), ch);
                    sb.Append(tbl2[idx]);
                    continue;
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }
        public static string ToHankaku(string buf)
        {
            return ConvertTo(buf, Convert.Zen2Han);
        }
        public static string ToZenkaku(string buf)
        {
            return ConvertTo(buf, Convert.Han2Zen);
        }
    }
}
