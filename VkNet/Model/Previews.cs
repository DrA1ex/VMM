namespace VkNet.Model
{
    using VkNet.Utils;

    /// <summary>
    /// ����� Url � ��������� � ��������� �����������.
    /// ������������ � <see cref="User"/>, <see cref="Group"/> � <see cref="Message"/>.
    /// </summary>
    public class Previews
    {
        /// <summary>
        /// Url ���������� ����������, ������� ������ 50 ��������.
        /// </summary>
        public string Photo50 { get; set; }

        /// <summary>
        /// Url ���������� ����������, ������� ������ 100 ��������.
        /// </summary>
        public string Photo100 { get; set; }

        /// <summary>
        /// Url ���������� ����������, ������� ������ 200 ��������.
        /// </summary>
        public string Photo200 { get; set; }

        /// <summary>
        /// Url ���������� ����������, ������� ������ 400 ��������.
        /// </summary>
        public string Photo400 { get; set; }

        /// <summary>
        /// Url ���������� ����������, ������� ������������ ������.
        /// </summary>
        public string PhotoMax { get; set; }

        #region ������ 

        internal static Previews FromJson(VkResponse response)
        {
            var previews = new Previews();

            previews.Photo50 = response["photo_50"] ?? response["photo"];
            previews.Photo100 = response["photo_100"] ?? response["photo_medium"];
            previews.Photo200 = response["photo_200"] ?? response["photo_200_orig"];
            previews.Photo400 = response["photo_400_orig"];
            previews.PhotoMax = response["photo_max"] ?? response["photo_max_orig"] ?? response["photo_big"] ?? previews.Photo400 ?? previews.Photo200 ?? previews.Photo100 ?? previews.Photo50;

            return previews;
        }

        #endregion
    }
}