using Store_Book.Data;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace Store_Book
{
    public partial class EditOrderWindow : Window
    {
        private Order currentOrder;

        public EditOrderWindow(Order order)
        {
            InitializeComponent();
            currentOrder = order;

            // Статусы для выбора
            string[] statuses = { "Новый", "В обработке", "Собран", "Отправлен", "Доставлен", "Отменен" };
            StatusComboBox.ItemsSource = statuses;
            StatusComboBox.SelectedItem = currentOrder.OrderStatus;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem == null) return;

            try
            {
                string query = "UPDATE Orders SET OrderStatus = @Status WHERE OrderID = @OrderID";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Status", StatusComboBox.SelectedItem.ToString()),
                    new SqlParameter("@OrderID", currentOrder.OrderID)
                };

                DataHelper.ExecuteNonQuery(query, parameters);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}