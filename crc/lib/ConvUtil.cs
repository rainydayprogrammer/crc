using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace crc.lib
{
    public class ConvUtil
    {
        public static string Decimal2String(decimal num)
        {
            string str = num.ToString();
            return (str.Replace("-","").Replace(".", "").Length > 16) ? num.ToString("e") : str; // 16を超えることは稀
        }

        public static decimal String2Decimal(string str)
        {
            return Decimal.Parse(str, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
        }
    }
}
