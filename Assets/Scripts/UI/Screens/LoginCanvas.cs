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
        [SerializeField] Text loggingInText;

        private bool alreadyConnected;
        private bool connectingToPhoton;
        private float _loginTimer;

        public GameObject[] hideWhenLoggingIn;

        private void Awake()
        {
            GoToScreen(this); //start screen.
            Debug.Log("HexaClash: " + MainController.settingsService.hexaClash);
        }

        protected override void Start()
        {
            base.Start();
            
            loggingInText.enabled = false;
            alreadyConnected = false;

            if (((MainController.settingsService.lastLoginType == LoginType.FACEBOOK && FB.IsInitialized && FB.IsLoggedIn) || MainController.settingsService.lastLoginType == LoginType.DEVICE) && !string.IsNullOrEmpty(MainController.settingsService.hexaClash))
            {
                alreadyConnected = true;
                if (PhotonNetwork.connected) { ConnectedToPhoton(); }
                else
                {
                    SetLoginText("Connecting to Photon..."); // Photon is only required for playing against other opponents.
                    connectingToPhoton = true;
                    PhotonController.Instance.EnsureConnection();
                }
                return;
            }

            ShowLoginFields();
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

                if (_loginTimer > Constants.loginTimeout)
                {
                    TimeOutLogin();
                }
            }
        }

        private void TimeOutLogin()
        {
            GoToScreen(FindObjectOfType<MainmenuCanvas>());
            connectingToPhoton = false;
            enabled = false;
            _loginTimer = 0f;

            UIEvent.Info("Failed to connect Photon. Matchmaking unavailable. Establishing connection.", PopupType.Warning);
        }

        public void Login()
        {
            HideLoginFields();
            MainController.settingsService.lastLoginType = LoginType.DEVICE;

            //MainController.ServicePlayer.Ping(OnInitConnect);
            
            SetLoginText("Logging in...");
            string mobile_id = string.IsNullOrEmpty(MainController.settingsService.mobileId) ? "new" : MainController.settingsService.mobileId;
            MainController.ServicePlayer.Login(null, mobile_id, OnPlayerLogin);
        }

        public void LoginFB()
        {
            HideLoginFields();
            MainController.settingsService.lastLoginType = LoginType.FACEBOOK;

            //MainController.ServicePlayer.Ping(OnInitConnect);

            SetLoginText("Initializing FaceBook...");
            if (FB.IsInitialized) { Debug.LogWarning("Already initialized"); InitCallback(); } else { Debug.Log("Not initialized"); FB.Init(InitCallback, MainController.Instance.OnHideUnity); }

        }
        private void OnInitConnect(bool isSuccess, string message, PlayerService.PingRequestObject pingObject)
        {
            SetLoginText("Pinging server...");
            if (!isSuccess)
            {
                MainController.settingsService.lastLoginType = LoginType.NONE;
                if (!string.IsNullOrEmpty(message))
                {
                    UIEvent.Info(message, PopupType.Error);
                    // TODO: display this for the player
                    Debug.LogError("Failed to ping the server.");
                    Debug.LogError(message);
                    return;
                }
                UIEvent.Info("Failed to ping the server", PopupType.Error);
            }
            if (pingObject == null)
            {
                MainController.settingsService.lastLoginType = LoginType.NONE;
                // TODO: display this for the player
                UIEvent.Info("Could not serialize the ping object", PopupType.Error);
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
            Debug.Log("FB.Init callback");
            if (FB.IsInitialized)
            {
                Debug.Log("FB.IsInitialized");
                FB.ActivateApp();
                var perms = new List<string>() { "public_profile", "user_friends" };
                SetLoginText("Logging to Facebook...");
                FB.LogInWithReadPermissions(perms, AuthCallback);
            }
            else
            {
                // TODO: Event for facebook login failure (popup).
                ShowLoginFields();
                UIEvent.Info("Facebook SDK init failure", PopupType.Error);
                Debug.LogError("Failed to Initialize the Facebook SDK");
            }
        }

        private void AuthCallback(ILoginResult result)
        {
            Debug.Log("AuthCallback");
            if (FB.IsLoggedIn)
            {
                SetLoginText("Logging to Server...");
                Debug.Log("IsLoggedIn");
                var aToken = AccessToken.CurrentAccessToken;
                Debug.Log("Facebook TOKEN: " + aToken.UserId);
                string mobile_id = string.IsNullOrEmpty(MainController.settingsService.mobileId) ? "" : MainController.settingsService.mobileId;
                MainController.ServicePlayer.Login(aToken.TokenString, mobile_id, OnPlayerLogin);
            }
            else
            {
                Debug.Log("User cancelled login");
                if (!string.IsNullOrEmpty(result.Error))
                {
                    UIEvent.Info(result.Error, PopupType.Error);
                }
                ShowLoginFields();
            }
        }

        private void OnPlayerLogin(bool isSuccess, string message, PlayerService.LoginRequestObject loginObject)
        {
            SetLoginText("Login finished");
            Debug.LogError("OnPlayerLogin");
            Debug.LogError(message);
			if (!isSuccess || !string.IsNullOrEmpty(message))
            {
				Debug.LogError ("Failed to login to the server.");
				if (!string.IsNullOrEmpty (message)) {
					UIEvent.Info (message, PopupType.Error);
					Debug.LogError (message);
				} else {
					UIEvent.Info ("Failed to login to the server.", PopupType.Error);
				}
				ResetLoginCanvas ();
				return;
            }
				
            if (loginObject == null)
            {
                MainController.settingsService.lastLoginType = LoginType.NONE;
                UIEvent.Info("Login serialization failed", PopupType.Error);
                Debug.Log("Something went wrong with serializing the loginObject.");
                return;
            }

            if (string.IsNullOrEmpty(loginObject.mobile_id))
            {
                Debug.Log("MOBILE_ID: " + loginObject.mobile_id);
                MainController.settingsService.mobileId = "";
            } else { MainController.settingsService.mobileId = loginObject.mobile_id; }

            if (!string.IsNullOrEmpty(loginObject.user_id)) { DevConsole.userId = loginObject.user_id; }

            if (MainController.settingsService.lastLoginType == LoginType.FACEBOOK && !string.IsNullOrEmpty(loginObject.user_id))
            {
                MainController.settingsService.mobileId = "";
            }

            PlayerEvent.PlayerLogin(MainController.settingsService.lastLoginType, loginObject); // Player has logged in already. We have the info from the server.

            if (PhotonNetwork.connected) { ConnectedToPhoton(); }
            else
            {
                SetLoginText("Connecting to Photon..."); // Photon is only required for playing against other opponents.
                connectingToPhoton = true;
                PhotonController.Instance.EnsureConnection();
            }
        }

        private void ConnectedToPhoton()
        {
            if (alreadyConnected) { MainController.Instance.playerData.Broadcast(); }
            GoToScreen(FindObjectOfType<MainmenuCanvas>());
            connectingToPhoton = false;
            enabled = false;
        }

        private void FailedToConnectToPhoton(DisconnectCause cause)
        {
            GoToScreen(FindObjectOfType<MainmenuCanvas>());
            connectingToPhoton = false;
            enabled = false;

            UIEvent.Info("Failed to connect Photon. Matchmaking unavailable. Establishing connection.", PopupType.Warning);
        }

		private void ResetLoginCanvas() {
			MainController.ServicePlayer.Logout ();
			MainController.settingsService.lastLoginType = LoginType.NONE;
			connectingToPhoton = false;
			enabled = true;
			ShowLoginFields ();
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