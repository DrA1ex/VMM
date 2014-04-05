﻿namespace VkNet.Model
{
    using System;

    using VkNet.Utils;

    /// <summary>
    /// Информация о последней активности пользователя.
    /// См. описание <see href="http://vk.com/dev/messages.getLastActivity"/>.
    /// </summary>
    public class LastActivity
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Текущий статус пользователя (true - в сети, false - не в сети).
        /// </summary>
        public bool? IsOnline { get; set; }

        /// <summary>
        /// Дата последней активности пользователя.
        /// </summary>
        public DateTime? Time { get; set; }

        #region Методы

        internal static LastActivity FromJson(VkResponse re)
        {
            var lastActivity = new LastActivity();

            lastActivity.IsOnline = re["online"];
            lastActivity.Time = Utilities.FromUnixTime(re["time"]);

            return lastActivity;
        }

        #endregion
    }
}