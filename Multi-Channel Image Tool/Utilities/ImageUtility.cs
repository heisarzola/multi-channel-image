using System;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Path = System.IO.Path;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Multi_Channel_Image_Tool
{
    public static class ImageUtility
    {
        public static class EditorImages
        {
            public static ImageSource GetImageFromFolder(string imagePath) => new BitmapImage(new Uri($"pack://application:,,,{imagePath}", UriKind.RelativeOrAbsolute));
            public static ImageSource Error => GetImageFromFolder("/Images/Error.png");
        }

        public static class Validation
        {
            private const string _PNG = ".png";
            private static readonly string[] _VALID_EXTENSIONS = new[] { ".jpg", ".jpeg", _PNG };

            public static bool IsValidImage(string imagePath) => File.Exists(imagePath) && _VALID_EXTENSIONS.Contains(Path.GetExtension(imagePath));
            public static bool ImageHasTransparency(string imagePath) => Path.GetExtension(imagePath).Equals(_PNG);
        }

        public static class ImageGeneration
        {
            #region Internal

            [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteObject([In] IntPtr hObject);

            #endregion Internal

            #region Converters

            private static ImageSource BitmapToImageSource(Bitmap bmp)
            {
                var handle = bmp.GetHbitmap();
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                finally { DeleteObject(handle); }
            }

            private static Bitmap BitmapSourceToBitmap(BitmapSource source)
            {
                int width = source.PixelWidth;
                int height = source.PixelHeight;
                int stride = width * ((source.Format.BitsPerPixel + 7) / 8);
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    ptr = Marshal.AllocHGlobal(height * stride);
                    source.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                    using (var btm = new System.Drawing.Bitmap(width, height, stride, PixelFormat.Format1bppIndexed, ptr))
                    {
                        // Clone the bitmap so that we can dispose it and
                        // release the unmanaged memory at ptr
                        return new Bitmap(btm);
                    }
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Marshal.FreeHGlobal(ptr);
                }
            }

            #endregion Converters

            public static ImageSource GenerateSolidColor(int r, int g, int b, int a)
            {
                Bitmap solidColor = new Bitmap(1, 1, PixelFormat.Format32bppRgb);
                solidColor.SetPixel(0, 0, Color.FromArgb(a, r, g, b));

                return BitmapToImageSource(solidColor);
            }

            public static ImageSource ExtractChannel(string imagePath, EChannel channelToExtract, EChannel finalChannel)
            {
                if (!ImageUtility.Validation.IsValidImage(imagePath)) { return ImageUtility.EditorImages.Error; }
                var bitmap = new Bitmap(imagePath);
                var extractedBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppRgb);

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color pixelColor = bitmap.GetPixel(x, y);

                        int intensity;

                        switch (channelToExtract)
                        {
                            case EChannel.R:
                                intensity = pixelColor.R;
                                break;
                            case EChannel.G:
                                intensity = pixelColor.G;
                                break;
                            case EChannel.B:
                                intensity = pixelColor.B;
                                break;
                            case EChannel.A:
                                intensity = ImageUtility.Validation.ImageHasTransparency(imagePath) ? pixelColor.A : 255;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(channelToExtract), channelToExtract, null);
                        }

                        if (intensity > 0 & intensity <= 255)
                        {
                            switch (finalChannel)
                            {
                                case EChannel.R:
                                    extractedBitmap.SetPixel(x, y, Color.FromArgb(255, intensity, 0, 0));
                                    break;
                                case EChannel.G:
                                    extractedBitmap.SetPixel(x, y, Color.FromArgb(255, 0, intensity, 0));
                                    break;
                                case EChannel.B:
                                    extractedBitmap.SetPixel(x, y, Color.FromArgb(255, 0, 0, intensity));
                                    break;
                                case EChannel.A:
                                    extractedBitmap.SetPixel(x, y,
                                        ImageUtility.Validation.ImageHasTransparency(imagePath)
                                            ? Color.FromArgb(intensity, intensity, intensity, intensity)
                                            : Color.FromArgb(255, 252, 255, 255));
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(finalChannel), finalChannel, null);
                            }
                        }
                        else
                        {
                            extractedBitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                        }
                    }
                }

                bitmap.Dispose();
                return BitmapToImageSource(extractedBitmap);
            }
            }
        }
    }
}
