using EnterpriseTemplate.MauiApp.Services;
using ProjectTemplate.SharedModels;

namespace EnterpriseTemplate.MauiApp.Pages;

public partial class OrdersPage : ContentPage
{
    private readonly IOrderService _orderService;
    private string _currentSearch = "";
    private string _currentStatus = null;
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

    private async void OnSearchCompleted(object sender, EventArgs e)
    {
        _currentSearch = SearchEntry.Text;
        _currentPage = 1;
        await LoadOrdersAsync();
    }

    private async void OnDateFilterClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Filtro de Data", "Filtro de data avançado disponível na próxima versão.", "OK");
    }

    private async void OnFilterAll(object sender, EventArgs e)
    {
        _currentStatus = null;
        _currentPage = 1;
        UpdateFilterButtons();
        await LoadOrdersAsync();
    }

    private async void OnFilterPending(object sender, EventArgs e)
    {
        _currentStatus = "Pending";
        _currentPage = 1;
        UpdateFilterButtons();
        await LoadOrdersAsync();
    }

    private async void OnFilterShipped(object sender, EventArgs e)
    {
        _currentStatus = "Shipped";
        _currentPage = 1;
        UpdateFilterButtons();
        await LoadOrdersAsync();
    }

    private async void OnFilterDelivered(object sender, EventArgs e)
    {
        _currentStatus = "Delivered";
        _currentPage = 1;
        UpdateFilterButtons();
        await LoadOrdersAsync();
    }

    private void UpdateFilterButtons()
    {
        BtnAll.BackgroundColor = _currentStatus == null ? Color.FromArgb("#0284C7") : Color.FromArgb("#F1F5F9");
        BtnAll.TextColor = _currentStatus == null ? Colors.White : Color.FromArgb("#64748B");

        BtnPending.BackgroundColor = _currentStatus == "Pending" ? Color.FromArgb("#0284C7") : Color.FromArgb("#F1F5F9");
        BtnPending.TextColor = _currentStatus == "Pending" ? Colors.White : Color.FromArgb("#64748B");

        BtnShipped.BackgroundColor = _currentStatus == "Shipped" ? Color.FromArgb("#0284C7") : Color.FromArgb("#F1F5F9");
        BtnShipped.TextColor = _currentStatus == "Shipped" ? Colors.White : Color.FromArgb("#64748B");

        BtnDelivered.BackgroundColor = _currentStatus == "Delivered" ? Color.FromArgb("#0284C7") : Color.FromArgb("#F1F5F9");
        BtnDelivered.TextColor = _currentStatus == "Delivered" ? Colors.White : Color.FromArgb("#64748B");
    }

    private async Task LoadOrdersAsync()
    {
        LoadingIndicator.IsVisible = true;
        OrdersList.IsVisible = false;
        EmptyState.IsVisible = false;

        try
        {
            var result = await _orderService.GetOrdersAsync(page: _currentPage, searchTerm: _currentSearch, status: _currentStatus);

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
            await DisplayAlert("Erro", $"Falha ao carregar pedidos: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }
}
