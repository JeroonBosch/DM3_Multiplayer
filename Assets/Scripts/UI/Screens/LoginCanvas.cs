using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class LoginCanvas : BaseMenuCanvas
    {
        //This class passes on the 'nickname' to Photon.
        private InputField _text;

        private bool _tryingToLogin;
        private float _loginTimer;

        public GameObject[] hideWhenLoggingIn;

        private void Awake()
        {
            MainController.Instance.FacebookInit();
            GoToScreen(this); //start screen.
        }

        protected override void Start()
        {
            base.Start();

            _text = GameObject.Find("NameField").GetComponent<InputField>();
            _text.text = "Guest" + Random.Range(1000, 9999).ToString();

            GameObject.Find("LoggingIn").GetComponent<Text>().enabled = false;
        }

        protected override void Update()
        {
            base.Update();

            if (_tryingToLogin)
            {
                _loginTimer += Time.deltaTime;

                bool isLoggedIn = true;
                if (MainController.Instance.wantsPhotonConnection && !PhotonNetwork.connected) //TODO Add check for other connections, such as Database or Facebook, here.
                    isLoggedIn = false;

                if (MainController.Instance.wantsFBConnection && !MainController.Instance.facebookConnected)
                    isLoggedIn = false;

                if (isLoggedIn)
                {
                    GoToScreen(FindObjectOfType<MainmenuCanvas>());
                    _tryingToLogin = false;
                    enabled = false;
                }

                if (_loginTimer > 10f)
                {
                    TimeOutLogin();
                }
            }
        }

        private void TimeOutLogin()
        {
            _loginTimer = 0f;
            ShowLoginFields();
        }


        public void Login()
        {
            _tryingToLogin = true;

            HideLoginFields();
            MainController.Instance.wantsFBConnection = false;
            PhotonController.Instance.EnsureConnection();

            if (_text.text != "")
                MainController.Instance.playerData.profileName = _text.text;
            else
                MainController.Instance.playerData.profileName = "Guest" + Random.Range(1000, 9999).ToString();
        }

        public void LoginFB()
        {
            _tryingToLogin = true;

            HideLoginFields();
            MainController.Instance.wantsFBConnection = true;
            PhotonController.Instance.EnsureConnection();

            MainController.Instance.CallFBLogin();
        }

        private void HideLoginFields ()
        {
            foreach (GameObject go in hideWhenLoggingIn)
            {
                go.SetActive(false);
            }
            GameObject.Find("LoggingIn").GetComponent<Text>().enabled = true;
        }

        private void ShowLoginFields()
        {
            foreach (GameObject go in hideWhenLoggingIn)
            {
                go.SetActive(true);
            }
            GameObject.Find("LoggingIn").GetComponent<Text>().enabled = false;
        }
    }
}