using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VMM.Model;

namespace VMM.Helper
{
    public static class CacheHelper
    {
        private const string TempPath = "VMM/Cache";
        private const int DefaultStreamReadBufferSize = 128 * 1024;

        private static readonly WebClient CacheWebClient = new WebClient();
        private static readonly WebClient PlayClient = new WebClient();

        private static readonly SemaphoreSlim CachingSyncSemaphore = new SemaphoreSlim(1);

        private static HttpWebRequest _sizeRetrievingRequest;

        private static readonly int SizeRetrievingTimeOut = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;

        public static Task<Stream> Download(MusicEntry entry)
        {
            return DownloadInternal(entry);
        }

        private static async Task<Stream> DownloadInternal(MusicEntry entry)
        {
            byte[] data;

            var cachePath = Path.Combine(Path.GetTempPath(), TempPath);
            if(!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            var cacheFilePath = Path.Combine(cachePath, entry.Id.ToString());
            var remoteFileSize = await GetRemoteFileSize(entry.Url);

            if(File.Exists(cacheFilePath))
            {
                entry.IsLoading = true;

                try
                {
                    var cacheSize = new FileInfo(cacheFilePath).Length;
                    if(remoteFileSize == cacheSize)
                    {
                        return new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read);
                    }
                }
                finally
                {
                    entry.IsLoading = false;
                }
            }

#pragma warning disable CS4014
            CachingSyncSemaphore.WaitAsync().ContinueWith(async o =>
            {
                try
                {
                    entry.IsLoading = true;

                    data = await CacheWebClient.DownloadDataTaskAsync(entry.Url);
                    File.WriteAllBytes(cacheFilePath, data);
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
#pragma warning restore CS4014

            if(remoteFileSize > 0)
            {
                PlayClient.CancelAsync();

                try
                {
                    return new BufferedStream(new SeekableStream(await PlayClient.OpenReadTaskAsync(entry.Url), remoteFileSize), DefaultStreamReadBufferSize);
                }
                catch(WebException e)
                {
                    if(e.Status == WebExceptionStatus.RequestCanceled)
                    {
                        throw new OperationCanceledException();
                    }

                    throw;
                }
            }

            return Stream.Null;
        }

        private static async Task<long> GetRemoteFileSize(Uri uri)
        {
            _sizeRetrievingRequest?.Abort();

            _sizeRetrievingRequest = (HttpWebRequest)WebRequest.Create(uri);
            _sizeRetrievingRequest.Timeout
                = _sizeRetrievingRequest.ContinueTimeout
                    = SizeRetrievingTimeOut;
            try
            {
                var response = await _sizeRetrievingRequest.GetResponseAsync().WithTimeout(SizeRetrievingTimeOut);
                var fileSize = response.ContentLength;
                response.Close();

                return fileSize;
            }
            catch(WebException e)
            {
                if(e.Status == WebExceptionStatus.RequestCanceled)
                    throw new OperationCanceledException();

                throw;
            }
            catch(TimeoutException)
            {
                _sizeRetrievingRequest.Abort();
                _sizeRetrievingRequest = null;

                throw;
            }
            catch(Exception e)
            {
                Trace.WriteLine($"While getting file size: {e}");

                throw;
            }
        }
    }
}