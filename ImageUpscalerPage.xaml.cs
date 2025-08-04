using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Numerics;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ToolX
{
    public sealed partial class ImageUpscalerPage : Page
    {
        private CanvasBitmap? loadedImage;
        private string originalFileName = "image";

        public ImageUpscalerPage()
        {
            this.InitializeComponent();
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".bmp");

            var hwnd = WindowNative.GetWindowHandle(MainWindow.m_window);
            InitializeWithWindow.Initialize(fileOpenPicker, hwnd);

            StorageFile? file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                this.originalFileName = Path.GetFileNameWithoutExtension(file.Name);
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    this.loadedImage = await CanvasBitmap.LoadAsync(ImageCanvas, stream);
                }
                SaveButton.IsEnabled = true;
                ImageCanvas.Invalidate();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.loadedImage == null) return;

            float scale = 2.0f;
            if (Scale4x.IsChecked == true) scale = 4.0f;
            if (Scale8x.IsChecked == true) scale = 8.0f;

            var fileSavePicker = new FileSavePicker();
            fileSavePicker.FileTypeChoices.Add("PNG Image", new[] { ".png" });
            fileSavePicker.SuggestedFileName = $"{this.originalFileName}_{scale}x_upscaled";

            var hwnd = WindowNative.GetWindowHandle(MainWindow.m_window);
            InitializeWithWindow.Initialize(fileSavePicker, hwnd);

            StorageFile? file = await fileSavePicker.PickSaveFileAsync();

            if (file != null)
            {
                var device = CanvasDevice.GetSharedDevice();
                var newWidth = (float)this.loadedImage.Size.Width * scale;
                var newHeight = (float)this.loadedImage.Size.Height * scale;

                using (var renderTarget = new CanvasRenderTarget(device, newWidth, newHeight, 96))
                {
                    using (var ds = renderTarget.CreateDrawingSession())
                    {
                        var scaleEffect = new ScaleEffect
                        {
                            Source = this.loadedImage,
                            Scale = new Vector2(scale, scale),
                            // Using HighQualityCubic as we discovered it works reliably on your system
                            InterpolationMode = CanvasImageInterpolation.HighQualityCubic
                        };
                        ds.DrawImage(scaleEffect);
                    }

                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        await renderTarget.SaveAsync(stream.AsRandomAccessStream(), CanvasBitmapFileFormat.Png);
                    }
                }
            }
        }

        private void ImageCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (this.loadedImage != null)
            {
                args.DrawingSession.DrawImage(this.loadedImage, new Windows.Foundation.Rect(0, 0, sender.ActualWidth, sender.ActualHeight));
            }
        }
    }
}