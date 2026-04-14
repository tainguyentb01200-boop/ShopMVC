using ShopMVC.Models;

namespace ShopMVC.ViewModels
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        
        // Filters
        public int? SelectedCategoryId { get; set; }
        public string? SearchTerm { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPageCount { get; set; }
        public int TotalProducts { get; set; }

        // Helpers
        public int StartProduct => (CurrentPage - 1) * PageSize + 1;
        public int EndProduct => Math.Min(CurrentPage * PageSize, TotalProducts);

        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public bool HasPreviousPage { get; internal set; }
        public bool HasNextPage { get; internal set; }
    }
}
