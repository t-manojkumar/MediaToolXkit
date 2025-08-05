using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace ToolX;

// This has been changed to inherit from ObservableObject directly
public partial class AnalyzableImage : ObservableObject
{
    private BitmapImage? _thumbnail;
    public BitmapImage? Thumbnail
    {
        get => _thumbnail;
        set => SetProperty(ref _thumbnail, value);
    }

    private double _sharpnessScore;
    public double SharpnessScore
    {
        get => _sharpnessScore;
        set => SetProperty(ref _sharpnessScore, value);
    }

    private bool _isBest;
    public bool IsBest
    {
        get => _isBest;
        set
        {
            if (SetProperty(ref _isBest, value))
            {
                OnPropertyChanged(nameof(IsBestVisibility)); // Manually notify that IsBestVisibility has changed
            }
        }
    }

    public Visibility IsBestVisibility => IsBest ? Visibility.Visible : Visibility.Collapsed;

    public StorageFile? File { get; set; }
}