using System;
using System.Collections.Generic;
using System.Text;
using KJlib.Kihon.Core.Models;
using Microsoft.Extensions.Logging;

namespace KJlib.Kihon.Core.Services
{
    public class CSVRow : List<string>
    {
        CSVFields fieldsPtr_;
        public CSVFields FieldsPtr { set { fieldsPtr_ = value; } get { return fieldsPtr_; } }

        int gyono_ = 0;
        public int GyoNo { get { return gyono_; } set { gyono_ = value; } }
        public CSVRow(Tokens tokens, CSVFields fldsptr, int gyono)
        {
            fieldsPtr_ = fldsptr;
            gyono_ = gyono;
            foreach (string token in tokens)
            {
                this.Add(token);
            }
        }
        public CSVRow(int gyono, CSVFields fldsptr)
        {
            gyono_ = gyono;
            FieldsPtr = fldsptr;
        }

        public string toString(string sep)
        {
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < this.Count; m++)
            {
                if (m != 0) sb.Append(sep);
                sb.Append(this[m]);
            }
            return sb.ToString();
        }
        /**
		 * 挿入する 
		 * */
        public void addCell(int col, string buf)
        {
            //１つずらす
            int num = this.Count;
            for (int m = num - 1; m >= col; m--)  //後ろから
            {
                int newcol = m + 1;
                string v = this[m];
                setCell(newcol, v);
            }
            //追加するデータをセットする
            setCell(col, buf);
        }
        /**
		 * 
		 * */
        public override string ToString()
        {
            return toString(",");
        }
        new public string this[int colno]
        {
            get { return getCell(colno); }
            set { setCell(colno, value); }
        }

        public string this[string fldname]
        {
            get
            {
                if (FieldsPtr == null)
                    throw new Exception("RowとFieldsのリンクがない(CSVRow)");
                int fldno = FieldsPtr.getColIndex(fldname);
                if (fldno < 0) throw new Exception("Fieldsにフィールド名がない(CSVRow) " + fldname);
                return this.getCell(fldno);
            }
            /*		  set   //動作未確認
                      {
                    if (FieldsPtr == null)
                      throw new Exception("RowとFieldsのリンクがない(CSVRow)");
                    int fldno = FieldsPtr.getColIndex(fldname);
                    if (fldno < 0)
                      throw new Exception("Fieldsにフィールド名がない(CSVRow) " + fldname);
                    setCell(fldno,value);
                  }*/
        }

        void setCell(int colno, string buf)
        {
            //足りない場合は追加
            while (colno >= this.Count) { this.Add(""); }
            base[colno] = buf;
        }
        public string getCell(CSVFields flds, string fldname)
        {
            int colno = flds.getColIndex(fldname);
            if (colno < 0) throw new Exception("フィールドがない FIELD=" + fldname);
            return getCell(colno);
        }

