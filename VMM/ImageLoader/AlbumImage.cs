using System;
using System.Linq;
using System.Net;
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
        private const string RequestPattern = @"https://www.google.ru/search?q={0}&biw=1920&tbm=isch";
        private const string UrlPrefix = "imgurl=";
        private const string UrlPostfix = "&amp;imgrefurl=";


        public static readonly DependencyProperty SongProperty = DependencyProperty.RegisterAttached(
            "Song", typeof(MusicEntry), typeof(AlbumImage), new PropertyMetadata(OnSongChanged));

        private static MusicEntry CurrentEntry { get; set; }
        private static BitmapImage CachedImage { get; set; }

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

                CachedImage = CachedImage ?? new BitmapImage(await GetAlbumImage($"{entry.Artist} {entry.Name} album"));

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

        private static async Task<Uri> GetAlbumImage(string song)
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
                    var imageRef = doc.DocumentNode.SelectNodes("//a[@target='_blank']").FirstOrDefault()?.GetAttributeValue("href", null);

                    if(!string.IsNullOrWhiteSpace(imageRef))
                    {
                        var startPos = imageRef.IndexOf(UrlPrefix, StringComparison.Ordinal);
                        var endPos = imageRef.IndexOf(UrlPostfix, StringComparison.Ordinal);

                        if(startPos != -1 && endPos != -1)
                        {
                            var firstSymbolIndex = startPos + UrlPrefix.Length;
                            var length = endPos - firstSymbolIndex;
                            var url = imageRef.Substring(firstSymbolIndex, length);

                            return new Uri(url, UriKind.Absolute);
                        }
                    }
                }
            }
            catch(WebException)
            {
                //ignore
            }

            return null;
        }
    }
}