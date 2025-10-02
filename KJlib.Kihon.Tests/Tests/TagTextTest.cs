using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KJlib.Kihon.Core.Extensions;
using KJlib.Kihon.Core.Models;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace KJlib.Kihon.Tests.Tests
{
    [TestFixture]
    public class TagTextTest
    {
        private ILogger<TagTextTest> _logger;

        public TagTextTest()
        {
            var loggerFactory = LoggerFactory.Create(_ => { });
            _logger = loggerFactory.CreateLogger<TagTextTest>();
        }

        [Test]
        public void 名前なし引数が二つ()
        {
            string buf = "<文章 窓左 集名>菟玖波集解題〔１菟玖波解〕</文章>";
            var lst = TagTextUtil.parseText(buf);
            Assert.That(3, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }


        [Test]
        public void TagTextUtil_改行を行番号としてカウント()
        {
            int gyono = 0;
            var taglst = TagTextUtil.parseText("<html>\r\naaa\r\n</html>", ref gyono, false);
            Assert.That("<html>", Is.EqualTo(taglst[0].ToString()));
            Assert.That(0, Is.EqualTo(taglst[0].GyoNo));
            Assert.That("\r\naaa\r\n", Is.EqualTo(taglst[1].ToString()));
            Assert.That(0, Is.EqualTo(taglst[1].GyoNo));
            Assert.That("</html>", Is.EqualTo(taglst[2].ToString()));
            Assert.That(2, Is.EqualTo(taglst[2].GyoNo));
        }
        [Test]
        public void TagTextUtil_改行を行番号としてカウント02()
        {
            var taglst = TagTextUtil.parseText("<html>\r\naaa\r\n</html>");
            Assert.That("<html>", Is.EqualTo(taglst[0].ToString()));
            Assert.That(0, Is.EqualTo(taglst[0].GyoNo));
            Assert.That("\r\naaa\r\n", Is.EqualTo(taglst[1].ToString()));
            Assert.That(0, Is.EqualTo(taglst[1].GyoNo));
            Assert.That("</html>", Is.EqualTo(taglst[2].ToString()));
            Assert.That(2, Is.EqualTo(taglst[2].GyoNo));
        }



        [Test]
        public void TagTextUtil_タグ開始だけ()
        {
            int gyono = 0;
            var taglst = TagTextUtil.parseText("<html",ref gyono, false);
            Assert.That(0, Is.EqualTo(taglst.Count));
        }
        [Test]
        public void TagTextUtil_タグ閉じだけ()
        {
            var taglst = TagTextUtil.parseText(">");
            Assert.That(1, Is.EqualTo(taglst.Count));
            Assert.That(">", Is.EqualTo(taglst[0].ToString()));
        }

        [Test]
        public void TagTextUtil_タグ名改行属性()
        {
            int gyono = 0;
            var lst = TagTextUtil.parseText("<span\nstyle=\'font-size:14.0pt;font-family:\"ＭＳ ゴシック\"\'>", ref gyono, false);
            Assert.That("span", Is.EqualTo(lst[0].getName()));
            Assert.That(1, Is.EqualTo(lst[0].Attrs.Count));
            Assert.That("<span style=\'font-size:14.0pt;font-family:\"ＭＳ ゴシック\"\'>", Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void TagTextUtil_タグ名改行属性_0d0a()
        {
            var lst = TagTextUtil.parseText("<span\r\nstyle=\'font-size:14.0pt;font-family:\"ＭＳ ゴシック\"\'>");
            Assert.That("span", Is.EqualTo(lst[0].getName()));
            Assert.That(1, Is.EqualTo(lst[0].Attrs.Count));
            Assert.That("<span style=\'font-size:14.0pt;font-family:\"ＭＳ ゴシック\"\'>", Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_属性にタグ()
        {
            string buf = "あああ<M 番号=\"いいい<M>\">ううう";
            TagList lst = TagTextUtil.parseText(buf);
            Assert.That(3, Is.EqualTo(lst.Count));
            Assert.That("あああ", Is.EqualTo(lst[0].ToString()));
            Assert.That("<M 番号=\"いいい<M>\">", Is.EqualTo(lst[1].ToString()));
            Assert.That("ううう", Is.EqualTo(lst[2].ToString()));
        }

        [Test]
        public void taglist_parseしてテキストを返す()
        {
            string buf = "あああ<ruby>いいい<rt>ううう</rt></ruby>";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(7, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_parseしてテキストを返す_改行コード()
        {
            string buf = "あああ\r\n<ruby>いいい<rt>ううう</rt></ruby>";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(7, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_parseして省略タグはスペースあり()
        {
            string buf = "あああ<br/>いいい<br />";
            string exp = "あああ<br />いいい<br />";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(exp, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_parseしてテキストを返す_改行コード2()
        {
            string buf = "あああ\r<ruby>いいい<rt>ううう</rt></ruby>";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(7, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_02()
        {
            string buf = "<a href=\"#mokuji-0001\"><font t-class=\"min-110per\">序文　　吉田　敦彦</font></a><br>";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(6, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_03()
        {
            string buf = "<html><!-- 角川書店 ttx仕様 ver.2.1 -->";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(2, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        // [Test]
        public void taglist_04()
        {
            string buf = "<html><ttime \r\naaa>";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(2, Is.EqualTo(lst.Count));
            Assert.That("<html><ttime aaa>", Is.EqualTo(lst.ToString()));
        }
        // [Test]
        public void taglist_05()
        {
            string buf = "<html><ttime \r\naaa>";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(2, Is.EqualTo(lst.Count));
            Assert.That("<html><ttime aaa>", Is.EqualTo(lst.ToString()));
        }
        /**
		 * 以下はコメントとして処理したい
		<!--
		<t-contents>
			<a href="#mokuji-0001"></a>
		</t-contents>
		 -->
		*/
        //[Test]
        public void taglist_06()
        {
            string buf = "<!-- \r\n";
            buf += "<t-contents>";
            buf += "-->";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(1, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_07()
        {
            string buf = "<head>\r\n";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(2, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_08()
        {
            string buf = "<t-r>大林太良（おお<!>ばやし）";   //切れ方がおかしいが仕方ない
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(4, Is.EqualTo(lst.Count));
            Assert.That("!", Is.EqualTo(lst[2].getName()));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        /**
		 * 挿入のテスト
		 **/
        [Test]
        public void taglist_09()
        {
            string buf = "bbb<t-code src=\"aaa\">ccc";
            string exp = "bbb<xxx><t-code src=\"aaa\">ccc";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            lst.Insert(1, new TagText("<xxx>", 0));   //追加
            Assert.That(4, Is.EqualTo(lst.Count));
            Assert.That(exp, Is.EqualTo(lst.ToString()));
            lst.RemoveAt(1);                       //削除
            Assert.That(3, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_10()
        {
            string buf = "気分<!--ス字 [なし] /-->になる。";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false, ref ama);
            Assert.That(3, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_11()
        {
            string buf = "気分[&C9999]になる";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, true/*bNameSP*/, ref ama);
            Assert.That(3, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_11b()
        {
            string buf = "気分[&C9999]になる";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false/*bNameSP*/, ref ama);
            Assert.That(1, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_12()
        {
            string buf = "気分&#x9999;になる";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, true/*bNameSP*/, ref ama);
            Assert.That(3, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_12b()
        {
            string buf = "気分&#x9999;になる";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, false/*bNameSP*/, ref ama);
            Assert.That(1, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_13()
        {
            string buf = "気分&#x9999;a[&C9999]になる";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, true/*bNameSP*/, ref ama);
            Assert.That(5, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_14()
        {
            string buf = "気分&#x9999;[&C9999]になる";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, true/*bNameSP*/, ref ama);
            Assert.That(buf, Is.EqualTo(lst.ToString()));
            Assert.That(4, Is.EqualTo(lst.Count));

        }
        [Test]
        public void taglist_15()
        {
            string buf = "&#x9999;気分&#x9999;[&C9999]になる[&C9999]";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, true/*bNameSP*/, ref ama);
            Assert.That(6, Is.EqualTo(lst.Count));
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_16()
        {
            string buf = "気分&#x9999;[&C9999]になる";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, true/*bNameSP*/, ref ama);
            Assert.That(buf, Is.EqualTo(lst.ToString()));
            Assert.That(4, Is.EqualTo(lst.Count));

            TagText utf = (TagText)lst[1];
            Assert.That(true, Is.EqualTo(utf.isUTF()), "UTFでない");
            Assert.That(false, Is.EqualTo(utf.isCID()), "CIDでない");
            Assert.That("9999", Is.EqualTo(utf.getUTFCode()));

            TagText cid = (TagText)lst[2];
            Assert.That(true, Is.EqualTo(cid.isCID()), "CIDでない");
            Assert.That(9999, Is.EqualTo(cid.getCID()), "CIDが取得できない " + cid.ToString());
        }
        [Test]
        public void taglist_17()
        {
            string buf = "気分&#x9999;[&H3f3f]になる";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, true, ref ama);
            TagText txt = (TagText)lst[2];
            Assert.That(true, Is.EqualTo(txt.isNamespace()), "Namespaceでない");
            Assert.That(true, Is.EqualTo(txt.isHojo()), "HOJOでない");
            Assert.That(false, Is.EqualTo(txt.isCID()), "CIDでない");
            Assert.That("3f3f", Is.EqualTo(txt.getHojoCode()), "HOJOが取得できない " + txt.ToString());
        }
        [Test]
        public void taglist_18()
        {
            string buf = "八代<図形 TYPE=\"09\"/><a>女</a><図形/>王";
            string exp = "八代<図形 TYPE=\"09\" /><a>女</a><図形 />王";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, true, ref ama);
            Assert.That(exp, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_19()
        {
            string buf = "<柱文 \"TEST\">";
            TagList lst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref lst, true, ref ama);
            Assert.That(buf, Is.EqualTo(lst.ToString()));
        }
        [Test]
        public void taglist_20()
        {
            string buf = "あああ（いいい）ううう「えええ」おおお";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseTextKugiriMulti(buf, 0, ref taglst, "（「", "）」", false, ref ama);
            Assert.That(5, Is.EqualTo(taglst.Count));
            string res = "あああ（いいい）ううう「えええ」おおお";
            Assert.That(res, Is.EqualTo(taglst.ToString()));
            Assert.That("あああ", Is.EqualTo(taglst[0].ToString()), "token=" + taglst[0] + " istag=" + taglst[0].isTag() + " istext=" + taglst[0].isText());
            Assert.That("（いいい）", Is.EqualTo(taglst[1].ToString()));
        }
        [Test]
        public void taglist_21()
        {
            string buf = "あああ（「いいい」）ううう";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseTextKugiriMulti(buf, 0, ref taglst, "（「", "）」", false, ref ama);
            Assert.That(3, Is.EqualTo(taglst.Count));
            string res = "あああ（「いいい」）ううう";
            Assert.That(res, Is.EqualTo(taglst.ToString()));
            Assert.That("あああ", Is.EqualTo(taglst[0].ToString()), "token=" + taglst[0] + " istag=" + taglst[0].isTag() + " istext=" + taglst[0].isText());
            Assert.That("（「いいい」）", Is.EqualTo(taglst[1].ToString()));
        }
        [Test]
        public void taglist_22()
        {
            string buf = "あああ（「いいい」えええ）ううう";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseTextKugiriMulti(buf, 0, ref taglst, "（「", "）」", false, ref ama);
            Assert.That(3, Is.EqualTo(taglst.Count));
            string res = "あああ（「いいい」えええ）ううう";
            Assert.That(res, Is.EqualTo(taglst.ToString()));
            Assert.That("あああ", Is.EqualTo(taglst[0].ToString()), "token=" + taglst[0] + " istag=" + taglst[0].isTag() + " istext=" + taglst[0].isText());
            Assert.That("（「いいい」えええ）", Is.EqualTo(taglst[1].ToString()));
        }
        [Test]
        public void taglist_23()
        {
            string buf = "あああ<ruby>いいい<rt>ううう</rt></ruby>えええ";
            TagList taglst = new TagList();
            string rep = "あああいいいえええ";
            Assert.That(rep, Is.EqualTo(TagTextUtil.getOyaTextFromRubyTag(buf)));
        }
        [Test]
        public void taglist_24()
        {
            string buf = "あああ（いいい（ううう）えええ）おおお";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseTextKugiriMulti(buf, 0, ref taglst, "（", "）", false, ref ama);
            string exp = "あああ（いいい（ううう）えええ）おおお";
            Assert.That(exp, Is.EqualTo(taglst.ToString()));
        }
        [Test]
        public void taglist_24a()
        {
            string buf = "＜添＞や＜Ｌ＞春上＜/添＞＜添＞ま＜*＞谷イ＜/添＞かせに/とくるこほりの/ひまことに/うちいつるなみや/春のはつ花";
            //string buf = "＜添＞や";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref taglst, '＜', '＞', false, ref ama);
            //string exp = "あああ（いいい（ううう）えええ）おおお";
            Assert.That(buf, Is.EqualTo(taglst.ToString()));
        }


        [Test]
        public void taglist_25()
        {
            string buf = "家集に他撰の『<参照 ID=#遍昭（照）集>遍昭集</参照>』。歌風は";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref taglst, '<', '>', false, ref ama);
            Assert.That("", Is.EqualTo( ama));

            string exp = buf;
            Assert.That(buf, Is.EqualTo(taglst.ToString()));
        }
        [Test]
        public void taglist_26()
        {
            string buf = "<td style='padding:0mm 4.95pt 0mm 4.95pt'>";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref taglst, '<', '>', false, ref ama);
            string exp = buf;
            Assert.That(buf, Is.EqualTo(taglst.ToString()));
        }
        [Test]
        public void taglist_26a()
        {
            string buf = "<td style='padding:0mm 4.95pt 0mm 4.95pt'>";
            string path = FileUtil.writeTempFile(buf, Encoding.UTF8);

            TagList taglst = new TagList();
            TagTextUtil util = new TagTextUtil(_logger);
            util.parseTextFromPath(path, Encoding.UTF8, ref taglst, false);
            string exp = buf;
            Assert.That(buf, Is.EqualTo(taglst.ToString()));
        }
        [Test]
        public void taglist_26b()
        {
            string buf = "<span style='font-size:14.0pt;font-family:\"ＭＳ ゴシック\"'>";
            string path = FileUtil.writeTempFile(buf, Encoding.UTF8);

            TagList taglst = new TagList();
            
            TagTextUtil util = new TagTextUtil(_logger);
            util.parseTextFromPath(path, Encoding.UTF8, ref taglst, false);
            string exp = buf;
            Assert.That(buf, Is.EqualTo(taglst.ToString()));
        }
        [Test]
        public void taglist_27()
        {
            string buf = "<柱文 \"首の姫と首なし騎士\" />";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref taglst, '<', '>', false, ref ama);
            string exp = buf;
            Assert.That(buf, Is.EqualTo(taglst.ToString()));
        }
        [Test]
        public void taglist_28()
        {
            string buf = "<画像 p001.jpg alt=\"text\">";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref taglst, '<', '>', false, ref ama);
            string exp = buf;
            Assert.That(buf, Is.EqualTo(taglst.ToString()));
        }
        [Test]
        public void taglist_29()
        {
            string buf = "<画像 p001.jpg alt=\"te xt\">";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref taglst, '<', '>', false, ref ama);
            string exp = buf;
            Assert.That(buf, Is.EqualTo(taglst.ToString()));
        }
        /**
		 * XMLをTREE構造で読み込み
		 * */
        [Test]
        public void taglist_TREE構造で読み込み()
        {
            string buf = "<p><span>aaa</span></p>\r\n";
            TagBase root = new TagItem("root", false, '<', '>', 0);
            TagTextUtil util = new TagTextUtil(_logger);

            string path = FileUtil.writeTempFile(buf, Encoding.UTF8);
            util.parseTextTreeFromXmlPath(path, Encoding.UTF8, ref root);
            string exp = buf;
            Assert.That(buf, Is.EqualTo(root.ChildList.ToString()));

            TagBase para = root.ChildList[0];  //<p>
            Assert.That("<span>aaa</span>", Is.EqualTo(para.getChildText()));
            Assert.That("aaa", Is.EqualTo(para.getChildTextOnly()));
        }
        [Test]
        public void taglist_31()
        {
            string buf = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref taglst, '<', '>', false, ref ama);
            string exp = buf;
            Assert.That(buf, Is.EqualTo(taglst.ToString()));
            Assert.That(TagBase.TYPE.Hatena, Is.EqualTo(taglst[0].Type));
        }
        [Test]
        public void taglist_32()
        {
            string buf = "<span style=\"font-size:14.0pt;font-family:'ＭＳ ゴシック'\"></span>	\r\n";
            TagBase root = new TagItem("root", false, '<', '>', 0);
            TagTextUtil util = new TagTextUtil(_logger);

            string path = FileUtil.writeTempFile(buf, Encoding.UTF8);
            util.parseTextTreeFromXmlPath(path, Encoding.UTF8, ref root);
            string exp = buf;
            Assert.That(buf, Is.EqualTo(root.ChildList.ToString()));
        }
        [Test]
        public void taglist_33()
        {
            string buf = "<p class=MsoNormal align=center style='text-align:center;text-autospace:none'><span\r\n" +
      "lang=EN-US><!--[if gte vml 1]><v:shape id=\"_x0000_i1025\" style='width:310.5pt;\r\n" +
      " height:76.5pt;visibility:visible;mso-wrap-style:square' coordsize=\"\" o:spt=\"100\"\r\n" +
      " adj=\"0,,0\" path=\"\" stroked=\"f\">";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref taglst, false, ref ama);
            string exp = buf;
            Assert.That(4, Is.EqualTo(taglst.Count));
            Assert.That("p", Is.EqualTo(taglst[0].getName()));
            Assert.That("span", Is.EqualTo(taglst[1].getName()));
            Assert.That(true, Is.EqualTo(taglst[2].isComment()));
            Assert.That("v:shape", Is.EqualTo(taglst[3].getName()));

        }
        [Test]
        public void taglist_TagListからStringList作成01()
        {
            string buf = "あ\r\nい<b>\r\nうえ";
            TagList taglst = new TagList();
            string ama = "";
            TagTextUtil.parseText(buf, ref taglst, false, ref ama);
            List<string> strlst = new List<string>();
            TagTextUtil.createListFromTagList(taglst, ref strlst);
            Assert.That(3, Is.EqualTo( strlst.Count));
            Assert.That("あ\r\n", Is.EqualTo( strlst[0]));
            Assert.That("い<b>\r\n", Is.EqualTo( strlst[1]));
            Assert.That("うえ", Is.EqualTo(strlst[2]));
        }
        [Test]
        public void taglist_stringからStringList作成01()
        {
            string buf = "あ\r\nい<b>\r\nうえ";
            List<string> strlst = new List<string>();
            TagTextUtil.createListFromText(buf, ref strlst);
            Assert.That(3, Is.EqualTo(strlst.Count));
            Assert.That("あ\r\n", Is.EqualTo(strlst[0]));
            Assert.That("い<b>\r\n", Is.EqualTo(strlst[1]));
            Assert.That("うえ", Is.EqualTo(strlst[2]));
        }
        [Test]
        public void taglist_iscomment()
        {
            TagList taglst = TagTextUtil.parseText("あああ<!--[if gte vml 1]>xxxx<![endif]-->いいい");
            Assert.That(5, Is.EqualTo(taglst.Count));
            Assert.That(true, Is.EqualTo(taglst[0].isText()));
            Assert.That(true, Is.EqualTo(taglst[1].isComment()));
            Assert.That(true, Is.EqualTo(taglst[2].isText()));
        }

    }
}
