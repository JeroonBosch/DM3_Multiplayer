using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using Facebook.MiniJSON;
using System.Collections;

namespace Com.Hypester.DM3
{
    //Could be renamed to something like DataController.
    //This singleton carries over between scenes and is meant to have database data, facebook data and such. 
    //Profile data (coins, xp) should be retrieved from this singleton.

    public class MainController : MonoBehaviour
    {
        public BaseMenuCanvas currentScreen;
        public PlayerData playerData;

        private bool _gotFacebookData;

        //Connections
        public bool wantsFBConnection = false;
        public bool wantsDBConnection = false; //TODO

        public bool facebookConnected;

        // Services
        public static SettingsService settingsService { get; private set; }

        private static AssetService serviceAsset;
        public static AssetService ServiceAsset
        {
            get
            {
                if (serviceAsset == null)
                {
                    serviceAsset = ((GameObject)Instantiate(Resources.Load("ServiceAsset"), Instance.transform)).GetComponent<AssetService>();
                }
                return serviceAsset;
            }
        }

        private static Database data;
        public static Database Data
        {
            get
            {
                if (data == null)
                {
                    data = ((GameObject)Instantiate(Resources.Load("Data"), Instance.transform)).GetComponent<Database>();
                }
                return data;
            }
        }
        private static EconomyService serviceEconomy;
        public static EconomyService ServiceEconomy
        {
            get
            {
                if (serviceEconomy == null)
                {
                    serviceEconomy = ((GameObject)Instantiate(Resources.Load("ServiceEconomy"), Instance.transform)).GetComponent<EconomyService>();
                }
                return serviceEconomy;
            }
        }
        private static NetworkService serviceNetwork;
        public static NetworkService ServiceNetwork
        {
            get
            {
                if (serviceNetwork == null)
                {
                    serviceNetwork = ((GameObject)Instantiate(Resources.Load("ServiceNetwork"), Instance.transform)).GetComponent<NetworkService>();
                    serviceNetwork.Init();
                }
                return serviceNetwork;
            }
        }
        private static PlayerService servicePlayer;
        public static PlayerService ServicePlayer
        {
            get
            {
                if (servicePlayer == null)
                {
                    servicePlayer = ((GameObject)Instantiate(Resources.Load("ServicePlayer"), Instance.transform)).GetComponent<PlayerService>();
                }
                return servicePlayer;
            }
        }
        private static GameService serviceGame;
        public static GameService ServiceGame
        {
            get
            {
                if (serviceGame == null)
                {
                    serviceGame = ((GameObject)Instantiate(Resources.Load("ServiceGame"), Instance.transform)).GetComponent<GameService>();
                }
                return serviceGame;
            }
        }

        private static volatile MainController instance;
        private static object syncRoot = new Object();

        private MainController() { }

