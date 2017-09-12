using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using Facebook.MiniJSON;

namespace Com.Hypester.DM3
{
    //Not currently used.
    //Meant to contain general info (see Player Data) later, when login systems are in place.

    public class MainController : MonoBehaviour
    {
        public BaseMenuCanvas currentScreen;
        public PlayerData playerData;

        private bool _gotFacebookData;

        private static MainController instance;
        public static MainController Instance
        {
            get { return instance ?? (instance = new GameObject("MainController").AddComponent<MainController>()); }
        }


        private void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
            BaseMenuCanvas firstScreen = GameObject.Find("LoginScreen").GetComponent<BaseMenuCanvas>();
            firstScreen.GoToScreen(firstScreen);
            currentScreen = firstScreen;
            playerData = new PlayerData();

            _gotFacebookData = false;

            FB.Init();
        }

        private void Update()
        {
            if (FB.IsLoggedIn && !_gotFacebookData)
            {
                FB.API("me?fields=name", HttpMethod.GET, GetFacebookName);
                //FB.API("/me/picture?redirect=false", HttpMethod.GET, ProfilePhotoCallback);
            }
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void GetPlayerData ()
        {
            //Retrieve data from DB.
        }

        public void CallFBLogin()
        {
            FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" });

            
        }

        public void CallFBLogout()
        {
            FB.LogOut();
        }

        void GetFacebookName(IGraphResult result)
        {
            _gotFacebookData = true;

            string fbName = result.ResultDictionary["name"].ToString();

            Debug.Log("fbName: " + fbName);

            playerData.profileName = fbName;
            PhotonConnect.Instance.profileName = fbName;
        }

        /*private void ProfilePhotoCallback(IGraphResult result)
        {
            if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
            {
                IDictionary data = result.ResultDictionary["data"] as IDictionary;
                string photoURL = data["url"] as String;

                StartCoroutine(fetchProfilePic(photoURL));
            }
        }

        private IEnumerator fetchProfilePic(string url)
        {
            WWW www = new WWW(url);
            yield return www;
            this.profilePic = www.texture;
        }*/
    }


    public class PlayerData
    {
        public int profileID;
        public string profileName;
        public string pictureURL;

        public int coins;
        public int trophies;
        public int xp;
        public int unspentSkill;
        public int blueSkill;
        public int greenSkill;
        public int redSkill;
        public int yellowSkill;
    }
}