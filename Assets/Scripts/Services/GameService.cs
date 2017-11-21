using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class GameService : RestServiceBase
    {
        private const string URL_GETGAME = "/api/getGame";
        private const string URL_STARTGAME = "/api/startGame";
        private const string URL_SETGAME = "/api/setGame";

        public void GetGame(string stage_id, string opponent_id, Delegates.ServiceCallback<GetGameRequestObject> loadCallback)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("stage_id", stage_id);
            parameters.Add("opponent_id", opponent_id);

            Delegates.ServiceCallback<GetGameRequestObject> requestCallback = (success, message, result) =>
            {
                if (loadCallback != null)
                {
                    loadCallback(success, message, result);
                }
            };
            AsyncServerRequest(URL_GETGAME, parameters, requestCallback, 3, true);
        }
        public void StartGame(int game_id, Delegates.ServiceCallback<StartGameRequestObject> loadCallback)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("game_id", game_id);

            Debug.Log("game_id: " + game_id);

            Delegates.ServiceCallback<StartGameRequestObject> requestCallback = (success, message, result) =>
            {
                if (loadCallback != null)
                {
                    loadCallback(success, message, result);
                }
            };
            AsyncServerRequest(URL_STARTGAME, parameters, requestCallback, 3, true);
        }
        public void SetGame(string game_id, string localPlayerHealth, string remotePlayerHealth, Delegates.ServiceCallback<SetGameRequestObject> loadCallback)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("game_id", game_id);
            parameters.Add("health", localPlayerHealth);
            parameters.Add("ohealth", remotePlayerHealth);

            Delegates.ServiceCallback<SetGameRequestObject> requestCallback = (success, message, result) =>
            {
                if (loadCallback != null)
                {
                    loadCallback(success, message, result);
                }
            };
            AsyncServerRequest(URL_SETGAME, parameters, requestCallback, 3, true);
        }

        [Serializable]
        public class Opponent
        {
            public string id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public int XPlevel { get; set; }
            public string pic { get; set; }
            public string frame_id { get; set; }
        }

        [Serializable]
        public class Stage
        {
            public string id { get; set; }
            public string name { get; set; }
            public string syscode { get; set; }
            public string buyin { get; set; }
            public string reward { get; set; }
            public string xp { get; set; }
        }

        [Serializable]
        public class Game
        {
            public int id { get; set; }
            public Opponent opponent { get; set; }
            public Stage stage { get; set; }
        }

        [Serializable]
        public class Skill
        {
            public string name { get; set; }
            public string syscode { get; set; }
            public int level { get; set; }
            public int maxlevel { get; set; }
            public object description { get; set; }
            public List<int> currLevel { get; set; }
        }

        [Serializable]
        public class GetGameRequestObject
        {
            public int verified { get; set; }
            public Game game { get; set; }
            public List<Skill> skills { get; set; }
            public int hourBonus { get; set; }
            public bool newFrameUnlocked { get; set; }
            public int experience { get; set; }
            public int XPlevel { get; set; }
            public int XPlevelGain { get; set; }
            public int XPlevelGainCurrent { get; set; }
            public int skillPoints { get; set; }
            public int coins { get; set; }
            public string user_id { get; set; }
            public int wheelEnabled { get; set; }
            public string chk { get; set; }
        }
        [Serializable]
        public class StartGameRequestObject
        {

        }
        [Serializable]
        public class SetGameRequestObject
        {

        }
    }
}