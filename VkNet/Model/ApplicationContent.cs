namespace VkNet.Model
{
    using VkNet.Utils;

    /// <summary>
    /// ������� ����������.
    /// ��. �������� <see href="http://vk.com/dev/attachments_w"/>. ������ "������� ����������".
    /// </summary>
    public class ApplicationContent
    {
        /// <summary>
        /// ������������� ����������, ������������� ������ �� �����.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// �������� ����������.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ����� ����������� ��� �������������.
        /// </summary>
        public string Photo130 { get; set; }

        /// <summary>
        /// ����� ��������������� �����������. 
        /// </summary>
        public string Photo604 { get; set; }

        #region ������

        internal static ApplicationContent FromJson(VkResponse response)
        {
            var application = new ApplicationContent();

            application.Id = response["id"];
            application.Name = response["name"];
            application.Photo130 = response["photo_130"];
            application.Photo604 = response["photo_604"];

            return application;
        }

        #endregion
    }
}