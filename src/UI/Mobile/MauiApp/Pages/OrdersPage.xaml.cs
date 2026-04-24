using System.Collections.ObjectModel;
using System.Linq;
using EnterpriseTemplate.MauiApp.Services;
using ProjectTemplate.Shared.Models;

namespace EnterpriseTemplate.MauiApp.Pages;

public partial class OrdersPage : ContentPage
{
    private readonly IOrderService _orderService;
    private string _currentSearch = "";
    private string? _currentStatus = null;
    private int _currentPage = 1;
    private bool _isBusy = false;
    private bool _hasNextPage = true;

    public ObservableCollection<OrderResponseDto> Orders { get; } = new();

    public OrdersPage(IOrderService orderService)
    {
        InitializeComponent();
        _orderService = orderService;
        OrdersList.ItemsSource = Orders;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (Orders.Count == 0)
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

    private async Task LoadOrdersAsync(bool append = false)
    {
        if (_isBusy) return;
        _isBusy = true;

        if (!append)
        {
            _currentPage = 1;
            Orders.Clear();
            LoadingIndicator.IsVisible = true;
            OrdersList.IsVisible = false;
        }
        
        EmptyState.IsVisible = false;

        try
        {
            var result = await _orderService.GetOrdersAsync(page: _currentPage, searchTerm: _currentSearch, status: _currentStatus);

            if (result?.Items != null && result.Items.Any())
            {
                foreach (var item in result.Items)
                    Orders.Add(item);
                
                _hasNextPage = result.HasNextPage;
                OrdersList.IsVisible = true;
            }
            else if (!append)
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
            _isBusy = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnRemainingItemsReached(object sender, EventArgs e)
    {
        if (_hasNextPage && !_isBusy)
        {
            _currentPage++;
            await LoadOrdersAsync(append: true);
        }
    }

    private async void OnViewDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderResponseDto order)
        {
            // Similar to Flutter details dialog
            var message = $"Cliente: {order.CustomerName}\nEmail: {order.CustomerEmail}\nEndereço: {order.ShippingAddress}\n\nItens:\n";
            foreach (var item in order.Items)
                message += $"- {item.ProductName} ({item.Quantity}x)\n";

            await DisplayAlert("Detalhes do Pedido", message, "Fechar");
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Editar", "Funcionalidade de edição em desenvolvimento para MAUI.", "OK");
    }
}
