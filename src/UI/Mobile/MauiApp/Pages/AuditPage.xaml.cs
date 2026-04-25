using EnterpriseTemplate.MauiApp.Services;

namespace EnterpriseTemplate.MauiApp.Pages;

public partial class AuditPage : ContentPage
{
    private readonly IAuditService _auditService;

    public AuditPage(IAuditService auditService)
    {
        InitializeComponent();
        _auditService = auditService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAuditLogsAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadAuditLogsAsync();
        RefreshView.IsRefreshing = false;
    }

    private async Task LoadAuditLogsAsync()
    {
        LoadingIndicator.IsVisible = true;
        AuditList.IsVisible = false;

        try
        {
            var result = await _auditService.GetAuditLogsAsync("Order");
            if (result?.Items != null)
            {
                AuditList.ItemsSource = result.Items;
                AuditList.IsVisible = true;
            }
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }
}