        /**
         * alpha : Ecel列のアルファベットで指定する a-z
         * */
        public string getCellExcelColumn(char alpha)
        {
            int colno = alpha.ToString().ToLower()[0] - 'a';
            return getCell(colno);
        }
        public string getCell(int colno)
        {
            try
            {
                if (colno >= this.Count) return "";
                return base[colno];
            }
            catch (Exception)
            {
                //no error
            }
            return "";
        }
    }
    /**
	 * フィールド一覧
	 * */
    public class CSVFields : List<string>
    {
        public void copy(Tokens tokens)
        {
            foreach (string token in tokens)
            {
                this.Add(token);
            }
        }
        /**
		 * 列番号を取得する
		 */
        public int getColIndex(string name)
        {
            for (int m = 0; m < this.Count; m++)
            {
                if (name == this[m]) return m;
            }
            return -1;
            //throw new Exception($"フィールド名がない name={name}");
        }
        /**
		 * 
		 **/
        public int addField(string name)
        {
            int col = getColIndex(name);
            if (col >= 0) return col;
            this.Add(name);
            return this.Count - 1;
        }
        public string toString(string sep)
        {
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < this.Count; m++)
            {
                if (m != 0) sb.Append(sep);
                sb.Append(this[m]);
            }
            return sb.ToString();
        }
        /**
		 * 
		 * */
        public override string ToString()
        {
            return toString(",");
        }
    }
    //
    //
    public class CSVRows : List<CSVRow>
    {
        private CSVFields Fields { get; set; }

        public CSVRows(CSVFields flds)
        {
            Fields = flds;
        }
        CSVRow getRow(int rowno)
        {
            //足りない場合は追加
            while (rowno >= this.Count) { this.Add(new CSVRow(0, Fields)); }
            return base[rowno];
        }
        new public CSVRow this[int rowno] { get { return getRow(rowno); } }
        /**
		 * 列にまとめてデータを設定する
		 * */
        public void setColList(int colno, string[] list)
        {
            for (int rowno = 0; rowno < list.Length; rowno++)
            {
                CSVRow row = getRow(rowno);
                row[colno] = list[rowno];
            }
        }
        /**
		 * 列の文字列をまとめて取得
		 */
        public List<string> getColList(int colno)
        {
            var lst = new List<string>();
            foreach (var row in this)
            {
                lst.Add(row[colno]);
            }
            return lst;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (CSVRow row in this)
            {
                sb.AppendLine(row.ToString());
            }
            return sb.ToString();
        }
    }
    /**
	 * 
	 */
    public class CSVData
    {
        string name_;
        public string Name { get { return name_; } set { name_ = value; } }

        public CSVFields Fields { get; set; }
        public CSVRows Rows { get; set; }
        public CSVData()
        {
            Fields = new CSVFields();
            Rows = new CSVRows(Fields);
        }

        public List<string> getColList(string fldname)
        {
            int colno = Fields.getColIndex(fldname);
            if (colno < 0) return null;
            return Rows.getColList(colno);
        }

    }
    /**
       * 
       * */
    public class CSVFileReader
    {
        private readonly ILogger _logeer;

        //StringBuilder ERR = new StringBuilder();
        char token_ = ',';
        char comment_ = '*';
        public int StartLineIndex { get; set; }   //2行目開始なら2
        public void setupTargetToken(char token)
        {
            token_ = token;
        }

        public void setupCommentToken(char comment)
        {
            comment_ = comment;
        }
        public CSVFileReader(ILogger logeer)
        {
            _logeer = logeer;
            StartLineIndex = 0;
        }

        public CSVData readFile(string path, Encoding enc, bool bFirstLineIsField)
        {
            var csv = new CSVData();
            readFile(path, enc, bFirstLineIsField, ref csv);
            return csv;
        }
        public CSVData readFile(string path, Encoding enc, bool bFirstLineIsField, char token)
        {
            setupTargetToken(token);
            CSVData csv = new CSVData();
            readFile(path, enc, bFirstLineIsField, ref csv);
            return csv;
        }
        /**
		 * Encoding.GetEncoding("Shift_JIS")
		 * */
        public void readFile(string path, Encoding enc, bool bFirstLineIsField, ref CSVData csv)
        {
            if (System.IO.File.Exists(path) != true)
            {
                //ERR.AppendLine("ファイルがありません(CSVFileReder) PATH=" + path);
                throw new Exception("ファイルがありません PATH=" + path);
            }
            System.IO.StreamReader sr = new System.IO.StreamReader(path, enc);
            string buf = null;
            bool bFirst = true;
            int gyono = 0;
            while ((buf = sr.ReadLine()) != null)
            {
                //Console.WriteLine($"{gyono}:{buf}");
                gyono++;
                if (StartLineIndex > 0 && StartLineIndex > gyono)  //2行目から開始はStartLineIndex=2  
                    continue;
                if (buf.Length == 0 || buf[0] == comment_) continue;
                Tokens tokens = new Tokens(buf, new char[] { token_ });
                if (bFirstLineIsField == true && bFirst == true) csv.Fields.copy(tokens);
                else csv.Rows.Add(new CSVRow(tokens, csv.Fields, gyono));
                bFirst = false;
            }
            sr.Close();
            if (csv.Rows.Count == 0)
            {
                throw new Exception("ファイルにデータがありません PATH=" + path);
            }
        }
    }
}
