using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace ToolX
{
    public sealed partial class FindBestImagePage : Page
    {
        public ObservableCollection<AnalyzableImage> Images { get; set; }

        public FindBestImagePage()
        {
            this.InitializeComponent();
            Images = new ObservableCollection<AnalyzableImage>();
            ImageGrid.ItemsSource = Images;
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");

            var hwnd = WindowNative.GetWindowHandle(MainWindow.m_window);
            InitializeWithWindow.Initialize(fileOpenPicker, hwnd);

            var files = await fileOpenPicker.PickMultipleFilesAsync();
            if (files != null && files.Any())
            {
                Images.Clear();
                ResultText.Text = "";
                AnalyzeButton.IsEnabled = false;
                foreach (var file in files)
                {
                    var image = new AnalyzableImage { File = file };
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        var thumb = new BitmapImage();
                        await thumb.SetSourceAsync(stream);
                        image.Thumbnail = thumb;
                    }
                    Images.Add(image);
                }
                AnalyzeButton.IsEnabled = true;
            }
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Images.Any()) return;

            Progress.IsActive = true;
            AnalyzeButton.IsEnabled = false;
            ResultText.Text = "Analyzing...";

            // Reset previous results
            foreach (var image in Images) { image.IsBest = false; }

            foreach (var image in Images)
            {
                image.SharpnessScore = await CalculateSharpnessAsync(image.File);
            }

            var bestImage = Images.OrderByDescending(img => img.SharpnessScore).FirstOrDefault();
            if (bestImage != null)
            {
                bestImage.IsBest = true;
                ResultText.Text = $"Best Image Found: {bestImage.File.Name}";
            }
            else
            {
                ResultText.Text = "Analysis complete. Could not determine the best image.";
            }

            Progress.IsActive = false;
            AnalyzeButton.IsEnabled = true;
        }

        private async Task<double> CalculateSharpnessAsync(StorageFile file)
        {
            try
            {
                var device = CanvasDevice.GetSharedDevice();
                using (var stream = await file.OpenReadAsync())
                {
                    var canvasBitmap = await CanvasBitmap.LoadAsync(device, stream);

                    var edgeEffect = new EdgeDetectionEffect
                    {
                        Source = canvasBitmap,
                        Amount = 1.0f,
                        BlurAmount = 0.0f
                    };

                    using (var renderTarget = new CanvasRenderTarget(device, canvasBitmap.SizeInPixels.Width, canvasBitmap.SizeInPixels.Height, 96))
                    {
                        using (var ds = renderTarget.CreateDrawingSession())
                        {
                            ds.DrawImage(edgeEffect);
                        }

                        var pixels = renderTarget.GetPixelBytes();
                        double totalBrightness = 0;
                        for (int i = 0; i < pixels.Length; i += 4)
                        {
                            totalBrightness += (pixels[i] + pixels[i + 1] + pixels[i + 2]);
                        }
                        return totalBrightness / (pixels.Length * 3.0);
                    }
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}