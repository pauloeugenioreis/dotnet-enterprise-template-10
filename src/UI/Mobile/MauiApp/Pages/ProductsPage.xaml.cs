using System.Collections.ObjectModel;
using System.Linq;
using EnterpriseTemplate.MauiApp.Services;
using ProjectTemplate.Shared.Models;

namespace EnterpriseTemplate.MauiApp.Pages;

public partial class ProductsPage : ContentPage
{
    private readonly IProductService _productService;
    private string _currentSearch = "";
    private bool? _currentIsActive = null;
    private int _currentPage = 1;
    private bool _isBusy = false;
    private bool _hasNextPage = true;

    public ObservableCollection<ProductResponseDto> Products { get; } = new();

    public ProductsPage(IProductService productService)
    {
        InitializeComponent();
        _productService = productService;
        ProductsList.ItemsSource = Products;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (Products.Count == 0)
            await LoadProductsAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        _currentPage = 1;
        await LoadProductsAsync();
        RefreshView.IsRefreshing = false;
    }

    private async void OnSearchCompleted(object sender, EventArgs e)
    {
        _currentSearch = SearchEntry.Text;
        _currentPage = 1;
        await LoadProductsAsync();
    }

    private async void OnFilterAll(object sender, EventArgs e)
    {
        _currentIsActive = null;
        _currentPage = 1;
        UpdateFilterButtons();
        await LoadProductsAsync();
    }

    private async void OnFilterActive(object sender, EventArgs e)
    {
        _currentIsActive = true;
        _currentPage = 1;
        UpdateFilterButtons();
        await LoadProductsAsync();
    }

    private async void OnFilterInactive(object sender, EventArgs e)
    {
        _currentIsActive = false;
        _currentPage = 1;
        UpdateFilterButtons();
        await LoadProductsAsync();
    }

    private void UpdateFilterButtons()
    {
        BtnAll.BackgroundColor = _currentIsActive == null ? Color.FromArgb("#0284C7") : Color.FromArgb("#F1F5F9");
        BtnAll.TextColor = _currentIsActive == null ? Colors.White : Color.FromArgb("#64748B");

        BtnActive.BackgroundColor = _currentIsActive == true ? Color.FromArgb("#0284C7") : Color.FromArgb("#F1F5F9");
        BtnActive.TextColor = _currentIsActive == true ? Colors.White : Color.FromArgb("#64748B");

        BtnInactive.BackgroundColor = _currentIsActive == false ? Color.FromArgb("#0284C7") : Color.FromArgb("#F1F5F9");
        BtnInactive.TextColor = _currentIsActive == false ? Colors.White : Color.FromArgb("#64748B");
    }

    private async Task LoadProductsAsync(bool append = false)
    {
        if (_isBusy) return;
        _isBusy = true;

        if (!append)
        {
            _currentPage = 1;
            Products.Clear();
            LoadingIndicator.IsVisible = true;
            ProductsList.IsVisible = false;
        }

        try
        {
            var result = await _productService.GetProductsAsync(page: _currentPage, searchTerm: _currentSearch, isActive: _currentIsActive);
            if (result?.Items != null && result.Items.Any())
            {
                foreach (var item in result.Items)
                    Products.Add(item);
                
                _hasNextPage = result.HasNextPage;
                ProductsList.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao carregar produtos: {ex.Message}", "OK");
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
            await LoadProductsAsync(append: true);
        }
    }
}
