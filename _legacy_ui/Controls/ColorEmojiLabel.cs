namespace NMSE.UI.Controls;

/// <summary>
/// A Label subclass that uses GDI+ text rendering (Graphics.DrawString) instead of
/// the default GDI rendering (TextRenderer.DrawText). This enables color emoji
/// rendering via COLR table 0 on Windows 10+ through the Uniscribe/DirectWrite
/// fallback in GDI+.
/// </summary>
public class ColorEmojiLabel : Label
{
    public ColorEmojiLabel()
    {
        // UseCompatibleTextRendering = true switches from GDI (TextRenderer) to
        // GDI+ (Graphics.DrawString) which supports color font rendering on Win10+.
        UseCompatibleTextRendering = true;
    }
}
