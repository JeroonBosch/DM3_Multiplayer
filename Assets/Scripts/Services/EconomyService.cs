using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class EconomyService : RestServiceBase
    {
        private const string URL_SHOP = "/api/shop";
        private const string URL_STAGES = "/api/stages";

        public void LoadShop(Delegates.ServiceCallback<ShopRequestObject> loadCallback)
        {
            var parameters = new Dictionary<string, object>();

            Delegates.ServiceCallback<ShopRequestObject> requestCallback = (success, message, result) =>
            {
                if (loadCallback != null)
                {
                    loadCallback(success, message, result);
                }
            };
            AsyncServerRequest(URL_SHOP, parameters, requestCallback, 3, true);
        }
        public void LoadStages(Delegates.ServiceCallback<StagesRequestObject> loadCallback)
        {
            var parameters = new Dictionary<string, object>();

            Delegates.ServiceCallback<StagesRequestObject> requestCallback = (success, message, result) =>
            {
                if (loadCallback != null)
                {
                    loadCallback(success, message, result);
                }
            };
            AsyncServerRequest(URL_STAGES, parameters, requestCallback, 3, true);
        }

        // SHOP
        [Serializable]
        public class ShopRequestObject
        {
            public List<Skill> skills;
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
        public class Level
        {
            public string id;
            public int level;
            public string description;
            public string xp;
            public string skills;
            public string coins;
        }

        // STAGES
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
        public class StagesRequestObject
        {
            public List<Stage> stages;
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