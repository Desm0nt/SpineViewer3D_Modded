using System.Text;

namespace Volot
{
    public static class Utils
    {
        public static string RemoveCharFromStr(string source, char which) {
            var sb = new StringBuilder();
            foreach (var sym in source)
            {
                if (!sym.Equals(which)) {
                    sb.Append(sym);
                }
            }
            return sb.ToString();
        }
    }
}
