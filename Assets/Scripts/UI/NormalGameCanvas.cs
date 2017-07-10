using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class NormalGameCanvas : BaseMenuCanvas
    {

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

        public override void Show()
        {
            base.Show();
            if (!PhotonNetwork.connected)
                PhotonNetwork.ConnectUsingSettings("v0.1");
            PhotonConnect.Instance.Connect();
        }

    }
}