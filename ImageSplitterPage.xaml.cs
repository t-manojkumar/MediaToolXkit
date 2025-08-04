using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ToolX
{
    public sealed partial class ImageSplitterPage : Page
    {
        private CanvasBitmap? loadedImage;
        private string originalFileName = "image";
        public ObservableCollection<BitmapImage> SplitImages { get; set; }

        public ImageSplitterPage()
        {
            this.InitializeComponent();
            SplitImages = new ObservableCollection<BitmapImage>();
            SplitResultsGrid.ItemsSource = SplitImages;
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
                SplitButton.IsEnabled = true;
                SplitImages.Clear();
                ImageCanvas.Invalidate();
            }
        }

        private async void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.loadedImage == null) return;

            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle(MainWindow.m_window);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder? folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                SplitImages.Clear();
                int rows = (int)RowsInput.Value;
                int cols = (int)ColumnsInput.Value;
                double singleWidth = this.loadedImage.Size.Width / cols;
                double singleHeight = this.loadedImage.Size.Height / rows;
                var device = CanvasDevice.GetSharedDevice();

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var sourceRect = new Rect(c * singleWidth, r * singleHeight, singleWidth, singleHeight);
                        using (var renderTarget = new CanvasRenderTarget(device, (float)singleWidth, (float)singleHeight, 96))
                        {
                            using (var ds = renderTarget.CreateDrawingSession())
                            {
                                ds.DrawImage(this.loadedImage, new Rect(0, 0, singleWidth, singleHeight), sourceRect);
                            }

                            var newFileName = $"{this.originalFileName}_{r + 1}x{c + 1}.png";
                            var newFile = await folder.CreateFileAsync(newFileName, CreationCollisionOption.GenerateUniqueName);

                            using (var stream = await newFile.OpenStreamForWriteAsync())
                            {
                                await renderTarget.SaveAsync(stream.AsRandomAccessStream(), CanvasBitmapFileFormat.Png);
                            }

                            var bitmapImage = new BitmapImage();
                            using (var savedStream = await newFile.OpenAsync(FileAccessMode.Read))
                            {
                                await bitmapImage.SetSourceAsync(savedStream);
                            }
                            SplitImages.Add(bitmapImage);
                        }
                    }
                }
            }
        }

        private void ImageCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (this.loadedImage != null)
            {
                args.DrawingSession.DrawImage(this.loadedImage, new Rect(0, 0, sender.ActualWidth, sender.ActualHeight));
            }
        }
    }
}