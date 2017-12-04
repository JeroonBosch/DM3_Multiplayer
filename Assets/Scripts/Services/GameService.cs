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
            public string id;
            public string first_name;
            public string last_name;
            public int XPlevel;
            public string pic;
            public string frame_id;
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
        }

        [Serializable]
        public class Game
        {
            public int id;
            public Opponent opponent;
            public Stage stage;
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
        }

        [Serializable]
        public class GetGameRequestObject
        {
            public int verified;
            public Game game;
            public List<Skill> skills;
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