using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FlightsManagmentSystemWebAPI.Middlewares
{
    /// <summary>
    /// Midlleware that is used for logging each request and response
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;//Next middleware in the pipeline
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;//Will be used to prevent potential memory leak in very large requests or responses (larger than 85,000 bytes)
        public RequestResponseLoggingMiddleware(RequestDelegate next,
                                                ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory
                      .CreateLogger<RequestResponseLoggingMiddleware>();
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }
        public async Task Invoke(HttpContext context)
        {
            await LogRequest(context);//log the request
            await LogResponse(context);//log the response
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();//Enable buffer for the large request, if it's over 30K write to file so it can be read multiple times
            await using var requestStream = _recyclableMemoryStreamManager.GetStream();//Create new memory stream
            await context.Request.Body.CopyToAsync(requestStream);//Read the body from the request
            _logger.LogInformation($"Http Request Information:{Environment.NewLine}" +
                                   $"Method Type: {context.Request.Method} " +
                                   $"Path: {context.Request.Path} " +
                                   $"QueryString: {context.Request.QueryString} " +
                                   $"Username: {context.User.Identity.Name} " +
                                   $"Request Body: {ReadStreamInChunks(requestStream)}");
            context.Request.Body.Position = 0;// Reset the request body stream position so the next middleware can read it

        }

        private async Task LogResponse(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;
            await _next(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            _logger.LogInformation($"Http Response Information:{Environment.NewLine}" +
                                   $"Method Type: {context.Request.Method} " +
                                   $"Path: {context.Request.Path} " +
                                   $"QueryString: {context.Request.QueryString} " +
                                   $"Username: {context.User.Identity.Name} " +
                                   $"Response Body: {text}");
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private static string ReadStreamInChunks(Stream stream)//Read stream in parts
        {
            const int readChunkBufferLength = 4096;//The length of the part
            stream.Seek(0, SeekOrigin.Begin);//Set the position in the stream
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];//array of char
            int readChunkLength;
            do
            {
                readChunkLength = reader.ReadBlock(readChunk,
                                                   0,
                                                   readChunkBufferLength);//Reads from the stream
                textWriter.Write(readChunk, 0, readChunkLength);//Writes to string
            } while (readChunkLength > 0);//do this while the stream not fully read
            return textWriter.ToString();
        }
    }

}
