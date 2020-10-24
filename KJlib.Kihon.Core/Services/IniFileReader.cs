using System;
using System.IO;
using System.Text;
using KJlib.Kihon.Core.Models;
using kjlib.lib.Models;
using Microsoft.Extensions.Logging;

namespace KJlib.Kihon.Core.Services
{
    public class IniFileReader
    {
        private readonly ILogger _logger;

        public IniFileReader(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Streamからinifileを読む
        /// Logger不要
        /// </summary>
        /// <param name="st"></param>
        /// <param name="enc"></param>
        /// <param name="bEmptyOK"></param>
        /// <param name="ini"></param>
        /// <returns></returns>
        public static void ReadFromStream(Stream st, Encoding enc, out IniGroups ini)
        {
            ini = new IniGroups();

            var sr = new StreamReader(st, enc);
            string buf = null;
            var gyono = 0;

            while ((buf = sr.ReadLine()) != null)
            {
                try
                {
                    gyono++;
                    if (string.IsNullOrEmpty(buf) || buf[0] == '*') continue;

                    if (buf[0] == '[' && buf[buf.Length - 1] == ']')
                    {
                        string name = buf.Substring(1, buf.Length - 2);
                        ini.Add(new IniGroup(name, gyono));
                    }
                    else
                    {
                        var idx = buf.IndexOf("=", StringComparison.Ordinal);
                        if (idx < 0) throw new Exception("readFile 名前=値になっていません [" + buf + "]");
                        ini.addItem(buf.Substring(0, idx), buf.Substring(idx + 1));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("readFile " + ex.Message);
                }
            }
        }

        public Constant.ErrCode readFile(string path, Encoding enc, ref IniGroups ini)
        {
            Constant.ErrCode rt = Constant.ErrCode.hFailure;
            do
            {
                /*ini.Clear();*/
                if (System.IO.File.Exists(path) != true)
                {
                    _logger.LogError($"{path} read_ini ファイルがありません");
                    break;
                }
                System.IO.StreamReader sr = new System.IO.StreamReader(path, enc);
                string buf = null;
                int gyono = 0;
                while ((buf = sr.ReadLine()) != null)
                {
                    try
                    {
                        gyono++;
                        if (string.IsNullOrEmpty(buf) || buf[0] == '*') continue;
                        if (buf[0] == '[' && buf[buf.Length - 1] == ']')
                        {
                            string name = buf.Substring(1, buf.Length - 2);
                            ini.Add(new IniGroup(name, gyono));
                        }
                        else
                        {
                            int idx = buf.IndexOf("=");
                            if (idx < 0) throw new Exception("名前=値になっていません [" + buf + "]");
                            //Tokens tokens = new Tokens(buf, new char[] { '=' });
                            //ini.addItem(tokens[0].Trim(), buf.Substring(tokens[0].Length + 1));
                            ini.addItem(buf.Substring(0, idx), buf.Substring(idx + 1));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"{path} read_ini {ex.Message}");
                    }
                }
                sr.Close();
                rt = Constant.ErrCode.hSuccess;
            } while (false);
            return rt;
        }
    }
}
