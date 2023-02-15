using System.IO;

namespace SwaggerDocCreator;

internal static class Ext
{
    public static TextWriter AddCell(this TextWriter w, string n)
    {
        w.Write("|" + (string.IsNullOrEmpty(n) ? "-" : n));
        return w;
    }

    public static TextWriter EndCell(this TextWriter w)
    {
        w.WriteLine("|");
        return w;
    }
}