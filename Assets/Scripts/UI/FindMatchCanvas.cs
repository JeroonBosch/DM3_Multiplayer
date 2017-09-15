using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class FindMatchCanvas : BaseMenuCanvas
    {
        private bool _matchFound;

        private GameObject _searchObject;
        private GameObject _opponentAvatar;

        private float _timer = 0f;
        private float _timeUntilStart = 3f;

        protected override void Start ()
        {
            base.Start();

            _matchFound = false;

            _searchObject = GameObject.Find("FindingOpponent");
            _opponentAvatar = GameObject.Find("player2");
            GameObject.Find("LookingForMatch").GetComponent<Text>().text = "Looking for match..."; 

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

                _timer += Time.deltaTime;

                if (_timer > _timeUntilStart) {
                    //GoToScreen(GameObject.Find("PlayScreen").GetComponent<BaseMenuCanvas>());
                    PhotonNetwork.LoadLevel("Match");
                    Debug.Log("Menu loaded because a player left the match.");
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

            if (!PhotonNetwork.isMasterClient) //okay
            {
                GameObject.Find("player1").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarB");
                GameObject.Find("player2").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarA");
            }
        }
    }
}