using MylitLibrary.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MylitLibrary
{

    public partial class LoginWindow : Window
    {
        private ConexionClass _conexion;

        public LoginWindow()
        {
            InitializeComponent();
            _conexion = new ConexionClass();
        }
        private async void LoginUser()
        {
            string username = txtUsername.Text;
            string password = pbPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor, ingresa tanto el nombre de usuario como la contraseña.");
                return;
            }

            bool loginSuccessful = await _conexion.VerifyCredentials(username, password);

            if (loginSuccessful)
            {
                
                SesionClass.CurrentUserId = await _conexion.GetUserIdByUsername(username);
                this.Close();
            }
            else
            {
                MessageBox.Show("Nombre de usuario o contraseña incorrecta.");
            }
        }
        private void txtUsername_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsername.Focus();
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                pbPassword.Focus();
            }
        }

        private void pbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginUser();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginUser();
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = pbPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor, ingresa tanto el nombre de usuario como la contraseña.");
                return;
            }

            bool exists = await _conexion.ExistsUserByUserName(username);
            if (exists)
            {
                MessageBox.Show("El nombre de usuario ya existe. Por favor, elige otro nombre.");
                return;
            }

            // Verificar si la contraseña cumple con los requisitos mínimos
            if (!PassWordValidator(password))
            {
                MessageBox.Show("La contraseña debe tener al menos 8 caracteres, un número y una letra mayúscula.");
                return;
            }

            await _conexion.RegisterUser(username, password);
            SesionClass.CurrentUserId = await _conexion.GetUserIdByUsername(username);
            MessageBox.Show("Registro exitoso. Por favor, inicia sesión.");
            await Task.Delay(TimeSpan.FromSeconds(2));
            this.Close();
        }

        // Método para verificar la validez de la contraseña
        private bool PassWordValidator(string password)
        {
            // Verificar si la contraseña tiene al menos 8 caracteres, un número y una letra mayúscula
            return System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*\d)(?=.*[A-Z]).{8,}$");
        }
    }
}

