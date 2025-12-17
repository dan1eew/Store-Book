using Store_Book.Users;
using System.Windows;

namespace Store_Book
{
    public partial class TableWindow : Window
    {
        private User currentUser;

        public bool IsAdmin => currentUser?.EmployeeRole == "Администратор";
        public bool IsManagerOrAdmin => currentUser?.EmployeeRole == "Менеджер" || currentUser?.EmployeeRole == "Администратор";
        public bool IsClient => currentUser?.EmployeeRole == "Авторизированный клиент" || currentUser?.EmployeeRole == "Авторизованный клиент";

        public TableWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            DataContext = this;
            InitializeUI();
            ShowProductsPage();
        }

        private void InitializeUI()
        {
            TextBlockUserInfo.Text = $"{currentUser.FullName} ({currentUser.EmployeeRole})";
        }

        private void ShowProductsPage()
        {
            BookPage productsPage = new BookPage(currentUser);
            MainFrame.Navigate(productsPage);
        }

        private void ButtonProducts_Click(object sender, RoutedEventArgs e)
        {
            ShowProductsPage();
        }

        private void ButtonAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser.EmployeeRole != "Администратор")
            {
                MessageBox.Show("Доступно только для администратора", "Ошибка доступа",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddEditProductWindow addWindow = new AddEditProductWindow();
            if (addWindow.ShowDialog() == true)
            {
                ShowProductsPage();
                MessageBox.Show("Товар успешно добавлен!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ButtonOrders_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser.EmployeeRole == "Менеджер" || currentUser.EmployeeRole == "Администратор")
            {
                OrdersWindow ordersWindow = new OrdersWindow(currentUser);
                ordersWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Доступно только для менеджера и администратора",
                    "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonMakeOrder_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser.EmployeeRole != "Авторизированный клиент" && currentUser.EmployeeRole != "Авторизованный клиент")
            {
                MessageBox.Show("Доступно только для авторизованных клиентов", "Ошибка доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MakeOrderWindow makeOrderWindow = new MakeOrderWindow(currentUser);
            makeOrderWindow.ShowDialog();

            // Обновляем страницу после оформления заказа
            if (makeOrderWindow.DialogResult == true)
            {
                ShowProductsPage();
            }
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}