using App_Notas___Grupo_2.Models;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using App_Notas___Grupo_2.Config;

namespace App_Notas___Grupo_2.Views
{
    public partial class NoteDetailPageNewPage1 : ContentPage
    {
        public readonly int noteId;

        public NoteDetailPageNewPage1(int noteId)
        {
            InitializeComponent();
            this.noteId = noteId;
            LoadNoteDetails(noteId);
        }

        private async void LoadNoteDetails(int noteId)
        {
            var note = await GetNoteAsync(noteId);
            if (note != null)
            {
                TitleLabel.Text = note.Title;
                ContentLabel.Text = note.Content;

                foreach (var imageBase64 in note.Images)
                {
                    var imageBytes = Convert.FromBase64String(imageBase64);
                    var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    var image = new Image { Source = imageSource, HeightRequest = 200, WidthRequest = 200 };
                    ImagesStackLayout.Children.Add(image);
                }
            }
        }

        private async Task<Note> GetNoteAsync(int noteId)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

            var response = await httpClient.GetAsync($"{Config.Config.EndPointGetNote}/{noteId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Note>(json);
            }
            else
            {
                await DisplayAlert("Error", "Hubo un problema al obtener los detalles de la nota", "OK");
                return null;
            }
        }

        private async void OnEditNoteClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditNotePage(noteId));
        }

        private async void OnDeleteNoteButtonClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert("Confirmar", "¿Estás seguro de que deseas eliminar esta nota?", "Sí", "No");
            if (confirm)
            {
                var success = await DeleteNoteAsync(noteId);
                if (success)
                {
                    await DisplayAlert("Éxito", "Nota eliminada exitosamente", "OK");
                    await Navigation.PushAsync(new Principal());
                }
                else
                {
                    await DisplayAlert("Error", "Hubo un problema al eliminar la nota", "OK");
                }
            }
        }

        private async Task<bool> DeleteNoteAsync(int noteId)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

            var response = await httpClient.DeleteAsync($"{Config.Config.EndPointDeleteNote}/{noteId}");
            return response.IsSuccessStatusCode;
        }
    

        private async void ButtonEditNote(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditNotePage(noteId));
        }
    }
}
