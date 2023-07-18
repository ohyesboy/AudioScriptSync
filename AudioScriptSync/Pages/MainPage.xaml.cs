using Plugin.Maui.Audio;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Timers;
using System.Text;
using System.Text.Json;

namespace AudioScriptSync;

public partial class MainPage : ContentPage
{
    
    private readonly MainPageModel model;
    private readonly IAudioManager audios;
    IAudioPlayer player;
    private System.Timers.Timer aTimer;

    int currentIndex = 0;


    public MainPage(MainPageModel model, IAudioManager audios)
	{
		InitializeComponent();
        this.model = model;
        this.audios = audios;
        this.BindingContext = model;
        model.TimelineFile = Preferences.Get("LastTimelineFile", "");
        model.AudioFile = Preferences.Get("LastAudioFile", "");
        LoadTimelineFile();
        model.ButtonText = "Start";

    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        SaveTimelineFile();
    }

    private void SetTimer()
    {
        // Create a timer with a two second interval.
        aTimer = new System.Timers.Timer(100);
        // Hook up the Elapsed event for the timer. 
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    private void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        model.ElapsedTime = TimeSpan.FromSeconds(player.CurrentPosition);
   
    }

    void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        if (model.ButtonText == "Start")
            Start();
        else if (model.ButtonText == "Stop")
            Stop();
    }

   


    private void Stop()
    {
        var last = model.Segments.LastOrDefault();
        if (last != null && last.TimeEnd.TotalSeconds == 0)
            last.TimeEnd = TimeSpan.FromSeconds(player.Duration);

        if(player.IsPlaying)
            player.Stop();
        aTimer.Stop();
        
        model.ButtonText = "Start";
        model.EditMode = true;
    }

 

    private void Start()
    {
        currentIndex = 0;
        model.EditMode = false;
        model.ButtonText = "Stop";
        if (player == null)
        {
            using (var fileStream = File.Open(model.AudioFile, FileMode.Open))
            {
                player = audios.CreatePlayer(fileStream);
                player.PlaybackEnded += Player_PlaybackEnded;
            }
        }

        ResetScript();

        if (model.Segments.Any())
            model.Segments[0].IsCurrent = true;
        player.Play();
        SetTimer();
    }

    private void ResetScript()
    {
        foreach(var seg in model.Segments)
        {
            seg.IsCurrent = false;
            seg.TimeStart = new TimeSpan();
            seg.TimeEnd = new TimeSpan();
        }
    }

    private void Player_PlaybackEnded(object sender, EventArgs e)
    {
        Stop();
    }

    void SaveTimelineFile()
    {
        var segments = model.Segments.ToList();
        JsonSerializerOptions jso = new JsonSerializerOptions();
        jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        var json = JsonSerializer.Serialize(segments, jso);
        File.WriteAllText(model.TimelineFile, json);

    }

    void LoadTimelineFile()
    {
     
        if (!File.Exists(model.TimelineFile))
            return;
        var json = File.ReadAllText(model.TimelineFile);
        var segments = JsonSerializer.Deserialize<List<ScriptSegment>>(json);
        foreach(var seg in segments)
        {
            seg.IsCurrent = false;
            seg.model = this.model;
        }
        model.Segments = new ObservableCollection<ScriptSegment>(segments);
        model.PickedFiles = true;
    }


    void LoadRawTextFile(string textFile)
    {
        if (!File.Exists(textFile))
            return;

        
        model.PickedFiles = true;
        
        model.EditMode = true;

        var content = File.ReadAllText(textFile);
        content = content.Replace("\r\n", "\n").Replace("\r", "\n");

        var segsRaw = content.Split(new[] { ',','.','!','?','\n' }, StringSplitOptions.None);
       
        var list = new List<ScriptSegment>();
        int index = 0;
        foreach(var seg in segsRaw)
        {
            var text = seg;
            if(index + seg.Length < content.Length)
                text += content[index + seg.Length];
            list.Add(new ScriptSegment(model) { Text = text.Trim() });
            index += seg.Length + 1;
        }
        
        list = list.Where(x => x.Text!="").ToList() ;

        model.Segments = new ObservableCollection<ScriptSegment>(list);

    }

   
    void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        
        if (player == null || !player.IsPlaying || !model.Segments.Any())
            return;
        var now = TimeSpan.FromSeconds(player.CurrentPosition);
        model.Segments[currentIndex].IsCurrent = false;
        model.Segments[currentIndex].TimeEnd = now;
        currentIndex++;
        if(currentIndex< model.Segments.Count)
        {
            var line = model.Segments[currentIndex];
            line.TimeStart = now;
            line.IsCurrent = true;
            
        }
           
    }

    async void ChooseAudio(System.Object sender, System.EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions { });
        model.AudioFile = result.FullPath;
        Preferences.Set("LastAudioFile", result.FullPath);

    }

    async void ChooseScript(System.Object sender, System.EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions { });
        model.TimelineFile = GetModifiedFilePath( result.FullPath, "",".json");
        Preferences.Set("LastTimelineFile", model.TimelineFile);
        LoadRawTextFile(result.FullPath);
    }

    private string GetModifiedFilePath(string originalFilePath, string postfix, string extension = null)
    {
        var file = new FileInfo(originalFilePath);
        return Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + postfix + (extension?? file.Extension));
    }

   

    void TapGestureRecognizer_SecondaryTapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        if (player == null)
            return;

        model.Segments[currentIndex].TimeStart = new TimeSpan();
        if (currentIndex == 0)
        {
            player.Seek(0);
            return;
        }

        model.Segments[currentIndex].IsCurrent = false;
        currentIndex--;
        model.Segments[currentIndex].IsCurrent = true;
        model.Segments[currentIndex].TimeEnd = new TimeSpan();
        player.Seek(model.Segments[currentIndex].TimeStart.TotalSeconds);
    }

    async void EditArticle_Clicked(System.Object sender, System.EventArgs e)
    {
        await GoToArticleEdit();
    }

    async Task GoToArticleEdit()
    {
        SaveTimelineFile();
        var path = GetModifiedFilePath(model.TimelineFile, "_all");
        await Shell.Current.GoToAsync(nameof(ArticleEditPage),
          new Dictionary<string, object> {
              {"TimelineFile",path },
              {"Segments",model.Segments.ToList() }
          });
    }
}

