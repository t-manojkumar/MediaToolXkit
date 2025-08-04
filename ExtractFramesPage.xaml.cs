using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace ToolX
{
    public sealed partial class ExtractFramesPage : Page
    {
        private StorageFile? loadedVideoFile;
        private MediaComposition? mediaComposition;
        private string originalFileName = "video";

        public ExtractFramesPage()
        {
            this.InitializeComponent();
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.FileTypeFilter.Add(".mp4");
            fileOpenPicker.FileTypeFilter.Add(".mov");
            fileOpenPicker.FileTypeFilter.Add(".wmv");

            var hwnd = WindowNative.GetWindowHandle(MainWindow.m_window);
            InitializeWithWindow.Initialize(fileOpenPicker, hwnd);

            this.loadedVideoFile = await fileOpenPicker.PickSingleFileAsync();

            if (this.loadedVideoFile != null)
            {
                this.originalFileName = Path.GetFileNameWithoutExtension(this.loadedVideoFile.Name);
                VideoPlayer.Source = MediaSource.CreateFromStorageFile(this.loadedVideoFile);

                this.mediaComposition = new MediaComposition();
                var clip = await MediaClip.CreateFromFileAsync(this.loadedVideoFile);
                this.mediaComposition.Clips.Add(clip);

                ExtractButton.IsEnabled = true;
            }
        }

        private async void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.mediaComposition == null || this.loadedVideoFile == null) return;

            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle(MainWindow.m_window);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder? folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Progress.IsActive = true;
                ExtractButton.IsEnabled = false;

                int frameCount = (int)FramesInput.Value;
                var timeSpans = new List<TimeSpan>();
                for (int i = 0; i < frameCount; i++)
                {
                    // Space the frames evenly throughout the video's duration
                    var time = TimeSpan.FromMilliseconds(this.mediaComposition.Duration.TotalMilliseconds * i / frameCount);
                    timeSpans.Add(time);
                }

                // This API call gets all the frames (thumbnails) at once
                var thumbnails = await this.mediaComposition.GetThumbnailsAsync(
                    timeSpans, 0, 0, VideoFramePrecision.NearestFrame);

                int frameNumber = 1;
                foreach (var thumbnail in thumbnails)
                {
                    await SaveThumbnailAsync(thumbnail, folder, frameNumber++);
                }

                Progress.IsActive = false;
                ExtractButton.IsEnabled = true;
            }
        }

        // Helper method to save the image stream to a file
        private async Task SaveThumbnailAsync(IInputStream stream, StorageFolder folder, int frameNumber)
        {
            var newFileName = $"{this.originalFileName}_frame_{frameNumber:D3}.png";
            var newFile = await folder.CreateFileAsync(newFileName, CreationCollisionOption.GenerateUniqueName);

            using (var fileStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await RandomAccessStream.CopyAsync(stream, fileStream);
            }
        }
    }
}