        public static MainController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new GameObject("MainController").AddComponent<MainController>();
                    }
                }
                return instance;
            }
        }

        private void OnEnable()
        {
            // Services
            settingsService = new SettingsService();

            PlayerEvent.OnPlayerLogin += PlayerLogin;

            instance = this;
            DontDestroyOnLoad(gameObject);
            BaseMenuCanvas firstScreen = GameObject.Find("LoginScreen").GetComponent<BaseMenuCanvas>();
            firstScreen.GoToScreen(firstScreen);
            currentScreen = firstScreen;
            playerData = new PlayerData();

            _gotFacebookData = false;
        }

        private void OnDisable()
        {
            PlayerEvent.OnPlayerLogin -= PlayerLogin;
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

        public void OnHideUnity(bool isGameShown)
        {
            Time.timeScale = isGameShown ? 1 : 0;
        }

        public void CallFBLogout()
        {
            FB.LogOut();
        }

        private void GetFacebookName(IGraphResult result)
        {
            string fbName = result.ResultDictionary["name"].ToString();

            Debug.Log("fbName: " + fbName);

            playerData.SetProfileName(fbName);
        }

        private void GetFacebookPicture(IGraphResult result)
        {
            Debug.Log(result.Texture);
            if (result.Texture != null)
            {
                GameObject.Find("AvatarIcon").GetComponent<Image>().sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2());
                playerData.SetProfilePicture(result.Texture);
            }
        }

        private void GetFacebookPictureURL(IGraphResult result)
        {
            IDictionary<string, object> innerDict = (Dictionary<string, object>)result.ResultDictionary["picture"];
            IDictionary<string, object> dataDict = (Dictionary<string, object>)innerDict["data"];

            playerData.pictureURL = dataDict["url"].ToString();
            Debug.Log("pic : " + dataDict["url"].ToString());
        }

        void PlayerLogin(LoginType loginType, PlayerService.LoginRequestObject loginObject)
        {
            Data.temporary.stages = loginObject.stages;

            List<PlayerStatsInfo> randomPsi = new List<PlayerStatsInfo>();
            PlayerStatsInfo psi1 = new PlayerStatsInfo(Random.Range(1, 4), Random.Range(10, 3000), Random.Range(1, 30), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2));
            PlayerStatsInfo psi2 = new PlayerStatsInfo(Random.Range(5, 11), Random.Range(300, 200), Random.Range(11, 90), Random.Range(0, 3), Random.Range(0, 3), Random.Range(0, 3), Random.Range(0, 3));
            PlayerStatsInfo psi3 = new PlayerStatsInfo(Random.Range(12, 14), Random.Range(200, 200), Random.Range(100, 250), Random.Range(1, 4), Random.Range(1, 4), Random.Range(1, 4), Random.Range(1, 4));
            PlayerStatsInfo psi4 = new PlayerStatsInfo(Random.Range(15, 17), Random.Range(200, 500), Random.Range(170, 600), Random.Range(2, 6), Random.Range(2, 6), Random.Range(2, 6), Random.Range(2, 6));
            PlayerStatsInfo psi5 = new PlayerStatsInfo(Random.Range(18, 22), Random.Range(300, 1000), Random.Range(600, 1500), Random.Range(2, 6), Random.Range(2, 6), Random.Range(2, 6), Random.Range(2, 6));
            randomPsi.Add(psi1);
            randomPsi.Add(psi2);
            randomPsi.Add(psi3);
            randomPsi.Add(psi4);
            randomPsi.Add(psi5);

            PlayerStatsInfo psi = randomPsi[Random.Range(0, randomPsi.Count)];

            PlayerService.User user = loginObject.user;

            playerData.avatarBorderSyscode = "frame0" + (Random.Range(1, 7)).ToString();
            playerData.SetAvatarBorder((Data.sprites.GetAvatarBorderEntry(playerData.avatarBorderSyscode)).normal);
            if (loginObject.user != null) { playerData.SetProfileName(loginObject.user.first_name + " " + loginObject.user.last_name); }
            playerData.SetUserId(loginObject.user_id);
            playerData.SetCoins(loginObject.coins);
            playerData.SetXp(loginObject.XPlevel);
            playerData.SetUnspentSkill(loginObject.skillPoints);
            playerData.SetSkillLevel("blue", psi.bluePowerLevel);
            playerData.SetSkillLevel("green", psi.greenPowerLevel);
            playerData.SetSkillLevel("red", psi.redPowerLevel);
            playerData.SetSkillLevel("yellow", psi.yellowPowerLevel);
        }
    }

    public class PlayerData
    {
        public string userId;
        public int profileID;
        public string profileName { get; private set; }
        public string pictureURL = "";
        public Texture2D profilePicture { get; private set; }
        public Sprite profilePictureSprite;
        public Sprite AvatarBorder { get; private set; }
        public string avatarBorderSyscode;

        public int coins { get; private set; }
        public int trophies { get; private set; }
        public int xp { get; private set; }
        public int unspentSkill { get; private set; }
        public int blueSkill { get; private set; }
        public int greenSkill { get; private set; }
        public int redSkill { get; private set; }
        public int yellowSkill { get; private set; }

        public void SetUserId(string value)
        {
            userId = value;
        }
        public void SetProfileName(string value)
        {
            profileName = value;
            PlayerEvent.ProfileNameChange(profileName);
        }

        public int GetSkillLevel(string skillColor)
        {
            switch (skillColor)
            {
                case "blue":
                    return blueSkill;
                case "green":
                    return greenSkill;
                case "red":
                    return redSkill;
                case "yellow":
                    return yellowSkill;
                default:
                    return 0;
            }
        }
        public void SetSkillLevel(string skillColor, int amount)
        {
            switch (skillColor)
            {
                case "blue":
                    blueSkill = amount;
                    PlayerEvent.SkillLevelChange("blue", blueSkill);
                    break;
                case "green":
                    greenSkill = amount;
                    PlayerEvent.SkillLevelChange("green", greenSkill);
                    break;
                case "red":
                    redSkill = amount;
                    PlayerEvent.SkillLevelChange("red", redSkill);
                    break;
                case "yellow":
                    yellowSkill = amount;
                    PlayerEvent.SkillLevelChange("yellow", yellowSkill);
                    break;
            }
        }

        public void SetProfilePicture(Texture2D newImage)
        {
            profilePicture = newImage;
            PlayerEvent.ProfileImageChange(profilePicture);
        }
        public void SetAvatarBorder(Sprite value)
        {
            AvatarBorder = value;
            PlayerEvent.AvatarBorderChange(AvatarBorder);
        }
        public void SetCoins(int value)
        {
            coins = value;
            PlayerEvent.CoinAmountChange(coins);
        }
        public void SetTrophies(int value)
        {
            PlayerEvent.TrophyAmountChange(value);
            trophies = value;
        }
        public void SetXp(int value)
        {
            PlayerEvent.XpAmountChange(value);
            xp = value;
        }
        public void SetUnspentSkill(int value)
        {
            PlayerEvent.UnspentSkillPointAmountChange(value);
            unspentSkill = value;
        }

        public void AddCoins(int value)
        {
            coins += value;
            PlayerEvent.CoinAmountChange(coins);
        }
        public void AddUnspentSkillPoints(int value)
        {
            unspentSkill += value;
            PlayerEvent.UnspentSkillPointAmountChange(unspentSkill);
        }
    }

    [System.Serializable]
    public class PlayerStatsInfo
    {
        public int xpLevel;
        public int coinAmount;
        public int skillAmount;

        public int bluePowerLevel;
        public int greenPowerLevel;
        public int redPowerLevel;
        public int yellowPowerLevel;

        public PlayerStatsInfo(int xpLevel, int coinAmount, int skillAmount, int bluePowerLevel, int greenPowerLevel, int redPowerLevel, int yellowPowerLevel)
        {
            this.xpLevel = xpLevel;
            this.coinAmount = coinAmount;
            this.skillAmount = skillAmount;

            this.bluePowerLevel = bluePowerLevel;
            this.greenPowerLevel = greenPowerLevel;
            this.redPowerLevel = redPowerLevel;
            this.yellowPowerLevel = yellowPowerLevel;
        }

        public int GetSkillLevel(string skillColor)
        {
            switch (skillColor)
            {
                case "blue":
                    return bluePowerLevel;
                case "green":
                    return greenPowerLevel;
                case "red":
                    return redPowerLevel;
                case "yellow":
                    return yellowPowerLevel;
                default:
                    return 0;
            }
        }

        public static PlayerStatsInfo CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerStatsInfo>(jsonString);
        }
    }
}