using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZocMonLib
{
    public static class ExceptionExtension
    {
        public static void ThrowIfNull(this object value, string name)
        {
            if (value == null)
                throw new NullReferenceException(string.Format("Value is null: {0}", name));
        }

        public static void ThrowIfNaN(this double value, string name)
        {
            if (double.IsNaN(value))
                throw new NullReferenceException(string.Format("Value is NaN: {0}", name));
        }
    }
}
