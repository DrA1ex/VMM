﻿namespace VkNet.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Newtonsoft.Json.Linq;

    internal static class Utilities
    {
        public static DateTime FromUnixTime(long ticks)
        {
            var startUnixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            startUnixTime = startUnixTime.AddSeconds(ticks).ToLocalTime();
            return startUnixTime;
        }

        public static long? ToUnixTime(DateTime? time)
        {
            if (!time.HasValue) return null;

            double totalSeconds = (time.Value - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
            return System.Convert.ToInt64(totalSeconds);
        }

        public static T EnumFrom<T>(int value)
        {
            if (!Enum.IsDefined(typeof(T), value))
                throw new ArgumentException(string.Format("Enum value {0} not defined!", value), "value");

            return (T)(object)value;
        }

        public static T? NullableEnumFrom<T>(int value) where T : struct
        {
            if (!Enum.IsDefined(typeof(T), value))
                return null;

            return (T)(object)value;
        }

        /// <summary>
        /// Получение идентификатора.
        /// 
        /// Применять когда id может быть задано как строкой так и числом в json'e.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static long? GetNullableLongId(VkResponse response)
        {
            return response != null ? System.Convert.ToInt64(response.ToString()) : (long?)null;
        }

        public static string JoinNonEmpty<T>(this IEnumerable<T> collection, string separator = ",")
        {
            if (collection == null)
                return string.Empty;

            return string.Join(separator, collection.Select(i => i.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToArray());
        }

        public static IEnumerable<T> Convert<T>(this VkResponseArray response, Func<VkResponse, T> selector)
        {
            if (response == null)
                return Enumerable.Empty<T>();

            return response.Select(selector).ToList();
        }

        public static string PreetyPrintApiUrl(string url)
        {
            return string.Format("            const string url = \"{0}\";", url);
        }

        public static string PreetyPrintJson(string json)
        {
            // DELME: 
            var jObject = JObject.Parse(json);
            var preety = jObject.ToString();
            preety = preety.Replace('"', '\'');
            var result = new StringBuilder();

            result.AppendLine("            const string json =");
            result.Append("                @\"");
            using (var reader = new StringReader(preety))
            {
                bool isFirst = true;
                for (;;)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;

                    if (!isFirst)
                    {
                        result.AppendLine();
                        result.Append("                  ");
                    }

                    result.Append(line);

                    isFirst = false;
                }
            }
            result.Append("\";");

            return result.ToString();
        }
    }
}