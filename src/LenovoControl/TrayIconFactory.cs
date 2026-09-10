using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace LenovoControl;

internal static class TrayIconFactory
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static Icon Create()
    {
        using var bitmap = new Bitmap(32, 32);

        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            using var backgroundBrush =
                new SolidBrush(Color.FromArgb(35, 40, 48));

            graphics.FillEllipse(
                backgroundBrush,
                1,
                1,
                30,
                30);

            PointF[] bolt =
            [
                new(18.5f, 4.5f),
                new(9.5f, 17.0f),
                new(15.0f, 17.0f),
                new(12.5f, 27.0f),
                new(22.5f, 13.5f),
                new(17.0f, 13.5f)
            ];

            using var boltBrush =
                new SolidBrush(Color.White);

            graphics.FillPolygon(
                boltBrush,
                bolt);
        }

        IntPtr handle = bitmap.GetHicon();

        try
        {
            using Icon temporary =
                Icon.FromHandle(handle);

            return (Icon)temporary.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }
}
