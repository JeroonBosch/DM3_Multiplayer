using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Com.Hypester.DM3
{
    public class MainController : MonoBehaviour
    {
        public BaseMenuCanvas currentScreen;
        public PlayerData playerData;

        private static MainController instance;
        public static MainController Instance
        {
            get { return instance ?? (instance = new GameObject("MainController").AddComponent<MainController>()); }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            BaseMenuCanvas firstScreen = GameObject.Find("LoginScreen").GetComponent<BaseMenuCanvas>();
            firstScreen.GoToScreen(firstScreen);
            currentScreen = firstScreen;
            playerData = new PlayerData();
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void StartMatch ()
        {
            BaseMenuCanvas firstScreen = GameObject.Find("LoadingScreen").GetComponent<BaseMenuCanvas>();
            firstScreen.GoToScreen(firstScreen);
            currentScreen = firstScreen;
        }

        public void GetPlayerData ()
        {

        }
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