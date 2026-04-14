using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ShopMVC.Helpers
{
    public static class ImageHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// Upload ảnh và lưu vào thư mục wwwroot/images/products
        /// </summary>
        public static async Task<(bool success, string? fileName, string? error)> UploadProductImageAsync(
            IFormFile file,
            IWebHostEnvironment environment)
        {
            try
            {
                // Validate
                var validation = ValidateImage(file);
                if (!validation.isValid)
                {
                    return (false, null, validation.error);
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var uploadsFolder = Path.Combine(environment.WebRootPath, "images", "products");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Resize và save
                using (var image = await Image.LoadAsync(file.OpenReadStream()))
                {
                    // Resize nếu ảnh quá lớn (max 800x800)
                    if (image.Width > 800 || image.Height > 800)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(800, 800),
                            Mode = ResizeMode.Max
                        }));
                    }

                    await image.SaveAsync(filePath);
                }

                return (true, fileName, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi khi upload ảnh: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa ảnh sản phẩm
        /// </summary>
        public static bool DeleteProductImage(string fileName, IWebHostEnvironment environment)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return false;

                var filePath = Path.Combine(environment.WebRootPath, "images", "products", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate ảnh (extension, size)
        /// </summary>
        public static (bool isValid, string? error) ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "Vui lòng chọn file ảnh");
            }

            // Check extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return (false, "Chỉ chấp nhận file ảnh: jpg, jpeg, png, gif, webp");
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                return (false, "Kích thước file không được vượt quá 5MB");
            }

            return (true, null);
        }

        /// <summary>
        /// Lấy đường dẫn ảnh đầy đủ
        /// </summary>
        public static string GetProductImagePath(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "/images/no-image.jpg";
            }

            return $"/images/products/{fileName}";
        }
    }
}