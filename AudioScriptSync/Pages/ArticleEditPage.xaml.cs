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
}
