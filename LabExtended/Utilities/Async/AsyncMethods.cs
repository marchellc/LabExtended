using System.Net.Http;

namespace LabExtended.Utilities.Async
{
    public static class AsyncMethods
    {
        public static AsyncOperation<HttpResponseMessage> HttpAsync(HttpRequestMessage request, HttpClient client = null)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            async Task<HttpResponseMessage> SendAsync()
            {
                var disposeClient = client is null;

                client ??= new HttpClient();

                var response = await client.SendAsync(request);

                if (disposeClient)
                    client.Dispose();

                return response;
            }

            return AsyncRunner.RunThreadAsync(SendAsync());
        }

        public static AsyncOperation<byte[]> GetByteArrayAsync(string requestUrl, HttpClient client = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
                throw new ArgumentNullException(nameof(requestUrl));

            async Task<byte[]> SendAsync()
            {
                var disposeClient = client is null;

                client ??= new HttpClient();

                var response = await client.GetByteArrayAsync(requestUrl);

                if (disposeClient)
                    client.Dispose();

                return response;
            }

            return AsyncRunner.RunThreadAsync(SendAsync());
        }
    }
}
