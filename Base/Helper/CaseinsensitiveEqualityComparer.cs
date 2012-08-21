using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garm.Base.Helper
{
    /// <summary>
    /// Provides an <code>IEqualityComparer&lt;string&gt;</code> ignoring case
    /// </summary>
    public class CaseinsensitiveEqualityComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// Caseinsensitively tests two strings for equality
        /// </summary>
        /// <param name="x">First string</param>
        /// <param name="y">Second string</param>
        /// <returns>True if both strings are equal, else false</returns>
        public bool Equals(string x, string y)
        {
            if (x == null || y == null)
                return false;
            return x.ToLowerInvariant().Equals(y.ToLowerInvariant());
        }

        /// <summary>
        /// Gets a caseinsensitive Hashcode by returning the HashCode of the all-lowered string
        /// </summary>
        /// <param name="obj">The string to generate the HashCode from</param>
        /// <returns>The caseinsensitive HashCode</returns>
        public int GetHashCode(string obj)
        {
            return obj.ToLowerInvariant().GetHashCode();
        }
    }
}
