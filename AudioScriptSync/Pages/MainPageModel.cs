using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AudioScriptSync;

public partial class MainPageModel : ObservableObject
{
    [ObservableProperty]
    string audioFile;

    [ObservableProperty]
    string timelineFile;

    [ObservableProperty]
    string saveToScriptFile;

    [ObservableProperty]
    ObservableCollection<ScriptSegment> segments = new ObservableCollection<ScriptSegment>();

    [ObservableProperty]
    TimeSpan elapsedTime;

    [ObservableProperty]
    string buttonText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotEditMode))]
    bool editMode = true;

    public bool NotEditMode => !EditMode;


    [ObservableProperty]
    bool scriptModified;

    [ObservableProperty]
    public bool pickedFiles;
       
}

