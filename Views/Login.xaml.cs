using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
            users = await Controllers.UserControllers.Get();

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
                return true;
            }
            else
            {
                await DisplayAlert("Error", "Correo o contraseņa incorrectos", "OK");
                return false;
            }
        }

        //Preferences
        public void SavePreferences(string key, string value)
        {
            Preferences.Set(key, value);
        }

        public void GetPreferences(string key, string value)
        {
            Preferences.Get(key, value);
        }

        public void RemovePreferences(string key)
        {
            Preferences.Remove(key);
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
            Navigation.PushAsync(new Principal());
        }
    }
}
