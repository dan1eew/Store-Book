using Store_Book.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Store_Book.Product
{
    public class Books
    {
        public int BookID { get; set; }
        public string Code { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public string Supplier { get; set; }
        public string Producer { get; set; }
        public string Category { get; set; }
        public decimal CurrentDiscount { get; set; }
        public int QuantityInStock { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }
        public decimal FinalPrice1 => Price * (1 - CurrentDiscount / 100);
        public double FinalPrice => (double)Math.Round(FinalPrice1, 2);
       
        public static List<Books> GetAllProducts()
        {
            List<Books> books = new List<Books>();
            string query = "SELECT * FROM Books";
            DataTable dataTable = DataHelper.ExecuteQuery(query);

            foreach (DataRow row in dataTable.Rows)
            {
                books.Add(new Books
                {
                    BookID = Convert.ToInt32(row["BookID"]),
                    Code = row["Code"].ToString(),
                    ProductName = row["ProductName"].ToString(),
                    Unit = row["Unit"].ToString(),
                    Price = Convert.ToDecimal(row["Price"]),
                    Supplier = row["Supplier"].ToString(),
                    Producer = row["Producer"].ToString(),
                    Category = row["Category"].ToString(),
                    CurrentDiscount = Convert.ToDecimal(row["CurrentDiscount"]),
                    QuantityInStock = Convert.ToInt32(row["QuantityInStock"]),
                    Description = row["Description"].ToString(),
                    Photo = row["Photo"]?.ToString()
                });
            }
            return books;
        }
        public static bool CodeExists(string code)
        {
            string query = "SELECT COUNT(*) FROM Books WHERE Code = @Code";
            var parameters = new[]
            {
                new System.Data.SqlClient.SqlParameter("@Code", code)
            };

            var result = DataHelper.ExecuteQuery(query, parameters);
            return result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0;
        }
        public BitmapImage GetProductImage()
        {
            try
            {
                string imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images");

                if (!Directory.Exists(imagesDirectory))
                {
                    Directory.CreateDirectory(imagesDirectory);
                    return LoadDefaultImage();
                }

                if (!string.IsNullOrEmpty(Photo) && Photo.ToLower() != "null")
                {
                    string photoFile = Photo.Trim();
                    string imagePath = Path.Combine(imagesDirectory, photoFile);

                    if (File.Exists(imagePath))
                    {
                        return LoadImageFromFile(imagePath);
                    }
                }

                string defaultImagePath = Path.Combine(imagesDirectory, "picture.png");
                if (File.Exists(defaultImagePath))
                {
                    return LoadImageFromFile(defaultImagePath);
                }

                return LoadDefaultImage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки изображения для {Code}: {ex.Message}");
                return LoadDefaultImage();
            }
        }

        private BitmapImage LoadImageFromFile(string path)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания BitmapImage из {path}: {ex.Message}");
                return LoadDefaultImage();
            }
        }
        private BitmapImage LoadDefaultImage()
        {
            try
            {
                string defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "picture.png");
                if (File.Exists(defaultImagePath))
                {
                    return LoadImageFromFile(defaultImagePath);
                }

                return CreateDefaultBitmapImage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки изображения по умолчанию: {ex.Message}");
                return CreateDefaultBitmapImage();
            }
        }
        private BitmapImage CreateDefaultBitmapImage()
        {
            try
            {

                int width = 100;
                int height = 100;

                RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                DrawingVisual drawingVisual = new DrawingVisual();

                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, width, height));

                    FormattedText text = new FormattedText(
                        "No Image",
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        12,
                        Brushes.Gray,
                        1.0);

                    drawingContext.DrawText(text, new Point(10, 40));
                }

                rtb.Render(drawingVisual);

                BitmapImage bitmapImage = new BitmapImage();
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }

                return bitmapImage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания программного изображения: {ex.Message}");
                return null;
            }
        }
    }
}