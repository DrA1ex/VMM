﻿namespace VkNet.Enums
{
    using Utils;

    /// <summary>
    /// Фильтры сообществ пользователя.
    /// </summary>
    public sealed class GroupsFilters : VkFilter
    {
        /// <summary>
        /// Вернуть все сообщества, в которых пользователь является администратором.
        /// </summary>
        public static readonly GroupsFilters Administrator = new GroupsFilters(1 << 0, "admin");

        /// <summary>
        /// Вернуть все сообщества, в которых пользователь является администратором или редактором.
        /// </summary>
        public static readonly GroupsFilters Editor = new GroupsFilters(1 << 1, "editor");

        /// <summary>
        /// Вернуть все сообщества, в которых пользователь является администратором, редактором или модератором.
        /// </summary>
        public static readonly GroupsFilters Moderator = new GroupsFilters(1 << 2, "moder");

        /// <summary>
        /// Вернуть все группы, в которые входит пользователь.
        /// </summary>
        public static readonly GroupsFilters Groups = new GroupsFilters(1 << 3, "groups");

        /// <summary>
        /// Вернуть все публичные страницы пользователя ???
        /// </summary>
        public static readonly GroupsFilters Publics = new GroupsFilters(1 << 4, "publics");

        /// <summary>
        /// Вернуть все события, в которых участвует пользователь.
        /// </summary>
        public static readonly GroupsFilters Events = new GroupsFilters(1 << 5, "events");

        /// <summary>
        /// Вернуть все сообщества, в которых задействован пользователь.
        /// </summary>
        public static readonly GroupsFilters All = Administrator | Editor | Moderator | Groups | Publics | Events;

        private GroupsFilters(int value, string name)
            : base(value, name)
        {

        }

        private GroupsFilters(GroupsFilters left, GroupsFilters right) : base(left, right)
        {
            
        }

        /// <summary>
        /// Оператор объединения фильтров сообществ пользователя.
        /// </summary>
        /// <param name="left">Левое поле выражения объединения.</param>
        /// <param name="right">Правое поле выражения объединения.</param>
        /// <returns>Результат объединения.</returns>
        public static GroupsFilters operator |(GroupsFilters left, GroupsFilters right)
        {
            return new GroupsFilters(left, right);
        }
    }
}