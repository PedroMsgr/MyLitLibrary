using MylitLibrary.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MylitLibrary.Entities;


namespace MylitLibrary
{
    
    public partial class LibraryWindow : Window
    {
        private ConexionClass _conexion;
        
        public ObservableCollection<UserBooks> MyBooks { get; set; }

        private string _initialFilter;

        public LibraryWindow(string initialFilter = "")
        {
            InitializeComponent();
            _conexion = new ConexionClass();
            MyBooks = new ObservableCollection<UserBooks>();
            DataContext = this;
            _initialFilter = initialFilter;
        }

        private void lbMyBooks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UserBooks libroSeleccionado = (UserBooks)lbMyBooks.SelectedItem;

            if (libroSeleccionado != null)
            {
                DetailBookWindow detalleLibroWindow = new DetailBookWindow(libroSeleccionado.IdBook);
                detalleLibroWindow.Owner = this;

                detalleLibroWindow.Closed += DetailBookWindow_Closed;

                detalleLibroWindow.ShowDialog();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBooks();
            FiltrerStatus(_initialFilter);
        }

        private async void DetailBookWindow_Closed(object sender, EventArgs e)
        {
            await LoadBooks();
            lbMyBooks.Items.Refresh();
        }


        private async Task LoadBooks()
        {

            if (SesionClass.CurrentUserId.HasValue)
            {
                // Limpiar la lista actual
                MyBooks.Clear();

                // Recargar los libros del usuario desde la base de datos
                var books = await _conexion.GetBooksByUser(SesionClass.CurrentUserId.Value);
                foreach (var book in books)
                {
                    MyBooks.Add(book);
                }
            }
        }

        private void OnFilterButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string filtro = menuItem.Tag.ToString();
                FiltrerStatus(filtro);
            }
        }

        // Aplica el filtro de estado a la lista
        private void FiltrerStatus(string filtro)
        {
            switch (filtro)
            {
                case "Todos":
                    lbMyBooks.ItemsSource = MyBooks;
                    break;
                case "Completados":
                    lbMyBooks.ItemsSource = MyBooks.Where(book => book.StatusBook == "Completado").ToList();
                    break;
                case "Leyendo":
                    lbMyBooks.ItemsSource = MyBooks.Where(book => book.StatusBook == "Leyendo").ToList();
                    break;
                case "Pendientes":
                    lbMyBooks.ItemsSource = MyBooks.Where(book => book.StatusBook == "Pendiente").ToList();
                    break;
                case "Abandonados":
                    lbMyBooks.ItemsSource = MyBooks.Where(book => book.StatusBook == "Abandonado").ToList();
                    break;
                
                case "Favoritos":
                    lbMyBooks.ItemsSource = MyBooks.Where(book => book.Favorite).ToList();
                    break;
            }
        }

    }
}

