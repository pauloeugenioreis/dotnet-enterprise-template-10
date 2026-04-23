using EnterpriseTemplate.MauiApp.Services;
using ProjectTemplate.SharedModels;

namespace EnterpriseTemplate.MauiApp.Pages;

public partial class ProductsPage : ContentPage
{
    private readonly IProductService _productService;
    private string _currentSearch = "";
    private bool? _currentIsActive = null;

    public ProductsPage(IProductService productService)
    {
        InitializeComponent();
        _productService = productService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProductsAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadProductsAsync();
        RefreshView.IsRefreshing = false;
    }

    private async void OnSearchCompleted(object sender, EventArgs e)
    {
        _currentSearch = SearchEntry.Text;
        await LoadProductsAsync();
    }

    private async void OnFilterAll(object sender, EventArgs e)
    {
        _currentIsActive = null;
        UpdateFilterButtons();
        await LoadProductsAsync();
    }

    private async void OnFilterActive(object sender, EventArgs e)
    {
        _currentIsActive = true;
        UpdateFilterButtons();
        await LoadProductsAsync();
    }

    private async void OnFilterInactive(object sender, EventArgs e)
    {
        _currentIsActive = false;
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

    private async Task LoadProductsAsync()
    {
        LoadingIndicator.IsVisible = true;
        ProductsList.IsVisible = false;

        try
        {
            var result = await _productService.GetProductsAsync(searchTerm: _currentSearch, isActive: _currentIsActive);
            if (result?.Items != null)
            {
                ProductsList.ItemsSource = result.Items;
                ProductsList.IsVisible = true;
            }
        }
        catch
        {
            await DisplayAlert("Erro", "Falha ao carregar produtos", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }
}
