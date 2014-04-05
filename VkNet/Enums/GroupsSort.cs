﻿namespace VkNet.Enums
{
    /// <summary>
    /// Порядок сортировки членов группы.
    /// </summary>
    public sealed class GroupsSort
    {
        private readonly string _name;

        /// <summary>
        /// По возрастанию численных значений идентификаторов.
        /// </summary>
        public static readonly GroupsSort IdAsc = new GroupsSort("id_asc");

        /// <summary>
        /// По убыванию численных значений идентификаторов.
        /// </summary>
        public static readonly GroupsSort IdDesc = new GroupsSort("id_desc");

        /// <summary>
        /// По возрастанию времени присоединения к группе.
        /// </summary>
        public static readonly GroupsSort TimeAsc = new GroupsSort("time_asc");

        /// <summary>
        /// По убыванию времени присоединения к группе.
        /// </summary>
        public static readonly GroupsSort TimeDesc = new GroupsSort("time_desc");

        private GroupsSort(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Возвращает порядок сортировки членов группы в виде строки.
        /// </summary>
        /// <returns>
        /// Порядок сортировки членов группы в виде строки.
        /// </returns>
        public override string ToString()
        {
            return _name;
        }
    }
}