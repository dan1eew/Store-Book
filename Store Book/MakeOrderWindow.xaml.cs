using Store_Book.Data;
using Store_Book.Product;
using Store_Book.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace Store_Book
{
    public partial class MakeOrderWindow : Window
    {
        private User currentUser;
        private Books selectedBook;
        private decimal totalPrice;
        private List<Books> availableBooks;

        // Конструктор с выбором книги
        public MakeOrderWindow(User user, Books book = null)
        {
            InitializeComponent();
            currentUser = user;

            LoadBooks();
            LoadPickUpPoints();

            if (book != null)
            {
                // Если передана конкретная книга, выбираем ее
                selectedBook = book;
                BooksComboBox.SelectedItem = availableBooks.FirstOrDefault(b => b.BookID == book.BookID);
            }

            CalculateTotalPrice();
        }

        private void LoadBooks()
        {
            try
            {
                availableBooks = Books.GetAllProducts()
                    .Where(b => b.QuantityInStock > 0) // Только книги в наличии
                    .ToList();

                BooksComboBox.ItemsSource = availableBooks;
                if (availableBooks.Count > 0)
                    BooksComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPickUpPoints()
        {
            try
            {
                string query = "SELECT * FROM PickUpPoint";
                DataTable dt = DataHelper.ExecuteQuery(query);

                var points = dt.Rows.Cast<DataRow>()
                    .Select(row => new PickUpPoint
                    {
                        PickUpPointID = Convert.ToInt32(row["PickUpPointID"]),
                        IndexAdrees = Convert.ToInt32(row["IndexAdrees"]),
                        City = row["City"].ToString(),
                        Street = row["Street"].ToString(),
                        HouseNumber = Convert.ToInt32(row["HouseNumber"])
                    }).ToList();

                PickUpComboBox.ItemsSource = points;
                if (points.Count > 0)
                    PickUpComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пунктов выдачи: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BooksComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedBook = BooksComboBox.SelectedItem as Books;
            if (selectedBook != null)
            {
                BookCodeText.Text = selectedBook.Code;
                BookPriceText.Text = $"{selectedBook.Price}₽";
                BookDiscountText.Text = $"{selectedBook.CurrentDiscount}%";
                BookStockText.Text = selectedBook.QuantityInStock.ToString();

                if (selectedBook.CurrentDiscount > 0)
                {
                    BookPriceText.Text = $"{selectedBook.Price}₽ → {selectedBook.FinalPrice}₽";
                }

                // Сбрасываем количество на 1 при смене книги
                QuantityTextBox.Text = "1";
                CalculateTotalPrice();
            }
        }

        private void CalculateTotalPrice()
        {
            if (selectedBook != null && int.TryParse(QuantityTextBox.Text, out int quantity) && quantity > 0)
            {
                totalPrice = (decimal)selectedBook.FinalPrice * quantity;
                TotalPriceText.Text = $"{totalPrice:0.00}₽";
            }
        }

        private void QuantityTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CalculateTotalPrice();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook == null)
            {
                MessageBox.Show("Выберите книгу", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PickUpComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите пункт выдачи", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (quantity > selectedBook.QuantityInStock)
            {
                MessageBox.Show($"Недостаточно товара на складе. В наличии: {selectedBook.QuantityInStock}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var selectedPoint = (PickUpPoint)PickUpComboBox.SelectedItem;
                Random rnd = new Random();

                // Создаем заказ
                string orderQuery = @"
                    INSERT INTO Orders (OrderNumber, OrderCode, OrderDate, DeliveryDate, 
                                        HouseNumber, FullNameСustomer, CodeToReceive, 
                                        OrderStatus, BookID, PickUpPointID, UserID, Quantity, TotalPrice)
                    VALUES (@OrderNumber, @OrderCode, @OrderDate, @DeliveryDate, 
                            @HouseNumber, @FullName, @CodeToReceive, @Status, 
                            @BookID, @PickUpPointID, @UserID, @Quantity, @TotalPrice)";

                var parameters = new System.Data.SqlClient.SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter("@OrderNumber", rnd.Next(100000, 999999)),
                    new System.Data.SqlClient.SqlParameter("@OrderCode", GenerateOrderCode()),
                    new System.Data.SqlClient.SqlParameter("@OrderDate", DateTime.Now),
                    new System.Data.SqlClient.SqlParameter("@DeliveryDate", DateTime.Now.AddDays(3)),
                    new System.Data.SqlClient.SqlParameter("@HouseNumber", selectedPoint.HouseNumber),
                    new System.Data.SqlClient.SqlParameter("@FullName", currentUser.FullName),
                    new System.Data.SqlClient.SqlParameter("@CodeToReceive", rnd.Next(1000, 9999)),
                    new System.Data.SqlClient.SqlParameter("@Status", "Новый"),
                    new System.Data.SqlClient.SqlParameter("@BookID", selectedBook.BookID),
                    new System.Data.SqlClient.SqlParameter("@PickUpPointID", selectedPoint.PickUpPointID),
                    new System.Data.SqlClient.SqlParameter("@UserID", currentUser.UserID),
                    new System.Data.SqlClient.SqlParameter("@Quantity", quantity),
                    new System.Data.SqlClient.SqlParameter("@TotalPrice", totalPrice)
                };

                DataHelper.ExecuteNonQuery(orderQuery, parameters);

                // Обновляем количество на складе
                string updateQuery = "UPDATE Books SET QuantityInStock = QuantityInStock - @Quantity WHERE BookID = @BookID";
                var updateParams = new System.Data.SqlClient.SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter("@Quantity", quantity),
                    new System.Data.SqlClient.SqlParameter("@BookID", selectedBook.BookID)
                };

                DataHelper.ExecuteNonQuery(updateQuery, updateParams);

                MessageBox.Show($"Заказ успешно оформлен! Код для получения: {parameters[6].Value}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateOrderCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}