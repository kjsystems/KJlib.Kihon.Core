//#define BUG_PROG

using System;
using System.Collections.Generic;
using System.Text;

namespace kjlib.Kihon.Models
{
    public class EncodingUtil
    {
        /**
		 * 
		 **/
        static public string bytes2string(byte[] bytes, Encoding enc)
        {
            List<byte> lst = new List<byte>();
            for (int m = 0; m < bytes.Length; m++)
            {
                if (bytes[m] == 0) break;       //814f00000000... NULL終端のデータに対応
                lst.Add(bytes[m]);
            }
            return enc.GetString(lst.ToArray());
        }
        static public string byte2string(byte b, Encoding enc)
        {
            //byte[] =bytes[1];
            return enc.GetString(new byte[] { b });
        }
        /**
		 *
		 */
        static public int getIntFromBytes(byte[] bytes)
        {
            if (bytes.Length == 2) return bytes[1] * 0x100 + bytes[0];
            if (bytes.Length == 4)
            {
                return bytes[3] * 0x1000000 + bytes[2] * 0x10000 +
                    bytes[1] * 0x100 + bytes[0];
            }
            throw new Exception("バイト数に対応していない bytes.length=" + bytes.Length);
        }

        // あ ==> 0x3042
        static public int getHexFromTextUTF16(string ch)
        {
            Encoding sjisEnc = Encoding.GetEncoding("UTF-16");
            byte[] bytes = sjisEnc.GetBytes(ch);
            return bytes[1] * 0x100 + bytes[0];
        }

        static public Encoding SJIS { get { return Encoding.GetEncoding("Shift_JIS"); } }

        /**
             * "3042" ==> あ を返す
             * 1文字だけ対応 shift_jisではバイトオーダーをひっくり返しているので
             * */
        static public string getTextFromHexText(string hex, Encoding enc)
        {
            int v = Convert.ToInt32(hex, 16);   //16進数に変換
            byte[] bytes = BitConverter.GetBytes(v);   //intなので4バイト
                                                       //return enc.GetString(bytes).Substring(0, 1);    //UNICODEはこれでOK

#if BUG_PROG
			Console.WriteLine("getTextFromHexText 0 HEX={0} ==> int=0x{1:x} ==> bytes.len={2}", hex, v, bytes.Length);
#endif
            if (enc != EncodingUtil.SJIS)
            {
                string res = enc.GetString(bytes);
#if BUG_PROG
				Console.WriteLine("getTextFromHexText 1 res ch={0} hex=0x{1:x}", res, getHexTextFromText(res,Encoding.UTF32) );
#endif
                return res;
            }
            int enclen = 0;   // 20b9f ==> f9b02
            for (int m = 0; m < bytes.Length; m++)
            {
                if (m != 0 && bytes[m] == 0x00) break;
#if BUG_PROG
				Console.WriteLine(" #{0}:{1:x}", m, bytes[m]);
#endif
                enclen++;
            }
            byte[] encbytes = new byte[enclen];
            Array.Copy(bytes, encbytes, enclen);

            //ひっくり返す  a082 ==> 82a0
            if (enclen >= 2 && (enc == Encoding.GetEncoding("shift_jis") /*|| enc == Encoding.GetEncoding("UTF-16")*/ ))
            {
#if BUG_PROG
				Console.WriteLine(" ==>reverse");
#endif
                Array.Reverse(encbytes);
            }
            return enc.GetString(encbytes);
        }
        /**
		 * あ ==> "3042" を返す
		 * */
        static public string getHexTextFromText(string text, Encoding enc)
        {
            byte[] bytes = enc.GetBytes(text);
            StringBuilder sb = new StringBuilder();
            //foreach (byte by in bytes)

            if (bytes.Length == 1) sb.Append("00");
            if (enc == Encoding.Unicode)
            {
                for (int m = bytes.Length - 1; m >= 0; m--)  //反対から
                {
                    sb.Append(string.Format("{0:x2}", bytes[m]));
                }
            }
            else
            {
                for (int m = 0; m < bytes.Length; m++)
                {
                    sb.Append(string.Format("{0:x2}", bytes[m]));
                }
            }
            return sb.ToString();
        }
        /**
         * 
         * */
        static bool canChangeToSJIS(char ch)
        {           //変換できない   			//EncidingUtil.utf2sjis_text
            string sjis = EncodingUtil.utf2sjis_char(ch);           //u+6b35
            byte[] bytes = EncodingUtil.SJIS.GetBytes(sjis);
            if (0x3F == bytes[0]) return false;
            return true;
        }
        /**
         * 変換できない文字を&#x9999;に変換
         * */
        static public string utf2sjis_text(string buf, bool bChkHatena)
        {
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < buf.Length; m++)
            {
                char ch = buf[m];
                bool surrogate = Char.IsSurrogate(ch);  //サロゲートペア
                if (surrogate == true)
                {
                    int code = Char.ConvertToUtf32(ch, buf[m + 1]);
                    sb.Append(string.Format("&#x{0:x4};", code));
                    m++;
                    continue;
                }

                //SJISに変換できなければ&#x9999;
                if (bChkHatena == true && canChangeToSJIS(ch) != true)
                {           //変換できない
                    Encoding enc = Encoding.GetEncoding("UTF-16");
                    byte[] bytes = enc.GetBytes(ch.ToString());
                    int hex = EncodingUtil.getIntFromBytes(bytes);
                    sb.Append(string.Format("&#x{0:x4};", hex));
                    continue;
                }
                sb.Append(buf[m]);
            }
            return sb.ToString();
        }
        /**
		 * UNICODEからSJISに変換
		 **/
        static public string utf2sjis_char(char utfString)
        {
            Encoding unicode = Encoding.Unicode;
            byte[] unicodeByte = unicode.GetBytes(utfString.ToString());       //UTFのバイトを取得
            Encoding s_jis = Encoding.GetEncoding("shift_jis");
            byte[] s_jisByte = Encoding.Convert(unicode, s_jis, unicodeByte);    //SJISのバイトに変換
            char[] s_jisChars = new char[s_jis.GetCharCount(s_jisByte, 0, s_jisByte.Length)];
            s_jis.GetChars(s_jisByte, 0, s_jisByte.Length, s_jisChars, 0);
            return new string(s_jisChars);
        }
        /**
		 * 
		 * */
        static public string sjis2utf8(char[] sjisStr)
        {
            Encoding utf8enc = Encoding.UTF8;
            Encoding sjisenc = EncodingUtil.SJIS;
            byte[] sjisByte = sjisenc.GetBytes(sjisStr);       //SJISのバイトを取得

            byte[] utf8Bytes = Encoding.Convert(sjisenc, utf8enc, sjisByte);    //UTF8のバイトに変換
            char[] utf8Chars = new char[utf8enc.GetCharCount(utf8Bytes, 0, utf8Bytes.Length)];
            utf8enc.GetChars(utf8Bytes, 0, utf8Bytes.Length, utf8Chars, 0);
            return new string(utf8Chars);
        }

    }
}
