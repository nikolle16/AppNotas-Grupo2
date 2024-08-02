using BCrypt.Net;

namespace App_Notas___Grupo_2.Views;

public partial class ActuUser : ContentPage
{
    private Controllers.UserControllers userControllers;
    private List<Models.User> users;
    private Models.User currentUser;
    Controllers.UserControllers controller;
    FileResult photo;

    public ActuUser()
    {
        InitializeComponent();
        controller = new Controllers.UserControllers();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        users = await Controllers.UserControllers.Get();

        // Obtener el userId guardado en las preferencias
        var userId = Preferences.Get("userId", null);

        if (userId != null)
        {
            // Buscar el usuario actual por su ID
            currentUser = users.FirstOrDefault(u => u.id.ToString() == userId);

            if (currentUser != null)
            {
                // Mostrar los datos del usuario en la interfaz
                imgFoto.Source = "avatar.png"; // O cambiarlo si tienes la foto del usuario
                txtNombre.Text = currentUser.nombre;
                txtCorreo.Text = currentUser.correo;
            }
        }
    }

    public string? GetImg64()
    {
        if (photo != null)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Stream stream = photo.OpenReadAsync().Result;
                stream.CopyTo(ms);
                byte[] data = ms.ToArray();

                String Base64 = Convert.ToBase64String(data);

                return Base64;
            }
        }
        return null;
    }

    private async void btnfoto_Clicked(object sender, EventArgs e)
    {
        photo = await MediaPicker.CapturePhotoAsync();

        if (photo != null)
        {
            string photoPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using Stream sourcephoto = await photo.OpenReadAsync();
            using FileStream streamlocal = File.OpenWrite(photoPath);

            imgFoto.Source = ImageSource.FromStream(() => photo.OpenReadAsync().Result);

            await sourcephoto.CopyToAsync(streamlocal);
        }
    }

    private async void btnCrear_Clicked(object sender, EventArgs e)
    {
        string Nombre = txtNombre.Text;
        string Correo = txtCorreo.Text;
        string OldPass = txtOldPassword.Text;
        string NewPass = txtNewPassword.Text;

        if (string.IsNullOrEmpty(Nombre))
        {
            await DisplayAlert("Error", "Por favor ingrese su nombre", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(Correo))
        {
            await DisplayAlert("Error", "Por favor ingrese un correo", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(NewPass))
        {
            await DisplayAlert("Error", "Por favor ingrese una contraseña", "OK");
            return;
        }

        // Verificar la contraseña actual
        if (currentUser != null && BCrypt.Net.BCrypt.Verify(OldPass, currentUser.password))
        {
            // Actualizar la información del usuario
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(NewPass);

            var updatedUser = new Models.User
            {
                id = currentUser.id, // Mantener el ID del usuario actual
                nombre = txtNombre.Text,
                correo = txtCorreo.Text,
                password = hashedPassword,
                foto = GetImg64() ?? currentUser.foto // Mantener la foto actual si no se seleccionó una nueva
            };

            try
            {
                if (controller != null)
                {
                    if (await Controllers.UserControllers.Update(updatedUser) > 0)
                    {
                        await DisplayAlert("Aviso", "Registro Actualizado con Éxito!", "OK");
                        await Navigation.PushAsync(new Principal());
                    }
                    else
                    {
                        await DisplayAlert("Error", "Ocurrió un Error", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un Error: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Contraseña actual incorrecta", "OK");
        }
    }

    private async void btnEliminarCuenta_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirmar", "¿Estás seguro de que quieres eliminar tu cuenta? Esta acción no se puede deshacer.", "Sí", "No");

        if (confirm)
        {
            if (currentUser != null)
            {
                int result = await Controllers.UserControllers.Delete(currentUser.id);

                if (result > 0)
                {
                    await DisplayAlert("Aviso", "Cuenta eliminada con éxito.", "OK");

                    // Limpiar las preferencias y navegar a la pantalla de inicio de sesión
                    Preferences.Clear();
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Ocurrió un error al intentar eliminar la cuenta.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "No se pudo encontrar la cuenta actual.", "OK");
            }
        }
    }
}
