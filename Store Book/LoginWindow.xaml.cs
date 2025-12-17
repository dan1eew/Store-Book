using Store_Book.Data;
using Store_Book.Users;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Store_Book
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            try
            {
                InitializeComponent();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Критическая ошибка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show($"{ex}");
            }
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            string password2 = PasswordBox2.Password;

            if (password != password2)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            User user = Authenticate(login, password);
            if (user != null)
            {
                TableWindow tablewindow = new TableWindow(user);
                tablewindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        // перенести в отдельный класс в папку Users
        public static User Authenticate(string login, string password)
        {
            string query = "SELECT UserID, EmployeeRole, FullName, Login, Password FROM Users WHERE Login = @Login AND Password = @Password";
            SqlParameter[] parameters = {
                new SqlParameter("@Login", login),
                new SqlParameter("@Password", password)
            };

            DataTable result = DataHelper.ExecuteQuery(query, parameters);
            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                return new User
                {
                    UserID = Convert.ToInt32(row["UserID"]),
                    EmployeeRole = row["EmployeeRole"].ToString(),
                    FullName = row["FullName"].ToString(),
                    Login = row["Login"].ToString(),
                    Password = row["Password"].ToString()
                };
            }
            return null;
        }

        private void ButtonGuest_Click(object sender, RoutedEventArgs e)
        {
            User guest = new User { EmployeeRole = "Гость", FullName = "Гость" };
            TableWindow tablewindow = new TableWindow(guest);
            tablewindow.Show();
            this.Close();
        }
    }
}
