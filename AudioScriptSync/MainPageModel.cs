using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AudioScriptSync;

public partial class MainPageModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PickedFiles))]
    string audioFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PickedFiles))]
    string scriptFile;

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
    bool isBusy;

    public bool PickedFiles
        => AudioFile != null && ScriptFile != null
        && File.Exists(AudioFile) && File.Exists(ScriptFile);
}

