using System;
using System.Collections.Generic;

namespace kjlib.kihon.Models
{
    public class BinUtil
    {
        static public string getPosHex(System.IO.BinaryReader rd)
        {
            return string.Format("0x{0:x}", rd.BaseStream.Position);
        }
        static public long getPos(System.IO.BinaryReader rd)
        {
            return rd.BaseStream.Position;
        }

        System.IO.BinaryWriter bw_;
        System.IO.BinaryReader br_;
        public BinUtil(System.IO.BinaryWriter bw) { bw_ = bw; }
        public BinUtil(System.IO.BinaryReader br) { br_ = br; }
        static public byte[] gb(string buf) { return System.Text.Encoding.UTF8.GetBytes(buf); }   //UTF8からbyteを取得
        static public string gs(byte[] by) { return System.Text.Encoding.UTF8.GetString(by); }    //byteからUTF8を取得

        public void wb(byte[] bytes) { bw_.Write(bytes); }
        public void w(string value) { wt(value); }
        public void wf(float value) { bw_.Write(value); }
        //public void w(int value) { wi(value); }
        public void w(bool value) { bw_.Write(value); }
        public void w(AttrList lst) { lst.write(bw_); }
        public void wl(long value) { bw_.Write((long)value); }
        public string r() { return rt(); }
        public float rf() { return br_.ReadSingle(); }
        public bool rb() { return br_.ReadBoolean(); }
        public long rl() { return br_.ReadInt64(); }
        public char[] readChars(int count) { return br_.ReadChars(count); }
        public char readChar() { return br_.ReadChar(); }
        public byte[] readBytes(int count) { return br_.ReadBytes(count); }
        public byte readByte() { return br_.ReadByte(); }

        static public int MASKFLAG = 0x29;
        static public void getMask(ref byte[] bytes)
        {
            for (int m = 0; m < bytes.Length; m++)
            {
                bytes[m] = (byte)((byte)MASKFLAG ^ bytes[m]);
            }
        }
        public void wt(string buf, bool bMask)
        {
            if (buf == null)
            {
                bw_.Write(0);
                return;
            }
            byte[] b = gb(buf);
            if (bMask == true)
            {
                getMask(ref b);
            }
            bw_.Write(b.Length);               //int
            bw_.Write(b);                      //string
        }
        /**
		 * 
		 * */
        public string rt(bool bMask)
        {
            int len = br_.ReadInt32();   //BYTE[]の長さ
            if (len == 0) return "";
            byte[] bytes = br_.ReadBytes(len);
            if (bMask == true)
            {
                getMask(ref bytes);
            }
            string buf = gs(bytes);
            return buf;
        }
        /**
		 * 
		 * */
        public void wt(string buf)
        {
            if (buf == null)
            {
                bw_.Write((int)0);
                return;
            }
            byte[] b = gb(buf);
            //Console.WriteLine("bin wt buf={0}", buf);  //KJ_TEST
            //Console.WriteLine("bin wt buf={0} bytes.len={1}", buf,b.Length);  //KJ_TEST
#if BUG_PROGxxx
			int curpos = (int)br_.BaseStream.Position;
			Console.WriteLine("bin wt curpos={0}", curpos);  //KJ_TEST  ==> これ使うとNG
#endif
            bw_.Write((int)b.Length);          //int
            bw_.Write(b);                      //bytes
        }

        /**
		 * 
		 * */
        public string rt()
        {
            //Console.WriteLine("binutil readtext curpos={0}", br_.BaseStream.Position);
            int len = br_.ReadInt32();   //BYTE[]の長さ
                                         //Console.WriteLine("binutil readtext read.len={0}",len);
            if (len == 0) return "";
            //Console.WriteLine("binutil readtext curpos={0}", br_.BaseStream.Position);
            byte[] bytes = br_.ReadBytes(len);
            string buf = gs(bytes);
            //Console.WriteLine("binutil readtext curpos={0} bytes.len={1}", br_.BaseStream.Position, bytes.Length);
            //Console.WriteLine("binutil readtext buf={0} buf.len={1}", buf, len);
            return buf;
        }
        public char rc()
        {
            return br_.ReadChar();
        }
        public int ri()
        {
            return br_.ReadInt32();   //Read()ではNG 16bitになる???
        }
        public void wc(char v)
        {
            bw_.Write((char)v);
        }
        public void wi(int v)
        {
            bw_.Write((int)v);
        }
        public void wilst(List<int> lst)
        {
            wi(lst.Count);
            foreach (int v in lst) wi(v);
        }
        public void rilst(ref List<int> lst)
        {
            int num = ri();
            for (int m = 0; m < num; m++)
            {
                lst.Add(ri());
            }
            if (num != lst.Count) throw new Exception("件数が異なる rilst " + num + " vs " + lst.Count);
        }
    }
}
