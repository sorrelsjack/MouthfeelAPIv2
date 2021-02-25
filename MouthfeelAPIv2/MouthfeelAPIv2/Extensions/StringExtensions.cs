using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhitespace (this string source)
        {
            if (String.IsNullOrWhiteSpace(source)) return true;
            return false;
        }
    }
}
