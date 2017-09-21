using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class LoginCanvas : BaseMenuCanvas
    {
        //This class passes on the 'nickname' to Photon.
        private InputField _text;

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

            if (PhotonNetwork.connected) { 
                GoToScreen(FindObjectOfType<MainmenuCanvas>());
                enabled = false;
            }
        }


        public void Login()
        {
            HideLoginFields();
            PhotonController.Instance.EnsureConnection();

            if (_text.text != "")
                MainController.Instance.playerData.profileName = _text.text;
            else
                MainController.Instance.playerData.profileName = "Guest" + Random.Range(1000, 9999).ToString();
        }

        public void LoginFB()
        {


            HideLoginFields();
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
    }
}