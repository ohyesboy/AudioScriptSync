using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AudioScriptSync;

public partial class ScriptSegment : ObservableObject
{
    private readonly MainPageModel model;
    [ObservableProperty]
    string text;

    [ObservableProperty]
    bool isCurrent;

    [ObservableProperty]
    TimeSpan timeStamp;

    public ScriptSegment(MainPageModel model)
    {
        this.model = model;

    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(Text))
        {
            if (string.IsNullOrEmpty(Text))
                model.Segments.Remove(this);
        }
    }
}

