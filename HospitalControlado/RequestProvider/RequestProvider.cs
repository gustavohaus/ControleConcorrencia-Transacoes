using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HospitalControlado.RequestProvider
{
    public class RequestProvider
    {
        public RequestProvider()
        {

        }

        public async Task<T> Get<T>(string url, string path)
        {
            HttpClient client = CreateClient();
            var uri = url + path;
            var response = await client.GetAsync(url + path);

            var result = await HandleResponse<T>(response);

            return result;
        }

        public async Task<byte[]> GetFile(string path)
        {
            HttpClient client = CreateClient();

            var response = await client.GetAsync(path);

            await HandleResponse(response);

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<T> Post<T>(string api, string path, object data)
        {
            HttpClient client = CreateClient();
            var json = JsonConvert.SerializeObject(data);
            var body = new StringContent(json,
                Encoding.UTF8,
                "application/json");
            var uri = api;
            var response = await client.PostAsync(uri + path, body);

            var result = await HandleResponse<T>(response);

            return result;

        }

        public async Task Post(string api, string path, object data)
        {
            HttpClient client = CreateClient();

            var json = JsonConvert.SerializeObject(data);
            var body = new StringContent(json, Encoding.UTF8,
                "application/json");
            var uri = api + path;
            var response = await client.PostAsync(uri, body);

            await HandleResponse(response);

        }



        private HttpClient CreateClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Language", "pt-BR");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));

            //client.DefaultRequestHeaders.TryAddWithoutValidation("Session_Id", HttpContext.Current.Session?.SessionID);
            //if (!string.IsNullOrEmpty(HttpContext.Current.Session["access_token"]?.ToString()))
            //    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {HttpContext.Current.Session["access_token"]}");

            return client;
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    ErrorObject error = JsonConvert.DeserializeObject<ErrorObject>(result);

                    string message = "";
                    foreach (var erro in error.errors)
                    {
                        message += $"{erro.message} <br >";
                    }

                  //  throw new ApiException(response.StatusCode, message);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                //    throw new ApiException(response.StatusCode, "Página não encontrada");
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                  //  throw new ApiException(response.StatusCode, "Erro interno do servidor");
                }
            }

            return default(T);
        }

        private async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    ErrorObject error = JsonConvert.DeserializeObject<ErrorObject>(result);

                    string message = "";
                    foreach (var erro in error.errors)
                    {
                        message += $"{erro.message} <br >";
                    }

                   // throw new ApiException(response.StatusCode, message);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                   // throw new ApiException(response.StatusCode, "Página não encontrada");
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    //throw new ApiException(response.StatusCode, "Erro interno do servidor");
                }
            }
        }



    }



    internal class ErrorObject
    {
        public string message { get; set; }
        public List<ErrorMessage> errors { get; set; }


    }

    internal class ErrorMessage
    {
        public string field { get; set; }
        public string message { get; set; }
    }

}
