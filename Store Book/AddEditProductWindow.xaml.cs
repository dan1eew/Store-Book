using Microsoft.Win32;
using Store_Book.Data;
using Store_Book.Product;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Store_Book
{
    public partial class AddEditProductWindow : Window
    {
        private string selectedImagePath;
        private bool isEditMode = false;
        private Books currentProduct;

        // Конструктор для добавления нового товара
        public AddEditProductWindow()
        {
            InitializeComponent();
            InitializeForm();
            ButtonDelete.Visibility = Visibility.Collapsed; // Скрываем кнопку удаления при добавлении
            Title = "Добавление товара";
        }

        // Конструктор для редактирования существующего товара
        public AddEditProductWindow(Books product) : this()
        {
            isEditMode = true;
            currentProduct = product;
            Title = "Редактирование товара";
            ButtonSave.Content = "Обновить";
            ButtonDelete.Visibility = Visibility.Visible; // Показываем кнопку удаления при редактировании
            LoadProductData();
        }

        private void InitializeForm()
        {
            string[] categories = { "Фэнтези", "Классическая литература", "Детская литература", "Деловая литература", "Ужасы", "Фантастика" };
            ComboBoxCategory.ItemsSource = categories;
            ComboBoxCategory.SelectedIndex = 0;

            string[] producers = { "АСТ", "Эксмо", "Просвещение" };
            ComboBoxProducer.ItemsSource = producers;
            ComboBoxProducer.SelectedIndex = 0;

            TextBoxPrice.PreviewTextInput += TextBoxNumeric_PreviewTextInput;
            TextBoxQuantity.PreviewTextInput += TextBoxNumeric_PreviewTextInput;
            TextBoxDiscount.PreviewTextInput += TextBoxNumeric_PreviewTextInput;
        }

        private void LoadProductData()
        {
            if (currentProduct == null) return;

            TextBoxArticle.Text = currentProduct.Code;
            TextBoxArticle.IsEnabled = false;
            TextBoxName.Text = currentProduct.ProductName;

            // Устанавливаем категорию
            if (!string.IsNullOrEmpty(currentProduct.Category))
            {
                ComboBoxCategory.SelectedItem = currentProduct.Category;
            }

            TextBoxDescription.Text = currentProduct.Description;

            // Устанавливаем производителя
            if (!string.IsNullOrEmpty(currentProduct.Producer))
            {
                ComboBoxProducer.SelectedItem = currentProduct.Producer;
            }

            TextBoxSupplier.Text = currentProduct.Supplier;
            TextBoxPrice.Text = currentProduct.Price.ToString("0.00");
            TextBoxUnit.Text = currentProduct.Unit;
            TextBoxQuantity.Text = currentProduct.QuantityInStock.ToString();
            TextBoxDiscount.Text = currentProduct.CurrentDiscount.ToString("0");

            // Загружаем изображение
            if (!string.IsNullOrEmpty(currentProduct.Photo) && currentProduct.Photo.ToLower() != "null")
            {
                try
                {
                    string imagesDirectory = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Images");

                    // Убедимся, что имя файла безопасное
                    string safePhotoName = currentProduct.Photo.Trim();

                    // Если фото содержит путь, берем только имя файла
                    if (safePhotoName.Contains("\\") || safePhotoName.Contains("/"))
                    {
                        safePhotoName = System.IO.Path.GetFileName(safePhotoName);
                    }

                    string imagePath = System.IO.Path.Combine(imagesDirectory, safePhotoName);

                    if (File.Exists(imagePath))
                    {
                        // Используем UriKind.Absolute для абсолютного пути
                        Uri imageUri = new Uri(imagePath, UriKind.Absolute);
                        ProductImage.Source = new BitmapImage(imageUri);
                        selectedImagePath = imagePath;
                    }
                    else
                    {
                        // Попробуем найти файл без учета регистра
                        var files = Directory.GetFiles(imagesDirectory);
                        var matchingFile = files.FirstOrDefault(f =>
                            System.IO.Path.GetFileName(f).Equals(safePhotoName, StringComparison.OrdinalIgnoreCase));

                        if (matchingFile != null)
                        {
                            Uri imageUri = new Uri(matchingFile, UriKind.Absolute);
                            ProductImage.Source = new BitmapImage(imageUri);
                            selectedImagePath = matchingFile;
                        }
                        else
                        {
                            // Загружаем изображение по умолчанию
                            LoadDefaultImage();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadDefaultImage();
                }
            }
            else
            {
                LoadDefaultImage();
            }
        }

        private void LoadDefaultImage()
        {
            try
            {
                string defaultImagePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Images", "picture.png");
                if (File.Exists(defaultImagePath))
                {
                    Uri defaultUri = new Uri(defaultImagePath, UriKind.Absolute);
                    ProductImage.Source = new BitmapImage(defaultUri);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения по умолчанию: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    selectedImagePath = openFileDialog.FileName;
                    ProductImage.Source = new BitmapImage(new Uri(selectedImagePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TextBoxNumeric_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            // Для цены и скидки разрешаем цифры и точку
            if (textBox == TextBoxPrice || textBox == TextBoxDiscount)
            {
                // Разрешаем только цифры и точку
                if (!char.IsDigit(e.Text, 0) && e.Text != ".")
                {
                    e.Handled = true;
                }

                // Проверяем, чтобы точка была только одна
                if (e.Text == "." && textBox.Text.Contains("."))
                {
                    e.Handled = true;
                }
            }
            // Для количества только цифры
            else if (textBox == TextBoxQuantity)
            {
                e.Handled = !char.IsDigit(e.Text, 0);
            }
        }

        private bool ValidateForm()
        {
            // Проверка артикула
            if (string.IsNullOrWhiteSpace(TextBoxArticle.Text))
            {
                MessageBox.Show("Введите артикул товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxArticle.Focus();
                return false;
            }

            if (TextBoxArticle.Text.Length > 5)
            {
                MessageBox.Show("Артикул не должен превышать 5 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxArticle.Focus();
                return false;
            }

            // Проверка наименования
            if (string.IsNullOrWhiteSpace(TextBoxName.Text))
            {
                MessageBox.Show("Введите наименование товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxName.Focus();
                return false;
            }

            if (TextBoxName.Text.Length > 64)
            {
                MessageBox.Show("Наименование не должно превышать 64 символа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxName.Focus();
                return false;
            }

            // Проверка цены
            if (!decimal.TryParse(TextBoxPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть положительным числом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxPrice.Focus();
                return false;
            }

            // Проверка количества
            if (!int.TryParse(TextBoxQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным числом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxQuantity.Focus();
                return false;
            }

            // Проверка скидки
            if (!decimal.TryParse(TextBoxDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка должна быть в диапазоне от 0 до 100%", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxDiscount.Focus();
                return false;
            }

            // Проверка уникальности артикула (только для добавления)
            if (!isEditMode && Books.CodeExists(TextBoxArticle.Text))
            {
                MessageBox.Show("Товар с таким артикулом уже существует", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxArticle.Focus();
                return false;
            }

            return true;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                string photoFileName = "picture.png";

                // Обработка изображения
                if (!string.IsNullOrEmpty(selectedImagePath) && File.Exists(selectedImagePath))
                {
                    string imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images");
                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }

                    // Генерируем имя файла на основе артикула
                    string fileExtension = Path.GetExtension(selectedImagePath);
                    photoFileName = $"{TextBoxArticle.Text}{fileExtension}";
                    string destinationPath = Path.Combine(imagesDirectory, photoFileName);

                    // Копируем изображение в папку приложения
                    File.Copy(selectedImagePath, destinationPath, true);
                }
                else if (isEditMode && !string.IsNullOrEmpty(currentProduct.Photo))
                {
                    // В режиме редактирования, если не выбрано новое изображение, оставляем старое
                    photoFileName = currentProduct.Photo;
                }

                if (isEditMode)
                {
                    // Режим редактирования
                    UpdateProduct(photoFileName);
                }
                else
                {
                    // Режим добавления
                    AddNewProduct(photoFileName);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении товара: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewProduct(string photoFileName)
        {
            string query = @"INSERT INTO Books 
                            (Code, ProductName, Unit, Price, Supplier, Producer, 
                             Category, CurrentDiscount, QuantityInStock, Description, Photo) 
                            VALUES (@Code, @ProductName, @Unit, @Price, @Supplier, @Producer, 
                                    @Category, @CurrentDiscount, @QuantityInStock, @Description, @Photo)";

            SqlParameter[] parameters = {
                new SqlParameter("@Code", TextBoxArticle.Text),
                new SqlParameter("@ProductName", TextBoxName.Text),
                new SqlParameter("@Unit", TextBoxUnit.Text),
                new SqlParameter("@Price", decimal.Parse(TextBoxPrice.Text)),
                new SqlParameter("@Supplier", TextBoxSupplier.Text),
                new SqlParameter("@Producer", ComboBoxProducer.SelectedItem?.ToString() ?? ""),
                new SqlParameter("@Category", ComboBoxCategory.SelectedItem?.ToString() ?? ""),
                new SqlParameter("@CurrentDiscount", decimal.Parse(TextBoxDiscount.Text)),
                new SqlParameter("@QuantityInStock", int.Parse(TextBoxQuantity.Text)),
                new SqlParameter("@Description", TextBoxDescription.Text),
                new SqlParameter("@Photo", photoFileName)
            };

            int result = DataHelper.ExecuteNonQuery(query, parameters);
            if (result > 0)
            {
                MessageBox.Show("Товар успешно добавлен!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateProduct(string photoFileName)
        {
            string query = @"UPDATE Books SET 
                            ProductName = @ProductName, 
                            Unit = @Unit, 
                            Price = @Price, 
                            Supplier = @Supplier, 
                            Producer = @Producer, 
                            Category = @Category, 
                            CurrentDiscount = @CurrentDiscount, 
                            QuantityInStock = @QuantityInStock, 
                            Description = @Description, 
                            Photo = @Photo 
                            WHERE Code = @Code";

            SqlParameter[] parameters = {
                new SqlParameter("@ProductName", TextBoxName.Text),
                new SqlParameter("@Unit", TextBoxUnit.Text),
                new SqlParameter("@Price", decimal.Parse(TextBoxPrice.Text)),
                new SqlParameter("@Supplier", TextBoxSupplier.Text),
                new SqlParameter("@Producer", ComboBoxProducer.SelectedItem?.ToString() ?? ""),
                new SqlParameter("@Category", ComboBoxCategory.SelectedItem?.ToString() ?? ""),
                new SqlParameter("@CurrentDiscount", decimal.Parse(TextBoxDiscount.Text)),
                new SqlParameter("@QuantityInStock", int.Parse(TextBoxQuantity.Text)),
                new SqlParameter("@Description", TextBoxDescription.Text),
                new SqlParameter("@Photo", photoFileName),
                new SqlParameter("@Code", TextBoxArticle.Text)
            };

            int result = DataHelper.ExecuteNonQuery(query, parameters);
            if (result > 0)
            {
                MessageBox.Show("Товар успешно обновлен!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentProduct == null) return;

            if (MessageBox.Show("Вы уверены, что хотите удалить этот товар?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM Books WHERE BookID = @BookID";
                    var param = new SqlParameter("@BookID", currentProduct.BookID);

                    DataHelper.ExecuteNonQuery(query, new[] { param });
                    MessageBox.Show("Товар успешно удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}