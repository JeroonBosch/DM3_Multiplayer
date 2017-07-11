using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            Transform textObject = transform.Find("AmountOfPlayersText");
            int playerCount = 0;
            if (PhotonNetwork.connected)
                playerCount = PhotonNetwork.countOfPlayers;
            if (textObject)
                textObject.GetComponent<Text>().text = "Current amount of players: " + playerCount;
        }

        public override void Show()
        {
            base.Show();
            if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
                PhotonNetwork.ConnectUsingSettings("v0.1");
            //PhotonConnect.Instance.Connect();
        }

        public void SetReady ()
        {
            PhotonConnect.Instance.MatchPlayers();
        }
    }
}