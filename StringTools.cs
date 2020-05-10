using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBayKleinanzeigenTracker
{
    class StringTools
    {
        public static bool StringIsInt(string str)
        {
            try {
                Convert.ToInt32(str);
                return true;
            } catch {
                return false;
            }
        }

        public static string ExtractIntFromString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach(char c in str.ToCharArray())
            {
                if (Char.IsDigit(c)) sb.Append(c);
            }

            if (sb.Length == 0) return "";
            int n = Int32.MaxValue;
            try
            {
                n = Int32.Parse(sb.ToString());
            } catch{}

            return Math.Min(99999999, n).ToString();
        }

        public static string[] ExtractGroups(string str, string start, string end)
        {
            List<string> groups = new List<string>();
            while(str.Contains(start) && str.Contains(end))
            {
                int startIndex = str.IndexOf(start) + start.Length;
                int endIndex = str.IndexOf(end, startIndex);
                if (endIndex == -1) break;

                string groupStr = "";
                if (endIndex > startIndex) groupStr = str.Substring(startIndex, endIndex - startIndex);
                groups.Add(groupStr);
                
                str = ReplaceFirst(str, start, "");
                str = ReplaceFirst(str, end, "");
            }
            return groups.ToArray();
        }

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }


    }
}
