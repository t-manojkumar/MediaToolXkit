using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using Windows.Storage;

namespace ToolX;

public partial class AnalyzableImage : ObservableObject
{
    [ObservableProperty]
    private BitmapImage? thumbnail;

    [ObservableProperty]
    private double sharpnessScore;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBestVisibility))]
    private bool isBest;

    public Visibility IsBestVisibility => !IsBest ? Visibility.Collapsed : Visibility.Visible; 

    public StorageFile? File { get; set; }
}