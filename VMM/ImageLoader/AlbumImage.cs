using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using VMM.Model;

namespace VMM.ImageLoader
{
    public static class AlbumImage
    {
        private const string RequestPattern = @"https://www.google.ru/search?q={0}&safe=off&tbm=isch&tbs=isz:l&cad=h";
        private const string UrlPrefix = "imgurl=";
        private const string UrlPostfix = "&amp;imgrefurl=";

        private const string TempPath = "VMM/Cache/Covers";
        private const int StreamCopyBufferSize = 81920;


        public static readonly DependencyProperty SongProperty = DependencyProperty.RegisterAttached(
            "Song", typeof(MusicEntry), typeof(AlbumImage), new PropertyMetadata(OnSongChanged));

        private static MusicEntry CurrentEntry { get; set; }
        private static BitmapImage CachedImage { get; set; }

        private static CancellationTokenSource CancellationTokenSource { get; set; }
        private static WebRequest CurrentRequest { get; set; }

        public static void SetSong(DependencyObject element, MusicEntry value)
        {
            element.SetValue(SongProperty, value);
        }

        public static MusicEntry GetSong(DependencyObject element)
        {
            return (MusicEntry)element.GetValue(SongProperty);
        }

        private static async void OnSongChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var entry = e.NewValue as MusicEntry;

            var image = d as Image;
            var imageBrush = d as ImageBrush;

            if(entry != null && (image != null || imageBrush != null))
            {
                if(CurrentEntry != entry)
                {
                    CurrentEntry = entry;
                    CachedImage = null;
                }

                if(CachedImage == null)
                {
                    try
                    {
                        await GetAlbumImage(entry);
                    }
                    catch(OperationCanceledException)
                    {
                        return;
                    }
                    catch(Exception)
                    {
                        //ignore
                    }
                }

                if(image != null)
                {
                    image.Source = CachedImage;
                }
                else
                {
                    imageBrush.ImageSource = CachedImage;
                }
            }
        }

        private static async Task GetAlbumImage(MusicEntry entry)
        {
            if(CurrentRequest != null)
            {
                try
                {
                    CurrentRequest.Abort();
                    CurrentRequest = null;
                }
                catch(Exception)
                {
                    //ignore
                }                
            }

            if(CancellationTokenSource != null && CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel(true);
                CancellationTokenSource.Dispose();
                CancellationTokenSource = null;
            }

            var path = Path.Combine(Path.GetTempPath(), TempPath);
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var cachedFilePath = Path.Combine(path, entry.Id.ToString());

            if(File.Exists(cachedFilePath))
            {
                CachedImage = new BitmapImage(new Uri(cachedFilePath, UriKind.Absolute));
            }
            else
            {
                if(CancellationTokenSource == null)
                {
                    CancellationTokenSource = new CancellationTokenSource();
                }

                var token = CancellationTokenSource.Token;
                var addresses = await GetAlbumImageUrl($"{entry.Artist} {entry.Name} album");
                foreach(var uri in addresses)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        CachedImage = await CacheImage(uri, cachedFilePath, token);
                        break;
                    }
                    catch(Exception e) when(!(e is OperationCanceledException))
                    {
                        //ignore
                    }
                }
            }
        }

        private static async Task<BitmapImage> CacheImage(Uri url, string cachedFilePath, CancellationToken ct)
        {
            BitmapImage result = null;
            CurrentRequest = WebRequest.CreateHttp(url);
            WebResponse response;
            try
            {
                response = await CurrentRequest.GetResponseAsync();
            }
            catch(WebException e) when(e.Status == WebExceptionStatus.RequestCanceled)
            {
                throw new OperationCanceledException();
            }

            using(Stream responseStream = response.GetResponseStream())
            {
                if(responseStream != null)
                {
                    using(var outStream = new FileStream(cachedFilePath, FileMode.Create))
                    {
                        await responseStream.CopyToAsync(outStream, StreamCopyBufferSize, ct);
                    }

                    result = new BitmapImage(new Uri(cachedFilePath, UriKind.Absolute));
                }
            }

            CurrentRequest = null;

            return result;
        }

        private static async Task<Uri[]> GetAlbumImageUrl(string song)
        {
            var requestUrl = string.Format(RequestPattern, Uri.EscapeUriString(song));
            var request = WebRequest.CreateHttp(requestUrl);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";

            try
            {
                var response = await request.GetResponseAsync();

                using(var stream = response.GetResponseStream())
                {
                    var doc = new HtmlDocument();
                    doc.Load(stream);
                    return doc.DocumentNode.SelectNodes("//a[@target='_blank']")
                        .Select(c => c.GetAttributeValue("href", null))
                        .Select(ExtractUri)
                        .Where(c => c != null)
                        .Take(5)
                        .ToArray();
                }
            }
            catch(WebException)
            {
                //ignore
            }

            return null;
        }

        private static Uri ExtractUri(string href)
        {
            if(!string.IsNullOrWhiteSpace(href))
            {
                var startPos = href.IndexOf(UrlPrefix, StringComparison.Ordinal);
                var endPos = href.IndexOf(UrlPostfix, StringComparison.Ordinal);

                if(startPos != -1 && endPos != -1)
                {
                    var firstSymbolIndex = startPos + UrlPrefix.Length;
                    var length = endPos - firstSymbolIndex;
                    var url = href.Substring(firstSymbolIndex, length);

                    return new Uri(url, UriKind.Absolute);
                }
            }

            return null;
        }
    }
}