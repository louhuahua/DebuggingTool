using DebuggingTool.Models;

namespace DebuggingTool.ViewModels.ShopViewModels;
public class ProductDetailViewModel : ViewModelBase
{
    public ProductDto Product { get; set; }

    public string Title => Product.Name;

	public ProductDetailViewModel(ProductDto product)
	{
        Product = product;
    }
}
