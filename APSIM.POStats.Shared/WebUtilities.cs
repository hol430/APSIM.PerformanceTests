using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace APSIM.POStats.Shared
{
    public class WebUtilities
    {
        public static async Task<string> PostAsync<T>(string requestUrl, T content)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var json = JsonSerializer.Serialize(content);
                var response = await httpClient.PostAsync(requestUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                return data;
            }
        }
    }
}