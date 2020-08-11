using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kjlib.kihon.Models
{
    public class RubyTextUtil
    {
        /// <summary>
        /// 親文字のみ出力
        /// </summary>
        public static string ToStringOyaOnly(List<RubyText> lst)
        {
            var sb = new StringBuilder();
            foreach (var txt in lst)
                sb.Append(txt.Oya);
            return sb.ToString();
        }

        public static string ToTagFromKakko(string buf, bool bEpubGroupRuby = false)
        {
            return RubyTextUtil.ToTag(RubyTextUtil.ChangeFromKakkoRuby(buf), bEpubGroupRuby);
        }

        public static string ToTag(List<RubyText> lst, bool bEpubGroupRuby = false)
        {
            var sb = new StringBuilder();
            if (bEpubGroupRuby != true)
            {
                foreach (var item in lst)
                {
                    sb.Append(item.toTag());
                }
            }
            else
            {
                //ルビが続くならまとめる
                bool isPrevRuby = false;
                foreach (var item in lst.Select((v, i) => new { v, i }))
                {
                    var rby = item.v;
                    if (isPrevRuby == true && rby.hasRuby != true) //直前を閉じる
                    {
                        sb.Append("</ruby>");
                        isPrevRuby = false;
                    }
                    if (isPrevRuby != true && rby.hasRuby == true)
                    {
                        sb.Append("<ruby>");
                        isPrevRuby = true;
                    }
                    sb.Append(rby.toTagRtOnly());
                    isPrevRuby = rby.hasRuby;
                }
                if (isPrevRuby == true) //直前を閉じる
                {
                    sb.Append("</ruby>");
                }
            }
            return sb.ToString();
        }

        public static string ToKakko(List<RubyText> lst)
        {
            var sb = new StringBuilder();
            foreach (var item in lst) sb.Append(item.toKakko());
            return sb.ToString();
        }

        //<ruby>xxx<rt>yyy</rt></ruby> ==> {xxx}(yyy)
        public static string ChangeToKakkoRuby(string ruby_tag)
        {
            CreateRubyList(ruby_tag, out List<RubyText> lst);
            return ToKakko(lst);
        }

        //{xxx}(yyy)
        public static List<RubyText> ChangeFromKakkoRuby(string kakko_tag)
        {
            var lst = new List<RubyText>();
            var buf = kakko_tag;

            var bRuby = false;
            var bRt = false;
            var oyatxt = "";
            var rbytxt = "";
            foreach (var item in buf.Select((v, i) => new { v, i }))
            {
                char ch = item.v;
                if (ch == '{')
                {
                    bRuby = true;
                    continue;
                }
                if (ch == '}')
                {
                    bRuby = false;
                    continue;
                }
                if (ch == '(')
                {
                    bRt = true;
                    //親文字の{を閉じてない
                    if (bRuby == true)
                        throw new Exception($"親文字の閉じ}}がない");
                    continue;
                }
                if (ch == ')')
                {
                    if (string.IsNullOrEmpty(rbytxt))
                        throw new Exception($"ルビ文字がない");
                    //グループルビでなければ直前の文字
                    if (string.IsNullOrEmpty(oyatxt))
                    {
                        if (lst.Count == 0)
                            throw new Exception($"親文字がなくて(がある");
                        if (lst.Last().hasRuby == true)
                            throw new Exception($"ルビが続いてる {buf}");
                        lst.Last().Ruby = rbytxt;
                    }
                    else
                    {
                        lst.Add(new RubyText()
                        {
                            Oya = oyatxt,
                            Ruby = rbytxt
                        });
                    }
                    oyatxt = "";
                    rbytxt = "";
                    bRt = false;
                    continue;
                }
                if (bRuby == true) //ルビの親文字
                {
                    oyatxt += ch;
                }
                else if (bRt == true) //ルビ
                {
                    rbytxt += ch;
                }
                else //通常の文字
                {
                    lst.Add(new RubyText()
                    {
                        Oya = ch.ToString(),
                    });
                }
            }
            return lst;
        }

        /// <summary>
        /// 親文字とルビのリストを作成する
        /// タグなど関係ない文字は親文字のみで
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="lst"></param>
        public static void CreateRubyList(string buf, out List<RubyText> lst)
        {
            lst = new List<RubyText>();

            var gyono = 0;
            var taglst = TagTextUtil.parseText(buf, ref gyono, false);
            var oyaText = "";
            var rubyText = "";
            var bRuby = false;
            var bRT = false;
            foreach (var item in taglst.Select((v, i) => new { v, i }))
            {
                if (item.v.getName().ToLower() == "rt")
                {
                    bRT = item.v.isOpen();
                    continue;
                }
                if (item.v.getName().ToLower() == "ruby")
                {
                    bRuby = item.v.isOpen();
                    if (item.v.isOpen() != true)
                    {
                        lst.Add(new RubyText
                        {
                            Oya = oyaText,
                            Ruby = rubyText
                        });
                        oyaText = "";
                        rubyText = "";
                    }
                    continue;
                }
                if (bRuby == true && bRT == true)
                {
                    rubyText += item.v.ToString(); //タグ＋文字もあるので+=
                    continue;
                }
                if (bRuby == true)
                {
                    oyaText += item.v.ToString();
                    continue;
                }

                // ルビに関係ないものはすべて親文字として追加
                lst.Add(new RubyText
                {
                    Oya = item.v.ToString()
                });
            }
        }
    }
}