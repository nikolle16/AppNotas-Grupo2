using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace App_Notas___Grupo_2.Controllers
{
    public class UserControllers
    {
        //Crud
        //Create
        public async static Task<int> Create(Models.User emple)
        {
            try
            {
                String jsonObject = JsonConvert.SerializeObject(emple);
                System.Net.Http.StringContent contenido = new StringContent(jsonObject, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = null;

                    response = await client.PostAsync(Config.Config.EndPointCreate, contenido);

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

        //Read
        public async static Task<List<Models.User>> Get()
        {
            List<Models.User> userList = new List<Models.User>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = null;
                    response = await client.GetAsync(Config.Config.EndPointList);
                    if (response != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            try
                            {
                                userList = JsonConvert.DeserializeObject<List<Models.User>>(result);
                            }
                            catch (JsonException jex)
                            {

                            }
                        }
                    }
                    return userList;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //Update


        //Delete
    }
}
