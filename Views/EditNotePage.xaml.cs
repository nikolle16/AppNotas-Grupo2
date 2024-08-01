using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using App_Notas___Grupo_2.Config;
using App_Notas___Grupo_2.Models;

namespace App_Notas___Grupo_2.Views
{
    public partial class EditNotePage : ContentPage
    {
        private int noteId;
        private List<byte[]> imageBytesList = new List<byte[]>();


        public EditNotePage(int noteId)
        {
            InitializeComponent();
            this.noteId = noteId;
            LoadNote();
        }

        private async void LoadNote()
        {
            var note = await GetNoteAsync(noteId);
            if (note != null)
            {
                titleEntry.Text = note.Title;
                contentEditor.Text = note.Content;
                foreach (var imageBase64 in note.Images)
                {
                    var imageBytes = Convert.FromBase64String(imageBase64);
                    imageBytesList.Add(imageBytes);
                    var imageContainer = new ImageContainer(imageBytes, OnDeleteImage);
                    imagesStackLayout.Children.Add(imageContainer);
                }
            }
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

            if (userId != null)
            {
                var base64Images = imageBytesList.ConvertAll(image => Convert.ToBase64String(image));
                await UpdateNoteAsync(noteId, title, content, base64Images, userId);
                await DisplayAlert("Éxito", "Nota actualizada exitosamente", "OK");
                await Navigation.PushAsync(new Principal());
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

        private async Task<Note> GetNoteAsync(int noteId)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

            var response = await httpClient.GetStringAsync($"{Config.Config.EndPointGetNote}/{noteId}");
            return JsonConvert.DeserializeObject<Note>(response);
        }

        private async Task UpdateNoteAsync(int noteId, string title, string content, List<string> images, string userId)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

            var note = new
            {
                id = noteId,
                title = title,
                content = content,
                userId = userId,
                images = images
            };

            var json = JsonConvert.SerializeObject(note);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync($"{Config.Config.EndPointUpdateNote}/{noteId}", httpContent);

            if (response.IsSuccessStatusCode)
            {
                // Nota actualizada exitosamente
            }
            else
            {
                // Manejar el error
                await DisplayAlert("Error", "Hubo un problema al actualizar la nota", "OK");
            }
        }
    }
}