using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Com.Hypester.DM3
{
    public class ServiceCMS : RestServiceBase
    {

        [SerializeField] KeyCode PingRequestKey;
        [SerializeField] KeyCode StagesRequestKey;
        [SerializeField] KeyCode ShopRequestKey;

        private void Update()
        {
            KeyControls();
        }

        void KeyControls()
        {
            if (Input.GetKeyDown(PingRequestKey))
            {
                StartCoroutine(PingServer());
            }
            if (Input.GetKeyDown(StagesRequestKey))
            {
                StartCoroutine(GetStages());
            }
            if (Input.GetKeyDown(ShopRequestKey))
            {
                StartCoroutine(AccessShop());
            }
        }

        public IEnumerator Request<Response>(string url, Action<Response> callback)
        {
            string requestUrl = url;
            WWW request = new WWW(requestUrl);

            yield return request;

            if (!string.IsNullOrEmpty(request.error))
            {
                print("Error pinging: " + request.error);
                callback(default(Response));
            }
            else
            {
                Response response = JsonUtility.FromJson<Response>(request.text);
                callback(response);
            }
        }

        IEnumerator PingServer()
        {
            Debug.Log("Pinging");
            string requestUrl = "https://clash.hypester.com/api/ping";
            WWW request = new WWW(requestUrl);

            yield return request;

            if (!string.IsNullOrEmpty(request.error))
            {
                print("Error pinging: " + request.error);
            }
            else
            {
                Debug.Log("Ping successful");
                PingRequestObject pingRequestObject = JsonUtility.FromJson<PingRequestObject>(request.text);
                Debug.Log("Pong: " + pingRequestObject.pong + ", checksum: " + pingRequestObject.chk);
            }
        }

        IEnumerator GetStages()
        {
            Debug.Log("Receiving stages");
            string requestUrl = "https://clash.hypester.com/api/stages";
            WWW request = new WWW(requestUrl);

            yield return request;

            if (!string.IsNullOrEmpty(request.error))
            {
                print("Error receiving stages: " + request.error);
            }
            else
            {
                Debug.Log("Stages received");
                StagesRequestObject stagesRequestObject = JsonConvert.DeserializeObject<StagesRequestObject>(request.text);
                Debug.Log("Stages count: " + stagesRequestObject.stages.Count + ", checksum: " + stagesRequestObject.chk);
                foreach (Stage stage in stagesRequestObject.stages)
                {
                    Debug.Log(stage.name);
                }
            }
        }

        IEnumerator AccessShop(int skillLevelIdToUpdate = -1)
        {
            Debug.Log("Accessing shop...");
            string requestUrl = "https://clash.hypester.com/api/shop";
            if (skillLevelIdToUpdate > 0)
            {
                // TODO: add additional params
            }
            WWW request = new WWW(requestUrl);

            yield return request;

            if (!string.IsNullOrEmpty(request.error))
            {
                print("Error accessing shop: " + request.error);
            }
            else
            {
                Debug.Log("Shop access successful");
                ShopRequestObject shopRequestObject = JsonConvert.DeserializeObject<ShopRequestObject>(request.text);
                Debug.Log("Skill count: " + shopRequestObject.skills.Count);
                foreach (Skill skill in shopRequestObject.skills)
                {
                    Debug.Log(skill.syscode + " level count: " + skill.levels.Count);
                }
            }
        }

        // Ping object
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

        // Stage objects
        [Serializable]
        public class StagesRequestObject
        {
            public List<Stage> stages = new List<Stage>();
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

        // Shop objects
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
    }
}