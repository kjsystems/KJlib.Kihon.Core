namespace kjlib.Kihon.Models
{
    public class ConvUtil
    {
        /**
		 * 
		 **/
        static public float toFloat2(string buf, float def)
        {
            float res = 0f;
            if (float.TryParse(buf, out res) != true)
            {
                res = def;   //取得できなかったら値をセットする
            }
            return res;
        }
        /**
		 * 
		 **/
        static public int toInt(string buf, int def)
        {
            int res = def;
            if (int.TryParse(buf, out res) != true)
            {
                res = def;   //取得できなかったら値をセットする
            }
            return res;
        }
        static public long toLong(string buf, long def)
        {
            long res = def;
            if (long.TryParse(buf, out res) != true)
            {
                res = def;   //取得できなかったら値をセットする
            }
            return res;
        }
    }
}
