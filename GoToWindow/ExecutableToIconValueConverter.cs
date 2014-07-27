﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GoToWindow
{
    [ValueConversion(typeof(string), typeof(BitmapFrame))]
    public class ExecutableToIconValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var iconStream = new MemoryStream();

            using(var icon = Icon.ExtractAssociatedIcon((string)value))
            {
                using (var bmp = icon.ToBitmap())
                {
                    bmp.Save(iconStream, ImageFormat.Png);
                }
            }

            iconStream.Position = 0;
            var decoder = new PngBitmapDecoder(iconStream, BitmapCreateOptions.None, BitmapCacheOption.None);
            return decoder.Frames.Last();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
