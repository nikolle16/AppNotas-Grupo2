using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using App_Notas___Grupo_2.Models;

namespace App_Notas___Grupo_2.Views
{
    public partial class Principal : ContentPage
    {
        public Principal()
        {
            InitializeComponent();
            LoadNotes();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadNotes();
        }

        private async void OnAgregarNotaClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Agregar Nota", "Cancel", null, "Nota de Texto", "Nota de Audio");
            switch (action)
            {
                case "Nota de Texto":
                    await Navigation.PushAsync(new AggNota());
                    break;
                case "Nota de Audio":
                    await Navigation.PushAsync(new AggNotaAudio());
                    break;
            }
        }

        private async Task LoadNotes()
        {
            var userId = Preferences.Get("userId", null);
            if (userId == null)
            {
                await DisplayAlert("Error", "No se pudo obtener el ID del usuario", "OK");
                return;
            }

            var notes = await GetNotesAsync(userId);
            if (notes != null)
            {
                NotasListView.ItemsSource = notes;
                NotasListView.ItemTapped += async (s, e) =>
                {
                    if (e.Item is Note selectedNote)
                    {
                        await Navigation.PushAsync(new NoteDetailPageNewPage1(selectedNote.Id));
                    }
                    ((ListView)s).SelectedItem = null; // Deselect item
                };
            }
        }

        private async Task<List<Note>> GetNotesAsync(string userId)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

            var response = await httpClient.GetAsync($"{Config.Config.EndPointGetNotes}?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Note>>(json);
            }
            else
            {
                await DisplayAlert("Error", "Hubo un problema al obtener las notas", "OK");
                return null;
            }
        }
    }
}



