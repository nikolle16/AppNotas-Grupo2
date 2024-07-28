using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using App_Notas___Grupo_2.Models;

namespace App_Notas___Grupo_2.Services
{
    public static class NotaService
    {
        public static async Task<List<Note>> GetNotasAsync(string userId)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

            var url = $"{Config.Config.EndPointGetNotes}?userId={userId}"; // Asumiendo que tu API soporta esta consulta
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Note>>(json);
            }

            return new List<Note>();
        }
    }
}
