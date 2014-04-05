﻿namespace VkNet.Model
{
    using Categories;
    using Utils;

    /// <summary>
    /// Информация о месте, в котором была сделана запись.
    /// См. описание <see href="http://vk.com/pages?oid=-1&amp;p=Описание_поля_geo"/> и <see href="http://vk.com/dev/fields_groups"/>. Раздел place.
    /// </summary>
    public class Place
    {
        /// <summary>
        /// Идентификатор места.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Название места.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Географическая широта, заданная в градусах (от -90 до 90).
        /// </summary>
        public int? Latitude { get; set; }

        /// <summary>
        /// Географическая долгота, заданная в градусах (от -90 до 90).
        /// </summary>
        public int? Longitude { get; set; }

        /// <summary>
        /// Идентификатор типа места, информацию о котором можно получить с помощью метода <see cref="DatabaseCategory.GetPlaceTypes"/> (пока не реализовано).
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// Идентификатор страны, название которой можно получить с помощью метода <see cref="DatabaseCategory.GetCountriesById"/>. 
        /// </summary>
        public long? CountryId { get; set; }

        /// <summary>
        /// Идентификатор города, название которого можно получить с помощью метода <see cref="DatabaseCategory.GetCitiesById"/>.
        /// </summary>
        public long? CityId { get; set; }

        /// <summary>
        /// Строка с указанием адреса места в городе. 
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Данный параметр указывается, если местоположение является прикреплённой картой. 
        /// </summary>
        public bool ShowMap { get; set; }

        #region Поля, установленные экспериментально

        /// <summary>
        /// Страна, в которой находится место.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Город, в котором находится место.
        /// </summary>
        public string City { get; set; }

        #endregion

        #region Методы

        internal static Place FromJson(VkResponse response)
        {
            var place = new Place();

            place.Id = response["place_id"];
            place.Title = response["title"];
            place.Latitude = response["latitude"];
            place.Longitude = response["longitude"];
            place.TypeId = response["type"];
            place.CountryId = response["country_id"];
            place.CityId = response["city_id"];
            place.Address = response["address"];
            place.ShowMap = response["showmap"];

            place.Country = response["country"]; // установлено экcпериментальным путем
            place.City = response["city"]; // установлено экcпериментальным путем

            return place;
        }

        #endregion
    }
}