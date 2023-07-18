using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioScriptSync;

public class ModelBase: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
public class MainPageModel: ModelBase
{

    string _AudioFile;
    public string AudioFile { get => _AudioFile; set { if (_AudioFile == value) return; _AudioFile = value; OnPropertyChanged(); } }



    string _TimelineFile;
    public string TimelineFile { get => _TimelineFile; set { if (_TimelineFile == value) return; _TimelineFile = value; OnPropertyChanged(); } }



    string _SaveToScriptFile;
    public string SaveToScriptFile { get => _SaveToScriptFile; set { if (_SaveToScriptFile == value) return; _SaveToScriptFile = value; OnPropertyChanged(); } }


    ObservableCollection<ScriptSegment> _Segments;
    public ObservableCollection<ScriptSegment> Segments { get => _Segments; set { if (_Segments == value) return; _Segments = value; OnPropertyChanged(); } }



    TimeSpan _ElapsedTime;
    public TimeSpan ElapsedTime { get => _ElapsedTime; set { if (_ElapsedTime == value) return; _ElapsedTime = value; OnPropertyChanged(); } }



    string _ButtonText;
    public string ButtonText { get => _ButtonText; set { if (_ButtonText == value) return; _ButtonText = value; OnPropertyChanged(); } }


    bool _EditMode;
    public bool EditMode { get => _EditMode; set { if (_EditMode == value) return; _EditMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(NotEditMode)); } }


    public bool NotEditMode => !EditMode;



    bool _ScriptModified;
    public bool ScriptModified { get => _ScriptModified; set { if (_ScriptModified == value) return; _ScriptModified = value; OnPropertyChanged(); } }



    bool _PickedFiles;
    public bool PickedFiles { get => _PickedFiles; set { if (_PickedFiles == value) return; _PickedFiles = value; OnPropertyChanged(); } }


}

