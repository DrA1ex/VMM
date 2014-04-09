using System;
using System.Diagnostics;
using System.IO;
using VMM.Model;

namespace VMM.Helper
{
    public static class CacheHelper
    {
        private const string TempPath = "VMM/Cache";

        public static MemoryStream Download(MusicEntry entry)
        {
            entry.IsLoading = true;

            var stream = DownloadInternal(entry);

            entry.IsLoading = false;

            return stream;
        }

        private static MemoryStream DownloadInternal(MusicEntry entry)
        {
            byte[] data;

            string cachePath = Path.Combine(Path.GetTempPath(), TempPath);
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            string cacheFilePath = Path.Combine(cachePath, entry.Id.ToString());

            if (File.Exists(cacheFilePath))
            {
                long remoteFileSize = GetRemoteFileSize(entry.Url);
                long cacheSize = new FileInfo(cacheFilePath).Length;

                if (remoteFileSize == cacheSize)
                {
                    return new MemoryStream(File.ReadAllBytes(cacheFilePath));
                }
            }

            lock (Vk.Instance.Client)
            {
                try
                {
                    data = Vk.Instance.Client.DownloadData(entry.Url);
                    File.WriteAllBytes(cacheFilePath, data);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                    data = new byte[0];
                }
            }

            return new MemoryStream(data);
        }

        private static long GetRemoteFileSize(Uri uri)
        {
            lock (Vk.Instance.Client)
            {
                try
                {
                    var stream = Vk.Instance.Client.OpenRead(uri);

                    var fileSize = long.Parse(Vk.Instance.Client.ResponseHeaders["Content-Length"]);

                    if (stream != null)
                    {
                        stream.Close();
                    }

                    return fileSize;
                }
                catch (Exception e)
                {
                    Trace.WriteLine(String.Format("While getting file size: {0}", e));

                    return 0;
                }
            }
        }
    }
}