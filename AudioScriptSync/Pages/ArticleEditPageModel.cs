using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AudioScriptSync;

[QueryProperty(nameof(TimelineFile), nameof(TimelineFile))]
[QueryProperty(nameof(Segments), nameof(Segments))]
public partial class ArticleEditPageModel: ObservableObject
{
    [ObservableProperty]
    string timelineFile;

    [ObservableProperty]
    List<ScriptSegment> segments = null;

    partial void OnSegmentsChanged(List<ScriptSegment> oldValue, List<ScriptSegment> scriptSegments)
    {
        if (oldValue != null)
            return;
        var pgs = new List<Paragraph>();
        var paragraph = new Paragraph();
        pgs.Add(paragraph);
        int order = 0;
        foreach(var seg in Segments)
        {
            order++;

            var seg2 = new ParagraphSegment() { Text = seg.Text, Order = order };
            paragraph.Segments.Add(seg2);
           if(seg.Text.EndsWith(".") || seg.Text.EndsWith("?")|| seg.Text.EndsWith("!"))
            {
                paragraph = new Paragraph();
                pgs.Add(paragraph);
            }
        }
        Paragraphs = new ObservableCollection<Paragraph>(pgs);
    }

    [ObservableProperty]
    ObservableCollection<Paragraph> paragraphs;


}

public partial class Paragraph: ObservableObject
{
    public Paragraph()
    {
        Segments.CollectionChanged += Segments_CollectionChanged;

    }

    private void Segments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SegmentNumbers));
    }

    [ObservableProperty]
    string translation;

    [ObservableProperty]
    [NonSerialized]
    [NotifyPropertyChangedFor(nameof(SegmentNumbers))]
    ObservableCollection<ParagraphSegment> segments = new ObservableCollection<ParagraphSegment>();

    [ObservableProperty]
    bool isDragHover;


    public string SegmentNumbers
    {
        get
        {
            if (Segments.Count == 0)
                return "[EMPTY]";
            return string.Join(",", Segments.Select(x => x.Order));
        }
    }
}

public partial class ParagraphSegment: ObservableObject
{
    [ObservableProperty]
    int order;

    [ObservableProperty]
    string text;
}