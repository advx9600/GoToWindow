using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GoToWindow.Plugins.Core.Utils
{
    public static class IconLoader
    {
        public static BitmapFrame LoadIcon(IntPtr iconHandle, string executableFile)
        {
            return ConvertFromHandle(iconHandle) ?? ConvertFromFile(executableFile);
        }

        private static BitmapFrame ConvertFromHandle(IntPtr iconHandle)
        {
            if (iconHandle == IntPtr.Zero)
                return null;

            using (var icon = Icon.FromHandle(iconHandle))
            {
                return ConvertFromIcon(icon);
            }
        }

        private static BitmapFrame ConvertFromFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            using (var icon = Icon.ExtractAssociatedIcon(path))
            {
                return ConvertFromIcon(icon);
            }
        }

        private static BitmapFrame ConvertFromIcon(Icon icon)
        {
            var iconStream = new MemoryStream();

            using (var bmp = icon.ToBitmap())
            {
                bmp.Save(iconStream, ImageFormat.Png);
            }

            iconStream.Position = 0;
            var decoder = new PngBitmapDecoder(iconStream, BitmapCreateOptions.None, BitmapCacheOption.None);
            return decoder.Frames.Last();
        }

        // https://docs.microsoft.com/en-us/dotnet/framework/winforms/advanced/how-to-construct-font-families-and-fonts
        // https://stackoverflow.com/questions/2070365/how-to-generate-an-image-from-text-on-fly-at-runtime
        public static ImageBrush DrawText(String text, Font font, System.Drawing.Color textColor, System.Drawing.Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            System.Drawing.Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            System.Drawing.Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            // convert bitmap to ImageBrush
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(((Bitmap)img).GetHbitmap(),
                                                                       IntPtr.Zero,
                                                                       Int32Rect.Empty,
                                                                       BitmapSizeOptions.FromEmptyOptions());

            img.Dispose();
            var brush = new ImageBrush(bitmapSource);

            return brush;

        }
    }
}