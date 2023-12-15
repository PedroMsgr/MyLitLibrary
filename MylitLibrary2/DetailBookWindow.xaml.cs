using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using Google.Apis.Services;
using MylitLibrary.Services;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;



namespace MylitLibrary
{
    public partial class DetailBookWindow : Window
    {
        private readonly string _bookId;
        private readonly BooksService _booksService;
        private ConexionClass _conexion;

        public DetailBookWindow(string bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            _booksService = new BooksService(new BaseClientService.Initializer()
            {
                ApplicationName = "MylitLibrary2",
            });
            _conexion = new ConexionClass();
            Loaded += DetailBookWindow_Loaded;
        }

        private async void DetailBookWindow_Loaded(object sender, RoutedEventArgs e)
        {
            btSaveBook.IsEnabled = SesionClass.CurrentUserId.HasValue;
            btDelete.IsEnabled = SesionClass.CurrentUserId.HasValue;

            try
            {
                var bookDetails = await _booksService.Volumes.Get(_bookId).ExecuteAsync();
                ShowDetails(bookDetails);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener los detalles del libro: {ex.Message}");
            }

            if (SesionClass.CurrentUserId.HasValue)
            {
                await LoadBookDetailsIfExist();
            }
        }




        private void ShowDetails(Volume book)
        {
            // Actualiza la interfaz de usuario con la información del libro
            tbTitle.Text = book.VolumeInfo.Title ?? "Título no disponible";
            tbAuthor.Text = book.VolumeInfo.Authors != null ? String.Join(", ", book.VolumeInfo.Authors) : "Author desconocido";
            tbDate.Text = book.VolumeInfo.PublishedDate ?? "Fecha desconocida";
            tbImpression.Text = book.VolumeInfo.Publisher;
            tbDescription.Text = book.VolumeInfo.Description ?? "Descripción no disponible";
            tbPage.Text = book.VolumeInfo.PageCount.HasValue ? book.VolumeInfo.PageCount.Value.ToString() : "N/D";
            tbLanguage.Text = book.VolumeInfo.Language ?? "Language desconocido";
            tbCategories.Text = book.VolumeInfo.MainCategory ?? "Sin clasificar";
            LoadCoverImage(book.VolumeInfo.ImageLinks?.Thumbnail);

        }

        private void LoadCoverImage(string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    imgPortada.Source = new BitmapImage(new Uri(url));
                }
                else
                {
                    LoadGenericImage();
                }
            }
            catch
            {
                LoadGenericImage();
            }
        }

        private void LoadGenericImage()
        {
            var directoriBase = System.AppDomain.CurrentDomain.BaseDirectory;
            var ruteGenericImage = System.IO.Path.Combine(directoriBase, "Assets", "PortadaGenerica.png");
            imgPortada.Source = new BitmapImage(new Uri(ruteGenericImage));
        }

        private async Task LoadBookDetailsIfExist()
        {
            if (!SesionClass.CurrentUserId.HasValue)
            {
                return; // Si no hay usuario logueado, no se carga nada
            }

            // Verificar si el libro existe en la lista del usuario
            if (await _conexion.ExistsBook(SesionClass.CurrentUserId.Value, _bookId))
            {
                var bookData = await _conexion.GetBookDataForUser(SesionClass.CurrentUserId.Value, _bookId);

                if (bookData != null)
                {
                    string statusBookValue = bookData.StatusBook;
                    ComboBoxItem statusBookItem = cbStatusBook.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == statusBookValue);
                    cbStatusBook.SelectedItem = statusBookItem;
                    cbkFavorite.IsChecked = bookData.Favorite;
                    txtComents.Text = bookData.Coments;

                    // Verifica si NoteOfBook tiene un valor antes de intentar acceder a él
                    if (bookData.NoteOfBook.HasValue)
                    {
                        int noteOfBookValue = bookData.NoteOfBook.Value;
                        string noteOfBookValueString = noteOfBookValue.ToString();
                        ComboBoxItem noteItem = cbNote.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == noteOfBookValueString);
                        cbNote.SelectedItem = noteItem;
                    }
                }
            }
        }



        private async void btSaveBook_Click(object sender, RoutedEventArgs e)
        {
            // Asigna los valores de la interfaz de usuario
            string status = (cbStatusBook.SelectedItem as ComboBoxItem)?.Content as string;
            bool boolFavorite = cbkFavorite.IsChecked ?? false;
            string coments = txtComents.Text;
            int? noteBook = cbNote.SelectedItem != null ? (int?)int.Parse((cbNote.SelectedItem as ComboBoxItem).Content.ToString()) : null;

            if (status == "Sin añadir")
            {
                MessageBox.Show("Por favor, selecciona un status para el libro.");
                return;
            }

            if (SesionClass.CurrentUserId.HasValue)
            {
                // detalles del libro de Google Books API
                var bookDetails = await _booksService.Volumes.Get(_bookId).ExecuteAsync();
                string coverUrl = bookDetails.VolumeInfo.ImageLinks?.Thumbnail ?? "URL por defecto si no hay imagen";

                bool existeLibro = await _conexion.ExistsBook(SesionClass.CurrentUserId.Value, _bookId);
                if (existeLibro)
                {
                    // Actualiza el libro con la nueva información
                    await _conexion.UpdateBook(SesionClass.CurrentUserId.Value, _bookId, tbTitle.Text, tbAuthor.Text, tbDate.Text, coverUrl, status, boolFavorite, coments, noteBook);
                    MessageBox.Show("Información del libro actualizada con éxito.");
                }
                else
                {
                    // Guarda el libro en la base de datos
                    await _conexion.SaveBook(SesionClass.CurrentUserId.Value, _bookId, tbTitle.Text, tbAuthor.Text, tbDate.Text, coverUrl, status, boolFavorite, coments, noteBook);
                    MessageBox.Show("Libro guardado con éxito.");
                }
            }
            else
            {
                MessageBox.Show("Debe iniciar sesión para realizar esta acción.");
            }

            this.Close();
        }



        private string CleanAuthors(IList<string> authorsList)
        {
            var authorsString = string.Join(", ", authorsList);
            var cleanString = Regex.Replace(authorsString, @"\s+", " ");
            return cleanString;
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SesionClass.CurrentUserId.HasValue)
            {
                bool existeLibro = await _conexion.ExistsBook(SesionClass.CurrentUserId.Value, _bookId);
                if (existeLibro)
                {
                    await _conexion.DeleteBook(SesionClass.CurrentUserId.Value, _bookId);
                    MessageBox.Show("Libro borrado");
                }
                else
                {
                    MessageBox.Show("No existe el libro en tu lista");
                }
            }
            else
            {
                MessageBox.Show("Debe iniciar sesión para realizar esta acción.");
            }

            this.Close();
        }
    }
}

