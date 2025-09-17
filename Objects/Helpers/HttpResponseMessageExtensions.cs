using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vanigam.CRM.Objects.Entities;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Vanigam.CRM.Objects.Helpers
{
    /// <summary>
    /// Class HttpResponseMessageExtensions.
    /// </summary>
    public static class VanigamAccountingHttpResponseMessageExtensions
    {
        /// <summary>
        /// Read as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">The response.</param>
        /// <returns>A Task&lt;T&gt; representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Unable to parse the response.</exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="Exception">Unable to parse the response.</exception>
        /// <exception cref="Exception"></exception>
        public static async Task<T> ReadAsync<T>(this HttpResponseMessage response, params JsonConverter[] converters)
        {
            try
            {
                response.EnsureSuccessStatusCode();
                //var responseAsString = await response.Content.ReadAsStringAsync();
                await using var stream = await response.Content.ReadAsStreamAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                };
                foreach (var jsonConverter in converters)
                {
                    options.Converters.Add(jsonConverter);
                }
                

                return stream.Length > 0 ? await JsonSerializer.DeserializeAsync<T>(stream,options) : default(T);
            }
            catch(Exception exception)
            {
                var responseAsString = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseAsString))
                {
                    if (response.Content.Headers.ContentType.MediaType == "application/json")
                    {
                        JsonDocument json;
                        try
                        {
                            json = JsonDocument.Parse(responseAsString);
                        }
                        catch
                        {
                            throw new Exception("Unable to parse the response.");
                        }

                        JsonElement error;
                        if (json.RootElement.TryGetProperty("error", out error))
                        {
                            JsonElement message;
                            if (error.TryGetProperty("message", out message))
                            {
                                throw new Exception(message.GetString());
                            }
                        }
                    }
                    else
                    {
                        XElement error = null;
                        try
                        {
                            var xml = XDocument.Parse(responseAsString);
                            var innerException = xml.Descendants().SingleOrDefault(p => p.Name.LocalName == "internalexception");
                            if (innerException != null)
                            {
                                error = innerException;
                            }
                            else
                            {
                                error = xml.Descendants().SingleOrDefault(p => p.Name.LocalName == "error");
                            }
                        }
                        catch
                        {
                            throw new Exception("Unable to parse the response.");
                        }

                        if (error != null)
                        {
                            throw new Exception(error.Value);
                        }
                    }
                }
                throw;
            }
        }
    }
}

