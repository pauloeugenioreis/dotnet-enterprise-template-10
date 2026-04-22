using EnterpriseTemplate.MauiApp.Services;

namespace EnterpriseTemplate.MauiApp.Pages;

public partial class ProductsPage : ContentPage
{
    private readonly IProductService _productService;

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

    private async Task LoadProductsAsync()
    {
        LoadingIndicator.IsVisible = true;
        ProductsList.IsVisible = false;

        try
        {
            var result = await _productService.GetProductsAsync();
            if (result?.Items != null)
            {
                ProductsList.ItemsSource = result.Items;
                ProductsList.IsVisible = true;
            }
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }
}
