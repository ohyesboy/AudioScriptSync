using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AudioScriptSync;

public class ScriptSegment : ModelBase
{
    internal MainPageModel model;

    string _Text;
    public string Text { get => _Text; set { if (_Text == value) return; var oldValue = _Text; _Text = value; OnTextChanged(oldValue, value); OnPropertyChanged(); } }



    bool _IsCurrent;
    public bool IsCurrent { get => _IsCurrent; set { if (_IsCurrent == value) return; _IsCurrent = value; OnPropertyChanged(); } }



    TimeSpan _TimeStart;
    public TimeSpan TimeStart { get => _TimeStart; set { if (_TimeStart == value) return; _TimeStart = value; OnPropertyChanged(); } }




    TimeSpan _TimeEnd;
    public TimeSpan TimeEnd { get => _TimeEnd; set { if (_TimeEnd == value) return; _TimeEnd = value; OnPropertyChanged(); } }


    public ScriptSegment()
    {

    }

    public ScriptSegment(MainPageModel model)
    {
        this.model = model;

    }

    void OnTextChanged(string? oldValue, string newValue)
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
            this._Text = parts[0];
            this.OnPropertyChanged(nameof(Text));
            var pos = model.Segments.IndexOf(this);
            for (int i = 1; i < parts.Length; i++)
            {
                model.Segments.Insert(pos + i, new ScriptSegment(model) { Text = parts[i] });
            }
        }
    }
}

