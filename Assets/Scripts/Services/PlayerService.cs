using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class PlayerService : RestServiceBase
    {
        private const string URL_LOGIN = "/api/login";
        private const string URL_PING = "/api/ping";

        public void Login(string fbToken, string mobile_id, Delegates.ServiceCallback<LoginRequestObject> loadCallback)
        {
            var parameters = new Dictionary<string, object>();
			if (!string.IsNullOrEmpty(fbToken)) {
				parameters.Add("token", fbToken);
			}
            parameters.Add("mobile_id", mobile_id);

            Delegates.ServiceCallback<LoginRequestObject> requestCallback = (success, message, result) =>
            {
                if (result != null && !string.IsNullOrEmpty(result.hexaclash))
                {
                    MainController.settingsService.hexaClash = result.hexaclash;
                }
                if (loadCallback != null)
                {
                    loadCallback(success, message, result);
                }
            };
            AsyncServerRequest(URL_LOGIN, parameters, requestCallback, 3, true);
        }
		public void Logout() {
			NetworkService.DestroySession ();
		}
        public void Ping(Delegates.ServiceCallback<PingRequestObject> pingCallback)
        {
            AsyncServerRequest<PingRequestObject>(URL_PING, null, (success, message, result) =>
            {
                if (pingCallback != null)
                {
                    pingCallback(success, message, result);
                }
            }, 7, true);
        }

        // LOGIN
        [Serializable]
        public class User
        {
            public string id;
            public string first_name;
            public string last_name;
            public object country_id;
            public string last_login;
            public string active;
            public string add_time;
            public string country;
            public string pic;
        }

        [Serializable]
        public class Fakeuser
        {
            public string id;
            public string first_name;
            public string last_name;
            public string xplevel;
            public string pic;
        }

        [Serializable]
        public class Settings
        {
            public string hourBonus;
            public string fullHealth;
            public string friendAskCoins;
            public string friendSendCoins;
        }

        [Serializable]
        public class Stage
        {
            public string id;
            public string name;
            public string syscode;
            public string buyin;
            public string reward;
            public string xp;
            public string tourna_buyin;
            public string tourna_reward;
            public string tourna_xp;
        }

        [Serializable]
        public class LoginRequestObject
        {
            public User user;
            public List<Fakeuser> fakeusers;
            public Settings settings;
            public List<object> messages;
            public List<Stage> stages;
            public string mobile_id;
            public string hexaclash;
            public int hourBonus;
            public int experience;
            public int XPlevel;
            public int XPlevelGain;
            public int XPlevelGainCurrent;
            public int skillPoints;
            public int coins;
            public string user_id;
            public int wheelEnabled;
            public string chk;
            public string sid;
        }

        // PING
        [Serializable]
        public class PingRequestObject
        {
            public int pong;
            public int hourBonus;
            public int experience;
            public int XPlevel;
            public int XPlevelGain;
            public int XPlevelGainCurrent;
            public int skillPoints;
            public int coins;
            public string user_id;
            public int wheelEnabled;
            public string chk;
        }
    }
}