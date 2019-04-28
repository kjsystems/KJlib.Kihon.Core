using System.Collections.Generic;
using System.Text;

namespace kjlib.Kihon.Models
{
    public class StrUtil
    {
        /**
		 * 改行付きテキストから一覧を作成
		 * */
        public static void createListFromTextCRLF(string buf, ref List<string> lst)
        {
            //文字列からリストを作る
            lst.Add("");
            bool b0d = true;
            for (int m = 0; m < buf.Length; m++)
            {
                char ch = buf[m];
                if (ch == 0x0d)
                {
                    b0d = true;
                    continue;
                }
                if (b0d == true && ch == 0x0a)
                {
                    lst.Add("");  //改行
                    b0d = false;
                    continue;
                }
                b0d = false;
                lst[lst.Count - 1] += ch;
            }
        }
        /**
		 * 0d0a ==> \r\n
		 * */
        public static string chgTextCRLF(string buf)
        {
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < buf.Length; m++)
            {
                char ch = buf[m];
                if (ch == 0x0d)
                {
                    sb.Append("\\r");
                    continue;
                }
                if (ch == 0x0a)
                {
                    sb.Append("\\n");
                    continue;
                }
                if (ch == 0x0a)
                {
                    sb.Append("\\n");
                    continue;
                }
                if (ch == 0x09)
                {
                    sb.Append("\\t");
                    continue;
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
