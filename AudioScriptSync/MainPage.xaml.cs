using Plugin.Maui.Audio;
using CommunityToolkit.Mvvm.ComponentModel;
namespace AudioScriptSync;

public partial class MainPage : ContentPage
{
    
    private readonly MainPageModel model;
    private readonly IAudioManager audios;

    public MainPage(MainPageModel model, IAudioManager audios)
	{
		InitializeComponent();
        this.model = model;
        this.audios = audios;
        this.BindingContext = model;

        model.ScriptFile = "/Users/zc/test/beedata/pumaatlarge.txt";
        model.AudioFile = "/Users/zc/test/beedata/pumaatlarge.mp3";
    }
    IAudioPlayer player;

    void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        

        if (player == null)
        {
            using (var fileStream = File.Open(model.AudioFile, FileMode.Open))
            {
                player = audios.CreatePlayer(fileStream);
            }
        }
        player.Play();
    }


    void OpenScript(System.Object sender, System.EventArgs e)
    {

    }

}


public partial class MainPageModel : ObservableObject
{
    [ObservableProperty]
    string audioFile;

    [ObservableProperty]
    string scriptFile;

}

