using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using Facebook.MiniJSON;
using System.Collections;

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

        public void FacebookInit() { FB.Init(); }


        private void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
            BaseMenuCanvas firstScreen = GameObject.Find("LoginScreen").GetComponent<BaseMenuCanvas>();
            firstScreen.GoToScreen(firstScreen);
            currentScreen = firstScreen;
            playerData = new PlayerData();

            _gotFacebookData = false;
        }

        private void Update()
        {
            if (FB.IsLoggedIn && !_gotFacebookData)
            {
                _gotFacebookData = true;
                FB.API("me?fields=name", HttpMethod.GET, GetFacebookName);
                //FB.API("me?fields=picture", HttpMethod.GET, GetFacebookPicture); 
                FB.API("me/picture?type=square&height=128&width=128", HttpMethod.GET, GetFacebookPicture);
                FB.API("me?fields=picture.type(large)", HttpMethod.GET, GetFacebookPictureURL);
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

        private void GetFacebookName(IGraphResult result)
        {
            string fbName = result.ResultDictionary["name"].ToString();

            Debug.Log("fbName: " + fbName);

            playerData.profileName = fbName;
        }

        private void GetFacebookPicture(IGraphResult result)
        {
            if (result.Texture != null)
            {
                GameObject.Find("ProfileAvatar").GetComponent<Image>().sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2());
                playerData.profilePicture = result.Texture;
            }
        }

        private void GetFacebookPictureURL(IGraphResult result)
        {
            IDictionary<string, object> innerDict = (Dictionary<string, object>)result.ResultDictionary["picture"];
            IDictionary<string, object> dataDict = (Dictionary<string, object>)innerDict["data"];

            playerData.pictureURL = dataDict["url"].ToString();
            Debug.Log("pic : " + dataDict["url"].ToString());
        }
    }


    public class PlayerData
    {
        public int profileID;
        public string profileName;
        public string pictureURL = "";
        public Texture2D profilePicture;

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