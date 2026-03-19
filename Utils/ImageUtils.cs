namespace ApiThiBangLaiXeOto.Utils
{
    public static class ImageUtils
    {
        public static async Task<string> UploadImage(IFormFile image)
        {
            if (image.Length > 5 * 1024 * 1024) throw new Exception("File quá lớn (max 5MB)");

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "uploads");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return fileName;
        }

        public static void DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "uploads", fileName);
            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu không xóa được (do file đang bị khóa hoặc quyền truy cập)
                Console.WriteLine($"Không thể xóa file: {ex.Message}");
            }
        }
    }
}
