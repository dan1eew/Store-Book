using Store_Book.Product;
using Store_Book.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Store_Book
{
    public partial class BookPage : Page
    {
        private User currentUser;
        static List<Books> allProducts;
        private bool sortAscending = true;

        public Visibility SearchPanelVisibility =>
            (currentUser.EmployeeRole == "Менеджер" || currentUser.EmployeeRole == "Администратор")
            ? Visibility.Visible : Visibility.Collapsed;

        public BookPage(User user)
        {
            InitializeComponent();
            currentUser = user;
            DataContext = this;
            LoadProducts();
            InitializeFilters();

            // Для клиента добавляем кнопку Купить
            if (currentUser.EmployeeRole == "Авторизованный клиент" ||
                currentUser.EmployeeRole == "Авторизированный клиент")
            {
                InitializeBuyButton();
            }

            // Для администратора добавляем двойной клик для редактирования
            if (currentUser.EmployeeRole == "Администратор")
            {
                ListViewProducts.MouseDoubleClick += ListViewProducts_MouseDoubleClick;
            }
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser.EmployeeRole != "Авторизированный клиент" &&
                currentUser.EmployeeRole != "Авторизованный клиент") return;

            var button = sender as Button;
            var bookViewModel = button?.DataContext as BookViewModel;

            if (bookViewModel != null)
            {
                var book = allProducts.FirstOrDefault(b => b.Code == bookViewModel.Code);
                if (book != null)
                {
                    if (book.QuantityInStock <= 0)
                    {
                        MessageBox.Show("Эта книга временно отсутствует на складе", "Нет в наличии",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var orderWindow = new MakeOrderWindow(currentUser, book);
                    if (orderWindow.ShowDialog() == true)
                    {
                        MessageBox.Show("Заказ успешно оформлен!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadProducts(); // Обновляем список после заказа
                    }
                }
            }
        }

        private void LoadProducts()
        {
            try
            {
                allProducts = Books.GetAllProducts();
                UpdateProductList(allProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeFilters()
        {
            if (SearchPanelVisibility == Visibility.Visible && allProducts != null)
            {
                var suppliers = allProducts.Select(p => p.Supplier).Distinct().OrderBy(s => s).ToList();
                suppliers.Insert(0, "Все поставщики");
                ComboBoxSupplier.ItemsSource = suppliers;
                ComboBoxSupplier.SelectedIndex = 0;
            }
        }

        private void UpdateProductList(List<Books> books)
        {
            var productViewModels = books.Select(p => new BookViewModel(p)).ToList();
            ListViewProducts.ItemsSource = productViewModels;

            TextBlockNoProducts.Visibility = books.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ComboBoxSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ButtonSort_Click(object sender, RoutedEventArgs e)
        {
            sortAscending = !sortAscending;
            ButtonSort.Content = sortAscending ? "По количеству ▲" : "По количеству ▼";
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (allProducts == null) return;

            var filteredProducts = allProducts.AsEnumerable();

            if (TextBoxSearch != null && !string.IsNullOrEmpty(TextBoxSearch.Text))
            {
                string searchText = TextBoxSearch.Text.ToLower();
                filteredProducts = filteredProducts.Where(p =>
                    (p.ProductName?.ToLower().Contains(searchText) == true) ||
                    (p.Category?.ToLower().Contains(searchText) == true) ||
                    (p.Description?.ToLower().Contains(searchText) == true) ||
                    (p.Producer?.ToLower().Contains(searchText) == true) ||
                    (p.Supplier?.ToLower().Contains(searchText) == true) ||
                    (p.Code?.ToLower().Contains(searchText) == true));
            }

            if (ComboBoxSupplier != null && ComboBoxSupplier.SelectedIndex > 0)
            {
                string selectedSupplier = ComboBoxSupplier.SelectedItem.ToString();
                filteredProducts = filteredProducts.Where(p => p.Supplier == selectedSupplier);
            }

            filteredProducts = sortAscending ?
                filteredProducts.OrderBy(p => p.QuantityInStock) :
                filteredProducts.OrderByDescending(p => p.QuantityInStock);

            UpdateProductList(filteredProducts.ToList());
        }

        private void ListViewProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (currentUser.EmployeeRole != "Администратор") return;

            var selectedItem = ListViewProducts.SelectedItem as BookViewModel;
            if (selectedItem != null)
            {
                var books = allProducts.FirstOrDefault(p => p.Code == selectedItem.Code);
                if (books != null)
                {
                    EditProduct(books);
                }
            }
        }

        private void InitializeBuyButton()
        {
            if (currentUser.EmployeeRole == "Авторизованный клиент" ||
                currentUser.EmployeeRole == "Авторизированный клиент")
            {
                var gridView = ListViewProducts.View as GridView;
                if (gridView != null)
                {
                    var buyColumn = new GridViewColumn
                    {
                        Header = "Действие",
                        Width = 100
                    };

                    DataTemplate buttonTemplate = new DataTemplate();
                    FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(Button));
                    buttonFactory.SetValue(Button.ContentProperty, "Купить");
                    buttonFactory.SetValue(Button.MarginProperty, new Thickness(5));
                    buttonFactory.SetValue(Button.PaddingProperty, new Thickness(5, 2, 5, 2));
                    buttonFactory.SetValue(Button.BackgroundProperty, Brushes.Green);
                    buttonFactory.SetValue(Button.ForegroundProperty, Brushes.White);
                    buttonFactory.SetValue(Button.FontSizeProperty, 12.0);
                    buttonFactory.SetValue(Button.FontWeightProperty, FontWeights.Bold);
                    buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(BuyButton_Click));
                    buttonTemplate.VisualTree = buttonFactory;

                    buyColumn.CellTemplate = buttonTemplate;
                    gridView.Columns.Add(buyColumn);
                }
            }
        }

        private void EditProduct(Books product)
        {
            AddEditProductWindow editWindow = new AddEditProductWindow(product);
            if (editWindow.ShowDialog() == true)
            {
                LoadProducts();
                MessageBox.Show("Товар успешно обновлен!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}