using System.Collections.Concurrent;
using System.Net.Http;

using LabExtended.Attributes;
using LabExtended.Utilities.Update;

namespace LabExtended.Utilities;

/// <summary>
/// Utilities targeting HTTP things.
/// </summary>
public static class HttpUtils
{
    /// <summary>
    /// Contains data about a response.
    /// </summary>
    public struct HttpCallbackData
    {
        /// <summary>
        /// The original request.
        /// </summary>
        public readonly HttpRequestMessage Request;
        
        /// <summary>
        /// The received response.
        /// </summary>
        public readonly HttpResponseMessage Response;
        
        /// <summary>
        /// The string content of the response (<see cref="HttpContent.ReadAsStringAsync"/>).
        /// </summary>
        public readonly string? StringContent;
        
        /// <summary>
        /// The raw bytes content of the response (<see cref="HttpContent.ReadAsByteArrayAsync"/>).
        /// </summary>
        public readonly byte[]? RawContent;
        
        /// <summary>
        /// The exception that occured while sending the request (does NOT include the status code of the response).
        /// </summary>
        public readonly Exception? Exception;

        internal HttpCallbackData(HttpRequestMessage request, HttpResponseMessage response, string? stringContent,
            byte[]? rawContent, Exception? exception)
        {
            Request = request;
            Response = response;
            
            StringContent = stringContent;
            RawContent = rawContent;
            
            Exception = exception;
        }
    }
    
    private class HttpQueuedRequest
    {
        internal volatile HttpClient? Client;
        
        internal volatile HttpRequestMessage Message;
        internal volatile HttpResponseMessage Response;

        internal volatile string? StringContent;
        internal volatile byte[]? RawContent;

        internal volatile bool ReadStringContent;
        internal volatile bool ReadBytesContent;
        
        internal volatile Exception? Exception;

        internal volatile Action<HttpCallbackData>? Callback;
    }

    private static volatile ConcurrentQueue<HttpQueuedRequest> queue = new();
    private static volatile ConcurrentQueue<HttpQueuedRequest> callbacks = new();
    
    private static volatile HttpClient client = new();

    /// <summary>
    /// Queues a new request.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="callback">The optional callback delegate.</param>
    /// <param name="readBytes">Whether or not to read the response as a byte array.</param>
    /// <param name="readString">Whether or not to read the response as a string.</param>
    /// <param name="customClient">A custom client, a global one will be used if left as null.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void Queue(HttpRequestMessage message, Action<HttpCallbackData>? callback = null, bool readBytes = false,
        bool readString = false, HttpClient? customClient = null)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));
        
        queue.Enqueue(new()
        {
            Message = message,
            
            ReadStringContent = readString,
            ReadBytesContent = readBytes,
            
            Callback = callback,
            
            Client = customClient ?? client,
        });
    }

    private static void UpdateCallbacks()
    {
        while (callbacks.TryDequeue(out var request))
        {
            request.Callback?.Invoke(new(request.Message, request.Response, request.StringContent, request.RawContent,
                request.Exception));

            try
            {
                request.Response.Dispose();
                request.Message.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
    
    private static async Task UpdateQueueAsync()
    {
        while (queue.TryDequeue(out var request))
        {
            try
            {
                var response = await request.Client.SendAsync(request.Message);

                if (response.IsSuccessStatusCode)
                {
                    if (request.ReadStringContent)
                        request.StringContent = await response.Content.ReadAsStringAsync();
                    
                    if (request.ReadBytesContent)
                        request.RawContent = await response.Content.ReadAsByteArrayAsync();
                }

                if (request.Callback != null)
                {
                    callbacks.Enqueue(request);
                }
                else
                {
                    try
                    {
                        request.Response.Dispose();
                        request.Message.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch (Exception ex)
            {
                request.Exception = ex;
                
                callbacks.Enqueue(request);
            }
        }
    }
    
    internal static void Internal_Init()
    {
        PlayerUpdateHelper.Component.OnUpdate += UpdateCallbacks;
        PlayerUpdateHelper.OnThreadUpdate += UpdateQueueAsync;
    }
}