using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace BaGet.Extensions
{
    public static class HttpRequestExtensions
    {
        public const string ApiKeyHeader = "X-NuGet-ApiKey";

        public static async Task<Stream> GetUploadStreamOrNullAsync(this HttpRequest request, FileCache fileCache, CancellationToken cancellationToken)
        {
            // Try to get the nupkg from the multipart/form-data. If that's empty,
            // fallback to the request's body.
            try
            {
                await using var rawUploadStream = request is { HasFormContentType: true, Form.Files.Count: > 0 }
                    ? request.Form.Files[0].OpenReadStream()
                    : request.Body;

                // Convert the upload stream into a temporary file stream to
                // minimize memory usage.
                await using var writeStream =
                    new BufferedStream(new FileStream(fileCache.FullPath, FileMode.Create, FileAccess.Write));
                writeStream.Close();
                var readStream =
                    new BufferedStream(new FileStream(fileCache.FullPath, FileMode.Open, FileAccess.Read));
                return readStream;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to read the upload stream", e);
            }
        }

        public static string? GetApiKey(this HttpRequest request)
        {
            return request.Headers[ApiKeyHeader];
        }
    }
}
