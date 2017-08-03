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

        GameObject findOpponent;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;

            findOpponent = GameObject.Find("FindOpponent");
            findOpponent.SetActive(false);
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            Transform textObject = GameObject.Find("AmountOfPlayersText").transform;
            int playerCount = 0;
            if (PhotonNetwork.connected)
                playerCount = PhotonNetwork.countOfPlayers;
            if (textObject)
                textObject.GetComponent<Text>().text = playerCount + " players online.";

            if (_findingMatch)
                timeOut += Time.deltaTime;

            if (timeOut > 30f)
            {
                timeOut = 0f;
                _findingMatch = false;
                if (PhotonNetwork.inRoom)
                    PhotonNetwork.LeaveRoom();
                else
                    PhotonNetwork.Reconnect();
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
            findOpponent.SetActive(true);
        }

        private void ResetReadyButton ()
        {
            Button readyButton = transform.Find("ReadyButton").GetComponent<Button>();
            Text buttonText = readyButton.transform.Find("Text").GetComponent<Text>();
            buttonText.text = "Boot Camp";
            readyButton.interactable = true;
            PhotonConnect.Instance.ConnectNormalGameroom();
            findOpponent.SetActive(false);
        }
    }
}