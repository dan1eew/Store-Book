using Store_Book.Data;
using Store_Book.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace Store_Book
{
    public partial class OrdersWindow : Window
    {
        private User currentUser;
        private List<Order> allOrders;

        public OrdersWindow(User user)
        {
            InitializeComponent();
            currentUser = user;

            if (currentUser.EmployeeRole == "Администратор")
            {
                EditButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }

            LoadOrders();
            LoadStatusFilter();
            SortComboBox.SelectedIndex = 2; // По умолчанию сортировка по дате (↑)
        }

        private void LoadOrders()
        {
            try
            {
                string query = @"
                    SELECT o.*, b.ProductName as BookTitle, 
                           p.City + ', ' + p.Street + ', д.' + CAST(p.HouseNumber as varchar) as PickUpAddress
                    FROM Orders o
                    LEFT JOIN Books b ON o.BookID = b.BookID
                    LEFT JOIN PickUpPoint p ON o.PickUpPointID = p.PickUpPointID
                    WHERE o.OrderStatus IS NOT NULL"; // Фильтруем только заказы со статусом

                DataTable dt = DataHelper.ExecuteQuery(query);

                allOrders = dt.Rows.Cast<DataRow>()
                    .Select(row => new Order
                    {
                        OrderID = GetIntSafe(row, "OrderID"),
                        OrderNumber = GetIntSafe(row, "OrderNumber"),
                        OrderCode = GetStringSafe(row, "OrderCode"),
                        OrderDate = GetDateTimeSafe(row, "OrderDate"),
                        DeliveryDate = GetDateTimeSafe(row, "DeliveryDate"),
                        HouseNumber = GetIntSafe(row, "HouseNumber"),
                        FullNameCustomer = GetStringSafe(row, "FullNameСustomer"), // Обратите внимание на букву С (русская)
                        CodeToReceive = GetIntSafe(row, "CodeToReceive"),
                        OrderStatus = GetStringSafe(row, "OrderStatus"),
                        BookID = GetIntSafe(row, "BookID"),
                        PickUpPointID = GetIntSafe(row, "PickUpPointID"),
                        UserID = GetIntSafe(row, "UserID"),
                        Quantity = GetIntSafe(row, "Quantity"),
                        TotalPrice = GetDecimalSafe(row, "TotalPrice"),
                        BookTitle = GetStringSafe(row, "BookTitle"),
                        PickUpAddress = GetStringSafe(row, "PickUpAddress")
                    }).ToList();

                ApplyFiltersAndSort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFiltersAndSort()
        {
            if (allOrders == null) return;

            var filteredOrders = allOrders.AsEnumerable();

            // Фильтрация по статусу
            if (StatusFilterCombo.SelectedIndex > 0)
            {
                string selectedStatus = StatusFilterCombo.SelectedItem.ToString();
                filteredOrders = filteredOrders.Where(o => o.OrderStatus == selectedStatus);
            }

            // Поиск
            if (!string.IsNullOrEmpty(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredOrders = filteredOrders.Where(o =>
                    (o.FullNameCustomer?.ToLower().Contains(searchText) == true) ||
                    (o.OrderCode?.ToLower().Contains(searchText) == true) ||
                    (o.BookTitle?.ToLower().Contains(searchText) == true));
            }

            // Сортировка
            var selectedSort = (SortComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString();

            switch (selectedSort)
            {
                case "OrderNumberAsc":
                    filteredOrders = filteredOrders.OrderBy(o => o.OrderNumber);
                    break;
                case "OrderNumberDesc":
                    filteredOrders = filteredOrders.OrderByDescending(o => o.OrderNumber);
                    break;
                case "OrderDateAsc":
                    filteredOrders = filteredOrders.OrderBy(o => o.OrderDate);
                    break;
                case "OrderDateDesc":
                    filteredOrders = filteredOrders.OrderByDescending(o => o.OrderDate);
                    break;
                case "TotalPriceAsc":
                    filteredOrders = filteredOrders.OrderBy(o => o.TotalPrice);
                    break;
                case "TotalPriceDesc":
                    filteredOrders = filteredOrders.OrderByDescending(o => o.TotalPrice);
                    break;
                default:
                    filteredOrders = filteredOrders.OrderBy(o => o.OrderDate);
                    break;
            }

            OrdersGrid.ItemsSource = filteredOrders.ToList();
        }

        private void LoadStatusFilter()
        {
            try
            {
                string query = "SELECT DISTINCT OrderStatus FROM Orders WHERE OrderStatus IS NOT NULL";
                DataTable dt = DataHelper.ExecuteQuery(query);

                var statuses = dt.Rows.Cast<DataRow>()
                    .Select(row => row["OrderStatus"].ToString())
                    .ToList();

                statuses.Insert(0, "Все статусы");
                StatusFilterCombo.ItemsSource = statuses;
                StatusFilterCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вспомогательные методы для безопасного получения значений
        private string GetStringSafe(DataRow row, string columnName)
        {
            return row[columnName] != DBNull.Value ? row[columnName].ToString() : string.Empty;
        }

        private int GetIntSafe(DataRow row, string columnName)
        {
            return row[columnName] != DBNull.Value ? Convert.ToInt32(row[columnName]) : 0;
        }

        private decimal GetDecimalSafe(DataRow row, string columnName)
        {
            if (row[columnName] == DBNull.Value) return 0;

            var value = row[columnName];
            if (value is decimal)
                return (decimal)value;
            if (value is double)
                return Convert.ToDecimal((double)value);
            if (value is float)
                return Convert.ToDecimal((float)value);
            if (value is int)
                return Convert.ToDecimal((int)value);

            return Convert.ToDecimal(value);
        }

        private DateTime GetDateTimeSafe(DataRow row, string columnName)
        {
            return row[columnName] != DBNull.Value ? Convert.ToDateTime(row[columnName]) : DateTime.MinValue;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = OrdersGrid.SelectedItem as Order;
            if (selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var editWindow = new EditOrderWindow(selectedOrder);
            if (editWindow.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = OrdersGrid.SelectedItem as Order;
            if (selectedOrder == null) return;

            if (MessageBox.Show("Удалить выбранный заказ?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM Orders WHERE OrderID = @OrderID";
                    var param = new System.Data.SqlClient.SqlParameter("@OrderID", selectedOrder.OrderID);

                    DataHelper.ExecuteNonQuery(query, new[] { param });
                    LoadOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StatusFilterCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void SortComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }
    }
}