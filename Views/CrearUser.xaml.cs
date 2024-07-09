using System.Collections.ObjectModel;
using BCrypt.Net;

namespace App_Notas___Grupo_2.Views;

public partial class CrearUser : ContentPage
{
    private Controllers.UserControllers userControllers;
    private List<Models.User> users;
    Controllers.UserControllers controller;
    FileResult photo;

    public CrearUser()
	{
		InitializeComponent();
        controller = new Controllers.UserControllers();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        users = await Controllers.UserControllers.Get();

        imgFoto.Source = "avatar.png";
        txtNombre.Text = string.Empty;
        txtCorreo.Text = string.Empty;
        txtPassword.Text = string.Empty;
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

    private async Task<bool> BuscarCorreo(string correo)
    {
        if (users == null)
        {
            await DisplayAlert("Error", "No se pudo obtener la lista de usuarios", "OK");
            return false;
        }

        var results = users
            .Where(user => user.correo == correo)
            .ToList();

        if (results.Count() != 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private async void btnCrear_Clicked(object sender, EventArgs e)
    {
        string Nombre = txtNombre.Text;
        string Correo = txtCorreo.Text;
        string Password = txtPassword.Text;
        bool corre = await BuscarCorreo(Correo);

        if (string.IsNullOrEmpty(Nombre))
        {
            await DisplayAlert("Error", "Porfavor ingrese su nombre", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(Correo))
        {
            await DisplayAlert("Error", "Porfavor ingrese un correo", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(Password))
        {
            await DisplayAlert("Error", "Porfavor ingrese una contraseña", "OK");
            return;
        }
        else if (corre == false)
        {
            await DisplayAlert("Error", "El correo ingresado ya esta registrado", "OK");
            return;
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

        var user = new Models.User
        {
            nombre = txtNombre.Text,
            correo = txtCorreo.Text,
            password = hashedPassword,
            foto = GetImg64()
        };

        try
        {
            if (controller != null)
            {
                if (await Controllers.UserControllers.Create(user) > 0)
                {
                    await DisplayAlert("Aviso", "Registro Ingresado con Exito!", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Ocurrio un Error", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrio un Error: {ex.Message}", "OK");
        }
    }

    private void btnRegresar_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

}