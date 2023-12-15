using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using MylitLibrary.Services;
using MylitLibrary.Models;

namespace MylitLibrary
{
    public partial class MainWindow : Window
    {
        private readonly BooksService booksService;
        private readonly ConexionClass _conexion;
        public ObservableCollection<VolumeApiModel> MyCollectionBooks { get; set; }

        private System.Timers.Timer searchTimer;
        private string lastSearchText;

        public MainWindow()
        {
            InitializeComponent();

            // Autenticar con Google Books API
            booksService = GoogleBooksAuthentication.Authenticate();
            _conexion = new ConexionClass();
            searchTimer = new System.Timers.Timer(500);
            searchTimer.Elapsed += OnSearchTimerElapsed;
            DataContext = this;
            if (booksService == null)
            {
                MessageBox.Show("Error al autenticar con Google Books API." +
                    " Verifica tus credenciales y conexión a Internet.");
                Close();
                return;
            }

            MyCollectionBooks = new ObservableCollection<VolumeApiModel>();
            

            Loaded += MainWindow_Loaded;
            

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            loginWindow.Closed += LoginWindow_Closed;
            loginWindow.ShowDialog();
            
        }

        private void OpenLoginWindow()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            loginWindow.Closed += LoginWindow_Closed;
            loginWindow.ShowDialog();
            UpdateLoginStatus();
        }

        private async void LoginWindow_Closed(object sender, EventArgs e)
        {
            UpdateLoginStatus();
        }

        private async void UpdateLoginStatus()
        {
            
            bool isLoggedIn = SesionClass.CurrentUserId.HasValue && SesionClass.CurrentUserId.Value > 0;
            btMyLibrary.IsEnabled = isLoggedIn;
            btReading.IsEnabled = isLoggedIn;
            btCompleted.IsEnabled = isLoggedIn;
            btAbandoned.IsEnabled = isLoggedIn;
            btFavorited.IsEnabled = isLoggedIn;
            btPending.IsEnabled = isLoggedIn;
            // Actualizar el contenido del botón de sesión y el label de usuario
            btSesion.Content = isLoggedIn ? "Cerrar sesión" : "Iniciar sesión";
            lblUser.Content = isLoggedIn ? await _conexion.GetUsernameById(SesionClass.CurrentUserId.Value) : "Invitado";
        }



        private async Task SearchBooks(string query)
        {
            try
            {
                // Asíncronamente, limpia la colección y realiza la búsqueda
                MyCollectionBooks.Clear();
                var volumesList = await SearchBooksVolume(query);

                if (volumesList != null && volumesList.Any())
                {
                    // Mostrar el mensaje si no hay resultados
                    lblNoResults.Visibility = Visibility.Collapsed;

                    foreach (var volume in volumesList)
                    {
                        var viewModel = CreateVolumeViewModel(volume);
                        MyCollectionBooks.Add(viewModel);
                        
                    }

                }
                else
                {
                    // Mostrar el mensaje si no hay resultados
                    lblNoResults.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar libros: {ex.Message}");
            }
        }

        private VolumeApiModel CreateVolumeViewModel(Volume volume)
        {
            var coverUrl = volume.VolumeInfo.ImageLinks?.Thumbnail ??
                           Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "PortadaGenerica.png");
            var title = volume.VolumeInfo.Title ?? "Título desconocido";
            var authors = volume.VolumeInfo.Authors != null ? CleanAuthors(volume.VolumeInfo.Authors) : "Autor desconocido";
            var date = volume.VolumeInfo.PublishedDate ?? "Fecha desconocida";
            return new VolumeApiModel
            {
                IdBook = volume.Id,
                TitleBook = title,
                Author = authors,
                DatePublicBook = date,
                CoverUrl = coverUrl,

            };
        }

        //Reemplaza múltiples espacios en blanco autor
        private string CleanAuthors(IList<string> authorsList)
        {
            var authorsString = string.Join(", ", authorsList);
            var cleanString = Regex.Replace(authorsString, @"\s+", " ");
            return cleanString;
        }
   

