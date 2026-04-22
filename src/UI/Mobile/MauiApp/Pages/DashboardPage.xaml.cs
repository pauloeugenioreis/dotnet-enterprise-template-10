using EnterpriseTemplate.MauiApp.Services;

namespace EnterpriseTemplate.MauiApp.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly IOrderService _orderService;

    public DashboardPage(IOrderService orderService)
    {
        InitializeComponent();
        _orderService = orderService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadDataAsync();
        RefreshView.IsRefreshing = false;
    }

    private async Task LoadDataAsync()
    {
        SkeletonLayout.IsVisible = true;
        StatsLayout.IsVisible = false;
        ErrorFrame.IsVisible = false;

        try
        {
            var stats = await _orderService.GetStatisticsAsync();

            if (stats is not null)
            {
                RevenueLabel.Text = stats.TotalRevenue.ToString("C");
                OrdersLabel.Text = stats.TotalOrders.ToString("N0");
                AvgLabel.Text = stats.AverageOrderValue.ToString("C");
                TopProductsList.ItemsSource = stats.TopProducts?.Take(5).ToList();

                StatsLayout.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erro ao carregar dados: {ex.Message}";
            ErrorFrame.IsVisible = true;
        }
        finally
        {
            SkeletonLayout.IsVisible = false;
        }
    }
}
