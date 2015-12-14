using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using VMM.Model;

namespace VMM.Helper
{
    public static class CacheHelper
    {
        private const string TempPath = "VMM/Cache";
        private const int DefaultStreamReadBufferSize = 128 * 1024;

        private static readonly WebClient CacheWebClient = new WebClient();
        private static readonly WebClient SizeRetrievingClient = new TimedOutWebClient(TimeSpan.FromSeconds(5));
        private static readonly WebClient PlayClient = new WebClient();

        private static readonly SemaphoreSlim CachingSyncSemaphore = new SemaphoreSlim(1);

        public static Stream Download(MusicEntry entry)
        {
            return DownloadInternal(entry);
        }

        private static Stream DownloadInternal(MusicEntry entry)
        {
            byte[] data;

            var cachePath = Path.Combine(Path.GetTempPath(), TempPath);
            if(!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            var cacheFilePath = Path.Combine(cachePath, entry.Id.ToString());
            var remoteFileSize = GetRemoteFileSize(entry.Url);

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

            if(remoteFileSize > 0)
            {
                return new BufferedStream(new SeekableStream(PlayClient.OpenRead(entry.Url), remoteFileSize), DefaultStreamReadBufferSize);
            }

            return Stream.Null;
        }

        private static long GetRemoteFileSize(Uri uri)
        {
            lock(SizeRetrievingClient)
            {
                try
                {
                    var stream = SizeRetrievingClient.OpenRead(uri);

                    var fileSize = long.Parse(SizeRetrievingClient.ResponseHeaders["Content-Length"]);
                    stream?.Dispose();

                    return fileSize;
                }
                catch(Exception e)
                {
                    Trace.WriteLine($"While getting file size: {e}");

                    return 0;
                }
            }
        }
    }
}