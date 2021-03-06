﻿using System;
using System.ComponentModel;
using System.Linq;
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

            var settings = SettingsVault.Read();

            Instance.AccessToken = settings.Token;
            Instance.UserId = settings.UserId;

            if(!string.IsNullOrEmpty(Instance.AccessToken))
            {
                Instance.Authorize(Instance.AccessToken);
            }
        }

        private Vk()
        {
        }


        public WebClient Client => _client ?? (_client = new WebClient());


        public static Vk Instance { get; }

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
                OnPropertyChanged();
            }
        }

        public long UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged();
            }
        }

        public bool LoggedIn
        {
            get { return _loggedIn; }
            set
            {
                _loggedIn = value;
                OnPropertyChanged();
            }
        }

        public AuthorizationResults Authorize(string login, string password, bool readOnly)
        {
            try
            {
                //We set flag Offline, so token will newer expired
                Api.Authorize(new ApiAuthParams
                {
                    ApplicationId = AppId,
                    Login = login,
                    Password = password,
                    Settings = VkNet.Enums.Filters.Settings.Audio | VkNet.Enums.Filters.Settings.Offline
                });

                AccessToken = (string)ReflectionHelper.GetPropertyValue(Api, "AccessToken");
                UserId = Api.UserId ?? 0;

                var settings = SettingsVault.Read();
                settings.Token = AccessToken;
                settings.UserId = UserId;
                settings.ReadOnly = readOnly;
                SettingsVault.Write(settings);

                LoggedIn = true;

                return new AuthorizationResults {Success = true};
            }
            catch(Exception e)
            {
                return new AuthorizationResults {Success = false, Message = e.Message};
            }
        }

        public AuthorizationResults Authorize(string token)
        {
            if(string.IsNullOrEmpty(AccessToken))
            {
                return new AuthorizationResults {Success = false, Message = "Access Token is missing"};
            }

            try
            {
                Api.Authorize(AccessToken);

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                if(Api.IsAuthorized)
                {
                    LoggedIn = true;
                    return new AuthorizationResults {Success = true};
                }

                return new AuthorizationResults { Success = false };
            }
            catch(Exception e)
            {
                return new AuthorizationResults {Success = false, Message = e.Message};
            }
        }

        public void Logout()
        {
            Api = null;
            AccessToken = null;
            LoggedIn = false;

            var settings = SettingsVault.Read();
            settings.Token = AccessToken;
            SettingsVault.Write(settings);
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }


    public struct AuthorizationResults
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}