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
        private const string URL_BUYCOINS = "/api/tmpBuy";

        public void LoadShop(Delegates.ServiceCallback<ShopRequestObject> loadCallback, string newLevelId = "")
        {
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(newLevelId))
            {
                parameters.Add("upgrade", newLevelId);
            }

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
        public void BuyCoinsTemp(int coinAmount, int skillAmount, Delegates.ServiceCallbackCustomInfo<BuyCoinsTempResponseObject> purchaseCallback)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("skills", skillAmount);
            parameters.Add("coins", coinAmount);

            var customInfo = new Dictionary<string, object>();
            customInfo.Add("purchase_amount", coinAmount);

            Delegates.ServiceCallback<BuyCoinsTempResponseObject> requestCallback = (success, message, result) =>
            {
                if (purchaseCallback != null)
                {
                    purchaseCallback(success, message, result, customInfo);
                }
            };

            AsyncServerRequest(URL_BUYCOINS, parameters, requestCallback, 3, true);
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
            public List<float> currLevel;
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

        [Serializable]
        public class BuyCoinsTempResponseObject
        {
            public int verified;
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
    }
}