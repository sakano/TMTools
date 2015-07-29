using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TMapCapture
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 保存する画像
        /// </summary>
        System.Drawing.Bitmap trimmedBitmap = null;
        
        public MainWindow()
        {
            InitializeComponent();
            beginTakeSnapshot();
        }

        private void grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 左クリックまたは中クリックで終了
            switch (e.ChangedButton) {
                case MouseButton.Right:
                case MouseButton.Middle:
                    Close();
                    break;
            }
        }

        private void retakeSnapshotMenuItem_Click(object sender, RoutedEventArgs e)
        {
            beginTakeSnapshot();
        }

        private void saveImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (trimmedBitmap == null)
                return;
            
            // 保存先選択ダイアログを出してjpegで保存
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "JPEG(*.jpg;*.jpeg;*.jpe)|*.jpg;*.jpeg;*.jpe";
            
            bool? result = dialog.ShowDialog(this);
            if (result == true) {
                saveBitmapAsJpeg(trimmedBitmap, dialog.FileName);
            }
        }

        /// <summary>
        /// ドラッグで選択された範囲のスクリーンショットを撮ってウィンドウに表示
        /// trimmedBitmapに保存用の画像を生成
        /// </summary>
        private void beginTakeSnapshot()
        {
            var window = new CaptureDragWindow();
            window.ReceiveDragAsync(resultRect =>
            {
                if (resultRect.Width < 10 || resultRect.Height < 10)
                    return;

                using (var screenBitmap = new System.Drawing.Bitmap((int)resultRect.Width, (int)resultRect.Height)) {
                    // ドラッグされた範囲のスクリーンショットを撮影
                    using (var g = Graphics.FromImage(screenBitmap)) {
                        g.CopyFromScreen((int)resultRect.Left, (int)resultRect.Top, 0, 0, screenBitmap.Size, System.Drawing.CopyPixelOperation.SourceCopy);
                    }
                    
                    // 画像を表示
                    var hBitmap = screenBitmap.GetHbitmap();
                    image.Source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    image.Width = screenBitmap.Width;
                    image.Height = screenBitmap.Height;
                    image.Visibility = Visibility.Visible;

                    // トリミングされる範囲を表示
                    var trimRect = measureTrimmingRect(screenBitmap);
                    trimRectangle.Margin = new Thickness(trimRect.Left - 1, trimRect.Top - 1, screenBitmap.Width - trimRect.Right, screenBitmap.Height - trimRect.Bottom);
                    trimRectangle.Visibility = Visibility.Visible;

                    // 保存用のトリミングされたBitmapを用意
                    if (trimmedBitmap != null)
                        trimmedBitmap.Dispose();
                    trimmedBitmap = new System.Drawing.Bitmap(trimRect.Width, trimRect.Height, screenBitmap.PixelFormat);
                    using (var g = Graphics.FromImage(trimmedBitmap)) {
                        g.DrawImage(screenBitmap, 0, 0, trimRect, GraphicsUnit.Pixel);
                    }
                }
            });
        }

        /// <summary>
        /// 周囲の黒い部分を除いた部分の範囲を返す
        /// </summary>
        /// <param name="bitmap">処理対象の画像</param>
        /// <returns>中央のマップ部分の範囲</returns>
        private static System.Drawing.Rectangle measureTrimmingRect(System.Drawing.Bitmap bitmap)
        {
            Contract.Requires<ArgumentNullException>(bitmap != null);

            var result = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var trimColor = System.Drawing.Color.Black;
            int centerX = bitmap.Width / 2, centerY = bitmap.Height / 2;
            if (compareSimilarColor(trimColor, bitmap.GetPixel(centerX, centerY))) {
                return result;
            }

            while (++result.X < bitmap.Width) {
                if (!compareSimilarColor(trimColor, bitmap.GetPixel(result.X, centerY))) {
                    break;
                }
            }
            while (++result.Y < bitmap.Height) {
                if (!compareSimilarColor(trimColor, bitmap.GetPixel(centerX, result.Y))) {
                    break;
                }
            }
            for (int right = bitmap.Width-1; right > result.X; --right) {
                if (!compareSimilarColor(trimColor, bitmap.GetPixel(right, centerY))) {
                    result.Width = right + 1 - result.X;
                    break;
                }
            }
            for (int bottom = bitmap.Height-1; bottom > result.Y; --bottom) {
                if (!compareSimilarColor(trimColor, bitmap.GetPixel(centerX, bottom))) {
                    result.Height = bottom + 1 - result.Y;
                    break;
                }
            }

            return result;
        }

        private static bool compareSimilarColor(System.Drawing.Color left, System.Drawing.Color right, int diff = 92)
        {
            Contract.Assume(left.R - right.R != Int32.MinValue);
            Contract.Assume(left.G - right.G != Int32.MinValue);
            Contract.Assume(left.B - right.B != Int32.MinValue);
            return Math.Abs(left.R - right.R) < diff
                && Math.Abs(left.G - right.G) < diff
                && Math.Abs(left.B - right.B) < diff;
        }

        private static void saveBitmapAsJpeg(Bitmap bitmap, string path)
        {
            Contract.Requires<ArgumentNullException>(bitmap != null);
            Contract.Requires<ArgumentNullException>(path != null);

            var info = getJpegCodecInfo();
            var param = getEncoderParameter();
            bitmap.Save(path, info, param);
        }

        private static ImageCodecInfo getJpegCodecInfo()
        {
            var encoders = ImageCodecInfo.GetImageEncoders();
            Contract.Assume(encoders != null);
            return encoders.FirstOrDefault(info => info.FormatID == ImageFormat.Jpeg.Guid);
        }

        private static EncoderParameters getEncoderParameter(long quality = 100L)
        {
            Contract.Requires<ArgumentOutOfRangeException>(0 <= quality && quality <= 100);
            Contract.Ensures(Contract.Result<EncoderParameters>() != null);

            EncoderParameters encParams = new EncoderParameters(1);
            encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            return encParams;
        }
    }
}
