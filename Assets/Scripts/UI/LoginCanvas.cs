using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class LoginCanvas : BaseMenuCanvas
    {
        //This class passes on the 'nickname' to Photon.
        private InputField _text;

        protected override void Start()
        {
            base.Start();

            _text = GameObject.Find("NameField").GetComponent<InputField>();
            _text.text = "Player" + Random.Range(1000, 9999).ToString();

            GameObject.Find("LoggingIn").GetComponent<Text>().enabled = false;
        }

        protected override void Update()
        {
            base.Update();

            if (PhotonNetwork.connected) { 
                GoToScreen(GameObject.Find("MainmenuScreen").GetComponent<BaseMenuCanvas>());
                enabled = false;
            }
        }


        public void Login()
        {
            GameObject.Find("LoggingIn").GetComponent<Text>().enabled = true;
            GameObject.Find("NameField").SetActive(false);
            GameObject.Find("LoginButton").SetActive(false);
            GameObject.Find("PleaseEnterNickname").SetActive(false);
            PhotonConnect.Instance.EnsureConnection();

            if (_text.text != "")
                PhotonConnect.Instance.profileName = _text.text;
            else
                PhotonConnect.Instance.profileName = "Player" + Random.Range(1000, 9999).ToString();
        }
    }
}