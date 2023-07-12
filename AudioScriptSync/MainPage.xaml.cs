using Plugin.Maui.Audio;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Timers;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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
        model.ButtonText = "Start";
        model.IsBusy = true;
        
    }
 
    private void SetTimer()
    {
        // Create a timer with a two second interval.
        aTimer = new System.Timers.Timer(0.2);
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
        if (model.ButtonText == "Start")
            Start();
        else if (model.ButtonText == "Stop")
            Stop();
    }

    private void Stop()
    {
        if(player.IsPlaying)
            player.Stop();
        aTimer.Stop();
        
        OutputFile();
        model.IsBusy = false;
        model.ButtonText = "Start";
    }

    private void OutputFile()
    {
        var sb = new StringBuilder();
        foreach (var seg in model.Segments)
        {
            sb.Append(seg.TimeStamp.ToString("hh':'mm':'ss'.'ff"));
            sb.AppendLine("\t" + seg.Text);
        }

        var scriptFile = new FileInfo(model.ScriptFile);
        var path = Path.Combine(scriptFile.Directory.FullName, scriptFile.Name+"_timeline"+scriptFile.Extension);
        File.WriteAllText(path, sb.ToString());
        
    }

    private void Start()
    {
        OpenScript();
        currentIndex = 0;
        model.IsBusy = true;
        model.ButtonText = "Stop";
        if (player == null)
        {
            using (var fileStream = File.Open(model.AudioFile, FileMode.Open))
            {
                player = audios.CreatePlayer(fileStream);
                player.PlaybackEnded += Player_PlaybackEnded;
            }
        }
       
        player.Play();
        SetTimer();
    }

    private void Player_PlaybackEnded(object sender, EventArgs e)
    {
        Stop();
    }

    void OpenScript()
    {
        if (!File.Exists(model.ScriptFile))
            return;
        var content = File.ReadAllText(model.ScriptFile);
        var segsRaw = content.Split(new char[] { ',', '.', '?', '!' });

        var list = new List<ScriptSegment>();
        int index = 0;
        foreach(var seg in segsRaw)
        {
            var text = seg;
            if(index + seg.Length < content.Length)
                text += content[index + seg.Length];
            list.Add(new ScriptSegment { Text = text.Trim().Replace(Environment.NewLine, " ") });
            index += seg.Length + 1;
        }
        
        list = list.Where(x => x.Text!="").ToList() ;

        if(list.Any())
            list[0].IsCurrent = true;
        model.Segments = new ObservableCollection<ScriptSegment>(list);

    }

   
    void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        
        if (!model.Segments.Any())
            return;
        model.Segments[currentIndex].IsCurrent = false;
        model.Segments[currentIndex].TimeStamp = (DateTime.Now - startTime);
        currentIndex++;
        if(currentIndex< model.Segments.Count)
        {
            var line = model.Segments[currentIndex];
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
}

public partial class ScriptSegment: ObservableObject
{
    [ObservableProperty]
    string text;

    [ObservableProperty]
    bool isCurrent;

    [ObservableProperty]
    TimeSpan timeStamp;

}


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
    bool isBusy;

    public bool PickedFiles
        => AudioFile != null && ScriptFile != null
        && File.Exists(AudioFile) && File.Exists(ScriptFile);
}

