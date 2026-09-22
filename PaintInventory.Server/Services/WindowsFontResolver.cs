using PdfSharp.Fonts;

namespace PaintInventory.Server.Services;

public sealed class WindowsFontResolver : IFontResolver
{
    private const string FontDir = @"C:\Windows\Fonts";

    public byte[]? GetFont(string faceName)
    {
        var file = faceName switch
        {
            "Arial#b" => "arialbd.ttf",
            "Arial#i" => "ariali.ttf",
            "Arial#bi" => "arialbi.ttf",
            _ => "arial.ttf"
        };
        var path = Path.Combine(FontDir, file);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
    {
        var suffix = (bold, italic) switch
        {
            (true, true) => "#bi",
            (true, false) => "#b",
            (false, true) => "#i",
            _ => "#"
        };
        return new FontResolverInfo("Arial" + suffix);
    }
}