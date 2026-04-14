namespace ShopMVC.Helpers
{
    public static class PaginationHelper
    {
        /// <summary>
        /// Tính tổng số trang
        /// </summary>
        public static int CalculateTotalPages(int totalItems, int pageSize)
        {
            return (int)Math.Ceiling(totalItems / (double)pageSize);
        }

        /// <summary>
        /// Validate page number
        /// </summary>
        public static int ValidatePageNumber(int pageNumber, int totalPages)
        {
            if (pageNumber < 1)
                return 1;

            if (pageNumber > totalPages && totalPages > 0)
                return totalPages;

            return pageNumber;
        }

        /// <summary>
        /// Lấy danh sách số trang để hiển thị (với ... nếu quá nhiều)
        /// </summary>
        public static List<int> GetPageNumbers(int currentPage, int totalPages, int maxPagesToShow = 5)
        {
            var pages = new List<int>();

            if (totalPages <= maxPagesToShow)
            {
                // Hiển thị tất cả nếu ít trang
                for (int i = 1; i <= totalPages; i++)
                {
                    pages.Add(i);
                }
            }
            else
            {
                // Hiển thị với ... nếu nhiều trang
                int halfMax = maxPagesToShow / 2;
                int startPage = Math.Max(1, currentPage - halfMax);
                int endPage = Math.Min(totalPages, currentPage + halfMax);

                // Điều chỉnh nếu gần đầu hoặc cuối
                if (currentPage <= halfMax)
                {
                    endPage = maxPagesToShow;
                }
                else if (currentPage >= totalPages - halfMax)
                {
                    startPage = totalPages - maxPagesToShow + 1;
                }

                for (int i = startPage; i <= endPage; i++)
                {
                    pages.Add(i);
                }
            }

            return pages;
        }

        /// <summary>
        /// Tạo query string cho pagination
        /// </summary>
        public static string BuildQueryString(
            int pageNumber,
            int? categoryId = null,
            string? searchTerm = null,
            string? status = null)
        {
            var parameters = new List<string>
            {
                $"page={pageNumber}"
            };

            if (categoryId.HasValue)
            {
                parameters.Add($"categoryId={categoryId.Value}");
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                parameters.Add($"search={Uri.EscapeDataString(searchTerm)}");
            }

            if (!string.IsNullOrEmpty(status))
            {
                parameters.Add($"status={Uri.EscapeDataString(status)}");
            }

            return "?" + string.Join("&", parameters);
        }
    }
}