using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using App_Notas___Grupo_2.Models;

namespace App_Notas___Grupo_2.Views
{
    public partial class Principal : ContentPage
    {
        private List<Note> notes;
        private List<Audio> audios;

        public Principal()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            NavigationPage.SetHasBackButton(this, false);
            base.OnAppearing();

            await LoadNotes();
            await LoadAudioFiles();
        }

        private async void OnToolbarItemClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Menú", "Cancel", null, "Actualizar Usuario", "Cerrar Sesión");
            switch (action)
            {
                case "Actualizar Usuario":
                    await Navigation.PushAsync(new ActuUser());
                    break;
                case "Cerrar Sesión":
                    Logout();
                    break;
            }
        }

        private void Logout()
        {
            Preferences.Remove("userId");
            Preferences.Remove("username");
            Preferences.Remove("password");

            Application.Current.MainPage = new NavigationPage(new Login());
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
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
                    var audioManager = MauiProgram.GetService<IAudioManager>();
                    if (audioManager != null)
                    {
                        await Navigation.PushAsync(new AggNotaAudio(audioManager));
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo obtener el administrador de audio", "OK");
                    }
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

            notes = await GetNotesAsync(userId);
            if (notes != null)
            {
                NotasListView.ItemsSource = notes;
                NotasListView.ItemTapped += async (s, e) =>
                {
                    if (e.Item is Note selectedNote)
                    {
                        await Navigation.PushAsync(new NoteDetailPageNewPage1(selectedNote.Id));
                    }
                    ((ListView)s).SelectedItem = null;
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
                return new List<Note>(); // Devuelve una lista vacía en caso de error.
            }
        }

        private async Task LoadAudioFiles()
        {
            var userId = Preferences.Get("userId", null);
            if (userId == null)
            {
                await DisplayAlert("Error", "No se pudo obtener el ID del usuario", "OK");
                return;
            }

            audios = await GetAudiosAsync(userId);
            if (audios != null)
            {
                var audiosOrdenados = audios.OrderByDescending(a => a.fecha).ToList();
                AudioListView.ItemsSource = audiosOrdenados;
                AudioListView.ItemTapped += async (s, e) =>
                {
                    if (e.Item is Audio selectedAudio)
                    {
                        var player = AudioManager.Current.CreatePlayer(File.OpenRead(selectedAudio.audio));
                        player.Play();
                        await DisplayAlert("Reproduciendo", $"Reproduciendo {selectedAudio.title}", "OK");
                    }
                    ((ListView)s).SelectedItem = null;
                };
            }
        }

        private async Task<List<Audio>> GetAudiosAsync(string userId)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

            var response = await httpClient.GetAsync($"{Config.Config.EndPointGetAudios}?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Audio>>(json);
            }
            else
            {
                await DisplayAlert("Error", "Hubo un problema al obtener los audios", "OK");
                return new List<Audio>(); // Devuelve una lista vacía en caso de error.
            }
        }

        private async void OnEliminarAudioClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is Audio audio)
            {
                bool isConfirmed = await DisplayAlert("Confirmar", $"¿Desea eliminar el audio '{audio.title}'?", "Sí", "No");
                if (isConfirmed)
                {
                    var result = await DeleteAudioAsync(audio);
                    if (result)
                    {
                        await DisplayAlert("Éxito", "El audio ha sido eliminado.", "OK");
                        await LoadAudioFiles();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Hubo un problema al eliminar el audio.", "OK");
                    }
                }
            }
        }

        private async Task<bool> DeleteAudioAsync(Audio audio)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

            try
            {
                var url = $"{Config.Config.EndPointDeleteAudio}/{audio.id}";
                var response = await httpClient.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception eliminando audio: {ex.Message}");
                return false;
            }
        }

        private async void OnCompartirNotaClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is Note note)
            {
                var shareTextRequest = new ShareTextRequest
                {
                    Title = note.Title,
                    Text = $"{note.Title}\n\n{note.Content}"
                };

                await Share.RequestAsync(shareTextRequest);
            }
        }

        private async void OnCompartirAudioClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is Audio audio)
            {
                var filePath = audio.audio;
                if (System.IO.File.Exists(filePath))
                {
                    var file = new ShareFile(filePath);
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = audio.title,
                        File = file
                    });
                }
                else
                {
                    await DisplayAlert("Error", $"El archivo de audio no se encuentra en la ruta: {filePath}", "OK");
                }
            }
        }

        private void searchBarNotas_TextChanged(object sender, TextChangedEventArgs e)
        {
            BuscarNotas(e.NewTextValue);
        }

        private void searchBarAudio_TextChanged(object sender, TextChangedEventArgs e)
        {
            BuscarAudio(e.NewTextValue);
        }

        private void BuscarNotas(string query)
        {
            if (notes != null)
            {
                var lowerCaseQuery = query?.ToLower();
                var results = notes
                    .Where(note => note.Title?.ToLower().StartsWith(lowerCaseQuery) ?? false)
                    .ToList();

                NotasListView.ItemsSource = results;
                EmptyMessageLabel.IsVisible = !results.Any();
            }
        }

        private void BuscarAudio(string query)
        {
            if (audios != null)
            {
                var lowerCaseQuery = query?.ToLower();
                var results = audios
                    .Where(audio => audio.title?.ToLower().StartsWith(lowerCaseQuery) ?? false)
                    .ToList();

                AudioListView.ItemsSource = results;
                EmptyMessageLabel.IsVisible = !results.Any();
            }
        }
    }
}