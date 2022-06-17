using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Multi_Channel_Image_Tool.Additional_Windows;
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
            public static ImageSource Icon => GetImageFromFolder("../Icon.ico");
        }

        public static class Validation
        {
            private const string _PNG = ".png";
            private static readonly string[] _VALID_EXTENSIONS = new[] { ".jpg", ".jpeg", _PNG };
            public const string _VALID_EXTENSIONS_AS_STRING_LIST = "png, jpg, jpeg";

            private static bool IsValidBitmap(string filename)
            {
                try
                {
                    using (var bmp = new Bitmap(filename)) { }
                    return true;
                }
                catch (Exception) { return false; }
            }
            public static bool IsValidImage(string imagePath) => File.Exists(imagePath) && _VALID_EXTENSIONS.Contains(Path.GetExtension(imagePath)) && IsValidBitmap(imagePath);
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

            public static ImageSource BitmapToImageSource(Bitmap bmp)
            {
                var handle = bmp.GetHbitmap();
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                finally { DeleteObject(handle); }
            }

            #endregion Converters

            private unsafe struct BitmapDataHelper
            {
                private byte* StartPos { get; }
                private int Stride { get; }
                private Bitmap Image { get; }
                private BitmapData Data { get; }

                public byte GetPixel(EChannel channel, int x, int y)
                {
                    byte* row = StartPos + (y * Stride);
                    int b = x * 4;

                    switch (channel)
                    {
                        case EChannel.R:
                            return row[b + 2];
                        case EChannel.G:
                            return row[b + 1];
                        case EChannel.B:
                            return row[b];
                        case EChannel.A:
                            return row[b + 3];
                        default:
                            return 0;
                    }
                }

                public void SetPixel(EChannel channel, int x, int y, byte newValue)
                {
                    byte* row = StartPos + (y * Stride);
                    int b = x * 4;

                    switch (channel)
                    {
                        case EChannel.R:
                            row[b + 2] = newValue;
                            return;
                        case EChannel.G:
                            row[b + 1] = newValue;
                            return;
                        case EChannel.B:
                            row[b] = newValue;
                            return;
                        case EChannel.A:
                            row[b + 3] = newValue;
                            return;
                    }
                }

                public void SetPixel(int x, int y, byte newR, byte newG, byte newB, byte newA)
                {
                    byte* row = StartPos + (y * Stride);
                    int b = x * 4;

                    row[b + 2] = newR;
                    row[b + 1] = newG;
                    row[b] = newB;
                    row[b + 3] = newA;
                }

                public void Unlock()
                {
                    Image.UnlockBits(Data);
                }

                public BitmapDataHelper(Bitmap image)
                {
                    Image = image;
                    Data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                    StartPos = (byte*)Data.Scan0.ToPointer();
                    Stride = Data.Stride;
                }
            }

            public static Bitmap GenerateSolidColor(int r, int g, int b, int a)
            {
                Bitmap solidColor = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
                solidColor.SetPixel(0, 0, Color.FromArgb(a, r, g, b));

                return solidColor;
            }

            public static ImageSource ExtractChannelAndGetSource(string imagePath, EChannel channelToExtract, EChannel finalChannel, bool invert)
            => BitmapToImageSource(ExtractChannel(imagePath, channelToExtract, finalChannel, invert));

            public static  Bitmap ExtractChannel(string imagePath, EChannel channelToExtract, EChannel finalChannel, bool invert, string popupTextExtra = "")
            {
                if (!ImageUtility.Validation.IsValidImage(imagePath)) { return new Bitmap(1, 1); }

                Bitmap image = new Bitmap(imagePath);

                PopupTextWindow.OpenWindowAndExecute($"Generating Texture By Extracting Channel, Please Wait {popupTextExtra}",
                    () =>
                    {
                        BitmapDataHelper helper = new BitmapDataHelper(image);
                        bool imageHasTransparency = ImageUtility.Validation.ImageHasTransparency(imagePath);

                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                switch (channelToExtract)
                                {
                                    case EChannel.R:
                                        byte pixelR = helper.GetPixel(EChannel.R, x, y);
                                        if (invert) { pixelR = (byte)(byte.MaxValue - pixelR); }

                                        helper.SetPixel(EChannel.R, x, y, pixelR);
                                        helper.SetPixel(EChannel.G, x, y, byte.MinValue);
                                        helper.SetPixel(EChannel.B, x, y, byte.MinValue);
                                        break;
                                    case EChannel.G:
                                        byte pixelG = helper.GetPixel(EChannel.G, x, y);
                                        if (invert) { pixelG = (byte)(byte.MaxValue - pixelG); }

                                        helper.SetPixel(EChannel.R, x, y, byte.MinValue);
                                        helper.SetPixel(EChannel.G, x, y, pixelG);
                                        helper.SetPixel(EChannel.B, x, y, byte.MinValue);
                                        break;
                                    case EChannel.B:
                                        byte pixelB = helper.GetPixel(EChannel.B, x, y);
                                        if (invert) { pixelB = (byte)(byte.MaxValue - pixelB); }

                                        helper.SetPixel(EChannel.R, x, y, byte.MinValue);
                                        helper.SetPixel(EChannel.G, x, y, byte.MinValue);
                                        helper.SetPixel(EChannel.B, x, y, pixelB);
                                        break;
                                    case EChannel.A:
                                        byte pixelA = imageHasTransparency ? helper.GetPixel(EChannel.A, x, y) : byte.MaxValue;
                                        if (invert) { pixelA = (byte)(byte.MaxValue - pixelA); }

                                        helper.SetPixel(EChannel.R, x, y, pixelA);
                                        helper.SetPixel(EChannel.G, x, y, pixelA);
                                        helper.SetPixel(EChannel.B, x, y, pixelA);
                                        helper.SetPixel(EChannel.A, x, y, byte.MaxValue);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException(nameof(channelToExtract), channelToExtract, null);
                                }
                            }
                        }

                        helper.Unlock();
                    });
                return image;
            }

            public static ImageSource CombineChannelsAndGetSource(Bitmap r, Bitmap g, Bitmap b, Bitmap a)
                => BitmapToImageSource(CombineChannels(r, g, b, a));

            public static Bitmap CombineChannels(Bitmap r, Bitmap g, Bitmap b, Bitmap a)
            {
                int width = Helpers.Max(r.Width, g.Width, b.Width, a.Width);
                int height = Helpers.Max(r.Height, g.Height, b.Height, a.Height);

                Bitmap merged = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                BitmapDataHelper mergedHelper = new BitmapDataHelper(merged);
                BitmapDataHelper rHelper = new BitmapDataHelper(r);
                BitmapDataHelper gHelper = new BitmapDataHelper(g);
                BitmapDataHelper bHelper = new BitmapDataHelper(b);
                BitmapDataHelper aHelper = new BitmapDataHelper(a);

                PopupTextWindow popupText = new PopupTextWindow
                {
                    PopupText = { Text = "Creating Texture, Please Wait" }
                };
                popupText.Show();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte rIntensity = rHelper.GetPixel(EChannel.R, x % r.Width, y % r.Height);
                        byte gIntensity = gHelper.GetPixel(EChannel.G, x % g.Width, y % g.Height);
                        byte bIntensity = bHelper.GetPixel(EChannel.B, x % b.Width, y % b.Height);
                        // This uses the generated previews, so the alpha preview has 255 alpha throughout. However, any other channel has the alpha value. So we use R instead of A here.
                        byte aIntensity = aHelper.GetPixel(EChannel.R, x % a.Width, y % a.Height);

                        mergedHelper.SetPixel(x, y, rIntensity, gIntensity, bIntensity, aIntensity);
                    }
                }

                mergedHelper.Unlock();
                rHelper.Unlock();
                gHelper.Unlock();
                bHelper.Unlock();
                aHelper.Unlock();

                popupText.Close();
                return merged;
            }
        }
    }
}
