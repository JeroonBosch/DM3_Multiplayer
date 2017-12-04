using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Facebook.Unity;

namespace Com.Hypester.DM3
{
    public class PlayerService : RestServiceBase
    {
        private const string URL_LOGIN = "/api/login";
        private const string URL_PING = "/api/ping";

        public void Login(string fbToken, string mobile_id, Delegates.ServiceCallback<LoginRequestObject> loadCallback)
        {
            var parameters = new Dictionary<string, object>();
			if (string.IsNullOrEmpty(fbToken)) {
                parameters.Add("mobile_id", (string.IsNullOrEmpty(mobile_id) ? "new" : mobile_id));
			}
            else
            {
                parameters.Add("token", fbToken);
                if (!string.IsNullOrEmpty(mobile_id) && mobile_id != "new")
                {
                    parameters.Add("mobile_id", mobile_id);
                }
            }
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
            /*
            if (MainController.settingsService.lastLoginType == LoginType.FACEBOOK) // For logout from the Facebook session we don't remove the session data, a player continues a game with the mobile_id.
            {
                FB.LogOut();
            }
            NetworkService.DestroySession ();
            */
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
            public string country_id;
            public bool country;
            public string pic;
            public string frame_id;
        }

        [Serializable]
        public class Fakeuser
        {
            public string id;
            public string first_name;
            public string last_name;
            public string xplevel;
            public string pic;
            public string frame_id;
        }

        [Serializable]
        public class Settings
        {
            public string hourBonus;
            public string fullHealth;
            public string friendAskCoins;
            public string friendSendCoins;
            public string booster_01;
            public string booster_02;
            public string booster_03;
            public string initial_coins;
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
        public class Stat
        {
            public int games;
            public int wins;
            public int losses;
            public int tournament_wins;
            public int tournament_games;
            public int coins_won;
            public int week_ranking;
        }

        [Serializable]
        public class Level
        {
            public string id;
            public int level;
            public string description;
            public string xp;
            public string skills;
            public string coins;
        }

        [Serializable]
        public class Skill
        {
            public string name;
            public string syscode;
            public int level;
            public int maxlevel;
            public object description;
            public List<int> currLevel;
            public List<Level> levels;
        }

        [Serializable]
        public class LoginRequestObject
        {
            public int verified;
            public string mobile_id;
            public User user;
            public List<Fakeuser> fakeusers;
            public Settings settings;
            public List<object> messages;
            public List<Stage> stages;
            public string hexaclash;
            public Stat stat;
            public List<Skill> skills;
            public string sid;
            public int hourBonus;
            public bool newFrameUnlocked;
            public int experience;
            public int XPlevel;
            public int XPlevelGain;
            public int XPlevelGainCurrent;
            public int skillPoints;
            public int coins;
            public string user_id;
            public int wheelEnabled;
            public string SESSION_ID_IS_THIS;
            public string chk;
            public string error;
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