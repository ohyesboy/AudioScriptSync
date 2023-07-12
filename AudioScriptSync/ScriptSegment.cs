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


    partial void OnTextChanged(string? oldValue, string newValue)
    {
        if (oldValue == null)
            return;
        model.ScriptModified = true;
        if (string.IsNullOrEmpty(newValue))
        {
            model.Segments.Remove(this);
        }
        var parts = newValue.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length > 1)
        {
            this.text = parts[0];
            this.OnPropertyChanged(nameof(Text));
            var pos = model.Segments.IndexOf(this);
            for (int i = 1; i < parts.Length; i++)
            {
                model.Segments.Insert(pos + i, new ScriptSegment(model) { Text = parts[i] });
            }
        }
    }
}

