﻿namespace VkNet
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Newtonsoft.Json.Linq;

    using Categories;
    using Enums;
    using Exception;
    using Utils;

    /// <summary>
    /// API для работы с ВКонтакте. Выступает в качестве фабрики для различных категорий API (например, для работы с пользователями, 
    /// группами и т.п.).
    /// </summary>
    public class VkApi
    {
        /// <summary>
        /// Используемая версия API ВКонтакте.
        /// </summary>
        public string ApiVersion { get; internal set; }

        internal const string InvalidAuthorization = "Invalid authorization";

        #region Categories Definition

        /// <summary>
        /// API для работы с пользователями.
        /// </summary>
        public UsersCategory Users { get; private set; }
        /// <summary>
        /// API для работы с друзьями.
        /// </summary>
        public FriendsCategory Friends { get; private set; }
        /// <summary>
        /// API для работы со статусом пользователя или сообщества.
        /// </summary>
        public StatusCategory Status { get; private set; }
        /// <summary>
        /// API для работы с сообщениями.
        /// </summary>
        public MessagesCategory Messages { get; private set; }
        /// <summary>
        /// API для работы с .
        /// </summary>
        public GroupsCategory Groups { get; private set; }
        /// <summary>
        /// API для работы с аудиозаписями.
        /// </summary>
        public AudioCategory Audio { get; private set; }
        /// <summary>
        /// API для получения справочной информации (страны, города, школы, учебные заведения и т.п.).
        /// </summary>
        public DatabaseCategory Database { get; private set; }
        /// <summary>
        /// API для работы со служебными методами.
        /// </summary>
        public UtilsCategory Utils { get; private set; }
        /// <summary>
        /// API для работы со стеной пользователя.
        /// </summary>
        public WallCategory Wall { get; private set; }
        /// <summary>
        /// API для работы с закладками.
        /// </summary>
        public FaveCategory Fave { get; private set; }
        /// <summary>
        /// API для работы с видеофайлами.
        /// </summary>
        public VideoCategory Video { get; private set; }

        #endregion

        internal IBrowser Browser { get; set; }

        internal string AccessToken { get; set; }

        /// <summary>
        /// Идентификатор пользователя, от имени которого была проведена авторизация.
        /// Если авторизация не была произведена с использованием метода <see cref="Authorize(int,string,string,VkNet.Enums.Settings)"/>, 
        /// то возвращается null.
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="VkApi"/>.
        /// </summary>
        public VkApi()
        {
            Browser = new Browser();

            Users = new UsersCategory(this);
            Friends = new FriendsCategory(this);
            Status = new StatusCategory(this);
            Messages = new MessagesCategory(this);
            Groups = new GroupsCategory(this);
            Audio = new AudioCategory(this);
            Wall = new WallCategory(this);
            Database = new DatabaseCategory(this);
            Utils = new UtilsCategory(this);
            Fave = new FaveCategory(this);
            Video = new VideoCategory(this);

            ApiVersion = "5.17";
        }

        /// <summary>
        /// Authorize application on vk.com and getting Access Token.
        /// </summary>
        /// <param name="appId">Appliation Id</param>
        /// <param name="email">Email or Phone</param>
        /// <param name="password">Password</param>
        /// <param name="settings">Access rights requested by your application</param>
        public void Authorize(int appId, string email, string password, Settings settings)
        {
            var authorization = Browser.Authorize(appId, email, password, settings);
            if (!authorization.IsAuthorized)
                throw new VkApiAuthorizationException(InvalidAuthorization, email, password);

            AccessToken = authorization.AccessToken;
            UserId = authorization.UserId;
        }

        /// <summary>
        /// Выполняет авторизацию с помощью маркера доступа (access token), полученного извне.
        /// </summary>
        /// <param name="accessToken">Маркер доступа, полученный извне.</param>
        /// <param name="userId">Идентификатор пользователя, установившего приложение (необязательный параметр).</param>
        public void Authorize(string accessToken, long? userId = null)
        {
            AccessToken = accessToken;
            UserId = userId;
        }

        #region Private & Internal Methods

        internal VkResponse Call(string methodName, VkParameters parameters, bool skipAuthorization = false)
        {
            if (!skipAuthorization)
                IfNotAuthorizedThrowException();

            string url = GetApiUrl(methodName, parameters);

            string answer = Browser.GetJson(url);

#if DEBUG
            Trace.WriteLine(Utilities.PreetyPrintApiUrl(url));
            Trace.WriteLine(Utilities.PreetyPrintJson(answer));
#endif
            VkErrors.IfErrorThrowException(answer);

            JObject json = JObject.Parse(answer);

            var rawResponse = json["response"];

            return new VkResponse(rawResponse) { RawJson = answer };
        }

        internal string GetApiUrl(string methodName, IDictionary<string, string> values)
        {
            var builder = new StringBuilder();

            builder.AppendFormat("{0}{1}?", "https://api.vk.com/method/", methodName);

            foreach (var pair in values)
                builder.AppendFormat("{0}={1}&", pair.Key, pair.Value);

            builder.AppendFormat("access_token={0}", AccessToken);

            return builder.ToString();
        }

        internal void IfNotAuthorizedThrowException()
        {
            if (string.IsNullOrEmpty(AccessToken))
                throw new AccessTokenInvalidException();
        }

        #endregion
    }
}