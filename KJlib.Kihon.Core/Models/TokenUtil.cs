using System;
using System.Text;

namespace kjlib.kihon.Models
{
    public class TokenUtil
    {
        static public string delSpace(string buf)
        {
            return buf.Replace(" ", "");
        }
        static public string delSpace2(string buf)
        {
            buf = buf.Replace(" ", "");
            buf = buf.Replace("�@", "");
            buf = buf.Replace("\t", "");
            return buf;
        }
        /**
		 �{���x�@1.0533469249065286E-2
		 */
        static public bool isBaiseido(string buf)
        {
            if (buf.Contains("E-") == true && buf.Contains(".") == true)
            {
                return true;
            }
            return false;
        }
        //�����������菜��
        static public string removeSujiFromText(string buf)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in buf)
            {
                if (char.IsNumber(ch)) continue;
                sb.Append(ch);
            }
            return sb.ToString();
        }
        //����������擾����
        static public string getSujiFromText(string buf)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in buf)
            {
                if (char.IsNumber(ch)) sb.Append(ch);
            }
            return sb.ToString();
        }
        //���pSP�������true
        static public bool hasSpace(string buf)
        {
            if (buf != null && buf.IndexOf(" ") >= 0)
            {
                return true;
            }
            return false;
        }
        static public string addQuotForce(string buf, char sep = '"')
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(sep);
            sb.Append(buf);
            sb.Append(sep);
            return sb.ToString();
        }
        // �K�v�Ȃ�""��ǉ�����
        static public string addQuot(string buf)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (buf == null)
            {
                ;//do nothing
            }
            else if (hasSpace(buf))
            {
                sb.Append("\"");
                sb.Append(buf);
                sb.Append("\"");
            }
            else
            {
                sb.Append(buf);
            }
            return sb.ToString();
        }
        /**
		 * 
		 * */
        /**
		 * 
		 * */
        static public string delQuot(string buf, char k1, char k2)
        {
            if (buf.Length >= 2 && buf[0] == k1 && buf[buf.Length - 1] == k2)
            {
                return buf.Substring(1, buf.Length - 2);
            }
            return buf;
        }
        static public string delQuot(string buf, char sep = '"')
        {
            return delQuot(buf, sep, sep);
        }
        static public string delKakko(string buf, string kakko_s, string kakko_e)
        {
            return delKakko(buf, kakko_s, kakko_e, true);
        }
        /**
		 * ��������Ƃ� 
		 **/
        static public string delKakko(string buf, string kakko_s, string kakko_e, bool bErrOut)
        {
            string res = buf;
            try
            {
                if (buf.Substring(0, 1) != kakko_s)
                {
                    if (bErrOut != true) return res;
                    throw new Exception("�擪�ɂ�����������܂���[" + kakko_s + "]");
                }
                if (buf.Substring(buf.Length - 1, 1) != kakko_e)
                {
                    if (bErrOut != true) return res;
                    throw new Exception("�I���ɂ�����������܂��� " + buf + " [" + kakko_e + "]");
                }
                res = buf.Substring(1, buf.Length - 2);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
    }
}
