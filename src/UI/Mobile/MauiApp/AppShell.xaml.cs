namespace EnterpriseTemplate.MauiApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Verificar se já está autenticado ao iniciar
        CheckAuthenticationAsync();
    }

    private async void CheckAuthenticationAsync()
    {
        var token = await Services.Base.AuthTokenHandler.GetTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            // Token encontrado → ir direto pro Dashboard
            await GoToAsync("//dashboard");
        }
        else
        {
            // Sem token → tela de Login
            await GoToAsync("//login");
        }
    }
}
