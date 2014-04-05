﻿using System.Windows.Forms;

namespace VkNet.Model
{
    using System;

    using Enums;
    using Utils;

    /// <summary>
    /// Аудиозапись пользователя или группы.
    /// См. описание <see href="http://vk.com/dev/audio_object"/>.
    /// </summary>
    public class Audio
    {
        /// <summary>
        /// Идентификатор аудиозаписи.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Идентификатор владельца аудиозаписи (пользователя или группы).
        /// Для сообщества (группы) идентификатор отрицательный.
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        /// Исполнитель аудиозаписи.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Название композиции.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Длительность аудиозаписи в секундах.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Ссылка на аудиозапись (привязана к ip-адресу клиентского приложения).
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Идентификатор текста аудиозаписи (если доступно).
        /// </summary>
        public long? LyricsId { get; set; }

        /// <summary>
        /// Идентификатор альбома аудиозаписи (если присвоен).
        /// </summary>
        public long? AlbumId { get; set; }

        /// <summary>
        /// Жанр аудиозаписи.
        /// </summary>
        public AudioGenre? Genre { get; set; }

        #region Методы

        internal static Audio FromJson(VkResponse response)
        {
            var audio = new Audio();

            VkResponse id = response["id"] ?? response["aid"];

            audio.Id = Convert.ToInt64(id.ToString());
            audio.OwnerId = response["owner_id"];
            audio.Artist = response["artist"];
            audio.Title = response["title"];
            audio.Duration = response["duration"];
            audio.Url = response["url"];
            audio.LyricsId = Utilities.GetNullableLongId(response["lyrics_id"]);
            audio.AlbumId = Utilities.GetNullableLongId(response["album"]);
            audio.Genre = response["genre_id"] ?? response["genre"];

            return audio;
        }

        #endregion
    }
}