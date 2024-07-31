using Newtonsoft.Json;
using System.Text;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace App_Notas___Grupo_2.Views
{
    public partial class AggNotaAudio : ContentPage
    {
        readonly IAudioManager _audioManager;
        readonly IAudioRecorder _audioRecorder;
        ObservableCollection<Models.Audio> _audioFiles;
        int _fileCounter;
        string _filePath;

        public AggNotaAudio(IAudioManager audioManager)
        {
            InitializeComponent();

            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();

            _audioFiles = new ObservableCollection<Models.Audio>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            btnPlayRecord.IsVisible = true;
            btnStopRecord.IsVisible = false;
            txtTitle.Text = string.Empty;
        }

        private async void btnPlayRecord_Clicked(object sender, EventArgs e)
        {
            if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Microphone permission is required to record audio.", "OK");
                return;
            }

            if (!_audioRecorder.IsRecording)
            {
                await _audioRecorder.StartAsync();
                btnPlayRecord.IsVisible = false;
                btnStopRecord.IsVisible = true;
            }
        }

        private async void btnStopRecord_Clicked(object sender, EventArgs e)
        {
            if (_audioRecorder.IsRecording)
            {
                var recordedAudio = await _audioRecorder.StopAsync();

                // Obtener el flujo de audio grabado
                var audioStream = recordedAudio.GetAudioStream();

                // Definir la ruta y el nombre del archivo
                _fileCounter++;
                string fileName = $"audio_recording_{_fileCounter}.wav";
                string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                _filePath = filePath;

                // Guardar el archivo en la ruta especificada
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await audioStream.CopyToAsync(fileStream);
                }

                // Informar al usuario que el archivo ha sido guardado
                await DisplayAlert("Audio Guardado", $"El archivo de audio se ha guardado en {filePath}", "OK");

                // Resetear la UI
                btnPlayRecord.IsVisible = true;
                btnStopRecord.IsVisible = false;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                await DisplayAlert("Alerta", "Por favor ingrese un título para el audio", "OK");
                return;
            }

            var title = txtTitle.Text;
            var audioPath = _filePath;
            var fecha = DateTime.Now;
            var userId = Preferences.Get("userId", null);
            Console.WriteLine($"userId recuperado de las preferencias: {userId}");

            if (userId != null)
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

                var audios = new
                {
                    userId = userId,
                    title = title,
                    audio = audioPath,
                    fecha = fecha
                };

                var json = JsonConvert.SerializeObject(audios);
                var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                try
                {
                    var response = await httpClient.PostAsync(Config.Config.EndPointCreateAudio, httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", "Audio guardado exitosamente", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        await DisplayAlert("Error", $"Hubo un problema al guardar el audio: {errorContent}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Excepción al intentar guardar el audio: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "No se pudo obtener el ID del usuario", "OK");
            }
        }

    }
}
