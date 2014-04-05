namespace VkNet.Model
{
    using System;
    using System.Diagnostics;

    using VkNet.Utils;

    /// <summary>
    /// ������� � �����������.
    /// ��. �������� <see href="http://vk.com/dev/tag.getTags"/>.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, Name = {Name}")]
    public class Tag
    {
        /// <summary>
        /// ������������� �������.
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// �������� �������.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ������������� ������������, �������� ������������� �������.
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// ������������� ������������, ���������� �������.
        /// </summary>
        public long? PlacerId { get; set; }

        /// <summary>
        /// ���� ���������� �������.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// ������ �������: true - ��������������, false - �� ��������������.
        /// </summary>
        public bool? IsViewed { get; set; }

        #region ������

        internal static Tag FromJson(VkResponse tag)
        {
            var result = new Tag();

            result.Id = tag["tag_id"];
            result.Name = tag["tagged_name"];
            result.UserId = tag["uid"];
            result.PlacerId = tag["placer_id"];
            result.Date = tag["tag_created"] ?? tag["date"];
            result.IsViewed = tag["viewed"];

            return result;
        }

        #endregion
    }
}