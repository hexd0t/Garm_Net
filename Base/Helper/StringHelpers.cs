using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.Base.Helper
{
	static public class StringHelpers
	{
		static public string Truncate(this string str, int maxLength, bool addpoints = true)
		{
			if (str == null)
				return null;
			if (str.Length > maxLength)
			{
				if (maxLength < 4 || !addpoints)
					str = str.Substring(0, maxLength);
				else
					str = str.Substring(0, maxLength - 3) + "...";
			}
			return str;
		}
	}
}
