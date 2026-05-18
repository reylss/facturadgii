using System;

namespace ApiCf.SharedNs
{
    public static class Extensions
    {
        public static int ToInt(this string value)
        {
            return Convert.ToInt32(value);
        }
        public static int ToShort(this string value)
        {
            return Convert.ToInt16(value);
        }
        public static bool ToBoolStr(this string value)
        {
            return value == "SI";
        }
    }
}




