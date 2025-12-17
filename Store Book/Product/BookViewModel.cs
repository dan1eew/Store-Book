using Store_Book.Product;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Store_Book.Product
{
    internal class BookViewModel
    {
        private Books book;

        public BookViewModel(Books book)
        {
            this.book = book;
        }

        public string Code => book.Code;
        public string ProductName => book.ProductName;
        public string Category => book.Category;
        public decimal Price => book.Price;
        public decimal CurrentDiscount => book.CurrentDiscount;
        public int QuantityInStock => book.QuantityInStock;
        public double FinalPrice => book.FinalPrice;
        public string Description => book.Description;
        public string Supplier => book.Supplier;
        public string Producer => book.Producer;
        public string Unit => book.Unit;

        public BitmapImage ProductImage => book.GetProductImage();

        public Brush RowColor
        {
            get
            {
                if (QuantityInStock == 0)
                    return new SolidColorBrush(Color.FromRgb(243, 137, 149));
                if (CurrentDiscount > 15)
                    return new SolidColorBrush(Color.FromRgb(137, 243, 144));
                return Brushes.Transparent;
            }
        }

        public TextDecorationCollection PriceDecoration =>
            CurrentDiscount > 0 ? TextDecorations.Strikethrough : null;

        public Brush PriceColor =>
            CurrentDiscount > 0 ? Brushes.Red : Brushes.Black;

        public Visibility DiscountVisibility =>
            CurrentDiscount > 0 ? Visibility.Visible : Visibility.Collapsed;

        public string PriceDisplay =>
            CurrentDiscount > 0 ? $"{Price}₽ → {FinalPrice:0}₽" : $"{Price:0}₽";
    }
}