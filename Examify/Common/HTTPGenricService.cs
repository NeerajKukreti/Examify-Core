
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using System.Text;



/// <summary>
/// A generic wrapper class to REST API calls
/// </summary>
/// <typeparam name="T"></typeparam>
public static class HTTPClientWrapper<T>
{ 

    /// <summary>
    /// For getting the resources from a web api
    /// </summary>
    /// <param name="url">API Url</param>
    /// <returns>A Task with result object of type T</returns>
    public static async Task<T> Get(string apiUrl,string url, StringBuilder parameters = default)
    {
        T result = default(T);

        using (HttpClient httpClient = new HttpClient())
        {
            UriBuilder baseUri = new UriBuilder(apiUrl + url);
            string queryToAppend = (parameters??new StringBuilder("")).ToString();

            if (baseUri.Query != null && baseUri.Query.Length > 1)
                baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
            else
                baseUri.Query = queryToAppend;

            var response = httpClient.GetAsync(new Uri(baseUri.ToString())).Result;

            response.EnsureSuccessStatusCode();

            await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
            {
                if (x.IsFaulted)
                    throw x.Exception;

                result = JsonConvert.DeserializeObject<T>(x.Result);
            });
        }

        return result;
    }

    /// <summary>
    /// For creating a new item over a web api using POST
    /// </summary>
    /// <param name="apiUrl">API Url</param>
    /// <param name="postObject">Passing as json</param>
    /// <returns>A Task with created item</returns>
    public static async Task<T> PostRequest(string apiUrl, string Url, object postObject)
    {
        T result = default(T);

        using (var client = new HttpClient())
        {
            var response = await client.PostAsync(apiUrl + Url, postObject, new JsonMediaTypeFormatter()).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
            {
                if (x.IsFaulted)
                    throw x.Exception;

                result = JsonConvert.DeserializeObject<T>(x.Result);

            });

        }

        return result;
    }

    /// For creating a new item over a web api using POST
    /// </summary>
    /// <param name="apiUrl">API Url</param>
    /// <param name="Parameters">Passing as query string</param>
    /// <returns>A Task with created item</returns>
    public static async Task<T> PostRequest(string apiUrl, string Url, StringBuilder Parameters = default)
    {
        T result = default(T);

        string queryToAppend = Parameters.ToString();
        UriBuilder baseUri = new UriBuilder(apiUrl + Url);

        if (baseUri.Query != null && baseUri.Query.Length > 1)
            baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
        else
            baseUri.Query = queryToAppend;

        using (var client = new HttpClient())
        {

            var response = await client.PostAsync(baseUri.ToString(), null).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
            {
                if (x.IsFaulted)
                    throw x.Exception;

                result = JsonConvert.DeserializeObject<T>(x.Result);

            });

        }

        return result;
    }

}
