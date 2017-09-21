using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class SelectStageCanvas : BaseMenuCanvas
    {
        //Currently used for both the Normal 1v1 game and tournament.
        GameObject _findOpponent;

        protected override void Start()
        {
            base.Start();
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;

            _findOpponent = transform.Find("FindOpponent").gameObject;
            _findOpponent.SetActive(false);
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
                playerCount = PhotonNetwork.countOfPlayers; //TODO Lobby player count instead of ALL players. Might not be possible.

            foreach (GameObject stage in stages)
            {
                Transform textObject = stage.transform.Find("AmountOfPlayersText").transform;
                if (textObject)
                    textObject.GetComponent<Text>().text = "Players online: " + playerCount;
            }
        }

        public void SetReady ()
        {
            PhotonController.Instance.MatchPlayers(); //This should load the next scene.
            //Button readyButton = transform.Find("ReadyButton").GetComponent<Button>();
            //readyButton.interactable = false;
            _findOpponent.SetActive(true);
        }
    }
}