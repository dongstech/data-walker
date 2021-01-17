using System.IO;

namespace DataWalker.Extensions
{
    public static class Extensions
    {
        public static string BaseName(this FileInfo fi)
        {
            var name = fi.Name;
            var index = name.LastIndexOf('.');
            return name.Substring(0, index);
        }
    }
}