using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class TypeExtensions
    {
        public static bool IsNumericType(this Type type)
        {
            bool rs = false;
            if (type == typeof(Byte) || type == typeof(Byte?)
                || type == typeof(SByte) || type == typeof(SByte?)
                || type == typeof(UInt16) || type == typeof(UInt16?)
                || type == typeof(UInt32) || type == typeof(UInt32?)
                || type == typeof(UInt64) || type == typeof(UInt64?)
                || type == typeof(Int16) || type == typeof(Int16?)
                || type == typeof(Int32) || type == typeof(Int32?)
                || type == typeof(Int64) || type == typeof(Int64?)
                || type == typeof(Decimal) || type == typeof(Decimal?)
                || type == typeof(Double) || type == typeof(Double?)
                || type == typeof(Single) || type == typeof(Single?))
            {
                rs = true;
            }
            return rs;
        }
    }
}
