using CantinaIBJ.Framework.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace CantinaIBJ.Framework.Requests
{
    /// <summary>
    /// Implementação da inteface básica de acesso a recursos básicos de API
    /// </summary>
    public class APIResource : IApiResources
    {
        //static HttpClient client = new HttpClient();
        private readonly JsonSerializerSettings JsonSettings;
        protected ILogger _logger;
        private string _endpoint;
        private string _baseURI;

        public string Auth;
        public string BaseURI
        {
            get { return _baseURI; }
            set { _baseURI = $"{_endpoint}/{value}"; }
        }

        public string EndPoint
        {
            get { return _endpoint; }
            set { _endpoint = value; }
        }

        public HttpClient Client { get; set; } = new();

        /// <summary>
        /// Construtor customizado que permite total controle sobre as configurações do client
        /// </summary>
        public APIResource(IHttpClientWrapper customClient,
            JsonSerializerSettings customJsonSerializerSettings = null)
        {
            JsonSettings = customJsonSerializerSettings ?? new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            _baseURI = $"{_endpoint}";
        }

        /// <summary>
        /// Construtor default que usa as configurações padrão do httpClient e do JsonSerializer
        /// </summary>
        public APIResource() : this(
            new StandardHttpClient (),
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
        {
        }

        public void Dispose()
        {
            //client.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<T> GetAsync<T>()
        {
            var response = await SendRequestAsync(HttpMethod.Get, BaseURI).ConfigureAwait(false);
            return await ProcessResponse<T>(response).ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(string id)
        {
            var response = await GetAsync<T>(id, null, null).ConfigureAwait(false);
            return response;
        }

        public async Task<T> GetAsync<T>(string id, List<KeyValueType> apiUserToken)
        {
            var response = await GetAsync<T>(id, null, apiUserToken).ConfigureAwait(false);
            return response;
        }

        public async Task GetAsync(string partOfUrl, string query)
        {
            await GetAsync(partOfUrl, query, null);
        }

        public async Task<HttpResponseMessage> GetAsync(string partOfUrl, string query, List<KeyValueType> apiUserToken)
        {
            var completeUrl = GetCompleteUrl(partOfUrl, query);
            return await SendRequestAsync(HttpMethod.Get, completeUrl, null, apiUserToken).ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(string id, string partOfUrl, List<KeyValueType> apiUserToken)
        {
            var completeUrl = GetCompleteUrl(partOfUrl, id);
            var response = await SendRequestAsync(HttpMethod.Get, completeUrl, null, apiUserToken).ConfigureAwait(false);
            return await ProcessResponse<T>(response).ConfigureAwait(false);
        }

        public async Task<T> PostAsync<T>()
        {
            var response = await PostAsync<T>(null, null, null, null, null).ConfigureAwait(false);
            return response;
        }
        public async Task<T> PostAsync<T>(object data)
        {
            var response = await PostAsync<T>(data, null, null, null, null).ConfigureAwait(false);
            return response;
        }

        public async Task<bool> PostDownloadFile(object data, string filePath)
        {
            var completeUrl = GetCompleteUrl(null, null);
            var response = await SendRequestAsync(HttpMethod.Post, completeUrl, data, null, null).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var length = response.Content?.Headers?.ContentLength;
                var base64string = await response.Content.ReadAsStringAsync();
                byte[] bytes = Convert.FromBase64String(base64string);
                File.WriteAllBytes(filePath, bytes);
                return true;
            }
            return false;
        }



        public async Task<T> PostAsync<T>(object data, string partOfUrl)
        {
            var response = await PostAsync<T>(data, partOfUrl, null, null, null).ConfigureAwait(false);
            return response;
        }

        public async Task<T> PostAsync<T>(object data, string partOfUrl, string query, List<KeyValueType> customApiToken, List<KeyValueType> form)
        {
            var completeUrl = GetCompleteUrl(partOfUrl, query);
            var response = await SendRequestAsync(HttpMethod.Post, completeUrl, data, customApiToken, form).ConfigureAwait(false);
            return await ProcessResponse<T>(response).ConfigureAwait(false);
        }

        public async Task<T> PatchAsync<T>(object data, string partOfUrl, string query, List<KeyValueType> customApiToken, List<KeyValueType> form)
        {
            var completeUrl = GetCompleteUrl(partOfUrl, query);
            var response = await SendRequestAsync(HttpMethod.Patch, completeUrl, data, customApiToken, form).ConfigureAwait(false);
            return await ProcessResponse<T>(response).ConfigureAwait(false);
        }

        public async Task<T> PutAsync<T>(string id, object data)
        {
            return await PutAsync<T>(data, id, null, null).ConfigureAwait(false);
        }

        public async Task<T> PutAsync<T>(object data, string partOfUrl)
        {
            return await PutAsync<T>(data, partOfUrl, null, null).ConfigureAwait(false);
        }

        public async Task<T> PutAsync<T>(object data, string partOfUrl, List<KeyValueType> customApiToken, List<KeyValueType> form)
        {
            var completeUrl = GetCompleteUrl(partOfUrl, null);
            var response = await SendRequestAsync(HttpMethod.Put, completeUrl, data, customApiToken, form).ConfigureAwait(false);
            return await ProcessResponse<T>(response).ConfigureAwait(false);
        }

        public async Task<T> DeleteAsync<T>(string id)
        {
            return await DeleteAsync<T>(id, null).ConfigureAwait(false);
        }

        public async Task<T> DeleteAsync<T>(string id, List<KeyValueType> customApiToken)
        {
            var response = await SendRequestAsync(HttpMethod.Delete, $"{BaseURI}/{id}", null, customApiToken).ConfigureAwait(false);
            return await ProcessResponse<T>(response).ConfigureAwait(false);
        }

        public async Task<T> DeleteAsync<T>(string id, object data, List<KeyValueType> customApiToken)
        {
            var response = await SendRequestAsync(HttpMethod.Delete, $"{BaseURI}/{id}", data, customApiToken).ConfigureAwait(false);
            return await ProcessResponse<T>(response).ConfigureAwait(false);
        }

        public async Task<T> ProcessResponse<T>(HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                _logger?.LogDebug($"Response Body: {data}");
            }
            else
            {
                _logger?.LogCritical($"Error at {response?.RequestMessage?.RequestUri}: Response: {data}");
            }
            try
            {
                return await Task.FromResult(JsonConvert.DeserializeObject<T>(data, JsonSettings)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogCritical($"Error at ProcessResponse. Could not deserialize JSON object: {data} to {typeof(T).FullName}. Exception: {ex}");
                throw new Exception($"Erro ao processar retorno da requisição. {data}.", ex);
            }
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, object data = null, List<KeyValueType> customHeader = null, List<KeyValueType> form = null)
        {
            try
            {
                using (var requestMessage = new HttpRequestMessage(method, url))
                {
                    SetAutorizationHeader(requestMessage);

                    if (customHeader != null)
                    {
                        SetCustomHeader(requestMessage, customHeader);
                    }

                    if (form != null)
                    {
                        SetFormData(requestMessage, form);
                    }

                    var watch = new Stopwatch();


                    await SetContent(data, requestMessage);
                    _logger?.LogInformation($"Request: {method} {url}");

                    watch.Start();
                    var response = await Client.SendAsync(requestMessage).ConfigureAwait(false);
                    watch.Stop();

                    _logger?.LogInformation($"Response: {response.StatusCode}. Took (ms): {watch.ElapsedMilliseconds}");

                    return response;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error formating HTTP msg", ex);
            }
        }

        private async Task SetContent(object data, HttpRequestMessage requestMessage)
        {
            if (data != null)
            {
                var content = await Task.FromResult(JsonConvert.SerializeObject(data, JsonSettings)).ConfigureAwait(false);
                _logger?.LogDebug($"Request Body: {content}");
                requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
                requestMessage.Content.Headers.ContentType.CharSet = string.Empty;
            }
        }

        private void SetAutorizationHeader(HttpRequestMessage requestMessage)
        {
            if (!string.IsNullOrEmpty(Auth))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Auth);
            }
        }

        private void SetCustomHeader(HttpRequestMessage requestMessage, List<KeyValueType> headers)
        {
            headers.ForEach(header =>
            {
                if (!string.IsNullOrEmpty(header.Key))
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            });
        }

        private void SetFormData(HttpRequestMessage requestMessage, List<KeyValueType> forms)
        {
            MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----" + Guid.NewGuid().ToString());

            foreach (var form in forms)
            {
                if (!string.IsNullOrEmpty(form.Key))
                {
                    if (form.Type == "file")
                    {
                        FileStream stream = File.Open(form.Value, FileMode.Open);
                        form.Value = form.Value.Split(Path.DirectorySeparatorChar).Last();
                        var fileContent = new StreamContent(stream);
                        fileContent.Headers.Add("Content-Type", "application/octet-stream");
                        fileContent.Headers.Add("Content-Disposition", "form-data; name=\"" + form.Key + "\"; filename=\"" + form.Value + "\"");
                        multiPartContent.Add(fileContent, "file", form.Value);
                        requestMessage.Content = multiPartContent;
                    }
                    else if (form.Type == "application/x-www-form-urlencoded")
                    {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                        var content = new FormUrlEncodedContent(forms.Select(item => KeyValuePair.Create<string, string?>(item.Key, item.Value)).AsEnumerable());
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                        requestMessage.Content = content;
                        break;
                    }
                    else
                    {
                        var content = new StringContent(form.Value);
                        content.Headers.Add("Content-Disposition", "form-data; name=\"" + form.Key + "\"");
                        multiPartContent.Add(content, form.Key);
                        requestMessage.Content = multiPartContent;
                    }
                }
            };
        }


        private string GetCompleteUrl(string partOfUrl, string query)
        {
            var url = string.IsNullOrEmpty(partOfUrl) ? $"{BaseURI}?{query}" : $"{BaseURI}/{partOfUrl}?{query}";
            if (url.Last().Equals('?'))
            {
                url = url.Remove(url.Length - 1);
            }
            if (url.Last().Equals('/'))
            {
                url = url.Remove(url.Length - 1);
            }
            return url;
        }
    }

    public class KeyValueType
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }
}
