using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class LoadingScreenCanvas : BaseMenuCanvas
    {
        private bool _matchFound;

        private GameObject _searchObject;
        private GameObject _opponentAvatar;

        float timer = 0f;
        float timeUntilStart = 3f;

        protected override void Start ()
        {
            base.Start();

            _matchFound = false;

            _searchObject = GameObject.Find("FindingOpponent");
            _opponentAvatar = GameObject.Find("player2");
            GameObject.Find("LookingForMatch").GetComponent<Text>().text = "Looking for match..."; 

            if (GameObject.FindGameObjectsWithTag("Player").Length >= 2)
            {
                if (GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Player>().localID == 0)
                    GameObject.Find("Player1Name").GetComponent<Text>().text = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Player>().profileName;
                if (GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Player>().localID == 1)
                    GameObject.Find("Player2Name").GetComponent<Text>().text = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Player>().profileName;
                if (GameObject.FindGameObjectsWithTag("Player")[1].GetComponent<Player>().localID == 0)
                    GameObject.Find("Player1Name").GetComponent<Text>().text = GameObject.FindGameObjectsWithTag("Player")[1].GetComponent<Player>().profileName;
                if (GameObject.FindGameObjectsWithTag("Player")[1].GetComponent<Player>().localID == 1)
                    GameObject.Find("Player2Name").GetComponent<Text>().text = GameObject.FindGameObjectsWithTag("Player")[1].GetComponent<Player>().profileName;
            }

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                go.transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
            }

            _opponentAvatar.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
                player.UpdateLabels();

            if (players.Length == 2)
            {
                if (!_matchFound) //Do once.
                    MatchFound();

                timer += Time.deltaTime;

                if (timer > timeUntilStart) { 
                    GoToScreen(GameObject.Find("PlayScreen").GetComponent<BaseMenuCanvas>());
                    enabled = false;
                }
            }
        }

        private void MatchFound ()
        {
            _matchFound = true;
            GameObject.Find("LookingForMatch").GetComponent<Text>().text = "Match found!";

            if (!_opponentAvatar.GetActive())
            {
                _opponentAvatar.SetActive(true);
                _searchObject.SetActive(false);
            }

            if (!PhotonNetwork.isMasterClient)
            {
                GameObject.Find("player1").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarB");
                GameObject.Find("player2").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarA");
            }
        }
    }
}