using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using kjlib.kihon.Extensions;

namespace kjlib.kihon.Models
{
    public class FileUtil
    {

        [DllImport("mpr.dll")]
        static extern int WNetGetConnection(string strLocalName, StringBuilder strbldRemoteName,
            ref int intRemoteNameLength); // ref <--- important

        public static void deleteFile(string path)
        {
            if (System.IO.File.Exists(path) == true)
            {
                System.IO.File.Delete(path);
            }
        }

        public static string getTextFromTextList(List<string> lst)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string buf in lst) sb.Append(buf);
            return sb.ToString();
        }

        /// <summary>
        /// タグ一覧から<改行>リストを作成する
        /// </summary>
        /// <param name="taglst"></param>
        /// <param name="strlst"></param>
        public void CreateKaigyoList(TagList taglst, ref List<string> strlst)
        {
            var cur = "";
            foreach (var tag in taglst)
            {
                if (tag.isTag() == true)
                {
                    if (Array.IndexOf(new[] { "改行", "改頁" }, tag.getName()) >= 0)
                    {
                        cur += tag.ToString();
                        strlst.Add(cur);
                        cur = "";
                        continue;
                    }

                    if (tag.getName() == "見出")
                    {
                        // それまでを追加する <見出>は追加しない
                        if (tag.isOpen())
                        {
                            // それあでそれまで空でなければ
                            if (!string.IsNullOrEmpty(cur))
                            {
                                strlst.Add(cur);
                            }
                            cur = "";
                            cur += tag.ToString();
                        }
                        else
                        {
                            // 閉じるときに追加する
                            cur += tag.ToString();
                            strlst.Add(cur);
                            cur = "";
                        }

                        continue;
                    }
                }
                cur += tag.ToString();
            }

            if (!string.IsNullOrEmpty(cur))
            {
                strlst.Add(cur);
            }
        }

        /**
         * tt *.jpg;*.gif
         * */
        static public List<string> getFilesFromDir(string dir, string tgt_multi, bool bOutException)
        {
            List<string> lst = new List<string>();
            Tokens tokens = new Tokens(tgt_multi, new char[] { ';' });
            foreach (string tgt in tokens)
            {
                string[] filelst = System.IO.Directory.GetFiles(dir, tgt);
                lst.AddRange(filelst);
            }
            if (bOutException == true && lst.Count == 0)
                throw new Exception("ファイルが１つもない dir=" + dir + " tgt=" + tgt_multi);
            return lst;
        }

        static public List<string> getFileListFromDir(string dir)
        {
            return getFileListFromDir(dir, "*.*");
        }

        static public List<string> getFileListFromDir(string dir, string tgt)
        {
            string[] filelst = System.IO.Directory.GetFiles(dir, tgt);
            return new List<string>(filelst);
        }

        /**
         * サブフォルダ一覧
         * */
        static public List<string> getSubDirList(string dir, bool bToLower)
        {
            string[] dirlst = System.IO.Directory.GetDirectories(dir);
            List<string> lst = new List<string>();
            if (dirlst.Length > 0)
            {
                //lst.AddRange(dirlst);
                foreach (string path in dirlst)
                {
                    if (bToLower == true) lst.Add(path.ToLower());
                    else lst.Add(path);
                }
            }
            return lst;
        }

        static public string createdir(string dir, string sub)
        {
            string tgtdir = System.IO.Path.Combine(dir, sub);
            if (System.IO.Directory.Exists(tgtdir) != true)
            {
                System.IO.Directory.CreateDirectory(tgtdir);
            }
            return tgtdir;
        }

        static public string getExt(string path)
        {
            return System.IO.Path.GetExtension(path);
        }

        static public string getFileName(string path)
        {
            return System.IO.Path.GetFileName(path);
        }

        static public string getFileNameWithoutExtension(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        static public string Desktop
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); }
        }

        static public string MyDocuments
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); }
        }

        static public string addYen(string dir)
        {
            try
            {
                if (dir.Length > 0 && dir.Substring(0, dir.Length - 1) != "\\")
                    return dir + "\\";
            }
            catch (Exception)
            {
                return "";
            }
            return dir;
        }

        static public string getUpDir(string dir)
        {
            StringBuilder sb = new StringBuilder();
            string cur = "";
            for (int m = 0; m < dir.Length; m++)
            {
                char ch = dir[m];
                cur += ch;
                if (ch == '\\')
                {
                    //最後が\\なら追加しないでおしまい
                    if (m + 1 == dir.Length) break;
                    sb.Append(cur);
                    cur = "";
                    continue;
                }
            }
            return sb.ToString();


            //return dir.Substring(0, dir.Length - FileUtil.getLastDirName(dir).Length);
        }

        /**
         * 無効な文字列がなければtrue
         * */
        static public bool isValidPathChar(string path)
        {
            //ファイル名に使用できない文字を取得
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            if (path.IndexOfAny(invalidChars) < 0)
            {
                return true;
            }
            {
#if BUG_PROG
				Console.WriteLine("KJEditSet isValidPathChar 1 PATHに無効な文字列あり path=[{0}]", path);
#endif
            }
            return false;
        }

        /**
         * 拡張子を変更する
         * */
        static public string changeExt(string path, string newext)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            string fname = System.IO.Path.GetFileNameWithoutExtension(path);
            return dir + "\\" + fname + newext;
        }

        /**
         * ファイルから文字列を取得 
         **/
        static public string getTextFromPath(string path, Encoding enc)
        {
            string res = "";
            getTextFromPath(path, enc, ref res);
            return res;
        }

        /**
         * すべてつなげる
         * */
        public static void getTextFromPath(string path, Encoding enc, ref string res)
        {
            StringBuilder sb = new StringBuilder();
            if (System.IO.File.Exists(path) != true)
            {
                throw new Exception("読み込むファイルがありません PATH=" + path);
            }
            using (System.IO.StreamReader sr = new System.IO.StreamReader(path, enc))
            {
                res = sr.ReadToEnd();
                sr.Close();
            }
        }

        /**
         * 基本は\r\nで、タグの途中で折り返しがある場合はタグが閉じるまでをまとめて文字列で取得
         * */
        static public void getTextListWithTagSetFromPath(string path, Encoding enc, ref List<string> reslst)
        {
            if (System.IO.File.Exists(path) != true)
            {
                throw new Exception("読み込むファイルがありません PATH=" + path);
            }
            System.IO.StreamReader sr = new System.IO.StreamReader(path, enc);
            string buf = null;
            string amaritxt = "";
            int gyono = 0;
            while ((buf = sr.ReadLine()) != null)
            {
                gyono++;
#if BUG_PROG
					Console.WriteLine("getTextListWithTagSetFromPath ["+buf+"]");
#endif
                //タグが途中で改行があれば次の行とつなげる
                buf = amaritxt + buf;
                TagList taglst = new TagList();
                TagTextUtil.parseText(buf, ref gyono, ref taglst, false, ref amaritxt); //amaritxtは中でリセットされる
                if (amaritxt.Length > 0)
                {
                    amaritxt += "\r\n";
                    continue;
                }
                reslst.Add(buf);
            }
            sr.Close();
            if (reslst.Count == 0)
            {
                throw new Exception("データがありません PATH=" + path);
            }
        }

        static public List<string> getTextListFromPath(string path, Encoding enc)
        {
            List<string> lst = new List<string>();
            FileUtil.getTextListFromPath(path, enc, ref lst);
            return lst;
        }

        /**
            *   *無視    ⇔ getTextListFromPath
            */
        static public List<string> getTextListFromPathNoKome(string path, Encoding enc)
        {
            List<string> strlst = new List<string>();
            getTextListFromPath(path, enc, ref strlst);

            return strlst.Where(v => (!string.IsNullOrEmpty(v) && v[0] != '*'))
                .ToList();
        }

        /**
         * 
         * */
        static public void getTextListFromPath(string path, Encoding enc, ref List<string> reslst)
        {
            if (System.IO.File.Exists(path) != true)
            {
                throw new Exception("読み込むファイルがありません PATH=" + path);
            }
            System.IO.StreamReader sr = new System.IO.StreamReader(path, enc);
            string buf = null;
            while ((buf = sr.ReadLine()) != null)
            {
                reslst.Add(buf);
            }
            sr.Close();
            if (reslst.Count == 0)
            {
                throw new Exception("データがありません PATH=" + path);
            }
        }

        static public string getUNCPathFromPath(string path)
        {
            string unc = FileUtil.getUNCPathFromDrive(path.Substring(0, 2)); // m: ==> \\KJSERVER-YN89P6\mdrive
            string resultPath = unc + path.Substring(2);
            return resultPath;
        }

        /**
         *   m: ==> \\KJSERVER-YN89P6
         * */
        static public string getUNCPathFromDrive(string drive)
        {
            StringBuilder strbldRemoteName = new StringBuilder();
            int intRemoteNameLength = 1024; // Set the max. number of char <--- important
            strbldRemoteName.Capacity = intRemoteNameLength;
            WNetGetConnection(drive, strbldRemoteName, ref intRemoteNameLength);
            return strbldRemoteName.ToString();
        }

        /**
         * 
         * */
        static public string writeTempFile(string buf, Encoding enc)
        {
            string path = System.IO.Path.GetTempFileName();
            writeTextToFile(buf, enc, path);
            return path;
        }

        public static void writeTextToFileUtf8NoBom(string buf, string path)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false)) //Encodingを省略でBOMなし
            {
                sw.Write(buf);
                sw.Close();
            }
        }

        static public void writeTextToFile(List<string> strlst, Encoding enc, string path, bool bAdd = false)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string gyo in strlst) sb.AppendLine(gyo);
            writeTextToFile(sb.ToString(), enc, path, bAdd);
        }

        /// <summary>
        /// オブジェクトの内容をファイルに保存する
        /// </summary>
        /// <param name="obj">保存するオブジェクト</param>
        /// <param name="path">保存先のファイル名</param>
        public static void writeToBinaryFile(byte[] bytes, string path)
        {
            /*
            FileStream fs = new FileStream(path,
                    FileMode.Create,
                    FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            //シリアル化して書き込む
            bf.Serialize(fs, obj);
            fs.Close();
             * 
             **/
            File.WriteAllBytes(path, bytes);
        }

        /**
         * 
         * */
        public static void writeTextToFile(string buf, Encoding enc, string path, bool bAppend = false)
        {
            try
            {
                string dir = System.IO.Path.GetDirectoryName(path);
                if (System.IO.Directory.Exists(dir) != true)
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path, bAppend /*false*/, enc))
                {
                    sw.Write(buf);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ファイルをオープンできない？ msg=" + ex.Message + " path=" + path);
            }
        }

        /**
         * 
         * */
        static public void getFilesFromPath(string path, ref List<string> lst)
        {
            Console.WriteLine("fileutil getfiles path={0}", path);
            if (path.IndexOf("*") >= 0)
            {
                //分解する
                string dir = System.IO.Path.GetDirectoryName(path);
                string fname = System.IO.Path.GetFileName(path);
                //Console.WriteLine("fileutil getfiles dir="+dir+" fname="+fname);
                Console.WriteLine("fileutil getfiles 2 dir={0} fname={1}", dir, fname);
                string[] files = System.IO.Directory.GetFiles(dir, fname);
                lst.AddRange(files);
            }
            else
            {
                if (System.IO.Directory.Exists(path) == true)
                {
                    string[] dirlst = System.IO.Directory.GetDirectories(path);
                    foreach (string dir in dirlst)
                    {
                        getFilesFromPath(dir, ref lst);
                    }
                    string[] filelst = System.IO.Directory.GetFiles(path);
                    foreach (string f in filelst)
                    {
                        lst.Add(f);
                    }
                }
                else if (System.IO.File.Exists(path) == true) lst.Add(path);
            }
        }

        /**
         * *.txtならファイル数
             フルパスなら単純にExist
         * */
        static public int getFiles(string path)
        {
            int fileCount = 0;
            if (path.IndexOf("*") >= 0)
            {
                //分解する
                string dir = System.IO.Path.GetDirectoryName(path);
                string fname = System.IO.Path.GetFileName(path);
                string[] files = System.IO.Directory.GetFiles(dir, fname);
                fileCount = files.Length;
            }
            else
            {
                if (System.IO.File.Exists(path) == true) fileCount = 1;
            }
            return fileCount;
        }

        //最後の\を削除
        static public string delLastYen(string path)
        {
            if (path.Length > 0 &&
                path.Substring(path.Length - 1, 1) == @"\\")
            {
                return path.Substring(0, path.Length - 1);
            }
            return path;
        }

        /**
         * １つ上のディレクトリ
         * */
        static public string getParentDir(string dir)
        {
            System.IO.DirectoryInfo i = System.IO.Directory.GetParent(dir);
            if (i == null) return "";
            return i.ToString();
        }

        /** ディレクトリの最後の部分を取得する
         * 
         * */
        static public string getLastDirName(string dir)
        {
            //ファイルならDIR
            if (System.IO.File.Exists(dir) == true)
            {
                dir = System.IO.Path.GetDirectoryName(dir);
            }
            if (System.IO.Directory.Exists(dir) != true) return "";
            //最後の"\"は削除
            string tempDir = delLastYen(dir);
            string lastDir = "";
            for (int m = 0; m < tempDir.Length; m++)
            {
                char ch = tempDir[m];
                if (ch == '\\')
                {
                    if (m == tempDir.Length - 1) break;
                    lastDir = ""; //最後でなければ
                }
                else lastDir += ch;
            }
            return lastDir;
        }
#if false
        static public string getExePath(string exename)
        {
            System.Text.StringBuilder path = new System.Text.StringBuilder();
            path.Append(Application.StartupPath);
            path.Append(@"\");
            path.Append(exename);
            return path.ToString();
        }
#endif
        public static string getTempDir()
        {
            return System.IO.Path.GetTempPath();
        }

        public static string getTempDirKJ()
        {
            string tempdir = System.IO.Path.GetTempPath().Combine("kj");
            tempdir.CreateDirIfNotExist();
            return tempdir;
        }

        public static string getTempPath(string fname)
        {
            var path = System.IO.Path.GetTempFileName();
            return System.IO.Path.GetDirectoryName(path).Combine(System.IO.Path.GetFileName(fname));
        }
    }
}
