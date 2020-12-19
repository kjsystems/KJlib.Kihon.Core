using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KJlib.Kihon.Core.Models;
using KJlib.Kihon.Core.Services;
using Microsoft.Extensions.Logging;

namespace kjlib.lib.Models
{
    public class IniFileWriter
    {
        private readonly ILogger _logger;

        public IniFileWriter(ILogger logger)
        {
            _logger = logger;
        }
        /**
		 * 
		 * */
        public void writeFile(string path, IniGroups grps, Encoding enc)
        {
            var wr = new System.IO.StreamWriter(path, false, enc);
            wr.Write(GetText(grps));
            wr.Close();
        }

        private string GetText(IniGroups grps)
        {
            var sb = new StringBuilder();
            foreach (var gr in grps)
            {
                sb.Append(gr.ToString());
            }

            return sb.ToString();
        }

        /**
		 * 追加する
		 * */
        public void writeAddFile(string path, IniGroup grp, Encoding enc)
        {
            Console.WriteLine("IniFile writeAddFile 1 path={0}", path);

            //読み込んで追加して書き出し
            IniFileReader rd = new IniFileReader(_logger);
            IniGroups grps = new IniGroups();
            if (System.IO.File.Exists(path) == true)
            {
                Console.WriteLine("IniFile writeAddFile 2 read path={0}", path);
                rd.readFile(path, enc , ref grps);
                grps.addGroupData(grp);  //重複チェック
            }
            writeFile(path, grps, enc);
            Console.WriteLine("IniFile writeAddFile Z");
        }
    }
}
