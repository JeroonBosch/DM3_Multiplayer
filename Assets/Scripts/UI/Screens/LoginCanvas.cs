using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Facebook.Unity;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class LoginCanvas : BaseMenuCanvas
    {
        //This class passes on the 'nickname' to Photon.
        [SerializeField] InputField nameFieldText;

        [SerializeField] Text loggingInText;

        private bool connectingToPhoton;
        private float _loginTimer;

        public GameObject[] hideWhenLoggingIn;

        private void Awake()
        {
            GoToScreen(this); //start screen.
        }

        protected override void Start()
        {
            base.Start();
            
            nameFieldText.text = "Guest" + UnityEngine.Random.Range(1000, 9999).ToString();
            loggingInText.enabled = false;
        }

        protected override void OnEnable()
        {
            NetworkEvent.OnFailedToConnectToPhoton += FailedToConnectToPhoton;
            NetworkEvent.OnConnectedToPhoton += ConnectedToPhoton;
        }

        protected override void OnDisable()
        {
            NetworkEvent.OnFailedToConnectToPhoton -= FailedToConnectToPhoton;
            NetworkEvent.OnConnectedToPhoton -= ConnectedToPhoton;
        }

        protected override void Update()
        {
            base.Update();

            if (connectingToPhoton)
            {
                _loginTimer += Time.deltaTime;
                if (_loginTimer > 3)
                {
                    loggingInText.text = "Connecting to Photon...(" + ((int)_loginTimer).ToString() + ")";
                }

                bool isLoggedIn = true;

                if (MainController.Instance.wantsFBConnection && !MainController.Instance.facebookConnected)
                    isLoggedIn = false;

                if (isLoggedIn)
                {
                    GoToScreen(FindObjectOfType<MainmenuCanvas>());
                    connectingToPhoton = false;
                    enabled = false;
                }

                if (_loginTimer > Constants.loginTimeout)
                {
                    TimeOutLogin();
                }
            }
        }

        private void TimeOutLogin()
        {
            connectingToPhoton = false;
            _loginTimer = 0f;
            ShowLoginFields();
        }

        public void Login()
        {
            MainController.settingsService.lastLoginType = LoginType.DEVICE;
            MainController.ServicePlayer.Ping(OnInitConnect);
            HideLoginFields();

            /*
            _tryingToLogin = true;
            
            MainController.Instance.wantsFBConnection = false;
            PhotonController.Instance.EnsureConnection();

            if (nameFieldText.text != "")  {
                MainController.Instance.playerData.SetProfileName(nameFieldText.text);
            }
            else {
                MainController.Instance.playerData.SetProfileName("Guest" + Random.Range(1000, 9999).ToString());
            }
            */
        }

        public void LoginFB()
        {
            MainController.settingsService.lastLoginType = LoginType.FACEBOOK;
            MainController.ServicePlayer.Ping(OnInitConnect);
            HideLoginFields();
        }
        private void OnInitConnect(bool isSuccess, string message, PlayerService.PingRequestObject pingObject)
        {
            SetLoginText("Pinging server...");
            if (!isSuccess)
            {
                MainController.settingsService.lastLoginType = LoginType.NONE;
                if (!string.IsNullOrEmpty(message))
                {
                    // TODO: display this for the player
                    Debug.Log("Failed to ping the server.");
                    Debug.LogError(message);
                    return;
                }
            }
            if (pingObject == null)
            {
                MainController.settingsService.lastLoginType = LoginType.NONE;
                // TODO: display this for the player
                Debug.Log("Something went wrong with serializing the pingObject.");
                return;
            }
            if (MainController.settingsService.lastLoginType == LoginType.FACEBOOK)
            {
                SetLoginText("Initializing FaceBook...");
                if (FB.IsInitialized) { InitCallback(); } else { FB.Init(InitCallback, MainController.Instance.OnHideUnity); }
            }
            else if (MainController.settingsService.lastLoginType == LoginType.DEVICE)
            {
                SetLoginText("Logging in...");
                string mobile_id = string.IsNullOrEmpty(MainController.settingsService.mobileId) ? "new" : MainController.settingsService.mobileId;
                MainController.ServicePlayer.Login("", mobile_id, OnPlayerLogin);
            }
        }
        

        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
                var perms = new List<string>() { "public_profile", "email", "user_friends" };
                SetLoginText("Logging in...");
                FB.LogInWithReadPermissions(perms, AuthCallback);
            }
            else
            {
                // TODO: Event for facebook login failure (popup).
                ShowLoginFields();
                Debug.Log("Failed to Initialize the Facebook SDK");
            }
        }

        private void AuthCallback(ILoginResult result)
        {
            if (FB.IsLoggedIn)
            {
                var aToken = AccessToken.CurrentAccessToken;
                Debug.Log("Facebook TOKEN: " + aToken.UserId);
                string mobile_id = string.IsNullOrEmpty(MainController.settingsService.mobileId) ? "new" : MainController.settingsService.mobileId;
                MainController.ServicePlayer.Login(aToken.TokenString, mobile_id, OnPlayerLogin);
            }
            else
            {
                Debug.Log("User cancelled login");
                ShowLoginFields();
            }
        }

        private void OnPlayerLogin(bool isSuccess, string message, PlayerService.LoginRequestObject loginObject)
        {
            if (!isSuccess)
            {
                MainController.settingsService.lastLoginType = LoginType.NONE;
                if (!string.IsNullOrEmpty(message))
                {
                    // TODO: display this for the player
                    Debug.Log("Failed to login to the server.");
                    Debug.LogError(message);
                    return;
                }
            }
            if (loginObject == null)
            {
                MainController.settingsService.lastLoginType = LoginType.NONE;
                // TODO: display this for the player
                Debug.Log("Something went wrong with serializing the loginObject.");
                return;
            }

            if (!string.IsNullOrEmpty(loginObject.mobile_id))
            {
                Debug.Log("MOBILE_ID: " + loginObject.mobile_id);
                MainController.settingsService.mobileId = loginObject.mobile_id;
            }

            PlayerEvent.PlayerLogin(MainController.settingsService.lastLoginType, loginObject); // Player has logged in already. We have the info from the server.
            SetLoginText("Connecting to Photon..."); // Photon is only required for playing against other opponents.
            connectingToPhoton = true;
            PhotonController.Instance.EnsureConnection();
        }

        private void ConnectedToPhoton()
        {
            GoToScreen(FindObjectOfType<MainmenuCanvas>());
            connectingToPhoton = false;
            enabled = false;
        }

        private void FailedToConnectToPhoton(DisconnectCause cause)
        {
            GoToScreen(FindObjectOfType<MainmenuCanvas>());
            connectingToPhoton = false;
            enabled = false;
        }

        private void HideLoginFields ()
        {
            foreach (GameObject go in hideWhenLoggingIn)
            {
                go.SetActive(false);
            }
        }

        private void ShowLoginFields()
        {
            SetLoginText("");
            foreach (GameObject go in hideWhenLoggingIn)
            {
                go.SetActive(true);
            }
        }
        private void SetLoginText(string info, bool enable = true)
        {
            loggingInText.text = info;
            loggingInText.enabled = (string.IsNullOrEmpty(info) || !enable) ? false : true;
        }
    }
}