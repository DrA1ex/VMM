using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using VkNet;
using VMM.Annotations;
using VMM.Helper;

namespace VMM.Model
{
    public class Vk : INotifyPropertyChanged
    {
        private const int AppId = 4286357;
        private string _accessToken;
        private VkApi _api;
        private WebClient _client;
        private bool _loggedIn;
        private long _userId;

        static Vk()
        {
            Instance = new Vk();

            Settings settings = SettingsVault.Read();

            Instance.AccessToken = settings.Token;
            Instance.UserId = settings.UserId;

            if (!String.IsNullOrEmpty(Instance.AccessToken))
            {
                Instance.Authorize(Instance.AccessToken);
            }
        }

        private Vk()
        {
        }



        public WebClient Client
        {
            get { return _client ?? (_client = new WebClient()); }
        }


        public static Vk Instance { get; private set; }

        public VkApi Api
        {
            get { return _api ?? (_api = new VkApi()); }

            private set { _api = value; }
        }

        public string AccessToken
        {
            get { return _accessToken; }
            set
            {
                _accessToken = value;
                OnPropertyChanged("AccessToken");
            }
        }

        public long UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged("UserId");
            }
        }

        public bool LoggedIn
        {
            get { return _loggedIn; }
            set
            {
                _loggedIn = value;
                OnPropertyChanged("LoggedIn");
            }
        }

        public AuthorizationResults Authorize(string login, string password)
        {
            try
            {
                Api.Authorize(AppId, login, password, VkNet.Enums.Settings.Audio);

                AccessToken = (string)ReflectionHelper.GetPropertyValue(Api, "AccessToken");
                UserId = Api.UserId ?? 0;

                Settings settings = SettingsVault.Read();
                settings.Token = AccessToken;
                settings.UserId = UserId;
                SettingsVault.Write(settings);

                LoggedIn = true;

                return new AuthorizationResults { Success = true };
            }
            catch (Exception e)
            {
                return new AuthorizationResults { Success = false, Message = e.Message };
            }
        }

        public AuthorizationResults Authorize(string token)
        {
            if (String.IsNullOrEmpty(AccessToken))
            {
                return new AuthorizationResults { Success = false, Message = "Access Token is missing" };
            }

            try
            {
                Api.Authorize(AccessToken);
                Api.Users.IsAppUser(UserId); //Dummy call, throw exception if authorization failed

                LoggedIn = true;

                return new AuthorizationResults { Success = true };
            }
            catch (Exception e)
            {
                return new AuthorizationResults { Success = false, Message = e.Message };
            }
        }

        public void Logout()
        {
            Api = null;
            AccessToken = null;
            LoggedIn = false;

            Settings settings = SettingsVault.Read();
            settings.Token = AccessToken;
            SettingsVault.Write(settings);
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }


    public struct AuthorizationResults
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}