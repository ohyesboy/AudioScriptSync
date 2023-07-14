using CommunityToolkit.Mvvm.ComponentModel;

namespace AudioScriptSync;

[QueryProperty(nameof(TimelineFile), nameof(TimelineFile))]
public partial class ArticleEditPageModel: ObservableObject
{
    [ObservableProperty]
    string timelineFile;
}