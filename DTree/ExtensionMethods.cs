using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTree
{
    public static class ExtensionMethods
    {
        public static string RemoveSingleQuoteIfAny(this String str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 2) {
                return str;
            }

            if (!str[0].Equals('\'') || !str[str.Length - 1].Equals('\''))
            {
                return str;
            }

            return str.Substring(1, str.Length - 2);
        }
    }
}
