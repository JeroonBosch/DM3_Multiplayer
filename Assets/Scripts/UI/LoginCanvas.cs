using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class LoginCanvas : BaseMenuCanvas
    {
        Text _text;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            _text = GameObject.Find("NameField").transform.Find("NameText").GetComponent<Text>();
            _text.text = "Player" + Random.Range(1000, 9999).ToString();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

        public void Login()
        {
            GoToScreen(GameObject.Find("MainmenuScreen").GetComponent<BaseMenuCanvas>());
            if (_text.text != "")
                PhotonConnect.Instance.profileName = _text.text;
            else
                PhotonConnect.Instance.profileName = "Player" + Random.Range(1000, 9999).ToString();

            PhotonConnect.Instance.EnsureConnection();
        }
    }
}