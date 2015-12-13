using System;
using System.Diagnostics;
using System.IO;
using VMM.Model;

namespace VMM.Helper
{
    public static class CacheHelper
    {
        private const string TempPath = "VMM/Cache";

        public static Stream Download(MusicEntry entry)
        {
            entry.IsLoading = true;

            var stream = DownloadInternal(entry);

            entry.IsLoading = false;

            return stream;
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

            if(File.Exists(cacheFilePath))
            {
                var remoteFileSize = GetRemoteFileSize(entry.Url);
                var cacheSize = new FileInfo(cacheFilePath).Length;

                if(remoteFileSize == cacheSize)
                {
                    return new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read);
                }
            }

            lock(Vk.Instance.Client)
            {
                try
                {
                    data = Vk.Instance.Client.DownloadData(entry.Url);
                    File.WriteAllBytes(cacheFilePath, data);
                }
                catch(Exception e)
                {
                    Trace.WriteLine(e);
                    data = new byte[0];
                }
            }

            return new MemoryStream(data);
        }

        private static long GetRemoteFileSize(Uri uri)
        {
            lock(Vk.Instance.Client)
            {
                try
                {
                    var stream = Vk.Instance.Client.OpenRead(uri);

                    var fileSize = long.Parse(Vk.Instance.Client.ResponseHeaders["Content-Length"]);
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