using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class MainmenuCanvas : BaseMenuCanvas
    {
        bool waitingForConnect = false;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (waitingForConnect)
                if (PhotonNetwork.connected)
                    PhotonConnect.Instance.ConnectNormalGameroom();
        }

        public void PlayNormalGame()
        {
            GoToScreen(GameObject.Find("NormalGameScreen").GetComponent<BaseMenuCanvas>());

            if (PhotonNetwork.connected)
                PhotonConnect.Instance.ConnectNormalGameroom();
            else
                waitingForConnect = true;
        }
    }
}