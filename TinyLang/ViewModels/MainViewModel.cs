using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using TinyLang.Language;

namespace TinyLang.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private string _source = "let value = 6;\nprint value * 7;";
    [ObservableProperty] private string _output = "A saída aparece aqui.";

    [RelayCommand]
    private void Run()
    {
        try { Output = TinyLanguage.Execute(Source).Output; }
        catch (FormatException ex) { Output = $"Erro: {ex.Message}"; }
    }
}
