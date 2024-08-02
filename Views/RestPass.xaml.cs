namespace App_Notas___Grupo_2.Views;

public partial class RestPass : ContentPage
{
    private Controllers.UserControllers userControllers;
    private List<Models.User> users;
    Controllers.UserControllers controller;

    public RestPass()
	{
		InitializeComponent();
        controller = new Controllers.UserControllers();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        users = await Controllers.UserControllers.Get();

        txtCorreo.Text = string.Empty;
        txtPassword.Text = string.Empty;
        txtConfirmPassword.Text = string.Empty;
    }

    private async Task<Models.User?> BuscarCorreo(string correo)
    {
        if (users == null)
        {
            await DisplayAlert("Error", "No se pudo obtener la lista de usuarios", "OK");
            return null;
        }

        var user = users.FirstOrDefault(u => u.correo == correo);

        if (user != null)
        {
            return user;
        }
        else
        {
            await DisplayAlert("Error", "El correo ingresado no está registrado", "OK");
            return null;
        }
    }

    private async void btnActualizar_Clicked(object sender, EventArgs e)
    {
        string correo = txtCorreo.Text;
        string password = txtPassword.Text;
        string confirmPassword = txtConfirmPassword.Text;

        if (string.IsNullOrEmpty(correo))
        {
            await DisplayAlert("Error", "Por favor ingrese un correo", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Por favor ingrese una contraseña", "OK");
            return;
        }
        else if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
            return;
        }

        var user = await BuscarCorreo(correo);
        if (user == null) return; // Si el correo no existe, detén la ejecución

        // Hashear la nueva contraseña
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        // Actualizar la contraseña del usuario
        user.password = hashedPassword;

        try
        {
            if (controller != null)
            {
                if (await Controllers.UserControllers.Update(user) > 0)
                {
                    await DisplayAlert("Aviso", "Contraseña actualizada con éxito!", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Ocurrió un error al intentar actualizar la contraseña", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }
}