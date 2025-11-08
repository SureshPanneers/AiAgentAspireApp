using System.Text;

namespace AiAgentAspireApp.Web
{
    public class AgentApiClient(HttpClient httpClient)
    {

        public async IAsyncEnumerable<string> StreamAgentChatAsync(string prompt)
        {
            var requestUri = $"/agent/api/agent/chat?prompt={Uri.EscapeDataString(prompt)}";
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // Extended timeout
            using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            char[] buffer = new char[1024];
            int bytesRead;
            while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                yield return new string(buffer, 0, bytesRead);
            }
        }
    }
}
