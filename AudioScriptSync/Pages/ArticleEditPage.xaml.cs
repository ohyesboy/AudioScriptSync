using CommunityToolkit.Mvvm.ComponentModel;

namespace AudioScriptSync;

public partial class ArticleEditPage : ContentPage
{
    private readonly ArticleEditPageModel model;

    public ArticleEditPage(ArticleEditPageModel model)
	{
		InitializeComponent();
		this.BindingContext = model;
        this.model = model;
    }
    void DropParagraph(System.Object sender, DropEventArgs e)
    {
        var control = (sender as Element)?.Parent;
        var targetParagraph = control.BindingContext as Paragraph;
        var sourceParagraph = e.Data.Properties["Paragraph"] as Paragraph;
        var sourceSegment = e.Data.Properties["Segment"] as ParagraphSegment;
        var sourceParagraphIndex = model.Paragraphs.IndexOf(sourceParagraph);
        var targetParagraphIndex = model.Paragraphs.IndexOf(targetParagraph);
        targetParagraph.IsDragHover = false;

        //source and target paragraph must be adjacent.
        if (Math.Abs(sourceParagraphIndex - targetParagraphIndex) != 1)
            return;

        var sourceSegIndex = sourceParagraph.Segments.IndexOf(sourceSegment);

        //if target is below (>) source,
        //move source segments (from end to sourceSegIndex) to beginning of target
        if (targetParagraphIndex > sourceParagraphIndex)
        {
            for (int i = sourceParagraph.Segments.Count - 1; i>=sourceSegIndex ; i--)
            {
      
                targetParagraph.Segments.Insert(0, sourceParagraph.Segments[i]);
                sourceParagraph.Segments.RemoveAt(i);
            }
        }
        else  //move source segments (from first to sourceSegIndex) to end of target
        {
            for (int i = 0; i <= sourceSegIndex; i++)
            {
                targetParagraph.Segments.Add(sourceParagraph.Segments[i]);
            }

            for (int i = 0; i <= sourceSegIndex; i++)
            {
                sourceParagraph.Segments.RemoveAt(0);
            }
        }
    }

    void DragOverParagraph(System.Object sender, DragEventArgs e)
    {
        var control = (sender as Element)?.Parent;
        var targetParagraph = control.BindingContext as Paragraph;
        var sourceParagraph = e.Data.Properties["Paragraph"] as Paragraph;
        var sourceParagraphIndex = model.Paragraphs.IndexOf(sourceParagraph);
        var targetParagraphIndex = model.Paragraphs.IndexOf(targetParagraph);

        //source and target paragraph must be adjacent.
        if (Math.Abs(sourceParagraphIndex - targetParagraphIndex) != 1)
            return;

        targetParagraph.IsDragHover = true;
    }

    void DragLeaveParagraph(System.Object sender, DragEventArgs e)
    {
        var control = (sender as Element)?.Parent;
        var targetParagraph = control.BindingContext as Paragraph;
        targetParagraph.IsDragHover = false;
    }


    void DragParagraph(System.Object sender, DragStartingEventArgs e)
    {
        var control = (sender as Element)?.Parent;
        var sourceParagraph = BindingHelper.GetAncestorBindingContext<Paragraph>(control); 
        var seg = control.BindingContext as ParagraphSegment;
        e.Data.Properties["Paragraph"] = sourceParagraph;
        e.Data.Properties["Segment"] = seg;


    }


}

public class BindingHelper
{
    public static T GetAncestorBindingContext<T>(Element ele)
    {
        while (ele!=null)
        {
            if (ele.BindingContext is T ctx)
                return ctx;
            ele = ele.Parent;
        }
        return default(T);
    }
}