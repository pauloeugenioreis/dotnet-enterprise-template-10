using EnterpriseTemplate.MauiApp.Services;

namespace EnterpriseTemplate.MauiApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;

    public LoginPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Por favor, preencha todos os campos.";
            ErrorLabel.IsVisible = true;
            return;
        }

        ErrorLabel.IsVisible = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var result = await _authService.LoginAsync(new ProjectTemplate.SharedModels.LoginRequestDto
            {
                Email = email,
                Password = password
            });

            if (result?.Token is not null)
            {
                // Navegar para o Dashboard
                await Shell.Current.GoToAsync("//dashboard");
            }
            else
            {
                ErrorLabel.Text = "Credenciais inválidas. Tente novamente.";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception)
        {
            ErrorLabel.Text = "Não foi possível conectar ao servidor.";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("register");
}
