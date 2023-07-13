using Plugin.Maui.Audio;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Timers;
using System.Text;


namespace AudioScriptSync;

public partial class MainPage : ContentPage
{
    
    private readonly MainPageModel model;
    private readonly IAudioManager audios;
    IAudioPlayer player;
    private System.Timers.Timer aTimer;
    private DateTime startTime;
    int currentIndex = 0;


    public MainPage(MainPageModel model, IAudioManager audios)
	{
		InitializeComponent();
        this.model = model;
        this.audios = audios;
        this.BindingContext = model;

        //"/Users/zc/test/beedata/pumaatlarge.txt"
        model.ScriptFile = Preferences.Get("LastScriptFile", "");
        OpenScript();
        model.AudioFile = Preferences.Get("LastAudioFile", "");

    }
 
    private void SetTimer()
    {
        // Create a timer with a two second interval.
        aTimer = new System.Timers.Timer(100);
        // Hook up the Elapsed event for the timer. 
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
        startTime = DateTime.Now;
    }

    private void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        var span = startTime - DateTime.Now;
        model.ElapsedTime = span;
   
    }

    void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        if (model.EditMode == true)
            EndEdit();
        else if (model.ButtonText == "Start")
            Start();
        else if (model.ButtonText == "Stop")
            Stop();
    }

    /// <summary>
    /// 
    /// </summary>
    private void EndEdit()
    {
        model.EditMode = false;
        model.ButtonText = "Start";

        if(model.ScriptModified)
        {
            var sb = new StringBuilder();
            foreach (var seg in model.Segments)
            {
                sb.AppendLine(seg.Text);
            }
            File.WriteAllText(model.SaveToScriptFile, sb.ToString());
        }
    }

    private void Stop()
    {
        var last = model.Segments.LastOrDefault();
        if (last != null && last.TimeEnd.TotalSeconds == 0)
            last.TimeEnd = TimeSpan.FromSeconds(player.Duration);

        if(player.IsPlaying)
            player.Stop();
        aTimer.Stop();
        
        OutputFile();

        model.ButtonText = "Start";

    }

    private void OutputFile()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < model.Segments.Count; i++)
        {

            var seg = model.Segments[i];
            sb.AppendLine((i+1).ToString());
            sb.Append(seg.TimeStart.ToString("hh':'mm':'ss','fff"));
            sb.Append(" --> ");
            sb.AppendLine(seg.TimeEnd.ToString("hh':'mm':'ss','fff"));
            sb.AppendLine(seg.Text);
            sb.AppendLine();
        }

        var path = GetModifiedFilePath(model.ScriptFile, "_timeline");
        File.WriteAllText(path, sb.ToString());
        
    }

    private void Start()
    {
        currentIndex = 0;
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

    //If originalName_edited.txt exists, SaveToScriptFile will point to it.
    void OpenScript()
    {
        if (!File.Exists(model.ScriptFile))
            return;

        model.ButtonText = "End Edit";
        model.EditMode = true;
        
        var firstLoadFrom = model.ScriptFile;
        var modifiedFile = GetModifiedFilePath(model.ScriptFile, "_edited");
        model.SaveToScriptFile = modifiedFile;
        if (File.Exists(modifiedFile))
            firstLoadFrom = modifiedFile;

        
        var content = File.ReadAllText(firstLoadFrom);
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
        model.ScriptFile = result.FullPath;
        Preferences.Set("LastScriptFile", result.FullPath);
        OpenScript();
    }

    private string GetModifiedFilePath(string originalFilePath, string postfix)
    {
        var file = new FileInfo(originalFilePath);
        return Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + postfix + file.Extension);
    }
}

