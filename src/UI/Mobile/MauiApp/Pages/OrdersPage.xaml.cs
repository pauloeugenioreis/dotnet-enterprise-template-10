using EnterpriseTemplate.MauiApp.Services;

namespace EnterpriseTemplate.MauiApp.Pages;

public partial class OrdersPage : ContentPage
{
    private readonly IOrderService _orderService;
    private int _currentPage = 1;

    public OrdersPage(IOrderService orderService)
    {
        InitializeComponent();
        _orderService = orderService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrdersAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        _currentPage = 1;
        await LoadOrdersAsync();
        RefreshView.IsRefreshing = false;
    }

    private async Task LoadOrdersAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        OrdersList.IsVisible = false;
        EmptyState.IsVisible = false;
        ErrorFrame.IsVisible = false;

        try
        {
            var result = await _orderService.GetOrdersAsync(_currentPage);

            if (result?.Items is { Count: > 0 })
            {
                OrdersList.ItemsSource = result.Items;
                OrdersList.IsVisible = true;
            }
            else
            {
                EmptyState.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erro: {ex.Message}";
            ErrorFrame.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
