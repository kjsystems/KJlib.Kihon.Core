using System;
using System.IO;
using System.Text;
using KJlib.Kihon.Core.Models;

namespace KJlib.Kihon.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToHankaku(this string source)
        {
            return ZenHanUtil.ConvertTo(source, ZenHanUtil.Convert.Zen2Han);
        }
        public static string ToZenkaku(this string source)
        {
            return ZenHanUtil.ConvertTo(source, ZenHanUtil.Convert.Han2Zen);
        }

        //最後の\を削除
        public static string DelLastYen(this string source)
        {
            if (source.Length > 0 &&
                source.Substring(source.Length - 1, 1) == @"\\")
            {
                return source.Substring(0, source.Length - 1);
            }
            return source;
        }

        public static string GetUpDir(this string source)
        {
            var sb = new StringBuilder();
            var cur = "";
            for (int m = 0; m < source.Length; m++)
            {
                char ch = source[m];
                cur += ch;
                if (ch == '\\')
                {
                    //最後が\\なら追加しないでおしまい
                    if (m + 1 == source.Length) break;
                    sb.Append(cur);
                    cur = "";
                    continue;
                }
            }
            return sb.ToString();
        }

        public static string GetLastDirName(this string source)
        {
            //ファイルならDIR
            if (System.IO.File.Exists(source) == true)
            {
                source = System.IO.Path.GetDirectoryName(source);
            }
            if (System.IO.Directory.Exists(source) != true) return "";
            //最後の"\"は削除
            var tempDir = DelLastYen(source);
            var lastDir = "";
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

        /// <summary>
        /// ファイルの有無を調べる
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bWithException">ファイルがない場合に例外を出力する場合はtrue</param>
        /// <returns></returns>
        public static bool ExistsFile(this string path, bool bWithException = false)
        {
            var isExist = File.Exists(path);
            if (!isExist && bWithException)
            {
                throw new FileNotFoundException($"ファイルがありません PATH={path}");
            }

            return isExist;
        }

        /// <summary>
        /// ディレクトリの有無を調べる
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bWithException">ディレクトリがない場合に例外を出力する場合はtrue</param>
        /// <returns></returns>
        public static bool ExistsDir(this string path, bool bWithException = false)
        {
            var isExist =  Directory.Exists(path);
            if (!isExist && bWithException)
            {
                throw new FileNotFoundException($"ディレクトリがありません PATH={path}");
            }

            return isExist;
        }

        public static string[] GetFiles(this string dir, string searchPattern, bool bThrowException = true)
        {
            return System.IO.Directory.GetFiles(dir, searchPattern);
        }

        public static string[] GetDirectories(this string source)
        {
            return System.IO.Directory.GetDirectories(source);
        }

        public static string GetFileName(this string source)
        {
            return System.IO.Path.GetFileName(source);
        }

        public static string GetFileNameWithoutExtension(this string source)
        {
            return System.IO.Path.GetFileNameWithoutExtension(source);
        }

        public static string GetExtension(this string source)
        {
            return System.IO.Path.GetExtension(source);
        }

        public static string GetDirectoryName(this string source)
        {
            return System.IO.Path.GetDirectoryName(source);
        }

        public static string Combine(this string source, string subdir)
        {
            return System.IO.Path.Combine(source, subdir);
        }
        public static string CreateDirIfNotExist(this string source, bool bClearFiles = false)
        {
            //あれば削除
            if (!string.IsNullOrEmpty(source) && System.IO.Directory.Exists(source) == true && bClearFiles == true)
            {
                foreach (var path in source.GetFiles("*.*", false))
                {
                    System.IO.File.Delete(path);
                }
            }

            if (!string.IsNullOrEmpty(source) && System.IO.Directory.Exists(source) != true)
            {
                System.IO.Directory.CreateDirectory(source);
            }

            return source;
        }

        public static int ToInt(this string source, int def)
        {
            int res = def;
            if (source == null)
            {
                return def;
            }

            if (int.TryParse(source, out res))
            {
                return res;
            }

            return def;
        }
        public static float ToFloat(this string source, float def)
        {
            float res = def;
            if (source == null)
            {
                return def;
            }

            if (float.TryParse(source, out res))
            {
                return res;
            }

            return def;
        }

        public static string DelKaigyo(this string buf)
        {
            StringBuilder sb = new StringBuilder();

            char[] tbl = { '\t', '\r', '\n' };
            foreach (char ch in buf)
            {
                if (Array.IndexOf(tbl, ch) >= 0) continue;
                sb.Append(ch);
            }

            return sb.ToString();
        }

        public static void DeleteFileIfExist(this string source)
        {
            if (System.IO.File.Exists(source) == true)
            {
                System.IO.File.Delete(source);
            }
        }
    }
}