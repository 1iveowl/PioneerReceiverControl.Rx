using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPioneerReceiverControl.Rx.CustomException;

namespace PioneerReceiverControl.Rx.ExtensionMethod
{
    public static class StringEx
    {
        public static string WildcardReplace(this string source, char wildcard, string replacement)
        {
            var sourceWildcardCount = source.Count(c => c == wildcard);

            if (sourceWildcardCount != replacement.Length)
            {
                throw new PioneerReceiverException($"There are {sourceWildcardCount} wildcards in template, but the parameter is {replacement.Length} long. " +
                                                   $"The number of wildcards and the length of the parameter must be identical.");
            }

            var sourceCharArray = source.ToCharArray();
            var replacementCharArray = replacement.ToCharArray();

            var replaceIndex = 0;

            var sb = new StringBuilder();

            foreach (var c in sourceCharArray)
            {
                if (c == wildcard)
                {
                    sb.Append(replacementCharArray[replaceIndex]);
                    replaceIndex++;
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
