using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using App_Notas___Grupo_2.Controllers;

namespace App_Notas___Grupo_2.Views
{
    public partial class Login : ContentPage
    {
        private List<Models.User> users;

        public Login()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            users = await UserControllers.Get();

            txtCorreo.Text = string.Empty;
            txtPassword.Text = string.Empty;
        }

        private async Task<bool> BuscarUsuario(string correo, string password)
        {
            if (users == null)
            {
                await DisplayAlert("Error", "No se pudo obtener la lista de usuarios", "OK");
                return false;
            }

            var user = users.FirstOrDefault(u => u.correo == correo);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.password))
            {
                Console.WriteLine($"Usuario encontrado: {user.id}");
                SavePreferences("userId", user.id.ToString());
                Console.WriteLine($"userId guardado en las preferencias: {user.id}");
                return true;
            }
            else
            {
                await DisplayAlert("Error", "Correo o contraseña incorrectos", "OK");
                return false;
            }
        }

        // Preferences
        public void SavePreferences(string key, string value)
        {
            Preferences.Set(key, value);
            Console.WriteLine($"Preferencia guardada: {key} = {value}");
        }

        public string GetPreferences(string key, string defaultValue)
        {
            var value = Preferences.Get(key, defaultValue);
            Console.WriteLine($"Preferencia obtenida: {key} = {value}");
            return value;
        }

        public void RemovePreferences(string key)
        {
            Preferences.Remove(key);
            Console.WriteLine($"Preferencia eliminada: {key}");
        }

        private async void btnIniSesion_Clicked(object sender, EventArgs e)
        {
            string Correo = txtCorreo.Text;
            string Password = txtPassword.Text;

            bool userValido = await BuscarUsuario(Correo, Password);

            if (userValido)
            {
                SavePreferences("username", Correo);
                SavePreferences("password", Password);
                await Navigation.PushAsync(new Principal());
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CrearUser());
        }

        private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            Navigation.PushAsync(new RestPass());
        }
    }
}
