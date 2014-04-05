﻿namespace VkNet.Model
{
    using VkNet.Utils;

    /// <summary>
    /// Информация о высшем учебном заведении пользователя.
    /// См. описание <see href="http://vk.com/dev/fields"/>. Раздел education.
    /// </summary>
    public class Education
    {
        /// <summary>
        /// Идентификатор университета.
        /// </summary>
        public long? UniversityId { get; set; }

        /// <summary>
        /// Название ВУЗа.
        /// </summary>
        public string UniversityName { get; set; }

        /// <summary>
        /// Идентификатор факультета.
        /// </summary>
        public long? FacultyId { get; set; }

        /// <summary>
        /// Название факультета.
        /// </summary>
        public string FacultyName { get; set; }

        /// <summary>
        /// Год окончания.
        /// </summary>
        public int? Graduation { get; set; }

        #region Поля, установленные экспериментально

        /// <summary>
        /// Форма обучения.
        /// </summary>
        public string EducationForm { get; set; }

        /// <summary>
        /// Текущий статус пользователя в высшем учебном заведении.
        /// </summary>
        public string EducationStatus { get; set; }

        #endregion

        #region Методы

        internal static Education FromJson(VkResponse response)
        {   
            if (response["university"] == null || response["university"].ToString() == "0")
                return null;

            var education = new Education();

            education.UniversityId = Utilities.GetNullableLongId(response["university"]);
            education.UniversityName = response["university_name"];
            education.FacultyId = Utilities.GetNullableLongId(response["faculty"]);
            education.FacultyName = response["faculty_name"];
            education.Graduation = (int?) Utilities.GetNullableLongId(response["graduation"]);

            if (education.UniversityId.HasValue && education.UniversityId == 0)
                education.UniversityId = null;

            if (education.FacultyId.HasValue && education.FacultyId == 0)
                education.FacultyId = null;

            if (education.Graduation.HasValue && education.Graduation == 0)
                education.Graduation = null;

            education.EducationForm = response["education_form"]; // установлено экcпериментальным путем
            education.EducationStatus = response["education_status"]; // установлено экcпериментальным путем

            return education;
        }

        #endregion
    }
}