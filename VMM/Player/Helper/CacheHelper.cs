using System;
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
        private static readonly int ResponseTimeOut = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;

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
                    return new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                finally
                {
                    entry.IsLoading = false;
                }
            }

            try
            {
                PlayRequest?.Abort();
            }
            catch(WebException e) when(e.Status == WebExceptionStatus.RequestCanceled)
            {
                //ignore
            }            
            PlayRequest = WebRequest.CreateHttp(entry.Url);

            try
            {
                var response = await PlayRequest.GetResponseAsync().WithTimeout(ResponseTimeOut, ct);
                var songStream = response.GetResponseStream();
                var stream = new SeekableStream(songStream, response.ContentLength);

                SaveWhenBuffered(stream, cacheFilePath);

                return stream;
            }
            catch(WebException e) when(e.Status == WebExceptionStatus.RequestCanceled)
            {
                throw new OperationCanceledException();
            }
        }

        private static void SaveWhenBuffered(IBufferedObservable observable, string filePath)
        {
            //Wrap delegate with array to capture modified closure

            EventHandler<long>[] onBuffered = {null};
            EventHandler<Exception>[] onBufferingFailed = {null};

            onBuffered[0] = async (sender, bufferedBytes) =>
            {
                if(bufferedBytes == observable.Length)
                {
                    var buffer = observable.GetBuffer;

                    using(var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(buffer, 0, buffer.Length);

                    }

                    observable.Buffed -= onBuffered[0];
                    observable.BufferingFailed -= onBufferingFailed[0];

                    onBuffered = null;
                    onBufferingFailed = null;
                }
            };

            
            onBufferingFailed[0] = (sender, exception) =>
            {
                observable.Buffed -= onBuffered[0];
                observable.BufferingFailed -= onBufferingFailed[0];

                onBuffered = null;
                onBufferingFailed = null;
            };

            observable.Buffed += onBuffered[0];
            observable.BufferingFailed += onBufferingFailed[0];
        }
    }
}