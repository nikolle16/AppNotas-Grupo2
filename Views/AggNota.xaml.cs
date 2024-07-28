using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using App_Notas___Grupo_2.Config;

namespace App_Notas___Grupo_2.Views
{
    public partial class AggNota : ContentPage
    {
        private List<byte[]> imageBytesList = new List<byte[]>();

        public AggNota()
        {
            InitializeComponent();
        }

        private async void OnSelectImagesButtonClicked(object sender, EventArgs e)
        {
            var images = await PickImagesAsync();
            if (images != null)
            {
                imageBytesList.AddRange(images);
                foreach (var image in images)
                {
                    var imageContainer = new ImageContainer(image, OnDeleteImage);
                    imagesStackLayout.Children.Add(imageContainer);
                }
            }
        }

        private void OnDeleteImage(ImageContainer imageContainer)
        {
            imageBytesList.Remove(imageContainer.ImageData);
            imagesStackLayout.Children.Remove(imageContainer);
        }

        private async void OnSaveNoteButtonClicked(object sender, EventArgs e)
        {
            var title = titleEntry.Text;
            var content = contentEditor.Text;
            var userId = Preferences.Get("userId", null); // Obtener el ID del usuario desde las preferencias
            Console.WriteLine($"userId recuperado de las preferencias: {userId}");

            if (userId != null)
            {
                var base64Images = imageBytesList.ConvertAll(image => Convert.ToBase64String(image));
                await CreateNoteAsync(title, content, base64Images, userId);
                await DisplayAlert("Éxito", "Nota guardada exitosamente", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo obtener el ID del usuario", "OK");
            }
        }

        public async Task<List<byte[]>> PickImagesAsync()
        {
            var result = new List<byte[]>();
            var photo = await Microsoft.Maui.Media.MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                using (var stream = await photo.OpenReadAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        result.Add(memoryStream.ToArray());
                    }
                }
            }
            return result;
        }

        private async Task CreateNoteAsync(string title, string content, List<string> images, string userId)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

            var note = new
            {
                title = title,
                content = content,
                userId = userId, // Incluir el ID del usuario en el cuerpo de la solicitud
                images = images
            };

            var json = JsonConvert.SerializeObject(note);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(Config.Config.EndPointCreateNote, httpContent);

            if (response.IsSuccessStatusCode)
            {
                // Nota creada exitosamente
            }
            else
            {
                // Manejar el error
                await DisplayAlert("Error", "Hubo un problema al guardar la nota", "OK");
            }
        }
    }
}





