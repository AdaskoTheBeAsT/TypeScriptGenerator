using System.Globalization;
using System.Text;

namespace TypeScriptGenerator
{
    public static class StringExtensions
    {
        public static string ToHyphenated(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var sb = new StringBuilder();
            foreach (var ch in str)
            {
                if (!char.IsUpper(ch))
                {
                    sb.Append(ch);
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append("-");
                }

                sb.Append(char.ToLower(ch, CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
    }
}
