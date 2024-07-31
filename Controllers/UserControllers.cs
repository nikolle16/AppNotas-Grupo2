using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace App_Notas___Grupo_2.Controllers
{
    public class UserControllers
    {
        // Create
        public async static Task<int> Create(Models.User emple)
        {
            try
            {
                string jsonObject = JsonConvert.SerializeObject(emple);
                StringContent contenido = new StringContent(jsonObject, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

                    HttpResponseMessage response = await client.PostAsync(Config.Config.EndPointCreate, contenido);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        // Puedes manejar el resultado aquí si es necesario
                        return 1;
                    }
                    else
                    {
                        Console.WriteLine($"Ha Ocurrido un Error: {response.ReasonPhrase}");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ha Ocurrido un Error: {ex}");
                return -1;
            }
        }

        // Read
        public async static Task<List<Models.User>> Get()
        {
            List<Models.User> userList = new List<Models.User>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);

                    HttpResponseMessage response = await client.GetAsync(Config.Config.EndPointList);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        userList = JsonConvert.DeserializeObject<List<Models.User>>(result);
                    }
                    else
                    {
                        Console.WriteLine($"Error al obtener usuarios: {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ha Ocurrido un Error: {ex}");
            }
            return userList;
        }

        //Update
        public static async Task<int> Update(Models.User user)
        {
            try
            {
                String jsonObject = JsonConvert.SerializeObject(user);
                StringContent contenido = new StringContent(jsonObject, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);
                    HttpResponseMessage response = await client.PutAsync($"{Config.Config.EndPointUpdate}/{user.id}", contenido);

                    if (response != null && response.IsSuccessStatusCode)
                    {
                        return 1;
                    }
                    else
                    {
                        Console.WriteLine($"Error al actualizar: {response?.ReasonPhrase}");
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar: {ex}");
                return -1;
            }
        }

        //Delete
        public async static Task<int> Delete(int id)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.Config.BearerToken);
                    HttpResponseMessage response = null;

                    response = await client.DeleteAsync($"{Config.Config.EndPointDelete}/{id}");

                    if (response != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            Console.WriteLine($"Ha Ocurrido un Error: {response.ReasonPhrase}");
                            return -1;
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ha Ocurrido un Error: {ex.ToString()}");
                return -1;
            }
        }
    }
}
