using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class NormalGameCanvas : BaseMenuCanvas
    {
        bool _findingMatch;
        float timeOut = 0f;

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

            if (_findingMatch)
                timeOut += Time.deltaTime;

            if (timeOut > 10f)
            {
                _findingMatch = false;
                PhotonNetwork.LeaveRoom();
                ResetReadyButton();
            }
        }

        public override void Show()
        {
            base.Show();
        }

        public void SetReady ()
        {
            PhotonConnect.Instance.MatchPlayers();
            _findingMatch = true;
            Button readyButton = transform.Find("ReadyButton").GetComponent<Button>();
            Text buttonText = readyButton.transform.Find("Text").GetComponent<Text>();
            buttonText.text = "Finding match...";
            readyButton.interactable = false;
        }

        private void ResetReadyButton ()
        {
            Button readyButton = transform.Find("ReadyButton").GetComponent<Button>();
            Text buttonText = readyButton.transform.Find("Text").GetComponent<Text>();
            buttonText.text = "Find match";
            readyButton.interactable = true;
            PhotonConnect.Instance.ConnectNormalGameroom();
        }
    }
}