using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class NormalGameCanvas : BaseMenuCanvas
    {

        GameObject findOpponent;

        protected override void Start()
        {
            base.Start();
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;

            findOpponent = GameObject.Find("FindOpponent");
            findOpponent.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();
            GameObject[] stages = GameObject.FindGameObjectsWithTag("CarouselStage");

            int playerCount = 0;
            if (PhotonNetwork.connected)
                playerCount = PhotonNetwork.countOfPlayers;
            foreach (GameObject stage in stages)
            {
                Transform textObject = stage.transform.Find("AmountOfPlayersText").transform;
                if (textObject)
                    textObject.GetComponent<Text>().text = "Players online: " + playerCount;
            }
        }

        public override void Show()
        {
            base.Show();
        }

        public void SetReady ()
        {
            PhotonConnect.Instance.MatchPlayers(); //This should load the next scene.
            //Button readyButton = transform.Find("ReadyButton").GetComponent<Button>();
            //readyButton.interactable = false;
            findOpponent.SetActive(true);
        }
    }
}