        private async Task<IList<Volume>> SearchBooksVolume(string query)
        {
            var request = booksService.Volumes.List(query);
            var response = await request.ExecuteAsync();
            return response.Items ?? new List<Volume>(); // Si es null, devuelve una lista vacía.
        }


        private void txbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Comprueba si el texto está vacío.
            if (string.IsNullOrWhiteSpace(txbSearch.Text))
            {
                // Limpia la lista y actualiza el último texto buscado.
                MyCollectionBooks.Clear();
                lastSearchText = "";
                return;
            }

            // Si el texto ha cambiado, reinicia el temporizador.
            if (txbSearch.Text != lastSearchText)
            {
                searchTimer.Stop();
                searchTimer.Start();
            }
        }

        //Temporizador para las busquedas del textbox
        private void OnSearchTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                var searchText = txbSearch.Text;
                if (!string.IsNullOrWhiteSpace(searchText) && searchText != lastSearchText)
                {
                    lastSearchText = searchText;
                    await SearchBooks(searchText);
                }
            });
        }



        private void lbBooks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VolumeApiModel libroSeleccionado = (VolumeApiModel)lbBooks.SelectedItem;

            if (libroSeleccionado != null)
            {
                DetailBookWindow detalleLibroWindow = new DetailBookWindow(libroSeleccionado.IdBook);
                detalleLibroWindow.Owner = this;
                detalleLibroWindow.ShowDialog();
            }
        }

        private void ToggleMenu()
        {
            if (drhMenu != null)
            {
                drhMenu.IsLeftDrawerOpen = !drhMenu.IsLeftDrawerOpen;
            }
        }

        private void btToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenu();
        }

        private async void txbSearch_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.Key == Key.Enter)
            {
                
                await SearchBooks(txbSearch.Text);
                
            }
        }

        private void btSesion_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenu(); // Abre-Cierra el menú lateral
            if (SesionClass.CurrentUserId.HasValue && SesionClass.CurrentUserId.Value > 0)
            {
                
                SesionClass.CurrentUserId = null;
                UpdateLoginStatus(); 
            }
            else
            {
                // Abrir ventana de inicio de sesión solo si el usuario no está logueado
                OpenLoginWindow();
            }
        }

        private void btMyLibrary_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar la ventana de LibraryWindow y mostrar un Estado en especifico
            LibraryWindow libraryWindow = new LibraryWindow("Todos");
            libraryWindow.Owner = this;
            libraryWindow.ShowDialog();
        }

        private void btReading_Click(object sender, RoutedEventArgs e)
        {
            LibraryWindow libraryWindow = new LibraryWindow("Leyendo");
            libraryWindow.Owner = this;
            libraryWindow.ShowDialog();
        }

        private void btCompleted_Click(object sender, RoutedEventArgs e)
        {
            LibraryWindow libraryWindow = new LibraryWindow("Completados");
            libraryWindow.Owner = this;
            libraryWindow.ShowDialog();
        }

        private void btAbandoned_Click(object sender, RoutedEventArgs e)
        {
            LibraryWindow libraryWindow = new LibraryWindow("Abandonados");
            libraryWindow.Owner = this;
            libraryWindow.ShowDialog();
        }

        private void btFavorited_Click(object sender, RoutedEventArgs e)
        {
            LibraryWindow libraryWindow = new LibraryWindow("Favoritos");
            libraryWindow.Owner = this;
            libraryWindow.ShowDialog();
        }

        private void btPending_Click(object sender, RoutedEventArgs e)
        {
            LibraryWindow libraryWindow = new LibraryWindow("Pendientes");
            libraryWindow.Owner = this;
            libraryWindow.ShowDialog();
        }

        // Muestra la ventana de Acerca de
        private void btAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog(); 
        }
    }
}
