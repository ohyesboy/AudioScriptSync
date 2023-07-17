using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace AudioScriptSync;

public partial class ArticleEditPage : ContentPage
{
    private readonly ArticleEditPageModel model;
    private readonly IConfiguration config;

    public ArticleEditPage(ArticleEditPageModel model, IConfiguration config)
	{
		InitializeComponent();
		this.BindingContext = model;
        this.model = model;
        this.config = config;
    }

    async void TranslateClicked(object sender, EventArgs e)
    {
        var apiKey = config["apikey"];
        if(apiKey == null)
        {
            await DisplayAlert("Error", "Missing [apikey] from config", "Ok");
            return;
        }
        var openai = new OpenAiClient(apiKey);

        //remove empty ones
        for (int i = 0; i < model.Paragraphs.Count; i++)
        {
            if (model.Paragraphs[i].Segments.Count == 0)
            {
                model.Paragraphs.RemoveAt(i);
                i--;
            }
        }
        var start = DateTime.Now;
        model.IsBusy = true;
        var separator = "\r\n----------\r\n";
        var combinedString = string.Join(separator, model.Paragraphs.Select(p => string.Join("", p.Segments.Select(x => x.Text))));
        
        var response = await openai.Talk($"Translate this text to Chinese(keep the line break and separator '----------'): {combinedString}");

        model.IsBusy = false;
        var responseParts = response.Split("----------", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        //if(responseParts.Length!= model.Paragraphs.Count)
        //{
        //    await DisplayAlert("Error",
        //        $"Translation parts count({responseParts.Length}) does not match original paragraph count({model.Paragraphs.Count})", "Ok");
        //    return;
        //}

        var length = Math.Min(responseParts.Length, model.Paragraphs.Count);
        for (int i = 0; i < length ; i++)
        {
            model.Paragraphs[i].Translation = responseParts[i];
        }
        var timespan = DateTime.Now - start;
        await DisplayAlert("Succeed", $"Took {timespan.TotalSeconds.ToString("0.0")} seconds", "Ok");
    }

   
    async void SaveClicked(object sender, EventArgs e)
    {
        var paras = model.Paragraphs.Select(x => new ParagraphOutput { Translation = x.Translation,Segments = x.Segments.Select(x=>x.Text).ToList() }).ToList();
        JsonSerializerOptions jso = new JsonSerializerOptions();
        jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        var json = JsonSerializer.Serialize(paras,jso);
        File.WriteAllText(model.TimelineFile, json);
        await DisplayAlert("Alert", "File saved\r\n" + model.TimelineFile, "OK");

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
