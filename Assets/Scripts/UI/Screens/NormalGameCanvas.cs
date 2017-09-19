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

            findOpponent = transform.Find("FindOpponent").gameObject;
            findOpponent.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            Transform carousel = transform.Find("StageCarousel");
            List<GameObject> stages = new List<GameObject>();
            foreach (Transform child in carousel)
            {
                if (child.tag == "CarouselStage")
                    stages.Add(child.gameObject);
            }

            int playerCount = 0;
            if (PhotonNetwork.connected)
                playerCount = PhotonNetwork.countOfPlayers; //TODO Lobby player count instead of ALL players
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
            PhotonController.Instance.MatchPlayers(); //This should load the next scene.
            //Button readyButton = transform.Find("ReadyButton").GetComponent<Button>();
            //readyButton.interactable = false;
            findOpponent.SetActive(true);
        }
    }
}