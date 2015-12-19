using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VMM.Helper;
using VMM.Model;
using VMM.Player.Reader;

namespace VMM.Player.Helper
{
    public static class CacheHelper
    {
        private const string TempPath = "VMM/Cache";
        private const int DefaultStreamReadBufferSize = 512 * 1024;
        private static readonly int ResponseTimeOut = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;

        private static readonly SemaphoreSlim CachingSyncSemaphore = new SemaphoreSlim(1);
        private static HttpWebRequest CachingRequest { get; set; }
        private static HttpWebRequest PlayRequest { get; set; }

        public static async Task<Stream> Download(MusicEntry entry, CancellationToken ct)
        {
            var cachePath = Path.Combine(Path.GetTempPath(), TempPath);
            if(!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            var cacheFilePath = Path.Combine(cachePath, entry.Id.ToString());

            if(File.Exists(cacheFilePath))
            {
                entry.IsLoading = true;

                try
                {
                    return new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read);
                }
                finally
                {
                    entry.IsLoading = false;
                }
            }

            QueueCaching(entry, cacheFilePath);
            PlayRequest?.Abort();
            PlayRequest = WebRequest.CreateHttp(entry.Url);

            try
            {
                var response = await PlayRequest.GetResponseAsync().WithTimeout(ResponseTimeOut, ct);
                var songStream = response.GetResponseStream();
                return new ReadAheadStream<SeekableStream>(new SeekableStream(songStream, response.ContentLength), DefaultStreamReadBufferSize);
            }
            catch(WebException e) when(e.Status == WebExceptionStatus.RequestCanceled)
            {
                throw new OperationCanceledException();
            }
        }

        private static void QueueCaching(MusicEntry entry, string cacheFilePath)
        {
            CachingRequest?.Abort();
            CachingRequest = WebRequest.CreateHttp(entry.Url);

            var thisRequest = CachingRequest;

            CachingSyncSemaphore.WaitAsync().ContinueWith(async o =>
            {
                try
                {
                    entry.IsLoading = true;

                    using(var response = await thisRequest.GetResponseAsync())
                    using(var stream = response.GetResponseStream())
                    using(var resultStream = new MemoryStream((int)response.ContentLength))
                    {
                        if(stream != null)
                        {
                            await stream.CopyToAsync(resultStream);

                            using(var fileStream = new FileStream(cacheFilePath, FileMode.Create))
                            {
                                await resultStream.CopyToAsync(fileStream);
                            }
                        }
                    }
                }
                catch(WebException e) when(e.Status == WebExceptionStatus.RequestCanceled)
                {
                    //ingore
                }
                catch(Exception e)
                {
                    Trace.WriteLine($"Unable to cache file: {e}");
                }
                finally
                {
                    entry.IsLoading = false;
                    CachingSyncSemaphore.Release();
                }
            });
        }
    }
}